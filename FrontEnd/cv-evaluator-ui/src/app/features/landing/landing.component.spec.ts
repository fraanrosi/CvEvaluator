import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { LandingComponent } from './landing.component';
import { SessionService } from '../../core/services/session.service';

describe('LandingComponent', () => {
  describe('when not authenticated', () => {
    beforeEach(async () => {
      await TestBed.configureTestingModule({
        imports: [LandingComponent],
        providers: [
          provideRouter([
            { path: '', component: LandingComponent },
            { path: 'dashboard', component: LandingComponent },
          ]),
          {
            provide: SessionService,
            useValue: { isAuthenticated: () => false },
          },
        ],
      }).compileComponents();
    });

    it('should create the component', () => {
      const fixture = TestBed.createComponent(LandingComponent);
      fixture.detectChanges();
      expect(fixture.componentInstance).toBeTruthy();
    });

    it('should have currentYear set', () => {
      const fixture = TestBed.createComponent(LandingComponent);
      fixture.detectChanges();
      expect(fixture.componentInstance.currentYear).toBe(new Date().getFullYear());
    });

    it('should not redirect', () => {
      const router = TestBed.inject(Router);
      const navigateSpy = vi.spyOn(router, 'navigate');
      const fixture = TestBed.createComponent(LandingComponent);
      fixture.detectChanges();
      expect(navigateSpy).not.toHaveBeenCalled();
    });

    it('should render the hero section', () => {
      const fixture = TestBed.createComponent(LandingComponent);
      fixture.detectChanges();
      const el = fixture.nativeElement as HTMLElement;
      expect(el.querySelector('h1')?.textContent).toContain('Evaluate resumes');
      expect(el.querySelector('h1')?.textContent).toContain('smarter and faster');
    });

    it('should render brand name in navbar', () => {
      const fixture = TestBed.createComponent(LandingComponent);
      fixture.detectChanges();
      const el = fixture.nativeElement as HTMLElement;
      const nav = el.querySelector('nav');
      expect(nav?.textContent).toContain('CvEvaluator');
    });

    it('should render sign in and get started links', () => {
      const fixture = TestBed.createComponent(LandingComponent);
      fixture.detectChanges();
      const el = fixture.nativeElement as HTMLElement;
      expect(el.querySelector('a[href="/login"]')).toBeTruthy();
      expect(el.querySelector('a[href="/register"]')).toBeTruthy();
    });

    it('should render all four feature cards', () => {
      const fixture = TestBed.createComponent(LandingComponent);
      fixture.detectChanges();
      const el = fixture.nativeElement as HTMLElement;
      const section = el.querySelector('#features');
      const cards = section?.querySelectorAll('.group.rounded-2xl') ?? [];
      expect(cards.length).toBe(4);
    });

    it('should render three steps', () => {
      const fixture = TestBed.createComponent(LandingComponent);
      fixture.detectChanges();
      const el = fixture.nativeElement as HTMLElement;
      expect(el.textContent).toContain('Define your position');
      expect(el.textContent).toContain('Upload CVs');
      expect(el.textContent).toContain('Get AI evaluations');
    });

    it('should render footer with copyright', () => {
      const fixture = TestBed.createComponent(LandingComponent);
      fixture.detectChanges();
      const el = fixture.nativeElement as HTMLElement;
      const footer = el.querySelector('footer');
      expect(footer?.textContent).toContain(`${new Date().getFullYear()}`);
      expect(footer?.textContent).toContain('CvEvaluator');
    });
  });

  describe('when authenticated', () => {
    beforeEach(async () => {
      await TestBed.configureTestingModule({
        imports: [LandingComponent],
        providers: [
          provideRouter([
            { path: '', component: LandingComponent },
            { path: 'dashboard', component: LandingComponent },
          ]),
          {
            provide: SessionService,
            useValue: { isAuthenticated: () => true },
          },
        ],
      }).compileComponents();
    });

    it('should redirect to dashboard', () => {
      const router = TestBed.inject(Router);
      const navigateSpy = vi.spyOn(router, 'navigate');
      const fixture = TestBed.createComponent(LandingComponent);
      fixture.detectChanges();
      expect(navigateSpy).toHaveBeenCalledWith(['/dashboard']);
    });
  });
});
