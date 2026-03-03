import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, tap } from 'rxjs';
import { SessionService } from '../../core/services/session.service';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private http = inject(HttpClient);
  private session = inject(SessionService);

  private loggedIn$ = new BehaviorSubject<boolean>(this.session.isAuthenticated());

  isLoggedIn$ = this.loggedIn$.asObservable();

  login(email: string, password: string) {
    return this.http.post<any>(`${environment.apiBaseUrl}/auth/login`, { email, password })
      .pipe(
        tap(response => {
          this.session.setSession(response.token, response.email);
          this.loggedIn$.next(true);
        })
      );
  }

  register(email: string, password: string, fullName: string) {
    return this.http.post(`${environment.apiBaseUrl}/auth/register`, {
      email,
      password,
      fullName
    });
  }
  getToken(): string | null {
    return this.session.getToken();
  }

  logout() {
    this.session.clearSession();
    this.loggedIn$.next(false);
  }
}