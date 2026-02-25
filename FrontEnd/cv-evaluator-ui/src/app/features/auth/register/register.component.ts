import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule,
  FormBuilder,
  Validators,
  AbstractControl,
  ValidationErrors
} from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../auth.service';

function passwordMatchValidator(group: AbstractControl): ValidationErrors | null {
  const password = group.get('password')?.value;
  const confirm = group.get('confirmPassword')?.value;

  return password === confirm ? null : { passwordMismatch: true };
}

@Component({
  standalone: true,
  selector: 'app-register',
  imports: [CommonModule, ReactiveFormsModule],
  template: `
  <div class="min-h-screen flex items-center justify-center bg-gray-100">
    <div class="bg-white p-8 rounded-xl shadow-md w-full max-w-md">
      <h2 class="text-2xl font-bold mb-6 text-center">Register</h2>

      <form [formGroup]="form" (ngSubmit)="submit()" class="space-y-4">

        <div>
          <label class="block text-sm font-medium mb-1">Full Name</label>
          <input
            type="text"
            formControlName="fullName"
            class="w-full border rounded-lg px-3 py-2 focus:outline-none focus:ring focus:ring-blue-300"
          />
          <p *ngIf="form.controls.fullName.invalid && form.controls.fullName.touched"
             class="text-red-500 text-sm mt-1">
            Nombre requerido
          </p>
        </div>

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

        <div>
          <label class="block text-sm font-medium mb-1">Confirm Password</label>
          <input
            type="password"
            formControlName="confirmPassword"
            class="w-full border rounded-lg px-3 py-2 focus:outline-none focus:ring focus:ring-blue-300"
          />
        </div>

        <p *ngIf="form.errors?.['passwordMismatch'] && form.touched"
           class="text-red-500 text-sm">
          Las contraseñas no coinciden
        </p>

        <button
          type="submit"
          [disabled]="form.invalid || loading"
          class="w-full bg-green-600 text-white py-2 rounded-lg hover:bg-green-700 disabled:opacity-50">
          {{ loading ? 'Registrando...' : 'Crear cuenta' }}
        </button>
      </form>

      <p class="text-center text-sm mt-4">
        ¿Ya tenés cuenta?
        <a routerLink="/login" class="text-blue-600 hover:underline">
          Iniciar sesión
        </a>
      </p>

      <p *ngIf="error" class="text-red-500 text-center mt-4">
        {{ error }}
      </p>
    </div>
  </div>
  `
})
export class RegisterComponent {

  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);

  loading = false;
  error: string | null = null;

  form = this.fb.group({
    fullName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    confirmPassword: ['', Validators.required]
  }, { validators: passwordMatchValidator });

  submit() {
    if (this.form.invalid) return;

    this.loading = true;
    this.error = null;

    const { email, password, fullName } = this.form.value;

    this.auth.register(email!, password!, fullName!)
      .subscribe({
        next: () => {
          this.router.navigate(['/login']);
        },
        error: () => {
          this.error = 'Error al registrar usuario';
          this.loading = false;
        }
      });
  }
}