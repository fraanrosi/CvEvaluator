import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../features/auth/auth.service';
import { Router } from '@angular/router';

@Component({
  standalone: true,
  selector: 'app-navbar',
  imports: [CommonModule, RouterModule],
  template: `
  <nav class="bg-gray-900 text-white px-6 py-4 flex justify-between items-center">
    <div class="font-bold text-lg">
      CvEvaluator
    </div>

    <div class="space-x-4" *ngIf="auth.isLoggedIn$ | async; else guest">
      <a routerLink="/dashboard" class="hover:underline">Dashboard</a>
      <a routerLink="/evaluations" class="hover:underline">Evaluations</a>
      <button (click)="logout()" class="bg-red-500 px-3 py-1 rounded">Logout</button>
    </div>

    <ng-template #guest>
      <a routerLink="/login" class="hover:underline mr-4">Login</a>
      <a routerLink="/register" class="hover:underline">Register</a>
    </ng-template>
  </nav>
  `
})
export class NavbarComponent {

  auth = inject(AuthService);
  private router = inject(Router);

  logout() {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}