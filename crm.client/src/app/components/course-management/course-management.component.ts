import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { HttpClientModule, HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { AuthService } from '../../services/auth.service'; 

export interface Course {
  id: string;
  title: string;
  description: string;
  instructor: string;
  durationHours: number;
  enrolled?: boolean;
}

@Component({
  selector: 'app-course-management',
  standalone: true,
  imports: [CommonModule, RouterModule, HttpClientModule, FormsModule, SidebarComponent],
  templateUrl: './course-management.component.html',
  styleUrls: ['./course-management.component.css']
})
export class CourseManagementComponent implements OnInit {
  courses: Course[] = [];
  newCourse: Course = {
    id: '',
    title: '',
    description: '',
    instructor: '',
    durationHours: 0
  };
  showForm: boolean = false;
  isAdmin: boolean = false;

  constructor(private http: HttpClient, private authService: AuthService) { }

  ngOnInit(): void {
    this.isAdmin = this.authService.isAdmin();
    console.log('isAdmin:', this.isAdmin); // Debug: sprawdź, czy isAdmin jest true/false
    this.loadCourses();
  }

  loadCourses() {
    this.http.get<Course[]>('http://localhost:5241/api/courses').subscribe({
      next: (data) => {
        this.courses = data.map(course => ({ ...course, enrolled: false }));
        console.log('Courses loaded:', this.courses); // Debug: sprawdź dane kursów
      },
      error: (err) => {
        console.error('Błąd ładowania kursów:', err.status, err.statusText);
        alert('Błąd ładowania kursów');
      }
    });
  }

  addCourse() {
    if (!this.isAdmin) {
      alert('Tylko administratorzy mogą dodawać kursy');
      return;
    }
    if (!this.newCourse.title || !this.newCourse.description || !this.newCourse.durationHours) {
      alert('Wypełnij wszystkie wymagane pola!');
      return;
    }
    this.http.post<Course>('http://localhost:5241/api/courses', this.newCourse).subscribe({
      next: (course) => {
        this.courses.push({ ...course, enrolled: false });
        this.resetForm();
        this.showForm = false;
        alert('Kurs dodany pomyślnie');
      },
      error: (err) => {
        console.error('Błąd dodawania kursu:', err);
        alert(err.status === 403 ? 'Brak uprawnień' : 'Błąd dodawania kursu');
      }
    });
  }

  deleteCourse(id: string) {
    if (!this.isAdmin) {
      alert('Tylko administratorzy mogą usuwać kursy');
      return;
    }
    if (confirm('Czy na pewno chcesz usunąć ten kurs?')) {
      this.http.delete(`http://localhost:5241/api/courses/${id}`).subscribe({
        next: () => {
          this.courses = this.courses.filter(course => course.id !== id);
          alert('Kurs usunięty');
        },
        error: (err) => alert(err.status === 403 ? 'Brak uprawnień' : 'Błąd usuwania kursu')
      });
    }
  }

  toggleEnrollment(course: Course) {
    const url = `http://localhost:5241/api/courses/${course.id}/enroll`;
    const action = course.enrolled ? 'unenroll' : 'enroll';
    this.http.post(url, { action }).subscribe({
      next: () => {
        course.enrolled = !course.enrolled;
        alert(course.enrolled ? 'Zapisano na kurs' : 'Wypisano z kursu');
      },
      error: (err) => alert('Błąd zmiany statusu zapisu')
    });
  }

  toggleForm() {
    if (!this.isAdmin) {
      alert('Tylko administratorzy mogą dodawać kursy');
      return;
    }
    this.showForm = !this.showForm;
  }

  private resetForm() {
    this.newCourse = { id: '', title: '', description: '', instructor: '', durationHours: 0 };
  }
}
