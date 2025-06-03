import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { LoginModel } from '../models/login.model';
import { RegisterModel } from '../models/register.model';

export interface User {
  firstName: string;
  lastName: string;
  email: string;
  role: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://localhost:5241/api/auth';
  private userSubject = new BehaviorSubject<User | null>(null);

  constructor(private http: HttpClient) { }

  login(model: LoginModel): Observable<{ token: string, user: User }> {
    return this.http.post<{ token: string, user: User }>(`${this.apiUrl}/login`, model, {
      headers: { 'Content-Type': 'application/json' }
    }).pipe(
      catchError(err => {
        console.error('Login error:', err);
        return throwError(err);
      })
    );
  }

  register(model: RegisterModel): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, model, {
      headers: { 'Content-Type': 'application/json' }
    }).pipe(
      catchError(err => {
        console.error('Register error:', err);
        return throwError(err);
      })
    );
  }

  setUser(user: User, token: string): void {
    localStorage.setItem('token', token);
    this.userSubject.next(user);
  }

  getUser(): User | null {
    return this.userSubject.value;
  }

  isAdmin(): boolean {
    const user = this.getUser();
    console.log('User:', user);  
    return user?.role === 'Admin';
  }

  isLoggedIn(): boolean {
    return !!localStorage.getItem('token');
  }

  logout(): void {
    localStorage.removeItem('token');
    this.userSubject.next(null);
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }
}
