import { Component } from '@angular/core';
import { FileUploadComponent } from '../components/file-upload.component';
import { EvaluationResultComponent } from '../components/evaluation-result.component';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-evaluation-page',
  standalone: true,
  imports: [CommonModule, FileUploadComponent, EvaluationResultComponent],
  template: `
    <div class="min-h-screen bg-gray-100 p-8">
      <div class="max-w-3xl mx-auto space-y-6">
        <h1 class="text-3xl font-bold text-gray-800">
          CvEvaluator
        </h1>

        <app-file-upload
          (evaluationCompleted)="onEvaluationCompleted($event)">
        </app-file-upload>

        <app-evaluation-result
          *ngIf="result"
          [result]="result">
        </app-evaluation-result>
      </div>
    </div>
  `,
})
export class EvaluationPageComponent {
  result: any;

  onEvaluationCompleted(result: any) {
    this.result = result;
  }
}