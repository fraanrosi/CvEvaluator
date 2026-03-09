# Base de datos — CvEvaluator

**Motor:** PostgreSQL
**ORM:** Entity Framework Core
**DbContext:** `CvEvaluatorDbContext` (Infrastructure/Persistence)

---

## Relaciones entre entidades

```
ApplicationUser (ASP.NET Identity)
    │
    │ UserId (Guid)
    ▼
JobPosition ─────────────────────────────────────────────────────┐
    │ Id (PK)                                                     │
    │ UserId                                                      │
    │ Title                                                       │
    │ Description                                                 │
    │ CreatedAt                                                   │
    │                                                             │
    │ 1                                                           │
    │                                                             │
    ▼ N                                                           │
CvEvaluation                                                      │
    │ Id (PK)                                                     │
    │ UserId                                                      │
    │ JobPositionId (FK) ─────────────────────────────────────────┘
    │ JobPosition (nav. property)
    │ ... (ver detalle abajo)
```

---

## Entidades de dominio

### `JobPosition`

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | `Guid` (PK) | Identificador único |
| `UserId` | `Guid` | FK al usuario que creó la posición |
| `Title` | `string` | Título del puesto (ej. "Senior .NET Developer") |
| `Description` | `string` | Descripción completa del puesto para el LLM |
| `CreatedAt` | `DateTime` | Fecha de creación (UTC) |
| `Evaluations` | `ICollection<CvEvaluation>` | Evaluaciones asociadas (nav. property) |

---

### `CvEvaluation`

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | `Guid` (PK) | Identificador único |
| `UserId` | `Guid` | FK al usuario dueño |
| `JobPositionId` | `Guid` (FK) | FK a JobPosition |
| `JobPosition` | `JobPosition` | Nav. property |
| **Metadata del archivo** | | |
| `OriginalFilename` | `string` | Nombre original del PDF subido |
| `FileSizeBytes` | `long` | Tamaño del archivo en bytes |
| `FileHash` | `string` | SHA256 del texto extraído (deduplicación) |
| **Contenido procesado** | | |
| `ExtractedText` | `string` | Texto extraído del PDF por `PdfDocumentParser` |
| `EvaluationResult` | `string?` | JSON completo retornado por el LLM (JSONB en Postgres) |
| **Scores desnormalizados** | | |
| `OverallScore` | `decimal?` | Score general 0-100 |
| `TechnicalScore` | `decimal?` | Score técnico (para futura extensión) |
| `ExperienceScore` | `decimal?` | Score de experiencia (para futura extensión) |
| `EducationScore` | `decimal?` | Score de educación (para futura extensión) |
| **Matching** | | |
| `JobTitle` | `string?` | Título del puesto al momento de evaluación |
| `JobDescription` | `string?` | Descripción del puesto al momento de evaluación |
| `MatchScore` | `decimal?` | Score de match (para futura extensión) |
| **Metadata de procesamiento** | | |
| `ModelUsed` | `string?` | Nombre del modelo LLM utilizado |
| `ProcessingTimeMs` | `int?` | Tiempo de procesamiento en ms |
| **Estado asíncrono** | | |
| `Status` | `EvaluationStatus` | Estado actual del procesamiento |
| `ErrorMessage` | `string?` | Mensaje de error si Status = Failed |
| `CreatedAt` | `DateTime` | Fecha de creación (UTC), default `DateTime.UtcNow` |
| `EvaluatedAt` | `DateTime?` | Fecha de completado (null si aún procesando) |

---

## Enums de dominio

### `EvaluationStatus`
```csharp
public enum EvaluationStatus
{
    Processing = 0,   // El worker aún no procesó esta evaluación
    Completed  = 1,   // LLM respondió exitosamente, datos persistidos
    Failed     = 2    // Error durante el procesamiento (ver ErrorMessage)
}
```

### `CvDecision`
```csharp
public enum CvDecision
{
    Suitable,    // CV es adecuado para el puesto
    NotSuitable  // CV no es adecuado
}
```

### `UserRole`
```csharp
public enum UserRole
{
    User  = 0,  // Reclutador estándar
    Admin = 1   // Administrador
}
```

---

## Model de dominio (value object)

### `CvEvaluationResult`
Representa el output del LLM deserializado. Se almacena serializado en `CvEvaluation.EvaluationResult`.

```csharp
public class CvEvaluationResult
{
    public int Score { get; set; }                  // 0-100
    public int YearsExperience { get; set; }        // Años de experiencia inferidos
    public bool MatchesRequirements { get; set; }   // Si cumple los requisitos
    public required List<string> Strengths { get; set; }    // Fortalezas
    public required List<string> Weaknesses { get; set; }   // Debilidades
}
```

---

## DTOs (Application Layer)

Los DTOs viven en `CvEvaluator.Application/DTOs/` y nunca exponen entidades directamente.

### Para Job Positions

| DTO | Uso | Campos |
|-----|-----|--------|
| `CreateJobPositionDto` | POST `/api/jobpositions` | `Title`, `Description` |
| `UpdateJobPositionDto` | PUT `/api/jobpositions/{id}` | `Title`, `Description` |
| `JobPositionDto` | GET listado | `Id`, `Title`, `Description`, `CreatedAt`, `EvaluationsCount` |
| `JobPositionDetailDto` | GET detalle | `Id`, `Title`, `Description`, `CreatedAt`, `List<EvaluationSummaryDto>` |
| `EvaluationSummaryDto` | Dentro de detalle | `Id`, `CandidateName`*, `FileName`, `Score`, `EvaluatedAt` |

> \* `CandidateName` se deriva del `OriginalFilename` usando `Path.GetFileNameWithoutExtension()`

### Para CV Evaluations

| DTO | Uso | Campos |
|-----|-----|--------|
| `CvEvaluationDto` | GET evaluación / SignalR | `Id`, `OriginalFilename`, `Status`, `OverallScore?`, `ErrorMessage?`, `CreatedAt`, `EvaluatedAt?` |

### Para Auth

| DTO | Uso | Campos |
|-----|-----|--------|
| `LoginRequestDto` | POST `/api/auth/login` | `Email`, `Password` |
| `RegisterRequestDto` | POST `/api/auth/register` | `Email`, `Password`, `FullName` |
| `AuthResponseDto` | Respuesta de auth | `Token` (JWT), `Email` |

---

## Repositories (Infrastructure)

### `IJobPositionRepository`

```csharp
Task<JobPosition?> GetByIdAsync(Guid id, CancellationToken ct);
Task<JobPosition?> GetByIdWithEvaluationsAsync(Guid id, CancellationToken ct); // eager loading
Task<IEnumerable<JobPosition>> GetByUserIdAsync(Guid userId, CancellationToken ct);
Task AddAsync(JobPosition jobPosition, CancellationToken ct);
void Update(JobPosition jobPosition);
void Delete(JobPosition jobPosition);
```

> `GetByUserIdAsync` y `GetByIdWithEvaluationsAsync` usan `.Include(j => j.Evaluations)` para eager loading.

### `ICvEvaluationRepository`

```csharp
Task AddAsync(CvEvaluation evaluation, CancellationToken ct);
Task<CvEvaluation?> GetByIdAsync(Guid id, CancellationToken ct);
Task<IEnumerable<CvEvaluation>> GetAllByUserIdAsync(Guid userId, CancellationToken ct);
void Update(CvEvaluation evaluation);
```

---

## EF Core — Configuración

### Conexión
```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=cvevaluator;Username=...;Password=..."
  }
}
```

### Migraciones
```bash
dotnet ef migrations add NombreMigracion \
  --project CvEvaluator.Infrastructure \
  --startup-project CvEvaluator.Api
```

### Unit of Work
```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct);
}
```
Todos los cambios se persisten vía `IUnitOfWork.SaveChangesAsync()` — nunca llamar `DbContext.SaveChangesAsync()` directamente desde servicios.

---

## Identity (ASP.NET Identity)

- `ApplicationUser : IdentityUser` — extiende el usuario de Identity con campos adicionales si es necesario
- Tabla gestionada por ASP.NET Identity en la misma base de datos PostgreSQL
- `UserId` en `JobPosition` y `CvEvaluation` referencia el `Id` de `ApplicationUser`
- `IdentityService` genera y valida JWT tokens
