import { Component, inject } from '@angular/core';
import { ToastService } from './toast.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  template: `
    <div class="fixed top-4 right-4 z-50 flex flex-col gap-2 max-w-sm">
      @for (toast of toastService.toasts(); track toast.id) {
        <div
          class="flex items-center gap-3 px-4 py-3 rounded-lg shadow-lg text-sm font-medium animate-slide-in"
          [class]="typeClasses[toast.type]"
          (click)="toastService.dismiss(toast.id)"
        >
          <span class="text-base">{{ icons[toast.type] }}</span>
          <span class="flex-1">{{ toast.message }}</span>
          <button class="opacity-60 hover:opacity-100 text-lg leading-none">&times;</button>
        </div>
      }
    </div>
  `,
  styles: [`
    @keyframes slideIn {
      from { transform: translateX(100%); opacity: 0; }
      to { transform: translateX(0); opacity: 1; }
    }
    .animate-slide-in {
      animation: slideIn 0.3s ease-out;
    }
  `]
})
export class ToastComponent {
  toastService = inject(ToastService);

  typeClasses: Record<string, string> = {
    success: 'bg-green-600 text-white',
    error: 'bg-red-600 text-white',
    warning: 'bg-yellow-500 text-white',
    info: 'bg-blue-600 text-white'
  };

  icons: Record<string, string> = {
    success: '\u2713',
    error: '\u2717',
    warning: '\u26A0',
    info: '\u2139'
  };
}
