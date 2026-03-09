# Especificaciones de IA — CvEvaluator

## Proveedor LLM actual

**Ollama** corriendo localmente con el modelo **llama3:8b**.

- Interfaz de abstracción: `ILlmClient` (Application/Interfaces)
- Implementación actual: `OllamaClient` (Infrastructure/Llm)
- El sistema está diseñado para ser **multi-proveedor** — ver sección al final

---

## Interfaz de abstracción

```csharp
// CvEvaluator.Application/Interfaces/ILlmClient.cs
public interface ILlmClient
{
    Task<string> EvaluateCvAsync(string prompt, CancellationToken ct);
}
```

Recibe el prompt completo y retorna un string JSON que debe coincidir con la estructura de `CvEvaluationResult`.

---

## Construcción del prompt

**Archivo:** `CvEvaluator.Application/Prompts/CvEvaluationPrompt.cs`

```csharp
public static class CvEvaluationPrompt
{
    public static string Build(string cvText, string jobTitle, string jobDescription)
```

### Template completo

```
You are an AI assistant evaluating a candidate CV.

TARGET ROLE:
{{JOB_TITLE}}

JOB DESCRIPTION:
{{JOB_DESCRIPTION}}

INSTRUCTIONS:
- Analyze the CV content
- The CV text may have lost visual formatting.
- Infer structure from headings, line breaks, and keywords.
- Evaluate the candidate strictly against the provided job description.
- Produce a STRICT JSON response
- Do NOT include explanations outside JSON
- Do NOT include markdown
- Do NOT add comments
- Your response must be only the JSON

OUTPUT FORMAT:
{
  "score": 0-100,
  "yearsExperience": number,
  "matchesRequirements": boolean,
  "strengths": ["string"],
  "weaknesses": ["string"]
}

CV CONTENT:
===== CV START =====
"
{{CV_TEXT}}
"
===== CV END =====
```

### Variables del template

| Variable | Origen |
|----------|--------|
| `{{JOB_TITLE}}` | `JobPosition.Title` |
| `{{JOB_DESCRIPTION}}` | `JobPosition.Description` |
| `{{CV_TEXT}}` | Texto extraído del PDF por `PdfDocumentParser` |

---

## Output esperado del LLM

El LLM debe retornar **únicamente** un JSON válido con esta estructura:

```json
{
  "score": 85,
  "yearsExperience": 7,
  "matchesRequirements": true,
  "strengths": [
    "Sólida experiencia en .NET Core y C#",
    "Experiencia con arquitecturas limpias",
    "Conocimiento de Docker y Kubernetes"
  ],
  "weaknesses": [
    "Sin experiencia en ML/AI",
    "No menciona experiencia con Angular"
  ]
}
```

Este JSON se deserializa a `CvEvaluationResult` (Domain/Models):

```csharp
public class CvEvaluationResult
{
    public int Score { get; set; }                        // 0-100
    public int YearsExperience { get; set; }
    public bool MatchesRequirements { get; set; }
    public required List<string> Strengths { get; set; }
    public required List<string> Weaknesses { get; set; }
}
```

---

## Extracción de texto del PDF

**Interfaz:** `IDocumentParser` (Application/Interfaces)
**Implementación:** `PdfDocumentParser` (Infrastructure/Parsing)

```csharp
public interface IDocumentParser
{
    Task<string> ParseAsync(Stream fileStream);
}
```

- Recibe el stream del PDF subido
- Extrae el texto plano (puede perder formato visual)
- El prompt le indica al LLM que debe inferir estructura desde keywords y saltos de línea

---

## Pipeline asíncrono de procesamiento

El procesamiento del LLM es asíncrono para no bloquear el request HTTP.

### Cola de evaluaciones

**Interfaz:** `IEvaluationQueue`
**Implementación:** `EvaluationQueue` — basada en `Channel<Guid>` de .NET

```csharp
public interface IEvaluationQueue
{
    ValueTask EnqueueAsync(Guid evaluationId);
}
```

- Se registra como **Singleton** en el DI container
- `CvEvaluationService` encola el `evaluationId` tras crear la entidad en DB
- `CvEvaluationWorker` consume de la cola en background

### Worker de procesamiento

**Clase:** `CvEvaluationWorker : BackgroundService` (Infrastructure/Background)

Flujo interno del worker:

```
loop:
  1. await queue.DequeueAsync(ct)              → obtiene evaluationId
  2. evaluation = repo.GetByIdAsync(id)        → fetch con jobPosition
  3. prompt = CvEvaluationPrompt.Build(        → construye el prompt
       evaluation.ExtractedText,
       evaluation.JobPosition.Title,
       evaluation.JobPosition.Description)
  4. json = llmClient.EvaluateCvAsync(prompt)  → llama al LLM (puede tardar)
  5. result = JsonSerializer.Deserialize<      → parsea el JSON
       CvEvaluationResult>(json)
  6. evaluation.Status = Completed             → actualiza la entidad
     evaluation.OverallScore = result.Score
     evaluation.EvaluationResult = json
     evaluation.EvaluatedAt = DateTime.UtcNow
  7. unitOfWork.SaveChangesAsync()             → persiste en PostgreSQL
  8. hub.SendAsync("EvaluationUpdated", dto)   → notifica al frontend
```

Si ocurre un error:
```
evaluation.Status = Failed
evaluation.ErrorMessage = exception.Message
unitOfWork.SaveChangesAsync()
hub.SendAsync("EvaluationUpdated", dto)  // frontend muestra el error
```

### SignalR

**Hub:** `EvaluationHub` (Infrastructure/SignalR)
**Ruta:** `/hubs/evaluations`
**Evento emitido:** `EvaluationUpdated`
**Payload:** `CvEvaluationDto`

El frontend conecta al hub vía `signalr.service.ts` y escucha `EvaluationUpdated` para actualizar el estado en tiempo real sin polling.

---

## Soporte multi-proveedor

### Estrategia

La interfaz `ILlmClient` en Application desacopla completamente la lógica de negocio del proveedor LLM. Para agregar un nuevo proveedor:

1. **Crear la implementación** en `Infrastructure/Llm/`:
   ```csharp
   // ClaudeClient.cs
   public class ClaudeClient : ILlmClient
   {
       public async Task<string> EvaluateCvAsync(string prompt, CancellationToken ct)
       {
           // Llamada a la Anthropic API
       }
   }
   ```

2. **Registrar en `Program.cs`** según configuración:
   ```csharp
   var provider = builder.Configuration["LlmProvider"]; // "Ollama" | "Claude" | "OpenAI"

   switch (provider)
   {
       case "Claude":
           builder.Services.AddHttpClient<ILlmClient, ClaudeClient>();
           break;
       case "OpenAI":
           builder.Services.AddHttpClient<ILlmClient, OpenAiClient>();
           break;
       default: // "Ollama"
           builder.Services.AddHttpClient<ILlmClient, OllamaClient>();
           break;
   }
   ```

3. **Configurar en `appsettings.json`**:
   ```json
   {
     "LlmProvider": "Ollama",
     "Ollama": {
       "BaseUrl": "http://localhost:11434",
       "Model": "llama3:8b"
     }
   }
   ```

**Principio:** No usar herencia ni patrón Strategy complejo — el DI container es suficiente. Application nunca sabe qué proveedor está activo.
