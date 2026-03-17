import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../auth.service';
import { ToastService } from '../../../shared/components/toast/toast.service';

@Component({
  standalone: true,
  selector: 'app-forgot-password',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
  <div class="min-h-screen flex items-center justify-center bg-surface-950 relative overflow-hidden">

    <div class="absolute inset-0 overflow-hidden pointer-events-none">
      <div class="absolute -top-40 -right-40 w-96 h-96 bg-accent/5 rounded-full blur-3xl"></div>
    </div>

    <div class="relative w-full max-w-md mx-4 animate-fade-in-up">

      <div class="text-center mb-8">
        <h1 class="font-heading text-3xl font-bold text-zinc-100 tracking-tight">
          Cv<span class="text-accent">Evaluator</span>
        </h1>
      </div>

      <div class="card p-8">
        <h2 class="font-heading text-xl font-semibold text-zinc-100 mb-2">Forgot password</h2>
        <p class="text-muted text-sm mb-6">
          Enter your email and we'll send you a reset link.
        </p>

        @if (!sent()) {
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

            <button
              type="submit"
              [disabled]="form.invalid || loading()"
              class="btn-primary w-full">
              {{ loading() ? 'Sending...' : 'Send reset link' }}
            </button>
          </form>
        } @else {
          <div class="animate-fade-in">
            <div class="bg-emerald-500/10 border border-emerald-500/20 rounded-lg p-4 mb-4">
              <p class="text-emerald-400 text-sm">
                If an account with that email exists, a reset link has been sent. Check your inbox.
              </p>
            </div>
            <button
              (click)="sent.set(false)"
              class="text-accent hover:text-accent-light text-sm font-medium transition-colors">
              Send again
            </button>
          </div>
        }

        <div class="divider mt-6 mb-4"></div>

        <p class="text-center text-sm text-muted">
          <a routerLink="/login" class="text-accent hover:text-accent-light font-medium transition-colors">
            Back to sign in
          </a>
        </p>
      </div>
    </div>
  </div>
  `
})
export class ForgotPasswordComponent {

  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private toast = inject(ToastService);

  loading = signal(false);
  sent = signal(false);

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]]
  });

  submit() {
    if (this.form.invalid) return;

    this.loading.set(true);
    const { email } = this.form.value;

    this.auth.forgotPassword(email!).subscribe({
      next: () => {
        this.sent.set(true);
        this.loading.set(false);
      },
      error: () => {
        this.toast.error('Something went wrong. Please try again.');
        this.loading.set(false);
      }
    });
  }
}
