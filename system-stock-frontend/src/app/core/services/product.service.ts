import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { Product } from '../models/product.model';
import { environment } from '../../../environments/environment';

export interface PagedResult<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface ProductQuery {
  page?: number;
  pageSize?: number;
  sortBy?: 'name' | 'price' | 'stock' | 'createdAt';
  desc?: boolean;
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

  getProductsPaged(query: ProductQuery = {}): Observable<PagedResult<Product>> {
    let params = new HttpParams()
      .set('page', query.page ?? 1)
      .set('pageSize', query.pageSize ?? 20);
    if (query.sortBy) params = params.set('sortBy', query.sortBy);
    if (query.desc) params = params.set('desc', true);
    return this.http.get<PagedResult<Product>>(this.apiUrl, { params });
  }

  searchProducts(q: string, query: ProductQuery = {}): Observable<PagedResult<Product>> {
    let params = new HttpParams()
      .set('q', q)
      .set('page', query.page ?? 1)
      .set('pageSize', query.pageSize ?? 20);
    return this.http.get<PagedResult<Product>>(`${this.apiUrl}/search`, { params });
  }

  getProductsByCategory(category: string, query: ProductQuery = {}): Observable<PagedResult<Product>> {
    let params = new HttpParams()
      .set('page', query.page ?? 1)
      .set('pageSize', query.pageSize ?? 20);
    return this.http.get<PagedResult<Product>>(
      `${this.apiUrl}/category/${encodeURIComponent(category)}`, { params });
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
