import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { RegisterComponent } from './register.component';
import { AuthService } from '../auth.service';
import { ToastService } from '../../../shared/components/toast/toast.service';

describe('RegisterComponent', () => {
  let component: RegisterComponent;
  let authService: { register: ReturnType<typeof vi.fn> };
  let router: Router;
  let toast: { success: ReturnType<typeof vi.fn>; error: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    authService = { register: vi.fn() };
    toast = { success: vi.fn(), error: vi.fn() };

    TestBed.configureTestingModule({
      imports: [RegisterComponent],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: authService },
        { provide: ToastService, useValue: toast },
      ],
    });

    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);

    const fixture = TestBed.createComponent(RegisterComponent);
    component = fixture.componentInstance;
  });

  it('creates successfully', () => {
    expect(component).toBeTruthy();
  });

  it('password mismatch shows validation error', () => {
    component.form.setValue({
      fullName: 'John',
      email: 'john@test.com',
      password: '123456',
      confirmPassword: '654321',
    });
    expect(component.form.errors).toEqual({ passwordMismatch: true });
  });

  it('register success navigates to login', () => {
    authService.register.mockReturnValue(of({}));
    component.form.setValue({
      fullName: 'John',
      email: 'john@test.com',
      password: '123456',
      confirmPassword: '123456',
    });
    component.submit();
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('register failure sets error signal', () => {
    authService.register.mockReturnValue(throwError(() => new Error('fail')));
    component.form.setValue({
      fullName: 'John',
      email: 'john@test.com',
      password: '123456',
      confirmPassword: '123456',
    });
    component.submit();
    expect(component.error()).toBe('Could not create account. Please try again.');
  });
});
