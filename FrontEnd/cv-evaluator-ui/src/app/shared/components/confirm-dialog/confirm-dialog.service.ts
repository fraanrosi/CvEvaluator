import { Injectable, signal } from '@angular/core';
import { Subject } from 'rxjs';

export interface ConfirmRequest {
  message: string;
  confirmText?: string;
  cancelText?: string;
}

@Injectable({ providedIn: 'root' })
export class ConfirmDialogService {
  visible = signal(false);
  request = signal<ConfirmRequest | null>(null);

  private result$ = new Subject<boolean>();

  confirm(message: string, confirmText = 'Confirm', cancelText = 'Cancel') {
    this.request.set({ message, confirmText, cancelText });
    this.visible.set(true);
    return this.result$.asObservable();
  }

  accept() {
    this.visible.set(false);
    this.result$.next(true);
  }

  cancel() {
    this.visible.set(false);
    this.result$.next(false);
  }
}
