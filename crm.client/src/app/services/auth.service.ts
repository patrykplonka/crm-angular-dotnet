import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { LoginModel } from '../models/login.model';
import { RegisterModel } from '../models/register.model';
import { JwtHelperService } from '@auth0/angular-jwt';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://localhost:5241/api/auth';
  private jwtHelper = new JwtHelperService();

  constructor(private http: HttpClient) { }

  login(model: LoginModel): Observable<{ token: string }> {
    return this.http
      .post<{ token: string }>(`${this.apiUrl}/login`, model, {
        headers: { 'Content-Type': 'application/json' }
      })
      .pipe(
        tap(response => {
          if (response.token) {
            localStorage.setItem('token', response.token);
            console.log('Decoded token:', this.jwtHelper.decodeToken(response.token)); // Debug
          }
        }),
        catchError(err => {
          console.error('Login error:', err);
          return throwError(err);
        })
      );
  }

  register(model: RegisterModel): Observable<any> {
    return this.http
      .post(`${this.apiUrl}/register`, model, {
        headers: { 'Content-Type': 'application/json' }
      })
      .pipe(
        catchError(err => {
          console.error('Register error:', err);
          return throwError(err);
        })
      );
  }

  getUserId(): string | null {
    const token = this.getToken();
    if (token) {
      try {
        const decoded: any = this.jwtHelper.decodeToken(token);
        return decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || null;
      } catch (err) {
        console.error('JWT decode error:', err);
        return null;
      }
    }
    return null;
  }

  isLoggedIn(): boolean {
    const token = this.getToken();
    return !!token && !this.jwtHelper.isTokenExpired(token);
  }

  logout(): void {
    localStorage.removeItem('token');
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  getUserRole(): string | null {
    const token = this.getToken();
    if (!token) return null;
    try {
      const decoded: any = this.jwtHelper.decodeToken(token);
      console.log('Decoded JWT:', decoded);
      return decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || decoded.role || null;
    } catch (err) {
      console.error('JWT decode error:', err);
      return null;
    }
  }

  isStudent(): boolean {
    return this.getUserRole() === 'Student';
  }

  isTutor(): boolean {
    return this.getUserRole() === 'Tutor';
  }

  isAdmin(): boolean {
    return this.getUserRole() === 'Admin';
  }
}
