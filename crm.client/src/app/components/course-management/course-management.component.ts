import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { SidebarComponent } from '../sidebar/sidebar.component';

export interface Course {
  id: string;
  title: string;
  description: string;
  instructor: string;
  durationHours: number;
  link: string; 
  enrolled?: boolean;
}

@Component({
  selector: 'app-course-management',
  standalone: true,
  imports: [CommonModule, RouterModule, HttpClientModule, FormsModule, SidebarComponent],
  templateUrl: './course-management.component.html',
  styleUrls: ['./course-management.component.css']
})
export class CourseManagementComponent {
  courses: Course[] = [];
  newCourse: Course = {
    id: '',
    title: '',
    description: '',
    instructor: '',
    durationHours: 0,
    link: '' 
  };
  showForm: boolean = false;

  constructor(private http: HttpClient) {
    this.loadCourses();
  }

  loadCourses() {
    this.http.get<Course[]>('http://localhost:5241/api/courses').subscribe({
      next: (data) => {
        this.courses = data.map(course => ({
          ...course,
          enrolled: false
        }));
      },
      error: (err) => console.error('Błąd ładowania kursów:', err.status, err.statusText, err.message)
    });
  }

  addCourse() {
    if (!this.newCourse.title || !this.newCourse.description || !this.newCourse.durationHours) {
      alert('Wypełnij wszystkie wymagane pola!');
      return;
    }

    this.http.post<Course>('http://localhost:5241/api/courses', this.newCourse).subscribe({
      next: (course) => {
        this.courses.push({ ...course, enrolled: false });
        this.resetForm();
        this.showForm = false;
      },
      error: (err) => console.error('Błąd dodawania kursu:', err.status, err.statusText, err.message)
    });
  }

  deleteCourse(id: string) {
    if (confirm('Czy na pewno chcesz usunąć ten kurs?')) {
      this.http.delete(`http://localhost:5241/api/courses/${id}`).subscribe({
        next: () => {
          this.courses = this.courses.filter(course => course.id !== id);
        },
        error: (err) => console.error('Błąd usuwania kursu:', err)
      });
    }
  }

  toggleEnrollment(course: Course) {
    const url = `http://localhost:5241/api/courses/${course.id}/enroll`;
    const action = course.enrolled ? 'unenroll' : 'enroll';
    this.http.post(url, { action }).subscribe({
      next: () => {
        course.enrolled = !course.enrolled;
      },
      error: (err) => console.error('Błąd zmiany statusu zapisu:', err)
    });
  }

  toggleForm() {
    this.showForm = !this.showForm;
  }

  private resetForm() {
    this.newCourse = {
      id: '',
      title: '',
      description: '',
      instructor: '',
      durationHours: 0,
      link: ''
    };
  }
}
