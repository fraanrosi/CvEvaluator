import { Routes } from '@angular/router';
import { authGuard } from './features/auth/auth.guard';

export const routes: Routes = [

  // default
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full'
  },

  // auth
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

  // dashboard
  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/dashboard/dashboard.component')
        .then(m => m.DashboardComponent)
  },

  // job positions
  {
    path: 'job-positions',
    canActivate: [authGuard],
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

  // fallback
  {
    path: '**',
    redirectTo: 'dashboard'
  }

];