namespace crm.Server.Models
{
    public class CourseEnrollment
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string CourseId { get; set; }
        public ApplicationUser User { get; set; }
        public Course Course { get; set; }
    }
}