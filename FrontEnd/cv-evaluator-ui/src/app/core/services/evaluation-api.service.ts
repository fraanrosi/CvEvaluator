import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { EvaluationResult } from '../models/evaluation-result.model';

@Injectable({
  providedIn: 'root',
})
export class EvaluationApiService {
  private readonly baseUrl = 'https://localhost:7184/api';

  constructor(private http: HttpClient) {}

  evaluate(files: File[]): Observable<EvaluationResult[]> {
    const formData = new FormData();

    files.forEach(file => {
      formData.append('files', file);
    });

    return this.http.post<EvaluationResult[]>(
      this.baseUrl + '/cv/evaluate-pdf',
      formData
    );
  }
}