import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-50 px-4">
      <div class="text-center">
        <p class="text-6xl font-bold text-blue-600 mb-4">404</p>
        <h1 class="text-2xl font-semibold text-gray-800 mb-2">Page not found</h1>
        <p class="text-gray-500 mb-8">The page you're looking for doesn't exist or has been moved.</p>
        <a routerLink="/dashboard"
           class="bg-blue-600 hover:bg-blue-700 text-white font-medium px-6 py-2.5 rounded-lg transition-colors">
          Go to Dashboard
        </a>
      </div>
    </div>
  `
})
export class NotFoundComponent {}
