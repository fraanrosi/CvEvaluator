import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { EvaluationResult } from '../models/evaluation-result.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class EvaluationApiService {

constructor(private http: HttpClient) {
}

  evaluate(files: File[]): Observable<EvaluationResult[]> {
    const formData = new FormData();

    files.forEach(file => {
      formData.append('files', file);
    });
    
    return this.http.post<EvaluationResult[]>(`${environment.apiBaseUrl}/cv/evaluate-pdf`, formData);
  }
}
