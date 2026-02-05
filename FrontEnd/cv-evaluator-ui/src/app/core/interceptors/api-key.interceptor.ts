import { HttpInterceptorFn } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export const apiKeyInterceptor: HttpInterceptorFn = (req, next) => {

  if (!environment.production || !environment.apiKey) {
    return next(req);
  }

  const cloned = req.clone({
    setHeaders: {
      'X-API-KEY': environment.apiKey
    }
  });

  return next(cloned);
};