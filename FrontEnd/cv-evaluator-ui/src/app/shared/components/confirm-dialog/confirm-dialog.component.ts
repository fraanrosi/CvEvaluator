import { Component, inject } from '@angular/core';
import { ConfirmDialogService } from './confirm-dialog.service';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  template: `
    @if (dialog.visible()) {
      <div class="fixed inset-0 z-50 flex items-center justify-center">
        <div class="absolute inset-0 bg-black/40" (click)="dialog.cancel()"></div>
        <div class="relative bg-white rounded-xl shadow-xl p-6 max-w-sm w-full mx-4">
          <p class="text-gray-800 text-base mb-6">{{ dialog.request()?.message }}</p>
          <div class="flex justify-end gap-3">
            <button
              (click)="dialog.cancel()"
              class="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-lg transition-colors">
              {{ dialog.request()?.cancelText }}
            </button>
            <button
              (click)="dialog.accept()"
              class="px-4 py-2 text-sm font-medium text-white bg-red-600 hover:bg-red-700 rounded-lg transition-colors">
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
