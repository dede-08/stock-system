import { HttpErrorResponse } from '@angular/common/http';

interface BackendErrorBody {
  message?: unknown;
  detail?: unknown;
  title?: unknown;
}

/** Mensaje legible desde los formatos del backend: {message}, ProblemDetails o texto plano. */
export function extractError(err: unknown): string {
  if (err instanceof HttpErrorResponse) {
    const body = err.error;
    if (typeof body === 'string' && body.trim()) return body;
    if (isErrorBody(body)) {
      if (typeof body.message === 'string' && body.message) return body.message;
      if (typeof body.detail === 'string' && body.detail.trim() && !body.detail.includes('\n'))
        return body.detail;
      if (typeof body.title === 'string' && err.status !== 500) return body.title;
    }
    if (err.status === 0) return 'No se pudo conectar con el servidor. Verifica que el backend esté corriendo.';
    if (err.status === 401) return 'No autorizado. Vuelve a iniciar sesión.';
    if (err.status === 404) return 'Recurso no encontrado.';
    if (err.status === 429) return 'Demasiados intentos. Espera un minuto e inténtalo de nuevo.';
    if (err.status >= 500) return 'Error del servidor. Inténtalo más tarde.';
  }
  return 'Ocurrió un error. Inténtalo de nuevo.';
}

function isErrorBody(value: unknown): value is BackendErrorBody {
  return typeof value === 'object' && value !== null;
}
