import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { SidebarComponent } from '../sidebar/sidebar.component';

interface Course {
  id: string;
  title: string;
  description: string;
  instructor: string;
  durationHours: number;
}

interface UserProfile {
  firstName: string;
  lastName: string;
  email: string;
  role: string;
}

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, HttpClientModule, SidebarComponent],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent {
  user: UserProfile | null = null;
  enrolledCourses: Course[] = [];

  constructor(private http: HttpClient) {
    this.loadUserProfile();
    this.loadEnrolledCourses();
  }

  loadUserProfile() {
    this.http.get<UserProfile>('http://localhost:5241/api/user/profile').subscribe({
      next: (data) => this.user = data,
      error: (err) => console.error('Błąd ładowania profilu:', err)
    });
  }

  loadEnrolledCourses() {
    this.http.get<Course[]>('http://localhost:5241/api/courses/enrolled').subscribe({
      next: (data) => this.enrolledCourses = data,
      error: (err) => console.error('Błąd ładowania zapisanych kursów:', err)
    });
  }
}
