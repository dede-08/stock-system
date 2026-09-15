import { Component, HostListener, OnDestroy, OnInit } from '@angular/core';
import { ProductService, ProductQuery } from '../../../core/services/product.service';
import { StatsService, ProductStats } from '../../../core/services/stats.service';
import { Product } from '../../../core/models/product.model';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';
import { extractError } from '../../../shared/http-error';

interface ProductForm {
  id?: number;
  name: string;
  description: string;
  category: string;
  price: number | null;
  stock: number | null;
}

const BLANK_FORM: ProductForm = { name: '', description: '', category: '', price: null, stock: null };

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit, OnDestroy {
  products: Product[] = [];
  categories: string[] = [];
  stats: ProductStats | null = null;

  searchTerm = '';
  selectedCategory = '';
  page = 1;
  pageSize = 10;
  total = 0;
  totalPages = 0;
  sortBy: NonNullable<ProductQuery['sortBy']> | null = null;
  desc = false;

  loading = false;
  error = '';
  notification: { type: 'success' | 'danger'; msg: string } | null = null;

  showModal = false;
  isEditMode = false;
  saving = false;
  currentProduct: ProductForm = { ...BLANK_FORM };

  pendingDelete: Product | null = null;
  deleting = false;

  private destroy$ = new Subject<void>();
  private search$ = new Subject<string>();
  private notifTimer: ReturnType<typeof setTimeout> | null = null;

  constructor(
    private productService: ProductService,
    private statsService: StatsService,
    private authService: AuthService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.search$.pipe(
      debounceTime(400),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.page = 1;
      this.loadProducts();
    });
    this.loadProducts();
    this.loadStats();
    this.loadCategories();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    if (this.notifTimer) clearTimeout(this.notifTimer);
  }

  // ---------- data ----------

  loadProducts(): void {
    this.loading = true;
    this.error = '';
    const q = this.searchTerm.trim();
    const query = { page: this.page, pageSize: this.pageSize, sortBy: this.sortBy ?? undefined, desc: this.desc };
    const req = q
      ? this.productService.searchProducts(q, query)
      : this.selectedCategory
        ? this.productService.getProductsByCategory(this.selectedCategory, query)
        : this.productService.getProductsPaged(query);

    req.pipe(takeUntil(this.destroy$)).subscribe({
      next: res => {
        this.products = res.items ?? [];
        this.total = res.total ?? 0;
        this.page = res.page ?? 1;
        this.pageSize = res.pageSize ?? this.pageSize;
        this.totalPages = res.totalPages ?? 0;
        this.loading = false;
      },
      error: err => {
        this.loading = false;
        this.error = extractError(err);
      }
    });
  }

  loadStats(): void {
    this.statsService.getProductStats().pipe(takeUntil(this.destroy$)).subscribe({
      next: s => (this.stats = s),
      error: () => (this.stats = null)
    });
  }

  loadCategories(): void {
    this.statsService.getProductsByCategory().pipe(takeUntil(this.destroy$)).subscribe({
      next: rows => (this.categories = (rows ?? []).map(r => r.category)),
      error: () => (this.categories = [])
    });
  }

  refreshAll(): void {
    this.loadProducts();
    this.loadStats();
    this.loadCategories();
  }

  // ---------- filters / sort / paging ----------

  onSearchInput(): void {
    this.search$.next(this.searchTerm);
  }

  clearSearch(): void {
    if (!this.searchTerm) return;
    this.searchTerm = '';
    this.page = 1;
    this.loadProducts();
  }

  onCategoryChange(): void {
    this.page = 1;
    this.loadProducts();
  }

  onPageSizeChange(): void {
    this.page = 1;
    this.loadProducts();
  }

  goToPage(p: number): void {
    const target = Math.min(Math.max(1, p), Math.max(1, this.totalPages));
    if (target === this.page) return;
    this.page = target;
    this.loadProducts();
  }

  toggleSort(col: NonNullable<ProductQuery['sortBy']>): void {
    if (this.sortBy !== col) {
      this.sortBy = col;
      this.desc = false;
    } else {
      this.desc = !this.desc;
    }
    this.page = 1;
    this.loadProducts();
  }

  sortIcon(col: NonNullable<ProductQuery['sortBy']>): string {
    if (this.sortBy !== col) return 'fas fa-sort text-muted';
    return this.desc ? 'fas fa-sort-down' : 'fas fa-sort-up';
  }

  // ---------- helpers ----------

  getStockBadgeClass(stock: number): string {
    if (stock <= 0) return 'bg-danger';
    if (stock <= 10) return 'bg-warning text-dark';
    return 'bg-success';
  }

  stockLabel(stock: number): string {
    if (stock <= 0) return 'Agotado';
    if (stock <= 10) return 'Bajo';
    return 'OK';
  }

  trackById(_: number, p: Product): number {
    return p.id;
  }

  // ---------- modal create/edit ----------

  openAddModal(): void {
    this.isEditMode = false;
    this.currentProduct = { ...BLANK_FORM };
    this.showModal = true;
  }

  openEditModal(product: Product): void {
    this.isEditMode = true;
    this.currentProduct = {
      id: product.id,
      name: product.name,
      description: product.description ?? '',
      category: product.category,
      price: product.price,
      stock: product.stock
    };
    this.showModal = true;
  }

  closeModal(): void {
    if (this.saving) return;
    this.showModal = false;
  }

  onEsc(): void {
    if (this.pendingDelete && !this.deleting) this.pendingDelete = null;
    else if (this.showModal) this.closeModal();
  }

  @HostListener('window:keydown.escape')
  onEscKey(): void {
    this.onEsc();
  }

  private formError(): string {
    const f = this.currentProduct;
    if (!f.name.trim()) return 'El nombre es obligatorio.';
    if (!f.category) return 'La categoría es obligatoria.';
    if (f.price === null || isNaN(f.price) || f.price <= 0) return 'El precio debe ser mayor a 0.';
    if (f.stock === null || isNaN(f.stock) || !Number.isInteger(f.stock) || f.stock < 0)
      return 'El stock debe ser un entero mayor o igual a 0.';
    return '';
  }

  saveProduct(): void {
    const err = this.formError();
    if (err) {
      this.notify('danger', err);
      return;
    }
    this.saving = true;
    const f = this.currentProduct;
    const payload = {
      name: f.name.trim(),
      description: (f.description ?? '').trim(),
      category: f.category,
      price: f.price as number,
      stock: f.stock as number
    };
    const req = this.isEditMode && f.id
      ? this.productService.updateProduct({ ...payload, id: f.id } as Product)
      : this.productService.addProduct(payload);

    req.pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.saving = false;
        this.showModal = false;
        this.notify('success', this.isEditMode ? 'Producto actualizado.' : 'Producto creado.');
        if (!this.isEditMode) this.page = 1;
        this.refreshAll();
      },
      error: e => {
        this.saving = false;
        this.notify('danger', extractError(e));
      }
    });
  }

  // ---------- delete ----------

  askDelete(product: Product): void {
    this.pendingDelete = product;
  }

  confirmDelete(): void {
    if (!this.pendingDelete || this.deleting) return;
    this.deleting = true;
    const id = this.pendingDelete.id;
    this.productService.deleteProduct(id).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.deleting = false;
        this.pendingDelete = null;
        this.notify('success', 'Producto eliminado.');
        if (this.products.length === 1 && this.page > 1) this.page--;
        this.refreshAll();
      },
      error: e => {
        this.deleting = false;
        this.pendingDelete = null;
        this.notify('danger', extractError(e));
      }
    });
  }

  // ---------- misc ----------

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  private notify(type: 'success' | 'danger', msg: string): void {
    this.notification = { type, msg };
    if (this.notifTimer) clearTimeout(this.notifTimer);
    this.notifTimer = setTimeout(() => (this.notification = null), 4000);
  }
}
