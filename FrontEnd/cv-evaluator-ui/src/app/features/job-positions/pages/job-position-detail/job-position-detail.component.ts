import { Component, signal, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';

import { JobPositionsService } from '../../job-positions.service';
import { CvEvaluationService } from '../../../../core/services/cv-evaluation.service';
import { JobPositionDetail } from '../../../../core/models/job-position-detail.model';

@Component({
  selector: 'app-job-position-detail',
  standalone: true,
  imports: [DatePipe, RouterLink],
  templateUrl: './job-position-detail.component.html',
  styleUrls: ['./job-position-detail.component.css']
})
export class JobPositionDetailComponent {

  private route = inject(ActivatedRoute);
  private service = inject(JobPositionsService);
  private cvEvaluationService = inject(CvEvaluationService);

  job = signal<JobPositionDetail | null>(null);
  loading = signal(false);
  uploading = signal(false);

  private jobId = this.route.snapshot.paramMap.get('id')!;

  constructor() {
    this.load();
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
    this.cvEvaluationService.uploadPdf(file, this.jobId).subscribe({
      next: () => this.load(),
      error: () => this.uploading.set(false),
      complete: () => this.uploading.set(false)
    });
  }
}
