import { Component, inject } from '@angular/core';
import { ToastService } from './toast.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  template: `
    <div class="fixed top-4 right-4 z-50 flex flex-col gap-2 max-w-sm">
      @for (toast of toastService.toasts(); track toast.id) {
        <div
          class="flex items-center gap-3 px-4 py-3 rounded-lg shadow-lg text-sm font-medium animate-slide-in-right backdrop-blur-sm cursor-pointer"
          [class]="typeClasses[toast.type]"
          (click)="toastService.dismiss(toast.id)"
        >
          <span class="text-base">{{ icons[toast.type] }}</span>
          <span class="flex-1">{{ toast.message }}</span>
          <button class="opacity-60 hover:opacity-100 text-lg leading-none">&times;</button>
        </div>
      }
    </div>
  `
})
export class ToastComponent {
  toastService = inject(ToastService);

  typeClasses: Record<string, string> = {
    success: 'bg-emerald-500/90 text-white border border-emerald-400/30',
    error: 'bg-rose-500/90 text-white border border-rose-400/30',
    warning: 'bg-amber-500/90 text-white border border-amber-400/30',
    info: 'bg-sky-500/90 text-white border border-sky-400/30'
  };

  icons: Record<string, string> = {
    success: '\u2713',
    error: '\u2717',
    warning: '\u26A0',
    info: '\u2139'
  };
}
