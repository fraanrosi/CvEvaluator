import { Component, viewChild } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { TopHeaderComponent } from '../top-header/top-header.component';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, SidebarComponent, TopHeaderComponent],
  template: `
    <app-sidebar />
    <div [class]="'transition-all duration-300 min-h-screen bg-surface-950 '
      + (sidebar()?.collapsed() ? 'lg:ml-16' : 'lg:ml-64')">
      <app-top-header (toggleSidebar)="sidebar()?.openMobile()" />
      <main class="p-6">
        <router-outlet />
      </main>
    </div>
  `
})
export class AppShellComponent {
  sidebar = viewChild(SidebarComponent);
}
