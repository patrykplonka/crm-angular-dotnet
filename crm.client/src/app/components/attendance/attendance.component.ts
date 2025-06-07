import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { ActivatedRoute } from '@angular/router';
import { SidebarComponent } from '../sidebar/sidebar.component';

interface Attendance {
  id: string;
  userId: string;
  courseId: string;
  meetingDate: Date;
  present: boolean;
  userName: string;
}

@Component({
  selector: 'app-attendance',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, SidebarComponent],
  templateUrl: './attendance.component.html',
  styleUrls: ['./attendance.component.css']
})
export class AttendanceComponent implements OnInit {
  attendances: Attendance[] = [];
  meetingDates: Date[] = [];
  selectedDate: string = '';
  courseId: string = '';
  courseTitle: string = '';
  errorMessage: string = '';

  constructor(
    private http: HttpClient,
    private authService: AuthService,
    private route: ActivatedRoute
  ) { }

  ngOnInit() {
    this.courseId = this.route.snapshot.paramMap.get('courseId') || '';
    if (!this.courseId) {
      this.errorMessage = 'Nieprawidłowy identyfikator kursu.';
      return;
    }
    this.loadCourseDetails();
  }

  private getAuthHeaders(): HttpHeaders {
    return new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`,
      'Content-Type': 'application/json'
    });
  }

  loadCourseDetails() {
    this.http.get<any>(`http://localhost:5241/api/courses/${this.courseId}`, { headers: this.getAuthHeaders() })
      .subscribe({
        next: (course) => {
          if (!course) {
            this.errorMessage = 'Kurs nie znaleziony.';
            return;
          }
          this.courseTitle = course.title || 'Brak tytułu';
          this.meetingDates = course.meetingDates
            ? course.meetingDates.map((d: string) => new Date(d))
            : [];
          if (this.meetingDates.length > 0) {
            this.selectedDate = this.meetingDates[0].toISOString();
            this.loadAttendance();
          } else {
            this.errorMessage = 'Brak dostępnych dat spotkań dla tego kursu.';
          }
        },
        error: (err) => {
          console.error('Błąd ładowania kursu:', err);
          this.errorMessage = 'Nie udało się załadować danych kursu. Sprawdź połączenie lub zaloguj się ponownie.';
        }
      });
  }

  loadAttendance() {
    if (!this.selectedDate) return;
    this.http.get<Attendance[]>(`http://localhost:5241/api/attendance/${this.courseId}?date=${this.selectedDate}`,
      { headers: this.getAuthHeaders() })
      .subscribe({
        next: (data) => {
          this.attendances = data.map(a => ({
            ...a,
            meetingDate: new Date(a.meetingDate)
          }));
        },
        error: (err) => {
          console.error('Błąd ładowania listy obecności:', err);
          this.errorMessage = 'Nie udało się załadować listy obecności.';
        }
      });
  }

  updateAttendance(attendance: Attendance) {
    this.http.put(`http://localhost:5241/api/attendance/${this.courseId}`,
      { ...attendance, meetingDate: attendance.meetingDate.toISOString() },
      { headers: this.getAuthHeaders() })
      .subscribe({
        next: () => console.log('Obecność zaktualizowana'),
        error: (err) => {
          console.error('Błąd aktualizacji obecności:', err);
          this.errorMessage = 'Nie udało się zaktualizować obecności.';
        }
      });
  }
}
