import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { EvaluationsService } from './evaluations.service';
import { EvaluationResult } from '../../core/models/evaluation-result.model';
import { interval, Subscription } from 'rxjs';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-evaluations-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './evaluations-page.component.html'
})
export class EvaluationsPageComponent implements OnInit, OnDestroy {

  private service = inject(EvaluationsService);

  evaluations: EvaluationResult[] = [];
  loading = false;
  selectedFiles: File[] = [];

  private pollingSub?: Subscription;

  ngOnInit() {
    this.loadEvaluations();
  }

  ngOnDestroy() {
    this.pollingSub?.unsubscribe();
  }

  loadEvaluations() {
    this.service.getAll().subscribe(res => {
      this.evaluations = res;
      this.startPollingIfNeeded();
    });
  }

  startPollingIfNeeded() {
    const hasProcessing = this.evaluations.some(e => e.status === 'Processing');

    if (hasProcessing && !this.pollingSub) {
      this.pollingSub = interval(3000).subscribe(() => {
        this.service.getAll().subscribe(res => {
          this.evaluations = res;

          const stillProcessing = this.evaluations.some(e => e.status === 'Processing');
          if (!stillProcessing) {
            this.pollingSub?.unsubscribe();
            this.pollingSub = undefined;
          }
        });
      });
    }
  }

  onFilesSelected(event: any) {
    this.selectedFiles = Array.from(event.target.files);
  }

  upload() {
    if (!this.selectedFiles.length) return;

    this.loading = true;

    this.service.uploadPdf(this.selectedFiles).subscribe({
      next: () => {
        this.selectedFiles = [];
        this.loading = false;
        this.loadEvaluations();
      },
      error: () => {
        this.loading = false;
      }
    });
  }
}