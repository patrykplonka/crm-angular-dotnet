namespace crm.Server.Models.Dto
{
    public class CourseDto
    {
        public string? Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Instructor { get; set; }
        public int DurationHours { get; set; }
        public string? Link { get; set; }
        public bool Enrolled { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? RecurrencePattern { get; set; }
        public List<DateTime> MeetingDates { get; set; } = new List<DateTime>();
        public string? RecurrenceDays { get; set; } 
        public int? RecurrenceWeeks { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
    }
}