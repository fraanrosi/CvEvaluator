# Arquitectura — CvEvaluator

## Stack tecnológico

| Capa | Tecnologías |
|------|------------|
| **Backend** | .NET 8 · C# · ASP.NET Core · PostgreSQL · EF Core · ASP.NET Identity · JWT · SignalR · Ollama |
| **Frontend** | Angular 21 · TypeScript · Tailwind CSS · Standalone Components · Lazy Loading · SignalR Client |
| **Infraestructura** | Docker Compose · Dockerfile multi-stage |

---

## Backend — Clean Architecture

### Estructura de capas

```
CvEvaluator.sln
│
├── CvEvaluator.Domain              ← Sin dependencias externas
│   ├── Entities/
│   │   ├── JobPosition.cs
│   │   └── CvEvaluation.cs
│   ├── Enums/
│   │   ├── EvaluationStatus.cs
│   │   ├── CvDecision.cs
│   │   └── UserRole.cs
│   └── Models/
│       └── CvEvaluationResult.cs
│
├── CvEvaluator.Application         ← Solo depende de Domain
│   ├── Services/
│   │   ├── CvEvaluationService.cs
│   │   └── JobPositionService.cs
│   ├── Interfaces/
│   │   ├── ICvEvaluationService.cs
│   │   ├── ICvEvaluationRepository.cs
│   │   ├── IJobPositionService.cs
│   │   ├── IJobPositionRepository.cs
│   │   ├── IDocumentParser.cs
│   │   ├── IEvaluationQueue.cs
│   │   ├── IIdentityService.cs
│   │   ├── ILlmClient.cs
│   │   └── IUnitOfWork.cs
│   ├── DTOs/
│   │   ├── JobPositionDto.cs
│   │   ├── JobPositionDetailDto.cs
│   │   ├── CreateJobPositionDto.cs
│   │   ├── UpdateJobPositionDto.cs
│   │   ├── CvEvaluationDto.cs
│   │   ├── LoginRequestDto.cs
│   │   ├── RegisterRequestDto.cs
│   │   └── AuthResponseDto.cs
│   └── Prompts/
│       └── CvEvaluationPrompt.cs
│
├── CvEvaluator.Infrastructure      ← Implementa interfaces de Application
│   ├── Persistence/
│   │   ├── CvEvaluatorDbContext.cs
│   │   ├── CvEvaluatorDbContextFactory.cs
│   │   ├── UnitOfWork.cs
│   │   └── Repositories/
│   │       ├── CvEvaluationRepository.cs
│   │       └── JobPositionRepository.cs
│   ├── Identity/
│   │   ├── ApplicationUser.cs
│   │   └── IdentityService.cs
│   ├── Llm/
│   │   └── OllamaClient.cs
│   ├── Parsing/
│   │   └── PdfDocumentParser.cs
│   ├── Background/
│   │   ├── CvEvaluationWorker.cs
│   │   └── EvaluationQueue.cs
│   └── SignalR/
│       └── EvaluationHub.cs
│
└── CvEvaluator.Api                 ← Orquesta todo
    ├── Controllers/
    │   ├── AuthController.cs
    │   ├── CvEvaluationController.cs
    │   └── JobPositionsController.cs
    ├── Extensions/
    │   └── SwaggerExtensions.cs
    └── Program.cs
```

### Regla de dependencias (estricta)

```
Domain  ←  Application  ←  Infrastructure  ←  Api
```

- **Domain**: sin referencias a ningún paquete externo
- **Application**: solo referencia a Domain. Nunca EF Core, HttpClient, ni nada de infra
- **Infrastructure**: implementa las interfaces de Application
- **Api**: registra todas las dependencias vía DI, expone HTTP endpoints

---

## Endpoints HTTP

### Auth — `AuthController`
| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/auth/register` | Registro de usuario |
| POST | `/api/auth/login` | Login, retorna JWT |

### Job Positions — `JobPositionsController`
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/jobpositions` | Lista posiciones del usuario autenticado |
| GET | `/api/jobpositions/{id}` | Detalle con evaluaciones |
| POST | `/api/jobpositions` | Crear posición |
| PUT | `/api/jobpositions/{id}` | Editar (solo owner) |
| DELETE | `/api/jobpositions/{id}` | Eliminar (solo owner) |

### CV Evaluations — `CvEvaluationController`
| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/cvEvaluations/evaluate-pdf` | Subir PDF(s) para evaluar |
| GET | `/api/cvEvaluations/{id}` | Obtener evaluación por ID |
| GET | `/api/cvEvaluations` | Listar evaluaciones del usuario |

---

## Flujo de evaluación completo

```
[1] Usuario selecciona JobPosition y sube PDF(s)
         ↓
[2] POST /api/cvEvaluations/evaluate-pdf
    CvEvaluationController.EvaluatePdf()
    - Valida que el archivo sea .pdf y no vacío
    - Extrae userId del JWT
         ↓
[3] CvEvaluationService.EvaluateAsync()
    - Verifica que jobPosition existe y pertenece al userId
    - PdfDocumentParser.ParseAsync(stream) → texto extraído
    - SHA256(texto) → fileHash
    - Crea CvEvaluation con Status = Processing
    - SaveChangesAsync()
    - EvaluationQueue.EnqueueAsync(evaluationId)
    - Retorna 202 Accepted + evaluationId
         ↓
[4] CvEvaluationWorker (BackgroundService, corre en paralelo)
    - Dequeue evaluationId del Channel<Guid>
    - Fetch CvEvaluation + JobPosition desde DB
    - CvEvaluationPrompt.Build(cvText, jobTitle, jobDescription)
    - ILlmClient.EvaluateCvAsync(prompt) → JSON string
    - Parsea JSON → CvEvaluationResult
    - Actualiza CvEvaluation:
        Status = Completed
        OverallScore = result.Score
        EvaluationResult = JSON completo
        EvaluatedAt = now
    - SaveChangesAsync()
    - EvaluationHub.Clients.User(userId).SendAsync("EvaluationUpdated", dto)
         ↓
[5] Frontend recibe evento SignalR "EvaluationUpdated"
    - Actualiza el estado de la evaluación en tiempo real
```

---

## Registro de dependencias (Program.cs)

```csharp
// Worker asíncrono
builder.Services.AddHostedService<CvEvaluationWorker>();

// Base de datos
builder.Services.AddDbContext<CvEvaluatorDbContext>(options =>
    options.UseNpgsql(connectionString));

// Repositorios e interfaces
builder.Services.AddScoped<ICvEvaluationRepository, CvEvaluationRepository>();
builder.Services.AddScoped<IJobPositionRepository, JobPositionRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ICvEvaluationService, CvEvaluationService>();
builder.Services.AddScoped<IJobPositionService, JobPositionService>();
builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<IDocumentParser, PdfDocumentParser>();

// Cola de evaluación (singleton)
builder.Services.AddSingleton<EvaluationQueue>();
builder.Services.AddSingleton<IEvaluationQueue>(sp => sp.GetRequiredService<EvaluationQueue>());

// LLM client (configurable por proveedor)
builder.Services.AddHttpClient<ILlmClient, OllamaClient>();

// SignalR
builder.Services.AddSignalR();
app.MapHub<EvaluationHub>("/hubs/evaluations");
```

---

## Frontend — Angular 21

### Estructura de carpetas

```
src/app/
├── core/
│   ├── interceptors/
│   │   ├── jwt.interceptor.ts          ← Agrega Bearer token a todas las requests
│   │   └── http-error.interceptor.ts   ← Maneja 401 → redirect a /login
│   ├── models/
│   │   ├── job-position.model.ts
│   │   ├── job-position-detail.model.ts
│   │   └── evaluation-result.model.ts
│   └── services/
│       ├── session.service.ts          ← JWT en sessionStorage
│       └── signalr.service.ts          ← Conexión al hub de SignalR
│
├── features/
│   ├── auth/
│   │   ├── auth.service.ts             ← Login/Register HTTP calls
│   │   ├── auth.guard.ts               ← Protege rutas privadas
│   │   ├── login/                      ← Formulario de login
│   │   └── register/                   ← Formulario de registro
│   ├── dashboard/                      ← Vista principal post-login
│   └── job-positions/
│       ├── job-positions.service.ts    ← CRUD HTTP /api/jobpositions
│       └── pages/
│           ├── job-positions-page/     ← Lista de posiciones
│           ├── create-job-position/    ← Formulario de creación
│           ├── edit-job-position/      ← Formulario de edición
│           └── job-position-detail/    ← Detalle + upload PDF
│
└── shared/layout/
    ├── app-shell/                      ← Layout con navbar + router-outlet
    └── navbar/                         ← Barra de navegación
```

### Configuración del entorno

```typescript
// environment.ts
export const environment = {
  apiBaseUrl: 'http://localhost:8080/api',
  hubBaseUrl: 'http://localhost:8080'
};
```

### Rutas (app.routes.ts)

```
'' → /dashboard
/login          (pública)
/register       (pública)
/dashboard      (authGuard) → dentro de AppShell
/job-positions  (authGuard) → dentro de AppShell
  /job-positions/create
  /job-positions/:id
  /job-positions/:id/edit
** → /dashboard
```

> **Bug conocido:** `login` y `register` están dentro del `AppShell` (que incluye el navbar). Ver [02-navbar-fix.md](./02-navbar-fix.md).

### SignalR en el frontend

```typescript
// signalr.service.ts
// Conecta a: http://localhost:8080/hubs/evaluations
// Evento escuchado: 'EvaluationUpdated'
// Reconexión automática configurada
```

---

## Convenciones del código

### Backend (C#)
- Siempre verificar `userId` del JWT antes de devolver/modificar recursos
- Controllers solo orquestan — lógica de negocio en Application Services
- `CancellationToken ct` en todos los métodos async
- DTOs separados de entidades de dominio (nunca exponer entidades directamente)
- Migraciones: `dotnet ef migrations add Nombre --project CvEvaluator.Infrastructure --startup-project CvEvaluator.Api`

### Frontend (Angular)
- Standalone components siempre
- `inject()` en lugar de constructor injection
- Signals (`signal()`, `computed()`) para estado local
- HttpClient con Observables
- Lazy loading para todos los feature modules

---

## Configuración local

**Backend** (requiere PostgreSQL corriendo):
```bash
cd BackEnd
dotnet run --project CvEvaluator.Api
# Escucha en http://0.0.0.0:8080
```

**Con Docker** (incluye PostgreSQL + Ollama):
```bash
docker-compose up
```

**Frontend:**
```bash
cd FrontEnd/cv-evaluator-ui
npm start
# Escucha en http://localhost:4200
```
