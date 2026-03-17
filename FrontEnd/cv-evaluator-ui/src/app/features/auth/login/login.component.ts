import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../auth.service';
import { ToastService } from '../../../shared/components/toast/toast.service';

@Component({
  standalone: true,
  selector: 'app-login',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
  <div class="min-h-screen flex items-center justify-center bg-surface-950 relative overflow-hidden">

    <!-- Background decoration -->
    <div class="absolute inset-0 overflow-hidden pointer-events-none">
      <div class="absolute -top-40 -right-40 w-96 h-96 bg-accent/5 rounded-full blur-3xl"></div>
      <div class="absolute -bottom-40 -left-40 w-96 h-96 bg-accent/3 rounded-full blur-3xl"></div>
    </div>

    <div class="relative w-full max-w-md mx-4 animate-fade-in-up">

      <!-- Logo / Brand -->
      <div class="text-center mb-8">
        <h1 class="font-heading text-3xl font-bold text-zinc-100 tracking-tight">
          Cv<span class="text-accent">Evaluator</span>
        </h1>
        <p class="text-muted text-sm mt-1">AI-powered resume analysis</p>
      </div>

      <div class="card p-8">
        <h2 class="font-heading text-xl font-semibold text-zinc-100 mb-6">Welcome back</h2>

        <form [formGroup]="form" (ngSubmit)="submit()" class="space-y-5">
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

          <div class="flex justify-end">
            <a routerLink="/forgot-password" class="text-sm text-accent hover:text-accent-light transition-colors">
              Forgot password?
            </a>
          </div>

          <button
            type="submit"
            [disabled]="form.invalid || loading()"
            class="btn-primary w-full">
            {{ loading() ? 'Signing in...' : 'Sign in' }}
          </button>
        </form>

        <!-- Email not confirmed warning -->
        @if (emailNotConfirmed()) {
          <div class="mt-5 bg-amber-500/10 border border-amber-500/20 rounded-lg p-4 animate-fade-in">
            <p class="text-amber-400 text-sm mb-2">
              Your email is not confirmed yet. Check your inbox or request a new link.
            </p>
            <button
              (click)="resendConfirmation()"
              [disabled]="resending()"
              class="text-accent hover:text-accent-light text-sm font-medium transition-colors disabled:opacity-50">
              {{ resending() ? 'Sending...' : 'Resend confirmation email' }}
            </button>
            @if (resentSuccess()) {
              <p class="text-emerald-400 text-xs mt-1">Confirmation email sent!</p>
            }
          </div>
        }

        @if (error() && !emailNotConfirmed()) {
          <p class="text-rose-400 text-sm text-center mt-4">
            {{ error() }}
          </p>
        }

        <div class="divider mt-6 mb-4"></div>

        <p class="text-center text-sm text-muted">
          Don't have an account?
          <a routerLink="/register" class="text-accent hover:text-accent-light font-medium transition-colors">
            Sign up
          </a>
        </p>
      </div>
    </div>
  </div>
  `
})
export class LoginComponent {

  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);
  private toast = inject(ToastService);

  loading = signal(false);
  error = signal<string | null>(null);
  emailNotConfirmed = signal(false);
  resending = signal(false);
  resentSuccess = signal(false);

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  submit() {
    if (this.form.invalid) return;

    this.loading.set(true);
    this.error.set(null);

    const { email, password } = this.form.value;

    this.auth.login(email!, password!)
      .subscribe({
        next: () => {
          this.toast.success('Welcome back!');
          this.router.navigate(['/dashboard']);
        },
        error: (err) => {
          this.loading.set(false);
          this.emailNotConfirmed.set(false);

          if (err.error?.code === 'EMAIL_NOT_CONFIRMED') {
            this.emailNotConfirmed.set(true);
            this.error.set(null);
          } else {
            this.toast.error('Invalid credentials');
            this.error.set('Invalid email or password');
          }
        }
      });
  }

  resendConfirmation() {
    const email = this.form.value.email;
    if (!email) return;

    this.resending.set(true);
    this.resentSuccess.set(false);

    this.auth.resendConfirmation(email).subscribe({
      next: () => {
        this.resentSuccess.set(true);
        this.resending.set(false);
      },
      error: () => {
        this.resending.set(false);
      }
    });
  }
}
