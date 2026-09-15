import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ProductStats {
  totalProducts: number;
  lowStockProducts: number;
  outOfStockProducts: number;
  totalValue: number;
  averagePrice: number;
}

export interface CategoryStats {
  category: string;
  count: number;
  totalValue: number;
  averagePrice: number;
}

@Injectable({
  providedIn: 'root'
})
export class StatsService {
  private apiUrl = `${environment.apiUrl}/stats`;

  constructor(private http: HttpClient) { }

  getProductStats(lowStockThreshold = 10): Observable<ProductStats> {
    const params = new HttpParams().set('lowStockThreshold', lowStockThreshold);
    return this.http.get<ProductStats>(`${this.apiUrl}/products`, { params });
  }

  getProductsByCategory(): Observable<CategoryStats[]> {
    return this.http.get<CategoryStats[]>(`${this.apiUrl}/products/by-category`);
  }
}
