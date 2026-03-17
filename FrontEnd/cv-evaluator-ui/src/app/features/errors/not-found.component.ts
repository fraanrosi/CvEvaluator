import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-surface-950 px-4">
      <div class="text-center animate-fade-in-up">
        <p class="font-heading text-7xl font-bold text-accent/30 mb-4">404</p>
        <h1 class="font-heading text-2xl font-semibold text-zinc-100 mb-2">Page not found</h1>
        <p class="text-muted mb-8">The page you're looking for doesn't exist or has been moved.</p>
        <a routerLink="/dashboard" class="btn-primary">
          Go to Dashboard
        </a>
      </div>
    </div>
  `
})
export class NotFoundComponent {}
