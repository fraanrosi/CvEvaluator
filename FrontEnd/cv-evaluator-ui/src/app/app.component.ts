import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from './shared/layout/navbar.component';

@Component({
  standalone: true,
  selector: 'app-root',
  imports: [RouterOutlet, NavbarComponent],
  template: `
    <app-navbar></app-navbar>
    <router-outlet></router-outlet>
  `
})
export class AppComponent {}