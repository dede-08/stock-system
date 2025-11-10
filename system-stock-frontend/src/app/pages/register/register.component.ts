import { Component, OnDestroy } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../services/auth.service';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { RegisterRequest } from '../../models/auth.model';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './register.component.html'
})
export class RegisterComponent implements OnDestroy {
  name = '';
  lastname = '';
  email = '';
  password = '';
  error = '';
  private destroy$ = new Subject<void>();

  constructor(private authService: AuthService, private router: Router) {}

  register() {
    this.error = '';
    const user: RegisterRequest = {
      name: this.name,
      lastname: this.lastname,
      email: this.email,
      password: this.password
    };

    this.authService.register(user)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.router.navigate(['/login']);
        },
        error: (err: HttpErrorResponse) => {
          if (err.error && typeof err.error === 'string') {
            this.error = err.error;
          } else {
            this.error = 'Failed to register. Please try again.';
          }
        }
      });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }
}