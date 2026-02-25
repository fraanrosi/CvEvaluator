import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { EvaluationResult } from '../../core/models/evaluation-result.model';

@Injectable({
  providedIn: 'root'
})
export class EvaluationsService {

  private http = inject(HttpClient);
  private baseUrl = `${environment.apiBaseUrl}/cvEvaluations`;

  uploadPdf(files: File[]): Observable<{ id: string }> {
    const formData = new FormData();
    files.forEach(file => formData.append('files', file));
    return this.http.post<{ id: string }>(
      `${this.baseUrl}/evaluate-pdf`,
      formData
    );
  }

  getById(id: string): Observable<EvaluationResult> {
    return this.http.get<EvaluationResult>(`${this.baseUrl}/${id}`);
  }

  getAll(): Observable<EvaluationResult[]> {
    return this.http.get<EvaluationResult[]>(this.baseUrl);
  }
}