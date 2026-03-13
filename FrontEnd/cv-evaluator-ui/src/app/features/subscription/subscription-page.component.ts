import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SubscriptionService } from '../../core/services/subscription.service';
import { UserSubscription, Plan } from '../../core/models/subscription.model';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';

@Component({
  selector: 'app-subscription-page',
  standalone: true,
  imports: [CommonModule, SpinnerComponent],
  templateUrl: './subscription-page.component.html'
})
export class SubscriptionPageComponent implements OnInit {
  private subscriptionService = inject(SubscriptionService);

  subscription = signal<UserSubscription | null>(null);
  plans = signal<Plan[]>([]);
  loading = signal(true);

  ngOnInit(): void {
    this.subscriptionService.getMySubscription().subscribe({
      next: (sub) => {
        this.subscription.set(sub);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });

    this.subscriptionService.getPlans().subscribe({
      next: (plans) => this.plans.set(plans)
    });
  }

  evalProgress(sub: UserSubscription): number {
    if (sub.maxEvaluationsPerMonth === -1) return 100;
    return Math.min((sub.evaluationsUsedThisMonth / sub.maxEvaluationsPerMonth) * 100, 100);
  }

  jobProgress(sub: UserSubscription): number {
    if (sub.maxJobPositions === -1) return 0;
    return Math.min((sub.jobPositionsCount / sub.maxJobPositions) * 100, 100);
  }

  formatLimit(value: number): string {
    return value === -1 ? 'Unlimited' : value.toString();
  }
}
