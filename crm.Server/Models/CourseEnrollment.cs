namespace crm.Server.Models
{
    public class CourseEnrollment
    {
        public string Id { get; set; }
        public string CourseId { get; set; }
        public string UserId { get; set; }
        public virtual Course Course { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}