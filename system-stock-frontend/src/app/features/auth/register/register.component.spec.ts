import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';

import { RegisterComponent } from './register.component';
import { AuthService } from '../../../core/services/auth.service';

describe('RegisterComponent', () => {
  let component: RegisterComponent;
  let fixture: ComponentFixture<RegisterComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RegisterComponent, HttpClientTestingModule, RouterTestingModule]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RegisterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('rechaza edad fuera de rango sin llamar al backend', () => {
    const auth = TestBed.inject(AuthService);
    const registerSpy = spyOn(auth, 'register');
    component.age = 15;
    component.password = 'Password123!';
    component.register();
    expect(registerSpy).not.toHaveBeenCalled();
    expect(component.error).toContain('18 y 120');
  });

  it('rechaza contraseña corta sin llamar al backend', () => {
    const auth = TestBed.inject(AuthService);
    const registerSpy = spyOn(auth, 'register');
    component.age = 25;
    component.password = '123';
    component.register();
    expect(registerSpy).not.toHaveBeenCalled();
    expect(component.error).toContain('8 caracteres');
  });
});
