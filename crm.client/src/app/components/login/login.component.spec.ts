import { Component } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { RegisterModel } from '../../models/register.model';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html'
})
export class RegisterComponent {
  model: RegisterModel = { firstName: '', lastName: '', email: '', password: '', role: 'Student' };

  constructor(private authService: AuthService) { }

  register() {
    this.authService.register(this.model).subscribe({
      next: () => alert('Registration successful'),
      error: (err) => alert('Registration failed')
    });
  }
}
