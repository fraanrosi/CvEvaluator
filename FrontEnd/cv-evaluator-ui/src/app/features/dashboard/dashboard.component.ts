import { Component, OnInit, signal, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { JobPositionsService } from '../job-positions/job-positions.service';
import { SubscriptionService } from '../../core/services/subscription.service';
import { UserSubscription } from '../../core/models/subscription.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {
  private jobPositionsService = inject(JobPositionsService);
  private subscriptionService = inject(SubscriptionService);

  totalPositions = signal(0);
  subscription = signal<UserSubscription | null>(null);

  ngOnInit(): void {
    this.jobPositionsService.getAll().subscribe({
      next: (positions) => this.totalPositions.set(positions.length)
    });

    this.subscriptionService.getMySubscription().subscribe({
      next: (sub) => this.subscription.set(sub)
    });
  }

  formatLimit(value: number): string {
    return value === -1 ? '∞' : value.toString();
  }
}