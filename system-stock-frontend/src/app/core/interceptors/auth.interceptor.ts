import { Injectable } from '@angular/core';
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Observable, catchError, finalize, shareReplay, switchMap, throwError } from 'rxjs';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { LoginResponse } from '../models/auth.model';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {

  private refreshInFlight$: Observable<LoginResponse> | null = null;

  constructor(private authService: AuthService, private router: Router) { }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = this.authService.getToken();
    const authReq = token && !this.isAuthUrl(req.url)
      ? req.clone({ headers: req.headers.set('Authorization', `Bearer ${token}`) })
      : req;

    return next.handle(authReq).pipe(
      catchError(err => {
        if (err?.status !== 401 || this.isAuthUrl(req.url) || req.headers.has('X-Retry')) {
          return throwError(() => err);
        }
        return this.handle401(authReq, next);
      })
    );
  }

  private handle401(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (!this.authService.getRefreshToken()) {
      this.forceLogout();
      return throwError(() => new Error('No refresh token'));
    }
    if (!this.refreshInFlight$) {
      this.refreshInFlight$ = this.authService.refresh().pipe(
        finalize(() => (this.refreshInFlight$ = null)),
        shareReplay(1)
      );
    }
    return this.refreshInFlight$.pipe(
      switchMap(res => next.handle(
        req.clone({ headers: req.headers.set('Authorization', `Bearer ${res.token}`).set('X-Retry', '1') })
      )),
      catchError(err => {
        this.forceLogout();
        return throwError(() => err);
      })
    );
  }

  private forceLogout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  private isAuthUrl(url: string): boolean {
    return /\/auth\/(login|add-user|refresh|logout)/.test(url);
  }
}
