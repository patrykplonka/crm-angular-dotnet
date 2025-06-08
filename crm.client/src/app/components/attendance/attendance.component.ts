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
  meetingDates: Date[] = [];
  selectedMeetingDate: Date | null = null;
  enrolledStudents: User[] = [];
  attendances: Attendance[] = [];
  errorMessage: string = '';
  debugInfo: string[] = [];

  constructor(
    private http: HttpClient,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router
  ) { }

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      this.courseId = params.get('courseId');
      this.debugInfo.push(`CourseId z URL: ${this.courseId}`);
      this.debugInfo.push(`IsTutor: ${this.authService.isTutor()}`);
      this.debugInfo.push(`Token: ${this.authService.getToken()?.substring(0, 20)}...`);
      console.log('AttendanceComponent Init:', this.debugInfo);

      if (this.courseId && this.authService.isTutor()) {
        this.loadCourseDetails();
      } else {
        this.errorMessage = `Brak dostępu lub nieprawidłowy kurs. courseId: ${this.courseId}, isTutor: ${this.authService.isTutor()}`;
        this.debugInfo.push(this.errorMessage);
        console.error(this.errorMessage);
        if (!this.courseId) {
          this.router.navigate(['/dashboard']);
        }
      }
    });
  }

  private getAuthHeaders(): HttpHeaders {
    const token = this.authService.getToken();
    if (!token) {
      this.errorMessage = 'Brak tokenu autoryzacji.';
      this.debugInfo.push(this.errorMessage);
      console.error(this.errorMessage);
      return new HttpHeaders();
    }
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });
    this.debugInfo.push(`Nagłówki: Authorization=Bearer ${token.substring(0, 20)}...`);
    return headers;
  }

  loadCourseDetails() {
    if (!this.courseId) return;

    this.debugInfo.push(`Wysyłanie GET /api/courses/${this.courseId}`);
    console.log('loadCourseDetails:', this.courseId);
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
        this.debugInfo.push(`Odpowiedź GET /api/courses/${this.courseId}: ${JSON.stringify(data, null, 2)}`);
        console.log('Course Details:', data);
        this.courseTitle = data.title;
        this.meetingDates = data.meetingDates.map(d => new Date(d));
        this.debugInfo.push(`Załadowano ${this.meetingDates.length} dat spotkań`);
        this.selectedMeetingDate = this.meetingDates.find(d => d >= new Date()) || this.meetingDates[0];
        if (this.selectedMeetingDate) {
          this.loadAttendance(this.selectedMeetingDate);
        } else {
          this.debugInfo.push('Brak dostępnych dat spotkań');
          this.errorMessage = 'Brak zaplanowanych spotkań dla tego kursu.';
        }
        this.loadEnrolledStudents();
      },
      error: (err) => {
        const errMsg = `Błąd ładowania szczegółów kursu: Status=${err.status}, Message=${err.error?.message || err.statusText}`;
        this.errorMessage = errMsg;
        this.debugInfo.push(errMsg);
        console.error(errMsg, err);
      }
    });
  }

  loadEnrolledStudents() {
    if (!this.courseId) return;

    this.debugInfo.push(`Wysyłanie GET /api/courses/${this.courseId}/enrollments`);
    console.log('loadEnrolledStudents:', this.courseId);
    this.http.get<User[]>(
      `http://localhost:5241/api/courses/${this.courseId}/enrollments`,
      { headers: this.getAuthHeaders() }
    ).subscribe({
      next: (students) => {
        this.debugInfo.push(`Odpowiedź GET /api/courses/${this.courseId}/enrollments: ${students.length} uczniów`);
        console.log('Enrolled Students:', students);
        this.enrolledStudents = students;
        if (students.length === 0) {
          this.debugInfo.push('Brak zapisanych uczniów na kurs');
          this.errorMessage = 'Brak zapisanych uczniów na ten kurs.';
        }
        if (this.selectedMeetingDate) {
          this.loadAttendance(this.selectedMeetingDate);
        }
      },
      error: (err) => {
        const errMsg = `Błąd ładowania uczniów: Status=${err.status}, Message=${err.error?.message || err.statusText}`;
        this.errorMessage = errMsg;
        this.debugInfo.push(errMsg);
        console.error(errMsg, err);
      }
    });
  }

  loadAttendance(meetingDate: Date) {
    if (!this.courseId) return;

    const dateStr = meetingDate.toISOString().split('T')[0];
    this.debugInfo.push(`Wysyłanie GET /api/courses/${this.courseId}/attendance?meetingDate=${dateStr}`);
    console.log('loadAttendance:', dateStr);
    this.http.get<Attendance[]>(
      `http://localhost:5241/api/courses/${this.courseId}/attendance?meetingDate=${dateStr}`,
      { headers: this.getAuthHeaders() }
    ).subscribe({
      next: (attendances) => {
        this.debugInfo.push(`Odpowiedź GET /api/courses/${this.courseId}/attendance: ${attendances.length} rekordów`);
        console.log('Attendance:', attendances);
        this.attendances = attendances.map(a => ({
          ...a,
          meetingDate: new Date(a.meetingDate)
        }));
        if (attendances.length === 0 && this.enrolledStudents.length > 0) {
          this.debugInfo.push('Inicjalizacja obecności dla uczniów');
          this.attendances = this.enrolledStudents.map(s => ({
            id: crypto.randomUUID(), // Generowanie unikalnego id
            courseId: this.courseId!,
            studentId: s.id,
            meetingDate: meetingDate,
            isPresent: false
          }));
        }
      },
      error: (err) => {
        const errMsg = `Błąd ładowania obecności: Status=${err.status}, Message=${err.error?.message || err.statusText}`;
        this.errorMessage = errMsg;
        this.debugInfo.push(errMsg);
        console.error(errMsg, err);
        if (this.enrolledStudents.length > 0) {
          this.debugInfo.push('Inicjalizacja obecności dla uczniów z powodu błędu');
          this.attendances = this.enrolledStudents.map(s => ({
            id: crypto.randomUUID(), // Generowanie unikalnego id
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
      this.debugInfo.push(this.errorMessage);
      console.error(this.errorMessage);
      return;
    }
    this.debugInfo.push(`Wysyłanie POST /api/courses/${this.courseId}/attendance z ${this.attendances.length} rekordami`);
    console.log('saveAttendance:', this.attendances);
    this.http.post(
      `http://localhost:5241/api/courses/${this.courseId}/attendance`,
      this.attendances.map(a => ({
        ...a,
        meetingDate: a.meetingDate.toISOString()
      })),
      { headers: this.getAuthHeaders() }
    ).subscribe({
      next: () => {
        this.debugInfo.push('Obecność zapisana pomyślnie');
        console.log('Attendance Saved');
        alert('Obecność zapisana pomyślnie!');
        this.errorMessage = '';
      },
      error: (err) => {
        const errMsg = `Błąd zapisywania obecności: Status=${err.status}, Message=${err.error?.message || err.statusText}`;
        this.errorMessage = errMsg;
        this.debugInfo.push(errMsg);
        console.error(errMsg, err);
      }
    });
  }

  onDateChange() {
    if (this.selectedMeetingDate) {
      this.debugInfo.push(`Zmiana daty na: ${this.selectedMeetingDate.toISOString()}`);
      console.log('onDateChange:', this.selectedMeetingDate);
      this.loadAttendance(this.selectedMeetingDate);
    }
  }

  getStudentName(studentId: string): string {
    const student = this.enrolledStudents.find(s => s.id === studentId);
    return student ? `${student.firstName || ''} ${student.lastName || student.username || 'Nieznany'}`.trim() : 'Nieznany';
  }
}
