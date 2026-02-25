import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { SessionService } from '../services/session.service';

export const httpErrorInterceptor: HttpInterceptorFn = (req, next) => {

  const router = inject(Router);
  const session = inject(SessionService);

  return next(req).pipe(
    catchError(error => {
      if (error.status === 401) {
        session.clearSession();
        router.navigate(['/login']);
      }
      return throwError(() => error);
    })
  );
};