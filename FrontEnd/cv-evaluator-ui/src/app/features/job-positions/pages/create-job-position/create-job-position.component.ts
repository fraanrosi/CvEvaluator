import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

import { JobPositionsService } from '../../job-positions.service';

@Component({
  selector: 'app-create-job-position',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './create-job-position.component.html',
  styleUrls: ['./create-job-position.component.css']
})
export class CreateJobPositionComponent {

  private fb = inject(FormBuilder);
  private service = inject(JobPositionsService);
  private router = inject(Router);

  form = this.fb.nonNullable.group({
    title: ['', Validators.required],
    description: ['', Validators.required]
  });

  errorMessage = signal<string | null>(null);

  submit() {
    if (this.form.invalid) return;
    this.errorMessage.set(null);

    this.service.create(this.form.getRawValue()).subscribe({
      next: () => this.router.navigate(['/job-positions']),
      error: (err: HttpErrorResponse) => {
        if (err.status === 402) {
          this.errorMessage.set(err.error?.error ?? 'You have reached the limit of your current plan.');
        } else {
          this.errorMessage.set('An unexpected error occurred. Please try again.');
        }
      }
    });
  }
}