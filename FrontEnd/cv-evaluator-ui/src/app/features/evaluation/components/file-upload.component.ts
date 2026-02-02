import { Component, EventEmitter, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EvaluationApiService } from '../../../core/services/evaluation-api.service';
@Component({
  selector: 'app-file-upload',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="bg-white p-6 rounded-2xl shadow">
      <h2 class="text-xl font-semibold mb-4">
        Subir CV (PDF)
      </h2>

      <input
        type="file"
        accept="application/pdf"
        multiple
        (change)="onFileSelected($event)"
        class="block w-full text-sm text-gray-600" />

      <button
        class="mt-4 px-4 py-2 bg-black text-white rounded-lg disabled:opacity-50"
        [disabled]="!files.length || loading"
        (click)="submit()">
        {{ loading ? 'Evaluando...' : 'Evaluar CV' }}
      </button>
    </div>
  `,
})
export class FileUploadComponent {
  @Output() evaluationCompleted = new EventEmitter<any>();

  files: File[] = [];
  loading = false;

  constructor(private api: EvaluationApiService) {}

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    this.files = input.files ? Array.from(input.files) : [];
  }

  submit() {
    if (!this.files.length) return;

    this.loading = true;

    this.api.evaluate(this.files).subscribe({
      next: (results) => {
        this.evaluationCompleted.emit(results);
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        alert('Error evaluando CV');
      },
    });
  }
}