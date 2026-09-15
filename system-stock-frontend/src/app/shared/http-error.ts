import { HttpErrorResponse } from '@angular/common/http';

/** Mensaje legible desde los formatos del backend: {message}, ProblemDetails o texto plano. */
export function extractError(err: unknown): string {
  if (err instanceof HttpErrorResponse) {
    const e: any = err.error;
    if (typeof e === 'string' && e.trim()) return e;
    if (e?.message) return String(e.message);
    if (typeof e?.detail === 'string' && e.detail.trim() && !e.detail.includes('\n')) return e.detail;
    if (e?.title && err.status !== 500) return String(e.title);
    if (err.status === 0) return 'No se pudo conectar con el servidor. Verifica que el backend esté corriendo.';
    if (err.status === 401) return 'No autorizado. Vuelve a iniciar sesión.';
    if (err.status === 404) return 'Recurso no encontrado.';
    if (err.status === 429) return 'Demasiados intentos. Espera un minuto e inténtalo de nuevo.';
    if (err.status >= 500) return 'Error del servidor. Inténtalo más tarde.';
  }
  return 'Ocurrió un error. Inténtalo de nuevo.';
}
