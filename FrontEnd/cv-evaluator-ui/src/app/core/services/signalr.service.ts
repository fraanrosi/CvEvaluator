import * as signalR from '@microsoft/signalr';
import { Injectable, inject } from '@angular/core';
import { Subject } from 'rxjs';
import { AuthService } from '../../features/auth/auth.service';
import { environment } from '../../../environments/environment';

export interface EvaluationUpdate {
  id: string;
  status: string;
  overallScore: number | null;
  errorMessage: string | null;
}

@Injectable({ providedIn: 'root' })
export class SignalRService {
  private authService = inject(AuthService);
  private hubConnection: signalR.HubConnection | null = null;

  evaluationUpdated$ = new Subject<EvaluationUpdate>();

  startConnection() {
    if (this.hubConnection) return;

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.hubBaseUrl}/hubs/evaluations`, {
        accessTokenFactory: () => this.authService.getToken() ?? ''
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('EvaluationUpdated', (data: EvaluationUpdate) => {
      this.evaluationUpdated$.next(data);
    });

    this.hubConnection
      .start()
      .then(() => console.log('SignalR connected'))
      .catch(err => console.error('SignalR error:', err));
  }

  stopConnection() {
    this.hubConnection?.stop();
    this.hubConnection = null;
  }
}
