import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { AuthService } from '../../services/auth.service';

interface Course {
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

interface Meeting {
  course: Course;
  date: Date;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, SidebarComponent],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  courses: Course[] = [];
  upcomingMeetings: Meeting[] = [];
  daysOfWeek: { pl: string; en: string }[] = [
    { pl: 'Poniedziałek', en: 'Monday' },
    { pl: 'Wtorek', en: 'Tuesday' },
    { pl: 'Środa', en: 'Wednesday' },
    { pl: 'Czwartek', en: 'Thursday' },
    { pl: 'Piątek', en: 'Friday' },
    { pl: 'Sobota', en: 'Saturday' },
    { pl: 'Niedziela', en: 'Sunday' }
  ];

  constructor(private http: HttpClient, public authService: AuthService) { }

  ngOnInit() {
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
        this.courses = data
          .filter(c => c.instructor === this.authService.getUserId())
          .map(course => ({
            ...course,
            startDate: new Date(course.startDate),
            endDate: course.endDate ? new Date(course.endDate) : undefined,
            meetingDates: course.meetingDates.map(d => new Date(d)),
            recurrenceDays: course.recurrenceDays || ''
          }));
        console.log('Dashboard Loaded Courses:', this.courses.map(c => ({ id: c.id, title: c.title })));
        if (this.courses.some(c => !c.id)) {
          console.warn('Dashboard: Niektóre kursy mają nieprawidłowe ID:', this.courses);
        }
        this.loadUpcomingMeetings();
      },
      error: (err) => {
        console.error('Dashboard Load Courses Error:', err.status, err.statusText, err.message);
      }
    });
  }

  loadUpcomingMeetings() {
    const now = new Date();
    this.upcomingMeetings = this.courses
      .flatMap(course =>
        course.meetingDates
          .filter(date => date > now)
          .map(date => ({ course, date }))
      )
      .sort((a, b) => a.date.getTime() - b.date.getTime())
      .slice(0, 5);
    console.log('Dashboard Upcoming Meetings:', this.upcomingMeetings);
  }

  getDisplayDays(recurrenceDays: string): string {
    if (!recurrenceDays) return 'Brak';
    const enDays = recurrenceDays.split(',').map(day => day.trim());
    return enDays
      .map(enDay => this.daysOfWeek.find(day => day.en === enDay)?.pl || enDay)
      .join(', ');
  }

  logout() {
    this.authService.logout();
  }
}
