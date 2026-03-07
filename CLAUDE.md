# CvEvaluator — Contexto del Proyecto

## ¿Qué hace este proyecto?

**CvEvaluator** es una aplicación web que permite a reclutadores subir CVs en PDF y evaluarlos automáticamente contra una posición de trabajo usando un LLM local (Ollama + llama3:8b). El sistema procesa las evaluaciones de forma asíncrona y notifica al frontend en tiempo real vía SignalR.

---

## Stack tecnológico

**Backend:** .NET 8 · C# · Clean Architecture · PostgreSQL (EF Core) · ASP.NET Identity · JWT · SignalR · Ollama (llama3:8b)

**Frontend:** Angular 21 · TypeScript · Tailwind CSS · Standalone Components · Lazy loading

**Infraestructura:** Docker Compose · Dockerfile multi-stage

---

## Arquitectura del Backend (Clean Architecture)

```
CvEvaluator.sln
├── CvEvaluator.Api           → Controllers, Program.cs, middlewares
├── CvEvaluator.Application   → Services, Interfaces, DTOs, Prompts
├── CvEvaluator.Domain        → Entities, Enums, Models (sin dependencias)
└── CvEvaluator.Infrastructure → EF Core, Identity, Ollama, SignalR, Background Worker
```

**Regla crítica:** Domain no depende de nada. Application solo depende de Domain. Infrastructure implementa las interfaces de Application. Api orquesta todo.

---

## Entidades principales

### `JobPosition`
```
Id (Guid) | UserId (Guid) | Title | Description | CreatedAt
→ ICollection<CvEvaluation> Evaluations
```

### `CvEvaluation`
```
Id | UserId | JobPositionId (FK) | OriginalFilename | FileSizeBytes | FileHash
ExtractedText | EvaluationResult (JSONB) | OverallScore | Status | ErrorMessage
CreatedAt | EvaluatedAt
```

### `EvaluationStatus` (enum)
`Processing` → `Completed` | `Failed`

---

## Flujo de evaluación

1. Usuario selecciona una `JobPosition` y sube uno o varios PDFs
2. `CvEvaluationController.EvaluatePdf` recibe el request
3. `CvEvaluationService.EvaluateAsync` valida la posición, extrae texto del PDF (PdfDocumentParser), crea la entidad con `Status = Processing` y la encola
4. `CvEvaluationWorker` (BackgroundService) consume la cola, llama a `OllamaClient` con el prompt construido por `CvEvaluationPrompt.Build()`
5. El worker persiste el resultado y notifica al usuario vía SignalR (`EvaluationHub`)
6. El frontend Angular escucha el hub y actualiza el estado en tiempo real

---

## Prompt del LLM

El prompt se construye en `CvEvaluationService.Application/Prompts/CvEvaluationPrompt.cs`. Recibe `cvText`, `jobTitle` y `jobDescription`. Usa el `JobPosition` vinculado a la evaluación para alimentar el contexto del LLM.

**Output esperado del LLM (JSON estricto):**
```json
{
  "score": 0-100,
  "yearsExperience": number,
  "matchesRequirements": boolean,
  "strengths": ["string"],
  "weaknesses": ["string"]
}
```

---

## Estado actual del proyecto (rama: `feature/JobPosition`)

### ✅ Implementado
- Autenticación JWT + ASP.NET Identity
- CRUD completo de `JobPosition` en **frontend** y **backend** (list, create, edit, detail)
- `JobPositionRepository` e `IJobPositionRepository` implementados
- Relación `CvEvaluation → JobPosition` en dominio y migración
- `CvEvaluationService` ya valida que el `jobPositionId` pertenezca al usuario
- Worker usa `JobPosition.Title` y `JobPosition.Description` para el prompt
- SignalR funcional para notificaciones de evaluación

## Decisiones de arquitectura tomadas

### LLM: soporte multi-proveedor (configurable)
El sistema debe soportar **Ollama (local)** y **proveedores externos (Claude API / OpenAI)** de forma intercambiable. La interfaz `ILlmClient` ya existe — la estrategia es:
- Mantener `ILlmClient` como abstracción en Application
- Implementar un `ClaudeClient` / `OpenAiClient` en Infrastructure (mismo patrón que `OllamaClient`)
- Seleccionar el proveedor vía configuración (`appsettings.json` → `"LlmProvider": "Ollama" | "Claude" | "OpenAI"`)
- Registrar el cliente correcto en `Program.cs` con un switch sobre la config
- **No usar herencia ni estrategia compleja** — el DI container es suficiente

### Prioridad de calidad: Clean Architecture estricta
- Nunca romper la regla de dependencias: `Domain ← Application ← Infrastructure ← Api`
- Nunca referenciar EF Core, HttpClient, ni ningún detalle de infraestructura desde Application o Domain
- Ante la duda, crear una interfaz nueva en Application antes de acoplar
- Los DTOs de request/response viven en Application, no en Api

---

## Convenciones del código

- **Siempre** verificar que el recurso pertenece al `userId` del JWT antes de devolver/modificar
- Los controllers solo orquestan — lógica de negocio en Application Services
- Usar `CancellationToken ct` en todos los métodos async
- DTOs separados de las entidades de dominio (nunca exponer entidades directamente)
- Migraciones con EF Core: `dotnet ef migrations add NombreMigracion --project CvEvaluator.Infrastructure --startup-project CvEvaluator.Api`
- **Angular:** Standalone components, `inject()` en lugar de constructor injection, HttpClient con Observables

---

## Configuración local

**Backend** (requiere PostgreSQL corriendo):
```bash
cd BackEnd
dotnet run --project CvEvaluator.Api
```

**Con Docker (Ollama incluido):**
```bash
docker-compose up
```

**Frontend:**
```bash
cd FrontEnd/cv-evaluator-ui
npm start
```
