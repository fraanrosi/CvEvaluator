import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { AnalyticsService } from './analytics.service';
import { environment } from '../../../environments/environment';

describe('AnalyticsService', () => {
  let service: AnalyticsService;
  let httpMock: HttpTestingController;
  const base = `${environment.apiBaseUrl}/analytics`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(AnalyticsService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('getOverview gets from correct endpoint', () => {
    service.getOverview().subscribe();
    const req = httpMock.expectOne(`${base}/overview`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('getScoresOverTime includes period param', () => {
    service.getScoresOverTime('7d').subscribe();
    const req = httpMock.expectOne(`${base}/scores-over-time?period=7d`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('getScoreDistribution gets from correct endpoint', () => {
    service.getScoreDistribution().subscribe();
    const req = httpMock.expectOne(`${base}/score-distribution`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('getTopCandidates includes jobPositionId and limit', () => {
    service.getTopCandidates('job-1', 5).subscribe();
    const req = httpMock.expectOne(`${base}/top-candidates?jobPositionId=job-1&limit=5`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });
});
