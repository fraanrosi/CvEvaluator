import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

import { JobPosition } from '../../core/models/job-position.model';
import { JobPositionDetail } from '../../core/models/job-position-detail.model';

@Injectable({ providedIn: 'root' })
export class JobPositionsService {

  private http = inject(HttpClient);
  private baseUrl = `${environment.apiBaseUrl}/jobpositions`;

  getAll() {
    return this.http.get<JobPosition[]>(this.baseUrl);
  }

  getById(id: string) {
    return this.http.get<JobPositionDetail>(`${this.baseUrl}/${id}`);
  }

  create(data: { title: string; description: string }) {
    return this.http.post(this.baseUrl, data);
  }

  update(id: string, data: { title: string; description: string }) {
    return this.http.put(`${this.baseUrl}/${id}`, data);
  }

  delete(id: string) {
    return this.http.delete(`${this.baseUrl}/${id}`);
  }
}