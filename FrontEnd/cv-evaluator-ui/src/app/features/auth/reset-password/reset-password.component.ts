import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { AuthService } from '../auth.service';
import { ToastService } from '../../../shared/components/toast/toast.service';

function passwordMatchValidator(group: AbstractControl): ValidationErrors | null {
  const password = group.get('newPassword')?.value;
  const confirm = group.get('confirmPassword')?.value;
  return password === confirm ? null : { passwordMismatch: true };
}

@Component({
  standalone: true,
  selector: 'app-reset-password',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
  <div class="min-h-screen flex items-center justify-center bg-surface-950 relative overflow-hidden">

    <div class="absolute inset-0 overflow-hidden pointer-events-none">
      <div class="absolute -bottom-40 -left-40 w-96 h-96 bg-accent/5 rounded-full blur-3xl"></div>
    </div>

    <div class="relative w-full max-w-md mx-4 animate-fade-in-up">

      <div class="text-center mb-8">
        <h1 class="font-heading text-3xl font-bold text-zinc-100 tracking-tight">
          Cv<span class="text-accent">Evaluator</span>
        </h1>
      </div>

      <div class="card p-8">
        <h2 class="font-heading text-xl font-semibold text-zinc-100 mb-6">Reset password</h2>

        <!-- Invalid link -->
        @if (invalidLink()) {
          <div class="animate-fade-in">
            <div class="bg-rose-500/10 border border-rose-500/20 rounded-lg p-4 mb-4">
              <p class="text-rose-400 text-sm">Invalid or missing reset link. Please request a new one.</p>
            </div>
            <a routerLink="/forgot-password" class="text-accent hover:text-accent-light font-medium text-sm transition-colors">
              Request new link
            </a>
          </div>
        }

        <!-- Reset form -->
        @if (!invalidLink() && !success()) {
          <form [formGroup]="form" (ngSubmit)="submit()" class="space-y-5">
            <div>
              <label class="label">New Password</label>
              <input
                type="password"
                formControlName="newPassword"
                placeholder="Min. 6 characters"
                class="input-field"
              />
              @if (form.controls.newPassword.invalid && form.controls.newPassword.touched) {
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
              {{ loading() ? 'Resetting...' : 'Reset password' }}
            </button>

            @if (error()) {
              <p class="text-rose-400 text-sm text-center">{{ error() }}</p>
            }
          </form>
        }

        <!-- Success -->
        @if (success()) {
          <div class="animate-fade-in text-center">
            <div class="bg-emerald-500/10 border border-emerald-500/20 rounded-lg p-4 mb-5">
              <p class="text-emerald-400 text-sm">Your password has been reset successfully.</p>
            </div>
            <a routerLink="/login" class="btn-primary inline-block">
              Go to sign in
            </a>
          </div>
        }
      </div>
    </div>
  </div>
  `
})
export class ResetPasswordComponent implements OnInit {

  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private toast = inject(ToastService);

  loading = signal(false);
  success = signal(false);
  invalidLink = signal(false);
  error = signal<string | null>(null);

  private token = '';
  private email = '';

  form = this.fb.group({
    newPassword: ['', [Validators.required, Validators.minLength(6)]],
    confirmPassword: ['', Validators.required]
  }, { validators: passwordMatchValidator });

  ngOnInit() {
    this.token = this.route.snapshot.queryParamMap.get('token') ?? '';
    this.email = this.route.snapshot.queryParamMap.get('email') ?? '';

    if (!this.token || !this.email) {
      this.invalidLink.set(true);
    }
  }

  submit() {
    if (this.form.invalid) return;

    this.loading.set(true);
    this.error.set(null);

    const { newPassword } = this.form.value;

    this.auth.resetPassword(this.email, this.token, newPassword!).subscribe({
      next: () => {
        this.success.set(true);
        this.loading.set(false);
        this.toast.success('Password reset successfully!');
      },
      error: () => {
        this.error.set('Failed to reset password. The link may have expired.');
        this.toast.error('Failed to reset password');
        this.loading.set(false);
      }
    });
  }
}
