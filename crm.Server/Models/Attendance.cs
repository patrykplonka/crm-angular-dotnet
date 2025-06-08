namespace crm.Server.Models
{
    public class Attendance
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string CourseId { get; set; } = null!;
        public string StudentId { get; set; } = null!;
        public DateTime MeetingDate { get; set; }
        public bool IsPresent { get; set; }
        public Course? Course { get; set; }
        public ApplicationUser? Student { get; set; }
    }
}