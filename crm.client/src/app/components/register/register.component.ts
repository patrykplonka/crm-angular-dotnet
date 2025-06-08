import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { RegisterModel } from '../../models/register.model';
import Swal from 'sweetalert2';

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
    console.log('Register model before submission:', this.model);
    this.authService.register(this.model).subscribe({
      next: () => {
        Swal.fire({
          title: 'Sukces!',
          text: 'Zarejestrowano!',
          icon: 'success',
          confirmButtonText: 'OK',
          customClass: {
            popup: 'modern-popup'
          }
        });
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
        Swal.fire({
          title: 'Błąd!',
          text: `Próba rejestracji zakończona niepowodzeniem: ${errorMessage}`,
          icon: 'error',
          confirmButtonText: 'OK',
          customClass: {
            popup: 'modern-popup'
          }
        });
      }
    });
  }
}
