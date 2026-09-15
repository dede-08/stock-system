import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { DashboardComponent } from './dashboard.component';
import { environment } from '../../../../environments/environment';

describe('DashboardComponent', () => {
  let component: DashboardComponent;
  let fixture: ComponentFixture<DashboardComponent>;
  let httpMock: HttpTestingController;
  const api = environment.apiUrl;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DashboardComponent, HttpClientTestingModule],
      providers: [{ provide: Router, useValue: jasmine.createSpyObj('Router', ['navigate']) }]
    }).compileComponents();

    fixture = TestBed.createComponent(DashboardComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
  });

  afterEach(() => httpMock.verify());

  function flushInitial() {
    httpMock.expectOne(r => r.url === `${api}/products`).flush(
      { items: [{ id: 1, name: 'A', price: 10, stock: 5, category: 'X' }], total: 1, page: 1, pageSize: 10, totalPages: 1 });
    httpMock.expectOne(r => r.url === `${api}/stats/products`).flush(
      { totalProducts: 1, lowStockProducts: 1, outOfStockProducts: 0, totalValue: 50, averagePrice: 10 });
    httpMock.expectOne(r => r.url === `${api}/stats/products/by-category`).flush([]);
  }

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('carga primera página + stats al iniciar', () => {
    flushInitial();
    expect(component.products.length).toBe(1);
    expect(component.total).toBe(1);
    expect(component.stats?.totalProducts).toBe(1);
    expect(component.loading).toBeFalse();
  });

  it('muestra error con reintento si falla la carga', () => {
    const req = httpMock.expectOne(r => r.url === `${api}/products`);
    req.flush('fail', { status: 500, statusText: 'Error' });
    httpMock.expectOne(r => r.url === `${api}/stats/products`).flush(
      { totalProducts: 0, lowStockProducts: 0, outOfStockProducts: 0, totalValue: 0, averagePrice: 0 });
    httpMock.expectOne(r => r.url === `${api}/stats/products/by-category`).flush([]);

    expect(component.loading).toBeFalse();
    expect(component.error).toContain('servidor');
  });
});
