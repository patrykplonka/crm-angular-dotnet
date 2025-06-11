import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { HttpClient, HttpClientModule, HttpHeaders } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { AuthService } from '../../services/auth.service';

export interface Course {
  id: string;
  title: string;
  description: string;
  instructor: string; 
  durationHours: number;
  link: string;
  enrolled: boolean;
  startDate: Date;
  endDate?: Date;
  recurrencePattern?: string;
  meetingDates: Date[];
  recurrenceDays: string;
  recurrenceWeeks?: number;
  startTime?: string;
  endTime?: string;
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
    link: '',
    enrolled: false,
    startDate: new Date(),
    meetingDates: [],
    recurrenceDays: '',
    recurrenceWeeks: 1,
    startTime: '18:00',
    endTime: '20:00'
  };
  showForm: boolean = false;
  daysOfWeek: { pl: string; en: string }[] = [
    { pl: 'Poniedziałek', en: 'Monday' },
    { pl: 'Wtorek', en: 'Tuesday' },
    { pl: 'Środa', en: 'Wednesday' },
    { pl: 'Czwartek', en: 'Thursday' },
    { pl: 'Piątek', en: 'Friday' },
    { pl: 'Sobota', en: 'Saturday' },
    { pl: 'Niedziela', en: 'Sunday' }
  ];
  selectedDays: boolean[] = new Array(this.daysOfWeek.length).fill(false);

  constructor(private http: HttpClient, public authService: AuthService) {
    if (this.authService.isLoggedIn()) {
      this.loadCourses();
    }
  }

  private getAuthHeaders(): HttpHeaders {
    return new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`,
      'Content-Type': 'application/json'
    });
  }

  loadCourses() {
    this.http.get<Course[]>('http://localhost:5241/api/courses', { headers: this.getAuthHeaders() }).subscribe({
      next: (data) => {
        this.courses = data.map(course => ({
          ...course,
          startDate: new Date(course.startDate),
          endDate: course.endDate ? new Date(course.endDate) : undefined,
          meetingDates: course.meetingDates.map(d => new Date(d)),
          recurrenceDays: course.recurrenceDays || ''
        }));
      },
      error: (err) => {
        console.error('Błąd ładowania kursów:', err.status, err.statusText, err.message);
        if (err.status === 401) {
          alert('Brak autoryzacji. Zaloguj się ponownie.');
        } else if (err.status === 403) {
          alert('Brak uprawnień do wyświetlenia kursów.');
        }
      }
    });
  }

  addCourse() {
    if (!this.authService.isAdmin()) {
      alert('Brak uprawnień do dodawania kursów!');
      return;
    }
    if (!this.newCourse.title || !this.newCourse.description || !this.newCourse.durationHours || !this.selectedDays.some(day => day) || !this.newCourse.startTime || !this.newCourse.endTime || !this.newCourse.recurrenceWeeks) {
      alert('Wypełnij wszystkie wymagane pola, w tym co najmniej jeden dzień!');
      return;
    }

    this.newCourse.recurrenceDays = this.daysOfWeek
      .filter((_, index) => this.selectedDays[index])
      .map(day => day.en)
      .join(',');

    const courseToSend: Course = {
      ...this.newCourse,
      recurrenceDays: this.newCourse.recurrenceDays
    };

    this.http.post<Course>('http://localhost:5241/api/courses', courseToSend, { headers: this.getAuthHeaders() }).subscribe({
      next: (course) => {
        this.courses.push({
          ...course,
          startDate: new Date(course.startDate),
          endDate: course.endDate ? new Date(course.endDate) : undefined,
          meetingDates: course.meetingDates.map(d => new Date(d)),
          recurrenceDays: course.recurrenceDays || ''
        });
        this.resetForm();
        this.showForm = false;
      },
      error: (err) => {
        console.error('Błąd dodawania kursu:', err.status, err.statusText, err.error);
        if (err.status === 400) {
          const errors = err.error?.errors ? Object.values(err.error.errors).flat().join(', ') : err.error?.title || 'Sprawdź poprawność wprowadzonych danych.';
          alert('Błąd w danych kursu: ' + errors);
        } else if (err.status === 403) {
          alert('Brak uprawnień do dodania kursu.');
        }
      }
    });
  }

  assignTutor(course: Course) {
    if (!this.authService.isTutor()) {
      alert('Brak uprawnień do przypisania się do kursu!');
      return;
    }
    const userId = this.authService.getUserId();
    if (!userId) {
      alert('Błąd: Nie można pobrać ID użytkownika. Zaloguj się ponownie.');
      return;
    }
    this.http.post(`http://localhost:5241/api/courses/${course.id}/assign-tutor`, {}, { headers: this.getAuthHeaders() }).subscribe({
      next: () => {
        course.instructor = userId;
        alert('Przypisano Cię do kursu!');
      },
      error: (err) => {
        console.error('Błąd przypisywania korepetytora:', err);
        if (err.status === 400) {
          alert('Kurs ma już przypisanego korepetytora.');
        } else if (err.status === 403) {
          alert('Brak uprawnień do przypisania się do kursu.');
        }
      }
    });
  }

  getDisplayDays(recurrenceDays: string): string {
    if (!recurrenceDays) return 'Brak';
    const enDays = recurrenceDays.split(',').map(day => day.trim());
    return enDays
      .map(enDay => this.daysOfWeek.find(day => day.en === enDay)?.pl || enDay)
      .join(', ');
  }

  deleteCourse(id: string) {
    if (!this.authService.isAdmin()) {
      alert('Brak uprawnień do usuwania kursów!');
      return;
    }
    if (confirm('Czy na pewno chcesz usunąć ten kurs?')) {
      this.http.delete(`http://localhost:5241/api/courses/${id}`, { headers: this.getAuthHeaders() }).subscribe({
        next: () => {
          this.courses = this.courses.filter(course => course.id !== id);
        },
        error: (err) => {
          console.error('Błąd usuwania kursu:', err);
          if (err.status === 403) {
            alert('Brak uprawnień do usunięcia kursu.');
          }
        }
      });
    }
  }

  toggleEnrollment(course: Course) {
    if (!this.authService.isStudent()) {
      alert('Brak uprawnień do zapisu na kurs!');
      return;
    }
    const url = `http://localhost:5241/api/courses/${course.id}/enroll`;
    const action = course.enrolled ? 'unenroll' : 'enroll';
    this.http.post<{ enrolled: boolean }>(url, { action }, { headers: this.getAuthHeaders() }).subscribe({
      next: (response) => {
        course.enrolled = response.enrolled;
      },
      error: (err) => {
        console.error('Błąd zmiany statusu zapisu:', err);
        if (err.status === 403) {
          alert('Brak uprawnień do zmiany zapisu.');
        }
      }
    });
  }

  toggleForm() {
    if (!this.authService.isAdmin()) {
      alert('Brak uprawnień do dodawania kursów!');
      return;
    }
    this.showForm = !this.showForm;
  }

  private resetForm() {
    this.newCourse = {
      id: '',
      title: '',
      description: '',
      instructor: '',
      durationHours: 0,
      link: '',
      enrolled: false,
      startDate: new Date(),
      meetingDates: [],
      recurrenceDays: '',
      recurrenceWeeks: 1,
      startTime: '18:00',
      endTime: '20:00'
    };
    this.selectedDays = new Array(this.daysOfWeek.length).fill(false);
  }
}
