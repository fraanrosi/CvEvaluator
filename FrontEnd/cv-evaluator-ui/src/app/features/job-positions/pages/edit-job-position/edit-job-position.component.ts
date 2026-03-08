import { Component, inject } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

import { JobPositionsService } from '../../job-positions.service';

@Component({
  selector: 'app-edit-job-position',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './edit-job-position.component.html',
  styleUrls: ['./edit-job-position.component.css']
})
export class EditJobPositionComponent {

  private fb = inject(FormBuilder);
  private service = inject(JobPositionsService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  private jobId = this.route.snapshot.paramMap.get('id')!;

  form = this.fb.nonNullable.group({
    title: ['', Validators.required],
    description: ['', Validators.required]
  });

  constructor() {
    this.service.getById(this.jobId).subscribe(job => {
      this.form.patchValue({
        title: job.title,
        description: job.description
      });
    });
  }

  submit() {
    if (this.form.invalid) return;

    this.service.update(this.jobId, this.form.getRawValue())
      .subscribe(() => this.router.navigate(['/job-positions']));
  }
}