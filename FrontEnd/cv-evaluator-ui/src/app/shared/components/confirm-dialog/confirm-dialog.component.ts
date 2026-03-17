import { Component, inject } from '@angular/core';
import { ConfirmDialogService } from './confirm-dialog.service';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  template: `
    @if (dialog.visible()) {
      <div class="fixed inset-0 z-50 flex items-center justify-center animate-fade-in">
        <div class="absolute inset-0 bg-black/60 backdrop-blur-sm" (click)="dialog.cancel()"></div>
        <div class="relative card p-6 max-w-sm w-full mx-4 animate-fade-in-up">
          <p class="text-zinc-200 text-base mb-6">{{ dialog.request()?.message }}</p>
          <div class="flex justify-end gap-3">
            <button
              (click)="dialog.cancel()"
              class="btn-secondary text-sm">
              {{ dialog.request()?.cancelText }}
            </button>
            <button
              (click)="dialog.accept()"
              class="bg-rose-600 hover:bg-rose-700 text-white text-sm font-medium px-4 py-2 rounded-lg transition-colors">
              {{ dialog.request()?.confirmText }}
            </button>
          </div>
        </div>
      </div>
    }
  `
})
export class ConfirmDialogComponent {
  dialog = inject(ConfirmDialogService);
}
