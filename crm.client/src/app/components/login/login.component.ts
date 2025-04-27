import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { LoginModel } from '../../models/login.model';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, RouterModule],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  model: LoginModel = { email: '', password: '' };

  constructor(private authService: AuthService) { }

  login() {
    this.authService.login(this.model).subscribe({
      next: (res) => {
        localStorage.setItem('token', res.token);
        alert('Login successful');
      },
      error: (err) => alert('Login failed: ' + err.message)
    });
  }
}
