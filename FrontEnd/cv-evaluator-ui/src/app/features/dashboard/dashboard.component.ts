import { Component, OnInit, signal, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { JobPositionsService } from '../job-positions/job-positions.service';
import { SubscriptionService } from '../../core/services/subscription.service';
import { AnalyticsService } from '../../core/services/analytics.service';
import { UserSubscription } from '../../core/models/subscription.model';
import { AnalyticsOverview, ScoreDataPoint, ScoreBucket } from '../../core/models/analytics.model';
import { StatCardComponent } from '../../shared/components/stat-card/stat-card.component';
import { ScoreChartComponent } from '../../shared/components/score-chart/score-chart.component';
import { DistributionChartComponent } from '../../shared/components/distribution-chart/distribution-chart.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink, StatCardComponent, ScoreChartComponent, DistributionChartComponent],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {
  private jobPositionsService = inject(JobPositionsService);
  private subscriptionService = inject(SubscriptionService);
  private analyticsService = inject(AnalyticsService);

  totalPositions = signal(0);
  subscription = signal<UserSubscription | null>(null);
  overview = signal<AnalyticsOverview | null>(null);
  timeSeriesData = signal<ScoreDataPoint[]>([]);
  distributionData = signal<ScoreBucket[]>([]);
  selectedPeriod = signal<string>('30d');

  ngOnInit(): void {
    this.jobPositionsService.getAll().subscribe({
      next: (positions) => this.totalPositions.set(positions.length)
    });

    this.subscriptionService.getMySubscription().subscribe({
      next: (sub) => this.subscription.set(sub)
    });

    this.analyticsService.getOverview().subscribe({
      next: (data) => this.overview.set(data)
    });

    this.loadTimeSeries('30d');

    this.analyticsService.getScoreDistribution().subscribe({
      next: (data) => this.distributionData.set(data.buckets)
    });
  }

  loadTimeSeries(period: string): void {
    this.selectedPeriod.set(period);
    this.analyticsService.getScoresOverTime(period).subscribe({
      next: (data) => this.timeSeriesData.set(data.dataPoints)
    });
  }

  Math = Math;

  formatLimit(value: number): string {
    return value === -1 ? '\u221e' : value.toString();
  }
}
