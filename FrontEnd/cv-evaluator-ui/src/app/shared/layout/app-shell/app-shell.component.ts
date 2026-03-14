import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from '../navbar/navbar.component';
@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, NavbarComponent],
  template: `
    <app-navbar></app-navbar>

    <main class="min-h-screen bg-gray-50">
      <router-outlet></router-outlet>
    </main>
  `
})
export class AppShellComponent {}