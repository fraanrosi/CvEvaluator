# CvEvaluator

App web para evaluar CVs en PDF contra una `JobPosition` usando un LLM. Evaluaciones asíncronas con notificación en tiempo real vía SignalR.

**Stack:** .NET 8 · Clean Architecture · PostgreSQL · ASP.NET Identity · JWT · SignalR · Angular 21 · Tailwind · Docker

---

## Arquitectura

```
Domain ← Application ← Infrastructure ← Api
```
- DTOs en Application, no en Api. Nunca exponer entidades directamente.
- Ante la duda, crear interfaz en Application antes de acoplar.

## Reglas de negocio
- Siempre verificar que el recurso pertenece al `userId` del JWT.
- Controllers solo orquestan — lógica en Application Services.
- `CancellationToken ct` en todos los métodos async.

## LLM multi-proveedor
`ILlmClient` en Application. Implementaciones en Infrastructure (`OllamaClient`, `ClaudeClient`, `OpenAiClient`). Selección vía `appsettings.json → "LlmProvider"`. Sin herencia compleja — DI container es suficiente.

## Angular
Standalone components · `inject()` en lugar de constructor injection · HttpClient con Observables.

## Migraciones
```bash
dotnet ef migrations add <Nombre> --project CvEvaluator.Infrastructure --startup-project CvEvaluator.Api
```
