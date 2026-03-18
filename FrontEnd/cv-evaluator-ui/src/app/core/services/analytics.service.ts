import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AnalyticsOverview, ScoreTimeSeries, ScoreDistribution, TopCandidate } from '../models/analytics.model';

@Injectable({ providedIn: 'root' })
export class AnalyticsService {
  private http = inject(HttpClient);
  private base = `${environment.apiBaseUrl}/analytics`;

  getOverview(): Observable<AnalyticsOverview> {
    return this.http.get<AnalyticsOverview>(`${this.base}/overview`);
  }

  getScoresOverTime(period: string = '30d'): Observable<ScoreTimeSeries> {
    return this.http.get<ScoreTimeSeries>(`${this.base}/scores-over-time?period=${period}`);
  }

  getScoreDistribution(): Observable<ScoreDistribution> {
    return this.http.get<ScoreDistribution>(`${this.base}/score-distribution`);
  }

  getTopCandidates(jobPositionId: string, limit: number = 10): Observable<TopCandidate[]> {
    return this.http.get<TopCandidate[]>(`${this.base}/top-candidates?jobPositionId=${jobPositionId}&limit=${limit}`);
  }
}
