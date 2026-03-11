import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { EvaluationResult } from '../models/evaluation-result.model';

@Injectable({ providedIn: 'root' })
export class CvEvaluationService {
  private http = inject(HttpClient);
  private base = `${environment.apiBaseUrl}/cvEvaluations`;

  uploadPdf(file: File, jobPositionId: string): Observable<{ id: string }[]> {
    const form = new FormData();
    form.append('files', file);
    form.append('jobPositionId', jobPositionId);
    return this.http.post<{ id: string }[]>(`${this.base}/evaluate-pdf`, form);
  }

  getById(id: string): Observable<EvaluationResult> {
    return this.http.get<EvaluationResult>(`${this.base}/${id}`);
  }
}
