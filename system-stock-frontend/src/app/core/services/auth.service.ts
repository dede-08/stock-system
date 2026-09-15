import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { LoginResponse, RegisterRequest } from '../models/auth.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/auth`;

  constructor(private http: HttpClient) {}

  login(email: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, { email, password }).pipe(
      tap((res: LoginResponse) => {
        sessionStorage.setItem('token', res.token);
        if (res.refreshToken) sessionStorage.setItem('refresh_token', res.refreshToken);
      })
    );
  }

  refresh(): Observable<LoginResponse> {
    const refreshToken = sessionStorage.getItem('refresh_token');
    return this.http.post<LoginResponse>(`${this.apiUrl}/refresh`, { refreshToken }).pipe(
      tap((res: LoginResponse) => {
        sessionStorage.setItem('token', res.token);
        if (res.refreshToken) sessionStorage.setItem('refresh_token', res.refreshToken);
      })
    );
  }

  register(user: RegisterRequest): Observable<object> {
    return this.http.post(`${this.apiUrl}/add-user`, user);
  }

  logout() {
    const refreshToken = sessionStorage.getItem('refresh_token');
    if (refreshToken) {
      this.http.post(`${this.apiUrl}/logout`, { refreshToken }).subscribe({ error: () => undefined });
    }
    sessionStorage.removeItem('token');
    sessionStorage.removeItem('refresh_token');
  }

  getToken(): string | null {
    return sessionStorage.getItem('token');
  }

  getRefreshToken(): string | null {
    return sessionStorage.getItem('refresh_token');
  }

  isAuthenticated(): boolean {
    const token = this.getToken();
    if (!token) return false;
    // Si el JWT trae exp, respétala; si no se puede leer, conserva compat (true).
    const parts = token.split('.');
    if (parts.length !== 3) return true;
    try {
      const payload = JSON.parse(atob(parts[1].replace(/-/g, '+').replace(/_/g, '/')));
      if (typeof payload.exp !== 'number') return true;
      return Date.now() < payload.exp * 1000 - 5000; // 5s de margen
    } catch {
      return true;
    }
  }
}