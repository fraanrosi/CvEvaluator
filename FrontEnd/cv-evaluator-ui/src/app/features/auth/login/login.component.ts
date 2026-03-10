import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../auth.service';

@Component({
  standalone: true,
  selector: 'app-login',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
  <div class="min-h-screen flex items-center justify-center bg-gray-100">
    <div class="bg-white p-8 rounded-xl shadow-md w-full max-w-md">
      <h2 class="text-2xl font-bold mb-6 text-center">Login</h2>

      <form [formGroup]="form" (ngSubmit)="submit()" class="space-y-4">

        <div>
          <label class="block text-sm font-medium mb-1">Email</label>
          <input
            type="email"
            formControlName="email"
            class="w-full border rounded-lg px-3 py-2 focus:outline-none focus:ring focus:ring-blue-300"
          />
          <p *ngIf="form.controls.email.invalid && form.controls.email.touched"
             class="text-red-500 text-sm mt-1">
            Email inválido
          </p>
        </div>

        <div>
          <label class="block text-sm font-medium mb-1">Password</label>
          <input
            type="password"
            formControlName="password"
            class="w-full border rounded-lg px-3 py-2 focus:outline-none focus:ring focus:ring-blue-300"
          />
          <p *ngIf="form.controls.password.invalid && form.controls.password.touched"
             class="text-red-500 text-sm mt-1">
            Mínimo 6 caracteres
          </p>
        </div>

        <button
          type="submit"
          [disabled]="form.invalid || loading"
          class="w-full bg-blue-600 text-white py-2 rounded-lg hover:bg-blue-700 disabled:opacity-50">
          {{ loading ? 'Ingresando...' : 'Login' }}
        </button>
      </form>

      <p class="text-center text-sm mt-4">
        ¿No tenés cuenta?
        <a routerLink="/register" class="text-blue-600 hover:underline">
          Registrate
        </a>
      </p>

      <p *ngIf="error" class="text-red-500 text-center mt-4">
        {{ error }}
      </p>
    </div>
  </div>
  `
})
export class LoginComponent {

  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);

  loading = false;
  error: string | null = null;

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  submit() {
    if (this.form.invalid) return;

    this.loading = true;
    this.error = null;

    const { email, password } = this.form.value;

    this.auth.login(email!, password!)
      .subscribe({
        next: () => {
          this.router.navigate(['/dashboard']);
        },
        error: () => {
          this.error = 'Credenciales inválidas';
          this.loading = false;
        }
      });
  }
}