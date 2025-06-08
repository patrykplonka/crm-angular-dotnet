import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { AuthService } from '../../services/auth.service';
import { Attendance, User } from '../../models/attendance.model';

@Component({
  selector: 'app-attendance',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, SidebarComponent],
  templateUrl: './attendance.component.html',
  styleUrls: ['./attendance.component.css']
})
export class AttendanceComponent implements OnInit {
  courseId: string | null = null;
  courseTitle: string = '';
  meetingDates: { date: Date; startTime: string; endTime: string }[] = [];
  selectedMeetingDate: Date | null = null;
  enrolledStudents: User[] = [];
  attendances: Attendance[] = [];
  errorMessage: string = '';

  constructor(
    private http: HttpClient,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router
  ) { }

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      this.courseId = params.get('courseId');
      if (this.courseId && this.authService.isTutor()) {
        this.loadCourseDetails();
      } else {
        this.errorMessage = 'Brak dostępu lub nieprawidłowy kurs.';
        if (!this.courseId) {
          this.router.navigate(['/dashboard']);
        }
      }
    });
  }

  private getAuthHeaders(): HttpHeaders {
    const token = this.authService.getToken();
    return token ? new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    }) : new HttpHeaders();
  }

  loadCourseDetails() {
    if (!this.courseId) return;

    this.http.get<{
      id: string;
      title: string;
      description: string;
      instructor: string;
      durationHours: number;
      link: string;
      enrolled: boolean;
      startDate: Date;
      endDate?: Date;
      recurrencePattern: string;
      meetingDates: string[];
      recurrenceDays: string;
      recurrenceWeeks?: number;
      startTime?: string;
      endTime?: string;
    }>(
      `http://localhost:5241/api/courses/${this.courseId}`,
      { headers: this.getAuthHeaders() }
    ).subscribe({
      next: (data) => {
        this.courseTitle = data.title;
        this.meetingDates = data.meetingDates.map(d => ({
          date: new Date(d),
          startTime: data.startTime || 'N/A',
          endTime: data.endTime || 'N/A'
        }));
        this.selectedMeetingDate = this.meetingDates.find(m => m.date >= new Date())?.date || this.meetingDates[0]?.date;
        if (this.selectedMeetingDate) {
          this.loadAttendance(this.selectedMeetingDate);
        } else {
          this.errorMessage = 'Brak zaplanowanych spotkań dla tego kursu.';
        }
        this.loadEnrolledStudents();
      },
      error: (err) => {
        this.errorMessage = `Błąd ładowania szczegółów kursu: ${err.statusText}`;
      }
    });
  }

  loadEnrolledStudents() {
    if (!this.courseId) return;

    this.http.get<User[]>(
      `http://localhost:5241/api/courses/${this.courseId}/enrollments`,
      { headers: this.getAuthHeaders() }
    ).subscribe({
      next: (students) => {
        this.enrolledStudents = students;
        if (students.length === 0) {
          this.errorMessage = 'Brak zapisanych uczniów na ten kurs.';
        }
        if (this.selectedMeetingDate) {
          this.loadAttendance(this.selectedMeetingDate);
        }
      },
      error: (err) => {
        this.errorMessage = `Błąd ładowania uczniów: ${err.statusText}`;
      }
    });
  }

  loadAttendance(meetingDate: Date) {
    if (!this.courseId) return;

    const dateStr = meetingDate.toISOString().split('T')[0];
    this.http.get<Attendance[]>(
      `http://localhost:5241/api/courses/${this.courseId}/attendance?meetingDate=${dateStr}`,
      { headers: this.getAuthHeaders() }
    ).subscribe({
      next: (attendances) => {
        this.attendances = attendances.map(a => ({
          ...a,
          meetingDate: new Date(a.meetingDate)
        }));
        if (attendances.length === 0 && this.enrolledStudents.length > 0) {
          this.attendances = this.enrolledStudents.map(s => ({
            id: crypto.randomUUID(),
            courseId: this.courseId!,
            studentId: s.id,
            meetingDate: meetingDate,
            isPresent: false
          }));
        }
      },
      error: (err) => {
        if (this.enrolledStudents.length > 0) {
          this.attendances = this.enrolledStudents.map(s => ({
            id: crypto.randomUUID(),
            courseId: this.courseId!,
            studentId: s.id,
            meetingDate: meetingDate,
            isPresent: false
          }));
        }
      }
    });
  }

  saveAttendance() {
    if (!this.selectedMeetingDate || !this.courseId) {
      this.errorMessage = 'Wybierz datę spotkania.';
      return;
    }
    this.http.post(
      `http://localhost:5241/api/courses/${this.courseId}/attendance`,
      this.attendances.map(a => ({
        ...a,
        meetingDate: a.meetingDate.toISOString()
      })),
      { headers: this.getAuthHeaders() }
    ).subscribe({
      next: () => {
        alert('Obecność zapisana pomyślnie!');
        this.errorMessage = '';
      },
      error: (err) => {
        this.errorMessage = `Błąd zapisywania obecności: ${err.statusText}`;
      }
    });
  }

  onDateChange() {
    if (this.selectedMeetingDate) {
      this.loadAttendance(this.selectedMeetingDate);
    }
  }

  getStudentName(studentId: string): string {
    const student = this.enrolledStudents.find(s => s.id === studentId);
    return student ? `${student.firstName || ''} ${student.lastName || student.username || 'Nieznany'}`.trim() : 'Nieznany';
  }
}
