import { Component, signal, inject } from '@angular/core';
import { Router } from '@angular/router';
import { DatePipe } from '@angular/common';

import { JobPositionsService } from '../../job-positions.service';
import { JobPosition } from '../../../../core/models/job-position.model';
import { SpinnerComponent } from '../../../../shared/components/spinner/spinner.component';

@Component({
  selector: 'app-job-positions-page',
  standalone: true,
  imports: [DatePipe, SpinnerComponent],
  templateUrl: './job-positions-page.component.html',
  styleUrls: ['./job-positions-page.component.css']
})
export class JobPositionsPageComponent {

  private service = inject(JobPositionsService);
  private router = inject(Router);

  jobPositions = signal<JobPosition[]>([]);
  loading = signal(false);

  constructor() {
    this.load();
  }

  load() {
    this.loading.set(true);

    this.service.getAll().subscribe({
      next: (res) => {
        this.jobPositions.set(res);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  open(job: JobPosition) {
    this.router.navigate(['/job-positions', job.id]);
  }

  create() {
    this.router.navigate(['/job-positions/create']);
  }
}