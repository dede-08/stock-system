import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AuthService } from './auth.service';
import { environment } from '../../../environments/environment';

function unsignedJwt(expOffsetSec: number): string {
  const payload = btoa(JSON.stringify({ exp: Math.floor(Date.now() / 1000) + expOffsetSec }));
  return `header.${payload}.sig`;
}

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;
  const base = `${environment.apiUrl}/auth`;

  beforeEach(() => {
    TestBed.configureTestingModule({ imports: [HttpClientTestingModule] });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
    sessionStorage.clear();
  });

  afterEach(() => {
    httpMock.verify();
    sessionStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('login guarda access y refresh token', () => {
    service.login('a@x.com', 'Password123!').subscribe(res => {
      expect(res.token).toBe('ACCESS');
    });
    const req = httpMock.expectOne(`${base}/login`);
    expect(req.request.method).toBe('POST');
    req.flush({ token: 'ACCESS', refreshToken: 'REFRESH', email: 'a@x.com' });

    expect(service.getToken()).toBe('ACCESS');
    expect(service.getRefreshToken()).toBe('REFRESH');
  });

  it('refresh rota el par con el refresh guardado', () => {
    sessionStorage.setItem('refresh_token', 'OLD');
    service.refresh().subscribe();
    const req = httpMock.expectOne(`${base}/refresh`);
    expect(req.request.body).toEqual({ refreshToken: 'OLD' });
    req.flush({ token: 'NEW_ACCESS', refreshToken: 'NEW_REFRESH' });

    expect(service.getToken()).toBe('NEW_ACCESS');
    expect(service.getRefreshToken()).toBe('NEW_REFRESH');
  });

  it('logout limpia sesión y revoca en servidor', () => {
    sessionStorage.setItem('token', 'ACCESS');
    sessionStorage.setItem('refresh_token', 'REFRESH');
    service.logout();
    const req = httpMock.expectOne(`${base}/logout`);
    expect(req.request.body).toEqual({ refreshToken: 'REFRESH' });
    req.flush(null);

    expect(service.getToken()).toBeNull();
    expect(service.getRefreshToken()).toBeNull();
  });

  it('isAuthenticated respeta exp del JWT', () => {
    expect(service.isAuthenticated()).toBeFalse();
    sessionStorage.setItem('token', unsignedJwt(3600));
    expect(service.isAuthenticated()).toBeTrue();
    sessionStorage.setItem('token', unsignedJwt(-10));
    expect(service.isAuthenticated()).toBeFalse();
  });
});
