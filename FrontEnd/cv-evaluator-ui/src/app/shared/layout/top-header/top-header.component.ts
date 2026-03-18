import { Component, inject, output } from '@angular/core';
import { SessionService } from '../../../core/services/session.service';

@Component({
  selector: 'app-top-header',
  standalone: true,
  template: `
    <header class="sticky top-0 z-30 h-16 bg-surface-900/80 backdrop-blur-md border-b border-border flex items-center justify-between px-6">

      <!-- Left: hamburger (mobile) + search -->
      <div class="flex items-center gap-4">
        <button (click)="toggleSidebar.emit()"
                class="lg:hidden text-muted hover:text-zinc-200 transition-colors">
          <svg xmlns="http://www.w3.org/2000/svg" class="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M3.75 6.75h16.5M3.75 12h16.5m-16.5 5.25h16.5" />
          </svg>
        </button>

        <div class="relative hidden sm:block">
          <svg xmlns="http://www.w3.org/2000/svg" class="w-4 h-4 text-muted absolute left-3 top-1/2 -translate-y-1/2" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M21 21l-5.197-5.197m0 0A7.5 7.5 0 105.196 5.196a7.5 7.5 0 0010.607 10.607z" />
          </svg>
          <input type="text" placeholder="Search..."
                 class="bg-surface-800 border border-border rounded-lg pl-9 pr-4 py-2 text-sm text-zinc-300 placeholder-muted/50 w-64 focus:outline-none focus:border-accent/50 transition-colors" />
        </div>
      </div>

      <!-- Right: user avatar -->
      <div class="flex items-center gap-4">
        <div class="flex items-center gap-3">
          <div class="w-8 h-8 rounded-full bg-accent/20 text-accent flex items-center justify-center text-sm font-semibold">
            {{ initials }}
          </div>
          <span class="text-sm text-zinc-300 hidden md:block">{{ email }}</span>
        </div>
      </div>
    </header>
  `
})
export class TopHeaderComponent {
  toggleSidebar = output();

  private session = inject(SessionService);

  get email() { return this.session.getEmail() ?? ''; }
  get initials() { return this.email.charAt(0).toUpperCase(); }
}
