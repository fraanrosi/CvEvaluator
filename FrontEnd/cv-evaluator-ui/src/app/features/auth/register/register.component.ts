import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule,
  FormBuilder,
  Validators,
  AbstractControl,
  ValidationErrors
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../auth.service';
import { ToastService } from '../../../shared/components/toast/toast.service';

function passwordMatchValidator(group: AbstractControl): ValidationErrors | null {
  const password = group.get('password')?.value;
  const confirm = group.get('confirmPassword')?.value;
  return password === confirm ? null : { passwordMismatch: true };
}

@Component({
  standalone: true,
  selector: 'app-register',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
  <div class="min-h-screen flex items-center justify-center bg-surface-950 relative overflow-hidden">

    <!-- Background decoration -->
    <div class="absolute inset-0 overflow-hidden pointer-events-none">
      <div class="absolute -top-40 -left-40 w-96 h-96 bg-accent/5 rounded-full blur-3xl"></div>
      <div class="absolute -bottom-40 -right-40 w-96 h-96 bg-accent/3 rounded-full blur-3xl"></div>
    </div>

    <div class="relative w-full max-w-md mx-4 animate-fade-in-up">

      <!-- Logo / Brand -->
      <div class="text-center mb-8">
        <h1 class="font-heading text-3xl font-bold text-zinc-100 tracking-tight">
          Cv<span class="text-accent">Evaluator</span>
        </h1>
        <p class="text-muted text-sm mt-1">Start evaluating resumes with AI</p>
      </div>

      <div class="card p-8">
        <h2 class="font-heading text-xl font-semibold text-zinc-100 mb-6">Create account</h2>

        <form [formGroup]="form" (ngSubmit)="submit()" class="space-y-5">

          <div>
            <label class="label">Full Name</label>
            <input
              type="text"
              formControlName="fullName"
              placeholder="John Doe"
              class="input-field"
            />
            @if (form.controls.fullName.invalid && form.controls.fullName.touched) {
              <p class="text-rose-400 text-xs mt-1.5">
                Name is required
              </p>
            }
          </div>

          <div>
            <label class="label">Email</label>
            <input
              type="email"
              formControlName="email"
              placeholder="you&#64;example.com"
              class="input-field"
            />
            @if (form.controls.email.invalid && form.controls.email.touched) {
              <p class="text-rose-400 text-xs mt-1.5">
                Enter a valid email address
              </p>
            }
          </div>

          <div>
            <label class="label">Password</label>
            <input
              type="password"
              formControlName="password"
              placeholder="Min. 6 characters"
              class="input-field"
            />
            @if (form.controls.password.invalid && form.controls.password.touched) {
              <p class="text-rose-400 text-xs mt-1.5">
                At least 6 characters required
              </p>
            }
          </div>

          <div>
            <label class="label">Confirm Password</label>
            <input
              type="password"
              formControlName="confirmPassword"
              placeholder="Repeat password"
              class="input-field"
            />
          </div>

          @if (form.errors?.['passwordMismatch'] && form.touched) {
            <p class="text-rose-400 text-xs">
              Passwords don't match
            </p>
          }

          <button
            type="submit"
            [disabled]="form.invalid || loading()"
            class="btn-primary w-full">
            {{ loading() ? 'Creating account...' : 'Create account' }}
          </button>
        </form>

        @if (error()) {
          <p class="text-rose-400 text-sm text-center mt-4">
            {{ error() }}
          </p>
        }

        <div class="divider mt-6 mb-4"></div>

        <p class="text-center text-sm text-muted">
          Already have an account?
          <a routerLink="/login" class="text-accent hover:text-accent-light font-medium transition-colors">
            Sign in
          </a>
        </p>
      </div>
    </div>
  </div>
  `
})
export class RegisterComponent {

  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);
  private toast = inject(ToastService);

  loading = signal(false);
  error = signal<string | null>(null);

  form = this.fb.group({
    fullName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    confirmPassword: ['', Validators.required]
  }, { validators: passwordMatchValidator });

  submit() {
    if (this.form.invalid) return;

    this.loading.set(true);
    this.error.set(null);

    const { email, password, fullName } = this.form.value;

    this.auth.register(email!, password!, fullName!)
      .subscribe({
        next: () => {
          this.toast.success('Account created! Check your email to confirm.');
          this.router.navigate(['/login']);
        },
        error: () => {
          this.toast.error('Registration failed');
          this.error.set('Could not create account. Please try again.');
          this.loading.set(false);
        }
      });
  }
}
