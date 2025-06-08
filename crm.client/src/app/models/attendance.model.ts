export interface Attendance {
  id: string;
  courseId: string;
  studentId: string;
  meetingDate: Date;
  isPresent: boolean;
  student?: User;
}

export interface User {
  id: string;
  username: string;
  email: string;
  role?: string;
  firstName?: string;
  lastName?: string;
}
