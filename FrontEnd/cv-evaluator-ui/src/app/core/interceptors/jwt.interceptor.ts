import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { SessionService } from '../services/session.service';

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {

  const session = inject(SessionService);
  const token = session.getToken();

  if (token) {
    const cloned = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });

    return next(cloned);
  }

  return next(req);
};