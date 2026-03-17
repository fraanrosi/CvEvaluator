# CvEvaluator v2 — Plan de Implementacion

## Contexto

El proyecto tiene las fases 1-4 de profesionalizacion completadas (UX, seguridad, auth flows, landing page, email con Resend). El usuario quiere tres mejoras mayores:

1. **Rediseno UI/UX completo** — Cambio de color system (gold → purple/violet), sidebar layout estilo TradUp, landing page estilo Portals.fi con asset 3D
2. **Analytics & Dashboards** — Endpoints de analitica, graficos con Chart.js, KPI widgets
3. **Billing & Payments** — Integracion Stripe Checkout, webhooks, upgrade de planes

**Referencias visuales:** Landing inspirada en Portals.fi (dark premium, 3D hero, purple glow). Dashboard inspirado en TradUp (sidebar izquierdo, stat cards, area chart con gradiente purple).

> Este archivo es el plan. Se copiara a `docs/plan-v2.md` al iniciar implementacion.

---

## Grafo de Dependencias

```
Fase 1 (Design System)
  └──> Fase 2 (Layout + Landing)
         └──> Fase 3 (Analytics Backend + Frontend)
                └──> Fase 4 (Stripe Backend + Frontend)
                       └──> Fase 5 (Polish + Testing)
```

---

## Fase 1: Design System Foundation

**Branch:** `feat/design-system-v2`

### Objetivo
Reemplazar el accent color gold (#e8a23e) por purple/violet (#7C3AED). Actualizar tokens, shadows, y clases utilitarias. Sin cambios de layout.

### Archivos a modificar

**`FrontEnd/cv-evaluator-ui/tailwind.config.js`**
- `accent.DEFAULT`: `#e8a23e` → `#7C3AED`
- `accent.light`: → `#8B5CF6`
- `accent.dark`: → `#6D28D9`
- `accent.50`: → `rgba(124, 58, 237, 0.08)`
- `accent.100`: → `rgba(124, 58, 237, 0.15)`
- `boxShadow.glow`: → `rgba(124, 58, 237, 0.15)`
- `boxShadow.card-hover`: purple glow ring
- `keyframes.pulseGlow`: purple
- Agregar: `sidebar: '#0c0c14'`
- Agregar: `shadow.glow-lg: '0 0 40px rgba(124, 58, 237, 0.2)'`

**`FrontEnd/cv-evaluator-ui/src/styles.css`**
- Actualizar `::selection`, `input:focus` a purple
- `.btn-primary`: purple bg con `text-white` (antes era `text-surface-950`)
- Agregar: `.sidebar-link`, `.sidebar-link-active`
- Agregar: `.stat-card`, `.chart-container`

### Verificacion
- `npm run build` sin errores
- Login page muestra acentos purple
- Grep: no queden `#e8a23e` hardcodeados

---

## Fase 2: Layout + Landing Page

**Branch:** `feat/sidebar-layout-landing`

### 2A: Sidebar Layout (reemplaza top navbar)

**Crear:**
- `shared/layout/sidebar/sidebar.component.ts + .html`
  - Standalone, `inject()` pattern
  - Signal: `collapsed`, `mobileOpen`
  - Menu: Dashboard, Job Positions, Subscription
  - Manage: Settings (futuro), Support (futuro)
  - Logo arriba, user email + logout abajo
  - `w-64` expanded, `w-16` collapsed, overlay en mobile

- `shared/layout/top-header/top-header.component.ts + .html`
  - Search bar (cosmetico), notification bell, user avatar (primera letra del email)
  - Hamburger en mobile → emite `toggleSidebar`

**Modificar:**
- `shared/layout/app-shell/app-shell.component.ts`
  - Reemplazar `NavbarComponent` por `SidebarComponent` + `TopHeaderComponent`
  - Layout: sidebar fixed left + main content con `ml-64` (o `ml-16` collapsed)

### 2B: Landing Page Redesign (estilo Portals.fi)

**Asset 3D:**
- Fuente: Spline (spline.design, free tier) o Shapefest (shapefest.com). Buscar orbes/geometrias abstractas con glow purple.
- Exportar como `.webp` + `.png` fallback → `src/assets/hero/`
- Integrar con `mix-blend-mode: screen` sobre fondo `bg-black`
- Alternativa fallback: gradientes radiales CSS + blur (sin asset externo)

**Modificar `features/landing/landing.component.html`:**
- Navbar: logo left, links center (Features, Pricing), CTAs right ("Sign in" ghost + "Get started" purple fill)
- Hero: fondo `#000`, imagen 3D o glow CSS, heading `text-5xl lg:text-7xl`, sub en `text-zinc-400`, CTA dual
- Features: 4 cards con purple glow on hover
- How it works: circulos purple
- CTA section: gradient purple bg
- Footer: minimal, purple accents

### Verificacion
- `/` muestra landing dark premium con purple
- Login → sidebar izquierdo en `/dashboard`
- Mobile: sidebar colapsa, hamburger funciona
- Sidebar links navegan correctamente con active state

---

## Fase 3: Analytics Backend + Frontend Charts

**Branch:** `feat/analytics-dashboard`

### 3A: Backend

**Crear DTOs en `Application/DTOs/Analytics/`:**
- `AnalyticsOverviewDto` — totalEvaluations, averageScore, totalJobPositions, evaluationsThisMonth, scoreTrendPercent, topJobPositionTitle
- `ScoreTimeSeriesDto` + `ScoreDataPoint` — date, averageScore, count
- `ScoreDistributionDto` + `ScoreBucket` — range ("0-20", etc.), count
- `TopCandidateDto` — evaluationId, filename, overallScore, technicalScore, experienceScore, evaluatedAt

**Crear interfaces en `Application/Interfaces/`:**
- `IAnalyticsRepository` — queries de agregacion (count, avg, group by date, distribution buckets, top candidates)
- `IAnalyticsService` — GetOverviewAsync, GetScoresOverTimeAsync, GetScoreDistributionAsync, GetTopCandidatesAsync

**Crear servicios:**
- `Application/Services/AnalyticsService.cs` — logica de negocio, parseo de periodos ("7d"/"30d"/"90d")
- `Infrastructure/Persistence/Repositories/AnalyticsRepository.cs` — queries EF Core con GroupBy, CASE buckets

**Crear controller:**
- `Api/Controllers/AnalyticsController.cs` — `[Authorize]`, 4 endpoints:
  - `GET /api/analytics/overview`
  - `GET /api/analytics/scores-over-time?period=30d`
  - `GET /api/analytics/score-distribution`
  - `GET /api/analytics/top-candidates?jobPositionId=X&limit=10`

**Modificar `Api/Program.cs`:** registrar `IAnalyticsRepository` + `IAnalyticsService`

### 3B: Frontend Charts

**Libreria:** `ng2-charts` (wrapper de Chart.js) — mas liviana que ngx-charts, mejor soporte para gradientes purple.
- `npm install chart.js ng2-charts`

**Crear:**
- `core/services/analytics.service.ts` — getOverview(), getScoresOverTime(period), getScoreDistribution(), getTopCandidates()
- `core/models/analytics.model.ts` — interfaces TypeScript
- `shared/components/score-chart/score-chart.component.ts` — area chart con gradiente purple (#7C3AED → transparent)
- `shared/components/distribution-chart/distribution-chart.component.ts` — bar chart con barras purple
- `shared/components/stat-card/stat-card.component.ts` — card KPI con trend arrow (verde up / rojo down)

**Modificar `features/dashboard/dashboard.component.ts + .html`:**
- Layout completo nuevo:
  - 4 stat cards (Total Evaluations, Avg Score, Job Positions, This Month)
  - Area chart "Evaluation Scores" con selector 7d/30d/90d
  - Bar chart "Score Distribution"
  - Panel derecho: plan summary card (gradient purple bg) + recent evaluations list

### Verificacion
- `GET /api/analytics/overview` devuelve JSON correcto
- Dashboard muestra charts con data real
- Selector de periodo actualiza el chart
- Charts responsivos con sidebar collapse

---

## Fase 4: MercadoPago Billing & Payments

**Branch:** `feat/mercadopago-billing`

### 4A: Backend

**NuGet:** `MercadoPago` en `CvEvaluator.Infrastructure.csproj`

**Modificar entidades:**
- `Plan.cs` — agregar `Price` (decimal), `Currency` (string, default "ARS")
- `UserSubscription.cs` — agregar `MpPayerId` (string?), `MpSubscriptionId` (string?), `MpPreapprovalId` (string?)

**Nueva migracion:** `AddMercadoPagoFields`

**Crear:**
- `Application/Interfaces/IPaymentService.cs` — CreateCheckoutAsync, HandlePaymentNotificationAsync, GetSubscriptionStatusAsync
- `Application/DTOs/CreateCheckoutRequestDto.cs` — PlanId
- `Infrastructure/Payments/MercadoPagoPaymentService.cs`:
  - Usa MercadoPago Checkout Pro (crea una "preference" con items, back_urls, auto_return)
  - Soporta suscripciones recurrentes via Preapproval API
  - CreateCheckout: crea preference con item del plan, URLs de retorno
  - HandlePaymentNotification: procesa IPN/webhook, actualiza UserSubscription
  - Al recibir "approved" → asigna plan pagado, status Active
  - Al recibir "cancelled"/"refunded" → revertir a Free

**Modificar:**
- `ISubscriptionRepository` — agregar GetByMpSubscriptionIdAsync, GetPlanByIdAsync
- `SubscriptionRepository.cs` — implementar nuevos metodos
- `SubscriptionsController.cs` — agregar:
  - `POST /create-checkout` → devuelve {init_point: "https://www.mercadopago.com.ar/checkout/v1/redirect?pref_id=..."}
  - `POST /webhook` `[AllowAnonymous]` → recibe notificacion IPN de MercadoPago
  - `GET /payment-status/{paymentId}` → consulta estado del pago
- `Program.cs` — registrar IPaymentService, configurar MercadoPago SDK con AccessToken
- `PlanDto.cs` — agregar Price, Currency
- `CvEvaluatorDbContext.cs` — actualizar seed data con precios (Free=$0, Pro=ARS$9999, Business=ARS$24999)

**Config (user-secrets/env vars):**
- `MercadoPago:AccessToken` (token de produccion)
- `MercadoPago:WebhookSecret` (para validar notificaciones)
- `MercadoPago:SuccessUrl`, `MercadoPago:FailureUrl`, `MercadoPago:PendingUrl`

### 4B: Frontend

**Modificar:**
- `subscription.service.ts` — agregar createCheckout(planId), getPaymentStatus(paymentId)
- `subscription-page.component.ts + .html` — habilitar botones Upgrade, mostrar precios en ARS, estado del pago

**Crear:**
- `features/subscription/checkout-success.component.ts` — mensaje de exito + link a dashboard
- `features/subscription/checkout-failure.component.ts` — mensaje de error + link a retry
- `features/subscription/checkout-pending.component.ts` — mensaje de pago pendiente (tipico de MercadoPago)

**Modificar `app.routes.ts`:** agregar rutas `/subscription/success`, `/subscription/failure`, `/subscription/pending`

### Verificacion
- Click Upgrade Pro → redirect a MercadoPago Checkout
- Pago exitoso → IPN webhook → UserSubscription actualizada a Pro
- Pago pendiente → muestra estado pendiente (tipico en MP con efectivo/transferencia)
- Dashboard refleja nuevo plan y limites

---

## Fase 5: Polish + Testing

**Branch:** `feat/polish-v2`

### 5A: Verificar colores en auth pages
Auth pages usan clases `text-accent`, `bg-accent` que se actualizan automaticamente con el token change de Fase 1. Verificar visualmente login, register, forgot-password, reset-password, confirm-email.

### 5B: Verificar job-positions y evaluations
Misma logica: cards, badges, buttons usan clases del design system. Verificar que todo se ve purple.

### 5C: PDF Export (bonus)
- Agregar boton "Export PDF" en evaluation-detail
- Opcion simple: `window.print()` con CSS `@media print`
- Opcion avanzada: `jspdf` + `html2canvas`

### 5D: Tests
**Backend (tests/backend/):**
- `AnalyticsServiceTests.cs` — mock IAnalyticsRepository, verificar overview, time series, distribution
- `MercadoPagoPaymentServiceTests.cs` — mock repository, verificar preference creation, IPN webhook handlers

**Frontend (Vitest):**
- `dashboard.component.spec.ts` — stat cards renderizan, charts reciben data
- `sidebar.component.spec.ts` — links, active state, collapse toggle
- `subscription-page.component.spec.ts` — botones upgrade, precios

---

## Resumen de Archivos Nuevos

| Capa | Archivo | Proposito |
|------|---------|-----------|
| Frontend | `shared/layout/sidebar/sidebar.component.ts+html` | Sidebar navigation |
| Frontend | `shared/layout/top-header/top-header.component.ts+html` | Top header bar |
| Frontend | `shared/components/stat-card/stat-card.component.ts` | KPI card |
| Frontend | `shared/components/score-chart/score-chart.component.ts` | Area chart |
| Frontend | `shared/components/distribution-chart/distribution-chart.component.ts` | Bar chart |
| Frontend | `core/services/analytics.service.ts` | Analytics HTTP |
| Frontend | `core/models/analytics.model.ts` | Analytics interfaces |
| Frontend | `features/subscription/checkout-success.component.ts` | Post-checkout success |
| Frontend | `features/subscription/checkout-failure.component.ts` | Post-checkout failure |
| Frontend | `features/subscription/checkout-pending.component.ts` | Post-checkout pending |
| Backend | `Application/DTOs/Analytics/*.cs` (4 archivos) | Analytics DTOs |
| Backend | `Application/Interfaces/IAnalyticsRepository.cs` | Data access interface |
| Backend | `Application/Interfaces/IAnalyticsService.cs` | Service interface |
| Backend | `Application/Interfaces/IPaymentService.cs` | Payment interface |
| Backend | `Application/Services/AnalyticsService.cs` | Analytics logic |
| Backend | `Application/DTOs/CreateCheckoutRequestDto.cs` | Checkout DTO |
| Backend | `Infrastructure/Repositories/AnalyticsRepository.cs` | EF Core queries |
| Backend | `Infrastructure/Payments/MercadoPagoPaymentService.cs` | MercadoPago integration |
| Backend | `Api/Controllers/AnalyticsController.cs` | Analytics endpoints |
| Backend | Nueva migracion EF | Campos Stripe |

## Archivos Criticos a Modificar

| Archivo | Cambio |
|---------|--------|
| `tailwind.config.js` | Color system gold → purple |
| `styles.css` | Utilidades actualizadas + nuevas |
| `app-shell.component.ts` | Navbar → sidebar + top-header |
| `landing.component.html` | Rediseno completo |
| `dashboard.component.ts+html` | Rediseno completo con charts |
| `subscription-page.component.ts+html` | Habilitar billing |
| `Domain/Entities/Plan.cs` | Price, Currency |
| `Domain/Entities/UserSubscription.cs` | MercadoPago fields |
| `Api/Controllers/SubscriptionsController.cs` | Checkout + webhook endpoints |
| `Api/Program.cs` | DI registrations |
