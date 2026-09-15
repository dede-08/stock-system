import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { Product } from '../app/models/product.model';
import { environment } from '../environments/environment';

export interface PagedResult<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private apiUrl = `${environment.apiUrl}/products`;

  constructor(private http: HttpClient) { }

  getProducts(page = 1, pageSize = 50): Observable<Product[]> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<Product[] | PagedResult<Product>>(this.apiUrl, { params }).pipe(
      map(res => Array.isArray(res) ? res : (res.items ?? []))
    );
  }

  getProductsPaged(page = 1, pageSize = 20): Observable<PagedResult<Product>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedResult<Product>>(this.apiUrl, { params });
  }

  addProduct(product: Partial<Product>): Observable<Product> {
    return this.http.post<Product>(this.apiUrl, product);
  }

  updateProduct(product: Product): Observable<Product> {
    return this.http.put<Product>(`${this.apiUrl}/${product.id}`, product);
  }

  deleteProduct(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
