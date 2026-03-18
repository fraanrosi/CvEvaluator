import { Component, inject, signal, output } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { SessionService } from '../../../core/services/session.service';
import { AuthService } from '../../../features/auth/auth.service';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './sidebar.component.html'
})
export class SidebarComponent {
  private session = inject(SessionService);
  private auth = inject(AuthService);
  private router = inject(Router);

  collapsed = signal(false);
  mobileOpen = signal(false);

  get email() { return this.session.getEmail() ?? ''; }
  get initials() { return this.email.charAt(0).toUpperCase(); }

  toggleCollapse() {
    this.collapsed.update(v => !v);
  }

  openMobile() {
    this.mobileOpen.set(true);
  }

  closeMobile() {
    this.mobileOpen.set(false);
  }

  logout() {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
