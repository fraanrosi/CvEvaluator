import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class SessionService {

  private readonly TOKEN_KEY = 'cv_token';
  private readonly EMAIL_KEY = 'cv_email';

  setSession(token: string, email: string) {
    sessionStorage.setItem(this.TOKEN_KEY, token);
    sessionStorage.setItem(this.EMAIL_KEY, email);
  }

  clearSession() {
    sessionStorage.removeItem(this.TOKEN_KEY);
    sessionStorage.removeItem(this.EMAIL_KEY);
  }

  getToken(): string | null {
    return sessionStorage.getItem(this.TOKEN_KEY);
  }

  getEmail(): string | null {
    return sessionStorage.getItem(this.EMAIL_KEY);
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }
}