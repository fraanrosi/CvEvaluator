import { SessionService } from './session.service';

describe('SessionService', () => {
  let service: SessionService;

  beforeEach(() => {
    sessionStorage.clear();
    service = new SessionService();
  });

  it('setSession stores token and email', () => {
    service.setSession('my-token', 'test@test.com');
    expect(sessionStorage.getItem('cv_token')).toBe('my-token');
    expect(sessionStorage.getItem('cv_email')).toBe('test@test.com');
  });

  it('clearSession removes token and email', () => {
    service.setSession('my-token', 'test@test.com');
    service.clearSession();
    expect(sessionStorage.getItem('cv_token')).toBeNull();
    expect(sessionStorage.getItem('cv_email')).toBeNull();
  });

  it('getToken returns stored token', () => {
    service.setSession('my-token', 'test@test.com');
    expect(service.getToken()).toBe('my-token');
  });

  it('getToken returns null when no token', () => {
    expect(service.getToken()).toBeNull();
  });

  it('getEmail returns stored email', () => {
    service.setSession('my-token', 'test@test.com');
    expect(service.getEmail()).toBe('test@test.com');
  });

  it('isAuthenticated returns true with token', () => {
    service.setSession('my-token', 'test@test.com');
    expect(service.isAuthenticated()).toBe(true);
  });

  it('isAuthenticated returns false without token', () => {
    expect(service.isAuthenticated()).toBe(false);
  });
});
