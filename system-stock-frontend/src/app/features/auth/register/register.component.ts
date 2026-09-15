import { Component, OnDestroy } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { RegisterRequest } from '../../../core/models/auth.model';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';
import { extractError } from '../../../shared/http-error';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule, CommonModule, RouterLink],
  templateUrl: './register.component.html'
})
export class RegisterComponent implements OnDestroy {
  name = '';
  lastname = '';
  age: number | null = null;
  telephone = '';
  email = '';
  password = '';
  error = '';
  success = false;
  private destroy$ = new Subject<void>();

  constructor(private authService: AuthService, private router: Router) {}

  register() {
    this.error = '';
    if (this.age === null || this.age < 18 || this.age > 120) {
      this.error = 'La edad debe estar entre 18 y 120 años.';
      return;
    }
    if (!this.password || this.password.length < 8) {
      this.error = 'La contraseña debe tener al menos 8 caracteres.';
      return;
    }
    const user: RegisterRequest = {
      name: this.name.trim(),
      lastname: this.lastname.trim(),
      age: this.age,
      email: this.email.trim(),
      telephone: this.telephone.trim() || undefined,
      password: this.password
    };

    this.authService.register(user)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.success = true;
          setTimeout(() => this.router.navigate(['/login']), 1500);
        },
        error: (err: HttpErrorResponse) => {
          this.error = extractError(err);
        }
      });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
