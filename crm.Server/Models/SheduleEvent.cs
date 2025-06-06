namespace crm.Server.Models
{
    public class ScheduleEvent
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? CourseId { get; set; }
        public Course? Course { get; set; }
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
    }
}