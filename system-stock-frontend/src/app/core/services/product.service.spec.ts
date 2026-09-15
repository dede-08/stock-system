import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ProductService } from './product.service';
import { environment } from '../../../environments/environment';

describe('ProductService', () => {
  let service: ProductService;
  let httpMock: HttpTestingController;
  const base = `${environment.apiUrl}/products`;

  beforeEach(() => {
    TestBed.configureTestingModule({ imports: [HttpClientTestingModule] });
    service = TestBed.inject(ProductService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('getProductsPaged envía page/pageSize/sort y desenvuelve items', () => {
    service.getProductsPaged({ page: 2, pageSize: 10, sortBy: 'price', desc: true }).subscribe(res => {
      expect(res.total).toBe(1);
      expect(res.items.length).toBe(1);
    });
    const req = httpMock.expectOne(r =>
      r.url === base &&
      r.params.get('page') === '2' &&
      r.params.get('pageSize') === '10' &&
      r.params.get('sortBy') === 'price' &&
      r.params.get('desc') === 'true');
    expect(req.request.method).toBe('GET');
    req.flush({ items: [{ id: 1 }], total: 1, page: 2, pageSize: 10, totalPages: 1 });
  });

  it('getProducts acepta array plano (compat) y envelope', () => {
    service.getProducts().subscribe(items => expect(items.length).toBe(2));
    httpMock.expectOne(r => r.url === base).flush([{ id: 1 }, { id: 2 }]);

    service.getProducts().subscribe(items => expect(items[0].id).toBe(3));
    httpMock.expectOne(r => r.url === base)
      .flush({ items: [{ id: 3 }], total: 1, page: 1, pageSize: 50, totalPages: 1 });
  });

  it('searchProducts envía q', () => {
    service.searchProducts('lapiz').subscribe(res => expect(res.total).toBe(0));
    const req = httpMock.expectOne(r =>
      r.url === `${base}/search` && r.params.get('q') === 'lapiz');
    req.flush({ items: [], total: 0, page: 1, pageSize: 20, totalPages: 0 });
  });

  it('deleteProduct usa DELETE con id', () => {
    service.deleteProduct(7).subscribe();
    const req = httpMock.expectOne(`${base}/7`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });
});
