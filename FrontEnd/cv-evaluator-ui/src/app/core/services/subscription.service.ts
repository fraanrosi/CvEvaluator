import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Plan, UserSubscription } from '../models/subscription.model';

@Injectable({ providedIn: 'root' })
export class SubscriptionService {
  private http = inject(HttpClient);
  private base = `${environment.apiBaseUrl}/subscriptions`;

  getPlans(): Observable<Plan[]> {
    return this.http.get<Plan[]>(`${this.base}/plans`);
  }

  getMySubscription(): Observable<UserSubscription> {
    return this.http.get<UserSubscription>(`${this.base}/my-subscription`);
  }

  createCheckout(planId: string): Observable<{ initPoint: string }> {
    return this.http.post<{ initPoint: string }>(`${this.base}/create-checkout`, { planId });
  }
}
