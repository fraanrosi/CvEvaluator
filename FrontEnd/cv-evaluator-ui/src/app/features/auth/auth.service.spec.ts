import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { AuthService } from './auth.service';
import { SessionService } from '../../core/services/session.service';
import { environment } from '../../../environments/environment';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  let sessionMock: any;

  beforeEach(() => {
    sessionMock = {
      setSession: vi.fn(),
      clearSession: vi.fn(),
      getToken: vi.fn().mockReturnValue('mock-token'),
      getEmail: vi.fn(),
      isAuthenticated: vi.fn().mockReturnValue(false),
    } as any;

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: SessionService, useValue: sessionMock },
      ],
    });

    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('login posts to correct endpoint', () => {
    service.login('a@b.com', 'pass').subscribe();
    const req = httpMock.expectOne(`${environment.apiBaseUrl}/auth/login`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ email: 'a@b.com', password: 'pass' });
    req.flush({ token: 'jwt', email: 'a@b.com' });
  });

  it('login on success sets session and emits loggedIn', () => {
    let loggedIn: boolean | undefined;
    service.isLoggedIn$.subscribe((v) => (loggedIn = v));

    service.login('a@b.com', 'pass').subscribe();
    httpMock
      .expectOne(`${environment.apiBaseUrl}/auth/login`)
      .flush({ token: 'jwt', email: 'a@b.com' });

    expect(sessionMock.setSession).toHaveBeenCalledWith('jwt', 'a@b.com');
    expect(loggedIn).toBe(true);
  });

  it('register posts with correct payload', () => {
    service.register('a@b.com', 'pass', 'Test User').subscribe();
    const req = httpMock.expectOne(`${environment.apiBaseUrl}/auth/register`);
    expect(req.request.body).toEqual({ email: 'a@b.com', password: 'pass', fullName: 'Test User' });
    req.flush({});
  });

  it('confirmEmail posts with email and token', () => {
    service.confirmEmail('a@b.com', 'tok').subscribe();
    const req = httpMock.expectOne(`${environment.apiBaseUrl}/auth/confirm-email`);
    expect(req.request.body).toEqual({ email: 'a@b.com', token: 'tok' });
    req.flush({});
  });

  it('forgotPassword posts with email', () => {
    service.forgotPassword('a@b.com').subscribe();
    const req = httpMock.expectOne(`${environment.apiBaseUrl}/auth/forgot-password`);
    expect(req.request.body).toEqual({ email: 'a@b.com' });
    req.flush({});
  });

  it('resetPassword posts with correct payload', () => {
    service.resetPassword('a@b.com', 'tok', 'newpass').subscribe();
    const req = httpMock.expectOne(`${environment.apiBaseUrl}/auth/reset-password`);
    expect(req.request.body).toEqual({ email: 'a@b.com', token: 'tok', newPassword: 'newpass' });
    req.flush({});
  });

  it('getToken delegates to SessionService', () => {
    expect(service.getToken()).toBe('mock-token');
    expect(sessionMock.getToken).toHaveBeenCalled();
  });

  it('logout clears session and emits false', () => {
    let loggedIn: boolean | undefined;
    service.isLoggedIn$.subscribe((v) => (loggedIn = v));

    service.logout();

    expect(sessionMock.clearSession).toHaveBeenCalled();
    expect(loggedIn).toBe(false);
  });
});
