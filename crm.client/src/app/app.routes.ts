import { Routes } from '@angular/router';
import { LoginComponent } from './components/login/login.component';
import { RegisterComponent } from './components/register/register.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { CourseManagementComponent } from './components/course-management/course-management.component';
import { AttendanceComponent } from './components/attendance/attendance.component';

export const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'dashboard', component: DashboardComponent },
  { path: '**', redirectTo: '/login' },
  { path: 'courses', component: CourseManagementComponent },
  { path: 'attendance', component: AttendanceComponent },
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' }, 
];
