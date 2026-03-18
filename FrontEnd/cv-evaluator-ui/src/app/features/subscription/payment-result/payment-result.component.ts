import { Component, OnInit, signal, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

type ResultStatus = 'success' | 'failure' | 'pending';

@Component({
  selector: 'app-payment-result',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="min-h-screen bg-surface-950 flex items-center justify-center p-6">
      <div class="max-w-md w-full text-center animate-fade-in-up">

        <!-- Icon -->
        <div class="flex justify-center mb-6">

          @if (status() === 'success') {
            <div class="w-24 h-24 rounded-full flex items-center justify-center text-emerald-400 bg-emerald-400/10">
              <svg class="w-14 h-14" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"/>
              </svg>
            </div>
          }

          @if (status() === 'failure') {
            <div class="w-24 h-24 rounded-full flex items-center justify-center text-rose-400 bg-rose-400/10">
              <svg class="w-14 h-14" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
              </svg>
            </div>
          }

          @if (status() === 'pending') {
            <div class="w-24 h-24 rounded-full flex items-center justify-center text-amber-400 bg-amber-400/10">
              <svg class="w-14 h-14" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"/>
              </svg>
            </div>
          }

        </div>

        <!-- Title -->
        <h1 class="font-heading text-3xl font-bold text-zinc-100 mb-3">
          @if (status() === 'success') { Payment successful! }
          @if (status() === 'failure') { Payment failed }
          @if (status() === 'pending') { Payment pending }
        </h1>

        <!-- Message -->
        <p class="text-zinc-400 mb-8 leading-relaxed">
          @if (status() === 'success') {
            Your plan has been upgraded. You can now enjoy all the benefits of your new subscription.
          }
          @if (status() === 'failure') {
            There was a problem processing your payment. You have not been charged. Please try again.
          }
          @if (status() === 'pending') {
            Your payment is being processed. We'll notify you once it's confirmed. This may take a few minutes.
          }
        </p>

        <!-- CTA -->
        @if (status() === 'failure') {
          <a routerLink="/subscription" class="btn-primary inline-block">Try again</a>
        } @else {
          <a routerLink="/dashboard" class="btn-primary inline-block">Go to Dashboard</a>
        }

      </div>
    </div>
  `
})
export class PaymentResultComponent implements OnInit {
  private route = inject(ActivatedRoute);

  status = signal<ResultStatus>('pending');

  ngOnInit(): void {
    const s = this.route.snapshot.data['status'] as ResultStatus;
    this.status.set(s ?? 'pending');
  }
}
