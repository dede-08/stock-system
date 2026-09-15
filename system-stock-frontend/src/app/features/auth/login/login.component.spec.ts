import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';

import { LoginComponent } from './login.component';
import { AuthService } from '../../../core/services/auth.service';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LoginComponent, HttpClientTestingModule, RouterTestingModule]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('no llama al backend con credenciales vacías', () => {
    const auth = TestBed.inject(AuthService);
    const loginSpy = spyOn(auth, 'login');
    component.email = '';
    component.password = '';
    component.onLogin();
    expect(loginSpy).not.toHaveBeenCalled();
    expect(component.error).toBe('');
    expect(component.loading).toBeFalse();
  });
});
