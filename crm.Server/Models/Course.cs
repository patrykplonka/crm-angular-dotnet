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
    }
}