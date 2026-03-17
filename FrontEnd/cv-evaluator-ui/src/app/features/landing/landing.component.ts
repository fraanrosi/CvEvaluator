import { Component, inject, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { SessionService } from '../../core/services/session.service';

@Component({
  standalone: true,
  selector: 'app-landing',
  imports: [RouterLink],
  templateUrl: './landing.component.html'
})
export class LandingComponent implements OnInit {

  private router = inject(Router);
  private session = inject(SessionService);

  currentYear = new Date().getFullYear();

  ngOnInit(): void {
    if (this.session.isAuthenticated()) {
      this.router.navigate(['/dashboard']);
    }
  }
}
