namespace crm.Server.Models
{
    public class Course
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Instructor { get; set; }
        public int DurationHours { get; set; }
        public string Link { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string RecurrencePattern { get; set; }
        public List<DateTime> MeetingDates { get; set; } = new List<DateTime>();
        public List<ApplicationUser> EnrolledStudents { get; set; } = new();
        public List<Attendance> Attendances { get; set; } = new();
    }
}