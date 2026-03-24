import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { SubscriptionService } from './subscription.service';
import { environment } from '../../../environments/environment';

describe('SubscriptionService', () => {
  let service: SubscriptionService;
  let httpMock: HttpTestingController;
  const base = `${environment.apiBaseUrl}/subscriptions`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(SubscriptionService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('getPlans gets from correct endpoint', () => {
    service.getPlans().subscribe();
    const req = httpMock.expectOne(`${base}/plans`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('getMySubscription gets from correct endpoint', () => {
    service.getMySubscription().subscribe();
    const req = httpMock.expectOne(`${base}/my-subscription`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('createCheckout posts with planId', () => {
    service.createCheckout('plan-123').subscribe();
    const req = httpMock.expectOne(`${base}/create-checkout`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ planId: 'plan-123' });
    req.flush({ initPoint: 'https://mp.com/checkout' });
  });
});
