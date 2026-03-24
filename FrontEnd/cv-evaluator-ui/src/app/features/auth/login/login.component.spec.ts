import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { LoginComponent } from './login.component';
import { AuthService } from '../auth.service';
import { ToastService } from '../../../shared/components/toast/toast.service';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let authService: {
    login: ReturnType<typeof vi.fn>;
    resendConfirmation: ReturnType<typeof vi.fn>;
  };
  let router: Router;
  let toast: { success: ReturnType<typeof vi.fn>; error: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    authService = { login: vi.fn(), resendConfirmation: vi.fn() };
    toast = { success: vi.fn(), error: vi.fn() };

    TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: authService },
        { provide: ToastService, useValue: toast },
      ],
    });

    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);

    const fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
  });

  it('creates successfully', () => {
    expect(component).toBeTruthy();
  });

  it('empty form submit does not call login', () => {
    component.submit();
    expect(authService.login).not.toHaveBeenCalled();
  });

  it('login success navigates to dashboard', () => {
    authService.login.mockReturnValue(of({ token: 't', email: 'e@e.com' }));
    component.form.setValue({ email: 'test@test.com', password: '123456' });
    component.submit();
    expect(router.navigate).toHaveBeenCalledWith(['/dashboard']);
  });

  it('login failure sets error signal', () => {
    authService.login.mockReturnValue(throwError(() => ({ error: { message: 'fail' } })));
    component.form.setValue({ email: 'test@test.com', password: '123456' });
    component.submit();
    expect(component.error()).toBe('Invalid email or password');
  });

  it('email not confirmed sets emailNotConfirmed signal', () => {
    authService.login.mockReturnValue(
      throwError(() => ({ error: { code: 'EMAIL_NOT_CONFIRMED' } })),
    );
    component.form.setValue({ email: 'test@test.com', password: '123456' });
    component.submit();
    expect(component.emailNotConfirmed()).toBe(true);
  });
});
