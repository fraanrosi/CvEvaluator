import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-spinner',
  standalone: true,
  template: `
    <div class="flex flex-col items-center justify-center py-12 gap-3">
      <svg
        class="animate-spin text-blue-600"
        [attr.width]="size"
        [attr.height]="size"
        viewBox="0 0 24 24"
        fill="none"
      >
        <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
        <path class="opacity-75" fill="currentColor"
          d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z">
        </path>
      </svg>
      @if (message) {
        <p class="text-sm text-gray-500">{{ message }}</p>
      }
    </div>
  `
})
export class SpinnerComponent {
  @Input() size = 32;
  @Input() message = '';
}
