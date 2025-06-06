import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { RegisterModel } from '../../models/register.model';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule, RouterModule],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
  model: RegisterModel = { firstName: '', lastName: '', email: '', password: '', role: 'Student' };

  constructor(private authService: AuthService) { }

  register() {
    console.log('Register model before submission:', this.model); // Debug: Log model
    this.authService.register(this.model).subscribe({
      next: () => {
        alert('Zarejestrowano!');
        console.log('Registration successful');
      },
      error: (err) => {
        let errorMessage = 'Nieznany błąd!';
        if (err.error?.Errors) {
          errorMessage = err.error.Errors.join(', ');
        } else if (err.error) {
          errorMessage = JSON.stringify(err.error);
        } else if (err.message) {
          errorMessage = err.message;
        }
        console.error('Błąd rejestracji:', err);
        alert(`Próba rejestracji zakończona niepowodzeniem: ${errorMessage}`);
      }
    });
  }
}
