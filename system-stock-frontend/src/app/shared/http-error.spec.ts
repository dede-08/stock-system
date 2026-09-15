import { HttpErrorResponse } from '@angular/common/http';
import { extractError } from './http-error';

describe('extractError', () => {
  it('devuelve string plano del backend', () => {
    expect(extractError(new HttpErrorResponse({ error: 'El email ya está registrado.' }))).toBe(
      'El email ya está registrado.');
  });

  it('usa message de {message}', () => {
    expect(extractError(new HttpErrorResponse({ error: { message: 'Credenciales incorrectas' } }))).toBe(
      'Credenciales incorrectas');
  });

  it('sin conexión (status 0)', () => {
    expect(extractError(new HttpErrorResponse({ status: 0 }))).toContain('No se pudo conectar');
  });

  it('rate limit 429 con mensaje amable', () => {
    expect(extractError(new HttpErrorResponse({ status: 429, error: {} }))).toContain('Demasiados intentos');
  });

  it('500 no filtra stacktrace', () => {
    const err = new HttpErrorResponse({
      status: 500,
      error: { title: 'Error', detail: 'System.NullReferenceException\n   at Foo()' }
    });
    expect(extractError(err)).toBe('Error del servidor. Inténtalo más tarde.');
  });
});
