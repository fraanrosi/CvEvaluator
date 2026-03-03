import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { EvaluationsService } from './evaluations.service';
import { EvaluationResult } from '../../core/models/evaluation-result.model';
import { Subscription } from 'rxjs';
import { CommonModule } from '@angular/common';
import { SignalRService } from '../../core/services/signalr.service';
import { AuthService } from '../auth/auth.service';
import { ChangeDetectorRef } from '@angular/core';
import { NgZone } from '@angular/core';

@Component({
  selector: 'app-evaluations-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './evaluations-page.component.html'
})
export class EvaluationsPageComponent implements OnInit, OnDestroy {

  private service = inject(EvaluationsService);
  private signalRService = inject(SignalRService);
  private authService = inject(AuthService);
  private cdr = inject(ChangeDetectorRef);
  private zone = inject(NgZone);

  evaluations: EvaluationResult[] = [];
  loading = false;
  selectedFiles: File[] = [];

  private pollingSub?: Subscription;

ngOnInit(): void {
  // 1️⃣ Carga inicial por HTTP
  this.loadEvaluations();

    // 2️⃣ Conexión SignalR si hay token
    if (this.authService.getToken()) {
      this.signalRService.startConnection();

      // 3️⃣ Listener realtime
      this.signalRService.onEvaluationUpdated((data) => {

      console.log("SignalR update:", data);

      this.zone.run(() => {

      const index = this.evaluations.findIndex(e => e.id === data.id);

      if (index !== -1) {

        const updatedEvaluation = {
          ...this.evaluations[index],
          status: data.status,
          overallScore: data.overallScore
        };

        this.evaluations = [
          ...this.evaluations.slice(0, index),
          updatedEvaluation,
          ...this.evaluations.slice(index + 1)
        ];
      }

    });

  });
    }
  }
  
  ngOnDestroy() {
    this.pollingSub?.unsubscribe();
    //this.signalRService.stopConnection();
  }

loadEvaluations() {
  this.service.getAll().subscribe(res => {
    console.log("Evaluations recibidas:", res);
    this.evaluations = res;
    this.cdr.detectChanges(); // 👈 esto es lo que fuerza el render
    this.signalRService.onEvaluationUpdated((data) => {
  console.log("SignalR update:", data);
});
  },
  err => {
    console.error("Error cargando evaluations:", err);
  });
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