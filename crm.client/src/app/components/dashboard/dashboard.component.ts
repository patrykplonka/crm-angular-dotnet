import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { HttpClient, HttpClientModule, HttpHeaders } from '@angular/common/http';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { AuthService } from '../../services/auth.service';

export interface Course {
  id: string;
  title: string;
  description: string;
  instructor: string; // Używane jako TutorId
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
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, HttpClientModule, SidebarComponent],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  courses: Course[] = [];
  upcomingMeetings: { course: Course, date: Date }[] = [];

  constructor(private http: HttpClient, public authService: AuthService) { }

  ngOnInit() {
    if (this.authService.isLoggedIn() && this.authService.isTutor()) {
      this.loadTutorCourses();
    }
  }

  private getAuthHeaders(): HttpHeaders {
    return new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`,
      'Content-Type': 'application/json'
    });
  }

  loadTutorCourses() {
    this.http.get<Course[]>('http://localhost:5241/api/courses/tutor', { headers: this.getAuthHeaders() }).subscribe({
      next: (data) => {
        this.courses = data.map(course => ({
          ...course,
          startDate: new Date(course.startDate),
          endDate: course.endDate ? new Date(course.endDate) : undefined,
          meetingDates: course.meetingDates.map(d => new Date(d)),
          recurrenceDays: course.recurrenceDays || ''
        }));
        this.calculateUpcomingMeetings();
      },
      error: (err) => {
        console.error('Błąd ładowania kursów korepetytora:', err);
        if (err.status === 401) {
          alert('Brak autoryzacji. Zaloguj się ponownie.');
        } else if (err.status === 403) {
          alert('Brak uprawnień do wyświetlenia kursów.');
        }
      }
    });
  }

  calculateUpcomingMeetings() {
    const now = new Date();
    this.upcomingMeetings = [];
    this.courses.forEach(course => {
      course.meetingDates
        .filter(date => date > now)
        .sort((a, b) => a.getTime() - b.getTime())
        .slice(0, 5) // Ogranicz do 5 najbliższych spotkań
        .forEach(date => {
          this.upcomingMeetings.push({ course, date });
        });
    });
  }

  getDisplayDays(recurrenceDays: string): string {
    if (!recurrenceDays) return 'Brak';
    const daysOfWeek = [
      { pl: 'Poniedziałek', en: 'Monday' },
      { pl: 'Wtorek', en: 'Tuesday' },
      { pl: 'Środa', en: 'Wednesday' },
      { pl: 'Czwartek', en: 'Thursday' },
      { pl: 'Piątek', en: 'Friday' },
      { pl: 'Sobota', en: 'Saturday' },
      { pl: 'Niedziela', en: 'Sunday' }
    ];
    const enDays = recurrenceDays.split(',').map(day => day.trim());
    return enDays
      .map(enDay => daysOfWeek.find(day => day.en === enDay)?.pl || enDay)
      .join(', ');
  }

  logout() {
    this.authService.logout();
  }
}
