import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { SessionService } from '../../../core/services/session.service';
import { AuthService } from '../../../features/auth/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './navbar.component.html'
})
export class NavbarComponent {

  private session = inject(SessionService);
  private auth = inject(AuthService);
  private router = inject(Router);

  get email() { return this.session.getEmail() ?? ''; }

  logout() {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
