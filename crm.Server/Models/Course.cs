namespace crm.Server.Models
{
    public class Course
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Instructor { get; set; }
        public int DurationHours { get; set; }
        public List<UserCourse> Enrollments { get; set; } = new List<UserCourse>();
    }

    public class UserCourse
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public string CourseId { get; set; }
        public Course Course { get; set; }
    }
}