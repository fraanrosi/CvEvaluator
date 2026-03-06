import { Component, inject } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';

import { JobPositionsService } from '../../job-positions.service';

@Component({
  selector: 'app-create-job-position',
  standalone: true,
  imports: [ReactiveFormsModule],
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

  submit() {
    if (this.form.invalid) return;

    this.service.create(this.form.getRawValue())
      .subscribe(() => this.router.navigate(['/job-positions']));
  }
}