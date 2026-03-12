import { Routes } from '@angular/router';
import { authGuard } from './features/auth/auth.guard';
import { AppShellComponent } from './shared/layout/app-shell/app-shell.component';

export const routes: Routes = [

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

  // protected routes — with navbar (AppShell)
  {
    path: '',
    component: AppShellComponent,
    canActivate: [authGuard],
    children: [

      // default redirect inside shell
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      },

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

      { path: '**', redirectTo: 'dashboard' }

    ]
  }

];
