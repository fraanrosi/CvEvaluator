# Deploy en Producción — Render

## ¿Es Render un buen servicio para CvEvaluator?

**Respuesta corta: Sí, con condiciones.**

### Lo que Render hace bien para este stack

| Aspecto | Valoración |
|---------|-----------|
| Dockerfile ya listo en el repo | Render lo detecta y usa automáticamente |
| Static Site para Angular | Gratis, siempre activo, CDN global |
| PostgreSQL managed | Simple de configurar, backups incluidos |
| Variables de entorno seguras | Manejo correcto de secrets |
| Deploy automático desde GitHub | Push → deploy sin configuración extra |
| Soporte para SignalR (WebSockets) | Funciona en todos los planes pagos |

### Lo que tenés que saber antes de usarlo

**1. Free tier: los servicios "duermen"**
Los Web Services gratuitos se detienen después de 15 minutos sin tráfico. El primer request tras inactividad puede tardar 30-60 segundos (cold start). Para una demo o MVP está bien. Para producción real, necesitás el plan **Starter ($7/mes)** que mantiene el servicio activo.

**2. Free PostgreSQL vence en 90 días**
El plan gratuito de base de datos expira. Después cuesta $7/mes (plan Starter). Migrá antes de que venza o perderás los datos.

**3. SignalR requiere plan pago**
Los WebSockets (que usa SignalR) no funcionan en el free tier de Web Services. Necesitás al menos el plan **Starter ($7/mes)** para que el hub de notificaciones funcione.

**4. Ollama NO va en Render**
`llama3:8b` necesita ~8GB de RAM. La instancia Standard Plus de Render cuesta $85/mes. No es viable. Ver sección de LLM al final.

### Costo estimado en producción real

| Servicio | Plan | Costo/mes |
|---------|------|----------|
| Web Service (backend) | Starter | $7 |
| PostgreSQL | Starter | $7 |
| Static Site (frontend) | Free | $0 |
| **Total (sin LLM)** | | **$14/mes** |

---

## Arquitectura en Render

```
GitHub (push)
    │
    ├─── Static Site (Angular) ──────► cvevaluator-ui.onrender.com
    │         build: npm run build
    │         dist: dist/cv-evaluator-ui/browser
    │
    ├─── Web Service (API .NET 8) ───► cvevaluator-api.onrender.com
    │         build: Docker (Dockerfile existente)
    │         puerto: 8080 (ya configurado)
    │
    └─── PostgreSQL ─────────────────► (internal connection string)
              region: Oregon (us-west-2)
```

---

## Paso 1: Base de datos PostgreSQL

1. En Render Dashboard → **New → PostgreSQL**
2. Configurar:
   - **Name:** `cvevaluator-db`
   - **Database:** `cvevaluator`
   - **Region:** la misma que usarás para el backend (recomendado: Oregon)
   - **Plan:** Starter ($7/mes) o Free (90 días)
3. Una vez creada, copiar la **Internal Database URL** — la usarás en el backend.
   Tiene el formato: `postgresql://user:password@host/cvevaluator`

> **Importante:** Usá la *Internal* URL para conexión backend → DB (gratis dentro de Render). La *External* URL se usa solo para acceso desde tu máquina local.

---

## Paso 2: Backend (.NET 8)

### 2.1 Crear el Web Service

1. Render Dashboard → **New → Web Service**
2. Conectar el repositorio de GitHub
3. Configurar:
   - **Name:** `cvevaluator-api`
   - **Root Directory:** `BackEnd`
   - **Environment:** Docker
   - **Region:** misma que la DB
   - **Plan:** Starter ($7/mes) — necesario para SignalR
4. Render detectará el `Dockerfile` automáticamente

### 2.2 Variables de entorno

En la sección **Environment** del Web Service, agregar:

```
ConnectionStrings__DefaultConnection = [Internal Database URL del paso 1]
Jwt__Key                             = [string aleatoria de mínimo 32 caracteres]
Jwt__Issuer                          = https://cvevaluator-api.onrender.com
Jwt__Audience                        = https://cvevaluator-ui.onrender.com
Cors__AllowedOrigins__0              = https://cvevaluator-ui.onrender.com
LlmProvider                          = [ver sección LLM]
```

> En .NET, la configuración anidada `Jwt:Key` se expresa como `Jwt__Key` (doble guión bajo) en variables de entorno.

### 2.3 Migraciones de base de datos

El backend usa EF Core. Las migraciones se deben correr **una sola vez** tras el primer deploy.

**Opción A (recomendada): Auto-migración al startup**

Agregar en `Program.cs` antes de `app.Run(...)`:

```csharp
// Solo en primer deploy o cuando hay migraciones pendientes
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CvEvaluatorDbContext>();
    db.Database.Migrate();
}
```

**Opción B: Correr manualmente desde tu PC**
```bash
# Apuntar a la External Database URL de Render
$env:ConnectionStrings__DefaultConnection="[External Database URL]"
cd BackEnd
dotnet ef database update --project CvEvaluator.Infrastructure --startup-project CvEvaluator.Api
```

### 2.4 Health check

El backend ya tiene un endpoint `/health` que devuelve `200 OK`. Configurarlo en Render:
- **Health Check Path:** `/health`

---

## Paso 3: Frontend (Angular)

### 3.1 Actualizar `environment.prod.ts`

El archivo ya apunta a Render. Verificar que esté completo:

```typescript
// FrontEnd/cv-evaluator-ui/src/environments/environment.prod.ts
export const environment = {
  production: true,
  apiBaseUrl: 'https://cvevaluator-api.onrender.com/api',
  hubBaseUrl: 'https://cvevaluator-api.onrender.com'   // ← agregar si falta
};
```

### 3.2 Crear el archivo `_redirects` (SPA routing)

Angular es una SPA — todas las rutas deben redirigir al `index.html`. Crear el archivo:

```
FrontEnd/cv-evaluator-ui/public/_redirects
```

Con el contenido:
```
/*    /index.html    200
```

### 3.3 Crear el Static Site en Render

1. Render Dashboard → **New → Static Site**
2. Conectar el repositorio
3. Configurar:
   - **Name:** `cvevaluator-ui`
   - **Root Directory:** `FrontEnd/cv-evaluator-ui`
   - **Build Command:** `npm install && npm run build`
   - **Publish Directory:** `dist/cv-evaluator-ui/browser`

Render hace el build automáticamente en cada push a `main`.

---

## Paso 4: El LLM — opciones reales

Tu arquitectura ya tiene `ILlmClient` como abstracción. Agregar un nuevo proveedor es crear una clase nueva en `Infrastructure/Llm/` y cambiar el registro en `Program.cs`. El Application layer no cambia nada.

### Opción A: Groq (recomendada para empezar)

**Por qué:** API gratuita, velocidad muy alta, compatible con Llama 3 8B (el mismo modelo que usás con Ollama), sin costo hasta ~14.400 requests/día.

- Registrarse en: groq.com
- API compatible con OpenAI → implementación muy simple
- Modelo disponible: `llama-3.1-8b-instant` o `llama3-8b-8192`

```csharp
// Infrastructure/Llm/GroqClient.cs
public class GroqClient : ILlmClient
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public GroqClient(HttpClient http, IConfiguration config)
    {
        _http = http;
        _http.BaseAddress = new Uri("https://api.groq.com/openai/v1/");
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", config["Groq:ApiKey"]);
    }

    public async Task<string> EvaluateCvAsync(string prompt, CancellationToken ct)
    {
        var request = new
        {
            model = "llama-3.1-8b-instant",
            messages = new[] { new { role = "user", content = prompt } },
            temperature = 0.1
        };

        var response = await _http.PostAsJsonAsync("chat/completions", request, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
        return result
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString()!;
    }
}
```

Variable de entorno en Render:
```
Groq__ApiKey = gsk_...
LlmProvider  = Groq
```

### Opción B: Claude API (Anthropic)

**Por qué:** Muy alta calidad de análisis. `claude-haiku-4-5` es barato y rápido (~$0.25 por millón de tokens de entrada).

- Costo estimado: ~$0.001 por evaluación de CV
- Sin free tier, pero hay créditos gratis al registrarse

Variable de entorno:
```
Claude__ApiKey = sk-ant-...
LlmProvider    = Claude
```

### Opción C: OpenAI

**Por qué:** El estándar de la industria. `gpt-4o-mini` es muy barato (~$0.15/1M tokens).

Variable de entorno:
```
OpenAI__ApiKey = sk-...
LlmProvider    = OpenAI
```

### Opción D: Ollama (solo local / no recomendado en Render)

Requiere una instancia con 8GB+ de RAM. El plan Standard Plus de Render ($85/mes) podría correrlo, pero ninguna de las opciones anteriores tiene sentido a ese precio cuando las APIs externas cuestan centavos.

### Comparativa

| Proveedor | Costo | Velocidad | Calidad | Límite gratis |
|-----------|-------|-----------|---------|---------------|
| **Groq** | Gratis/muy barato | Muy alta | Buena (llama3) | 14.400 req/día |
| **Claude Haiku** | ~$0.001/eval | Alta | Excelente | $5 crédito inicial |
| **GPT-4o-mini** | ~$0.001/eval | Alta | Muy buena | $5 crédito inicial |
| **Ollama en Render** | $85/mes | Media | Buena | No aplica |

**Recomendación:** Empezá con **Groq** (gratis, mismo modelo que ya usás). Cuando quieras más calidad de análisis, migrá a **Claude Haiku**.

---

## Registro de servicios en `Program.cs`

Actualizar el switch de proveedor LLM:

```csharp
var provider = builder.Configuration["LlmProvider"] ?? "Ollama";

switch (provider)
{
    case "Groq":
        builder.Services.AddHttpClient<ILlmClient, GroqClient>();
        break;
    case "Claude":
        builder.Services.AddHttpClient<ILlmClient, ClaudeClient>();
        break;
    case "OpenAI":
        builder.Services.AddHttpClient<ILlmClient, OpenAiClient>();
        break;
    default:
        builder.Services.AddHttpClient<ILlmClient, OllamaClient>();
        break;
}
```

---

## Checklist de deploy

### Antes del primer deploy
- [ ] Crear PostgreSQL en Render y copiar la Internal URL
- [ ] Generar un `Jwt__Key` seguro (mínimo 32 caracteres, aleatorio)
- [ ] Elegir proveedor LLM y obtener API key
- [ ] Agregar `_redirects` en `FrontEnd/cv-evaluator-ui/public/`
- [ ] Verificar `environment.prod.ts` tiene `apiBaseUrl` y `hubBaseUrl`
- [ ] Decidir si agregar auto-migración al startup

### Variables de entorno en el Web Service (backend)
- [ ] `ConnectionStrings__DefaultConnection`
- [ ] `Jwt__Key`
- [ ] `Jwt__Issuer`
- [ ] `Jwt__Audience`
- [ ] `Cors__AllowedOrigins__0`
- [ ] `LlmProvider` + la API key correspondiente

### Tras el primer deploy
- [ ] Verificar `/health` responde `200`
- [ ] Correr migraciones si no se configuró auto-migración
- [ ] Registrar un usuario desde el frontend
- [ ] Subir un PDF de prueba y verificar que la evaluación se complete
- [ ] Verificar que SignalR notifica la actualización en tiempo real

---

## Gotchas conocidos de Render

**CORS:** El frontend en Render tiene URL fija `https://[name].onrender.com`. Asegurarse de que `Cors__AllowedOrigins__0` coincide exactamente (sin trailing slash).

**SignalR en free tier:** No funciona. Necesitás plan Starter o superior para WebSockets.

**Build del frontend:** Si `npm run build` falla por falta de memoria, agregar la variable de entorno `NODE_OPTIONS=--max-old-space-size=4096` en el Static Site.

**Logs del backend:** Disponibles en tiempo real en el Dashboard de Render → Web Service → Logs.

**Custom domain:** Se configura en cada servicio (Static Site y Web Service) por separado. Render provee certificado SSL gratis.
