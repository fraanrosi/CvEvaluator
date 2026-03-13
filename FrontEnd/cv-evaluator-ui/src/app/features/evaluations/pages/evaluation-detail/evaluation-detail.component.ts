import { Component, signal, inject, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';

import { CvEvaluationService } from '../../../../core/services/cv-evaluation.service';
import { EvaluationResult, isCompleted, isProcessing, isFailed } from '../../../../core/models/evaluation-result.model';
import { SpinnerComponent } from '../../../../shared/components/spinner/spinner.component';

@Component({
  selector: 'app-evaluation-detail',
  standalone: true,
  imports: [DatePipe, RouterLink, SpinnerComponent],
  templateUrl: './evaluation-detail.component.html'
})
export class EvaluationDetailComponent implements OnInit {

  private route = inject(ActivatedRoute);
  private service = inject(CvEvaluationService);

  evaluation = signal<EvaluationResult | null>(null);
  loading = signal(false);

  isCompleted = isCompleted;
  isProcessing = isProcessing;
  isFailed = isFailed;

  private evalId = this.route.snapshot.paramMap.get('id')!;

  ngOnInit() {
    this.loading.set(true);
    this.service.getById(this.evalId).subscribe({
      next: (res) => {
        this.evaluation.set(res);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }
}
