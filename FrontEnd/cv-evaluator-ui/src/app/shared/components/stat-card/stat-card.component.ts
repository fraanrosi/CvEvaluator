import { Component, input } from '@angular/core';

@Component({
  selector: 'app-stat-card',
  standalone: true,
  template: `
    <div class="stat-card">
      <span class="text-xs text-muted uppercase tracking-wider font-medium">{{ title() }}</span>
      <div class="flex items-end gap-2">
        <span class="text-2xl font-heading font-bold text-zinc-100">{{ value() }}</span>
        @if (trendPercent() !== null) {
          <span [class]="'text-xs font-medium mb-0.5 ' + (trendPercent()! >= 0 ? 'text-emerald-400' : 'text-rose-400')">
            {{ trendPercent()! >= 0 ? '+' : '' }}{{ trendPercent() }}%
          </span>
        }
      </div>
      @if (subtitle()) {
        <span class="text-xs text-muted">{{ subtitle() }}</span>
      }
    </div>
  `
})
export class StatCardComponent {
  title = input.required<string>();
  value = input.required<string | number>();
  subtitle = input<string>();
  trendPercent = input<number | null>(null);
}
