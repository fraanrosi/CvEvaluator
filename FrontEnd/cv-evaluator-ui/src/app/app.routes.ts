import { Routes } from '@angular/router';
import { authGuard } from './features/auth/auth.guard';
import { AppShellComponent } from './shared/layout/app-shell/app-shell.component';

export const routes: Routes = [

  // landing — public
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () =>
      import('./features/landing/landing.component')
        .then(m => m.LandingComponent)
  },

  // public routes — no navbar
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login/login.component')
        .then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./features/auth/register/register.component')
        .then(m => m.RegisterComponent)
  },
  {
    path: 'forgot-password',
    loadComponent: () =>
      import('./features/auth/forgot-password/forgot-password.component')
        .then(m => m.ForgotPasswordComponent)
  },
  {
    path: 'reset-password',
    loadComponent: () =>
      import('./features/auth/reset-password/reset-password.component')
        .then(m => m.ResetPasswordComponent)
  },
  {
    path: 'confirm-email',
    loadComponent: () =>
      import('./features/auth/confirm-email/confirm-email.component')
        .then(m => m.ConfirmEmailComponent)
  },

  // protected routes — with navbar (AppShell)
  {
    path: '',
    component: AppShellComponent,
    canActivate: [authGuard],
    children: [

      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/dashboard/dashboard.component')
            .then(m => m.DashboardComponent)
      },

      {
        path: 'job-positions',
        children: [

          // list
          {
            path: '',
            loadComponent: () =>
              import('./features/job-positions/pages/job-positions-page/job-positions-page.component')
                .then(m => m.JobPositionsPageComponent)
          },

          // create
          {
            path: 'create',
            loadComponent: () =>
              import('./features/job-positions/pages/create-job-position/create-job-position.component')
                .then(m => m.CreateJobPositionComponent)
          },

          // detail
          {
            path: ':id',
            loadComponent: () =>
              import('./features/job-positions/pages/job-position-detail/job-position-detail.component')
                .then(m => m.JobPositionDetailComponent)
          },

          // edit
          {
            path: ':id/edit',
            loadComponent: () =>
              import('./features/job-positions/pages/edit-job-position/edit-job-position.component')
                .then(m => m.EditJobPositionComponent)
          }

        ]
      },

      {
        path: 'evaluations/:id',
        loadComponent: () =>
          import('./features/evaluations/pages/evaluation-detail/evaluation-detail.component')
            .then(m => m.EvaluationDetailComponent)
      },

      {
        path: 'subscription',
        loadComponent: () =>
          import('./features/subscription/subscription-page.component')
            .then(m => m.SubscriptionPageComponent)
      },

    ]
  },

  // 404 — catch-all
  {
    path: '**',
    loadComponent: () =>
      import('./features/errors/not-found.component')
        .then(m => m.NotFoundComponent)
  }

];
