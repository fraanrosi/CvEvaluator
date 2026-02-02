import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-evaluation-result',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="bg-white p-6 rounded-2xl shadow">
      <h2 class="text-xl font-semibold mb-4">
        Resultado
      </h2>

      <pre class="bg-gray-100 p-4 rounded text-sm overflow-auto">
{{ result | json }}
      </pre>
    </div>
  `,
})
export class EvaluationResultComponent {
  @Input() result: any;
}