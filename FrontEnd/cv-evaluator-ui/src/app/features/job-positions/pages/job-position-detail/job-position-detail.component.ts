import { Component, signal, inject, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { Subscription } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';

import { JobPositionsService } from '../../job-positions.service';
import { CvEvaluationService } from '../../../../core/services/cv-evaluation.service';
import { SignalRService } from '../../../../core/services/signalr.service';
import { JobPositionDetail } from '../../../../core/models/job-position-detail.model';
import { SpinnerComponent } from '../../../../shared/components/spinner/spinner.component';
import { ToastService } from '../../../../shared/components/toast/toast.service';

@Component({
  selector: 'app-job-position-detail',
  standalone: true,
  imports: [DatePipe, RouterLink, SpinnerComponent],
  templateUrl: './job-position-detail.component.html',
  styleUrls: ['./job-position-detail.component.css']
})
export class JobPositionDetailComponent implements OnInit, OnDestroy {

  private route = inject(ActivatedRoute);
  private service = inject(JobPositionsService);
  private cvEvaluationService = inject(CvEvaluationService);
  private signalR = inject(SignalRService);
  private toast = inject(ToastService);
  private sub?: Subscription;

  job = signal<JobPositionDetail | null>(null);
  loading = signal(false);
  uploading = signal(false);
  uploadError = signal<string | null>(null);

  private jobId = this.route.snapshot.paramMap.get('id')!;

  ngOnInit() {
    this.load();
    this.signalR.startConnection();
    this.sub = this.signalR.evaluationUpdated$.subscribe(update => {
      const current = this.job();
      if (!current) return;

      const eval_ = current.evaluations.find(e => e.id === update.id);
      if (eval_) {
        eval_.score = update.overallScore;
        this.job.set({ ...current });
      }
    });
  }

  ngOnDestroy() {
    this.sub?.unsubscribe();
  }

  load() {
    this.loading.set(true);

    this.service.getById(this.jobId).subscribe({
      next: (res) => {
        this.job.set(res);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.uploading.set(true);
    this.uploadError.set(null);

    this.cvEvaluationService.uploadPdf(file, this.jobId).subscribe({
      next: () => {
        this.uploading.set(false);
        this.toast.success('PDF uploaded, evaluation in progress...');
        this.load();
      },
      error: (err: HttpErrorResponse) => {
        this.uploading.set(false);
        if (err.status === 402) {
          const msg = err.error?.error ?? 'You have reached the evaluation limit of your current plan.';
          this.uploadError.set(msg);
          this.toast.warning(msg);
        } else {
          this.uploadError.set('Upload failed. Please try again.');
          this.toast.error('Upload failed');
        }
      }
    });
  }
}
