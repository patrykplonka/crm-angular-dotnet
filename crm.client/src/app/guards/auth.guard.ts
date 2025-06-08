import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(private authService: AuthService, private router: Router) { }

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    const isLoggedIn = this.authService.isLoggedIn();
    const token = this.authService.getToken();
    const userRole = this.authService.getUserRole();
    console.log('AuthGuard canActivate:', {
      isLoggedIn,
      token: token?.substring(0, 20) + '...',
      userRole,
      url: state.url
    });

    if (isLoggedIn) {
      return true;
    }
    console.warn('AuthGuard: Przekierowanie do /login');
    this.router.navigate(['/login']);
    return false;
  }
}
