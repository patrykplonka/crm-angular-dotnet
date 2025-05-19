import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Course } from '../course-management/course-management.component';

@Component({
  selector: 'app-course-details',
  standalone: true,
  imports: [CommonModule, RouterModule], // Add CommonModule
  templateUrl: './course-details.component.html',
  styleUrls: ['./course-details.component.css']
})
export class CourseDetailsComponent implements OnInit {
  course: Course | null = null;

  constructor(private route: ActivatedRoute, private http: HttpClient) { }

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.http.get<Course>(`http://localhost:5241/api/courses/${id}`).subscribe({
        next: (course) => this.course = course,
        error: (err) => console.error('Błąd ładowania kursu:', err)
      });
    }
  }
}
