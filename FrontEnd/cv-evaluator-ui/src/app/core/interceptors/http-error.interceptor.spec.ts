import { TestBed } from '@angular/core/testing';
import { provideHttpClient, withInterceptors, HttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { httpErrorInterceptor } from './http-error.interceptor';
import { SessionService } from '../services/session.service';

describe('httpErrorInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;
  let router: Router;
  let sessionService: SessionService;

  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([httpErrorInterceptor])),
        provideHttpClientTesting(),
        { provide: Router, useValue: { navigate: vi.fn() } },
      ],
    });
    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
    sessionService = TestBed.inject(SessionService);
  });

  afterEach(() => httpMock.verify());

  it('on 401 clears session and navigates to login', () => {
    sessionService.setSession('token', 'test@test.com');
    http.get('/api/test').subscribe({ error: () => {} });
    httpMock
      .expectOne('/api/test')
      .flush('Unauthorized', { status: 401, statusText: 'Unauthorized' });
    expect(sessionStorage.getItem('cv_token')).toBeNull();
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('on 403 does not clear session or redirect', () => {
    sessionService.setSession('token', 'test@test.com');
    http.get('/api/test').subscribe({ error: () => {} });
    httpMock.expectOne('/api/test').flush('Forbidden', { status: 403, statusText: 'Forbidden' });
    expect(sessionStorage.getItem('cv_token')).toBe('token');
    expect(router.navigate).not.toHaveBeenCalled();
  });

  it('on 500 does not clear session or redirect', () => {
    sessionService.setSession('token', 'test@test.com');
    http.get('/api/test').subscribe({ error: () => {} });
    httpMock
      .expectOne('/api/test')
      .flush('Error', { status: 500, statusText: 'Internal Server Error' });
    expect(sessionStorage.getItem('cv_token')).toBe('token');
    expect(router.navigate).not.toHaveBeenCalled();
  });

  it('on 401 rethrows error', () => {
    let caughtError: any;
    http.get('/api/test').subscribe({ error: (e) => (caughtError = e) });
    httpMock
      .expectOne('/api/test')
      .flush('Unauthorized', { status: 401, statusText: 'Unauthorized' });
    expect(caughtError).toBeTruthy();
    expect(caughtError.status).toBe(401);
  });
});
