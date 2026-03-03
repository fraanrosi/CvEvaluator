import * as signalR from '@microsoft/signalr';
import { Injectable } from '@angular/core';
import { AuthService } from '../../features/auth/auth.service';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class SignalRService {

  private hubConnection!: signalR.HubConnection;

  constructor(private authService: AuthService) {}

  public startConnection() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.hubBaseUrl}/hubs/evaluations`, {
        accessTokenFactory: () => this.authService.getToken() ?? ''
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(() => console.log('SignalR conectado 🔥'))
      .catch(err => console.error(err));
  }

  public onEvaluationUpdated(callback: (data: any) => void) {
    this.hubConnection.on('EvaluationUpdated', (data) => {
        callback(data);
    });
  }
}