import { TestBed } from '@angular/core/testing';
import { Router, UrlTree } from '@angular/router';
import { authGuard } from './auth.guard';
import { SessionService } from '../../core/services/session.service';

describe('authGuard', () => {
  let sessionService: SessionService;
  let router: Router;

  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        {
          provide: Router,
          useValue: {
            createUrlTree: vi.fn(
              (segments: string[]) =>
                ({ toString: () => segments.join('/') }) as unknown as UrlTree,
            ),
          },
        },
      ],
    });
    sessionService = TestBed.inject(SessionService);
    router = TestBed.inject(Router);
  });

  it('returns true when authenticated', () => {
    sessionService.setSession('token', 'test@test.com');
    const result = TestBed.runInInjectionContext(() => authGuard({} as any, {} as any));
    expect(result).toBe(true);
  });

  it('returns UrlTree to login when not authenticated', () => {
    const result = TestBed.runInInjectionContext(() => authGuard({} as any, {} as any));
    expect(result).not.toBe(true);
    expect(router.createUrlTree).toHaveBeenCalledWith(['/login']);
  });

  it('redirects to login when no token exists', () => {
    const result = TestBed.runInInjectionContext(() => authGuard({} as any, {} as any));
    expect(result).not.toBe(true);
    expect(router.createUrlTree).toHaveBeenCalled();
  });
});
