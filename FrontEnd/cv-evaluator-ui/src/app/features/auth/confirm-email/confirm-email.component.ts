import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { AuthService } from '../auth.service';

@Component({
  standalone: true,
  selector: 'app-confirm-email',
  imports: [CommonModule, RouterLink],
  template: `
  <div class="min-h-screen flex items-center justify-center bg-surface-950 relative overflow-hidden">

    <div class="absolute inset-0 overflow-hidden pointer-events-none">
      <div class="absolute -top-40 -left-40 w-96 h-96 bg-accent/5 rounded-full blur-3xl"></div>
    </div>

    <div class="relative w-full max-w-md mx-4 animate-fade-in-up">

      <div class="text-center mb-8">
        <h1 class="font-heading text-3xl font-bold text-zinc-100 tracking-tight">
          Cv<span class="text-accent">Evaluator</span>
        </h1>
      </div>

      <div class="card p-8 text-center">

        <!-- Loading -->
        @if (loading()) {
          <div class="py-8">
            <div class="animate-spin h-10 w-10 border-2 border-accent/30 border-t-accent rounded-full mx-auto mb-4"></div>
            <p class="text-muted">Confirming your email...</p>
          </div>
        }

        <!-- Success -->
        @if (!loading() && confirmed()) {
          <div class="animate-fade-in">
            <div class="w-16 h-16 bg-emerald-500/15 rounded-full flex items-center justify-center mx-auto mb-4">
              <svg class="w-8 h-8 text-emerald-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"/>
              </svg>
            </div>
            <h2 class="font-heading text-xl font-semibold text-zinc-100 mb-2">Email Confirmed!</h2>
            <p class="text-muted text-sm mb-6">Your email has been verified. You can now sign in.</p>
            <a routerLink="/login" class="btn-primary inline-block">
              Go to sign in
            </a>
          </div>
        }

        <!-- Error -->
        @if (!loading() && !confirmed()) {
          <div class="animate-fade-in">
            <div class="w-16 h-16 bg-rose-500/15 rounded-full flex items-center justify-center mx-auto mb-4">
              <svg class="w-8 h-8 text-rose-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
              </svg>
            </div>
            <h2 class="font-heading text-xl font-semibold text-zinc-100 mb-2">Confirmation Failed</h2>
            <p class="text-muted text-sm mb-5">{{ error() }}</p>

            <button
              (click)="resend()"
              [disabled]="resending()"
              class="text-accent hover:text-accent-light text-sm font-medium transition-colors disabled:opacity-50">
              {{ resending() ? 'Sending...' : 'Resend confirmation email' }}
            </button>
            @if (resent()) {
              <p class="text-emerald-400 text-xs mt-2">
                A new confirmation email has been sent.
              </p>
            }

            <div class="divider mt-5 mb-4"></div>
            <a routerLink="/login" class="text-accent hover:text-accent-light text-sm font-medium transition-colors">
              Back to sign in
            </a>
          </div>
        }

      </div>
    </div>
  </div>
  `
})
export class ConfirmEmailComponent implements OnInit {

  private route = inject(ActivatedRoute);
  private auth = inject(AuthService);

  loading = signal(true);
  confirmed = signal(false);
  error = signal('');
  resending = signal(false);
  resent = signal(false);

  private email = '';

  ngOnInit() {
    const token = this.route.snapshot.queryParamMap.get('token') ?? '';
    this.email = this.route.snapshot.queryParamMap.get('email') ?? '';

    if (!token || !this.email) {
      this.loading.set(false);
      this.error.set('Invalid or missing confirmation link.');
      return;
    }

    this.auth.confirmEmail(this.email, token).subscribe({
      next: () => {
        this.confirmed.set(true);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('The confirmation link is invalid or has expired.');
        this.loading.set(false);
      }
    });
  }

  resend() {
    if (!this.email) return;
    this.resending.set(true);

    this.auth.resendConfirmation(this.email).subscribe({
      next: () => {
        this.resent.set(true);
        this.resending.set(false);
      },
      error: () => {
        this.resending.set(false);
      }
    });
  }
}
