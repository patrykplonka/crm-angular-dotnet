namespace crm.Server.Models
{
    public class Attendance
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string CourseId { get; set; }
        public DateTime MeetingDate { get; set; }
        public bool Present { get; set; }
        public ApplicationUser User { get; set; }
        public Course Course { get; set; }
    }
}