namespace crm.Server.Models.Dto
{
    public class CourseDto
    {
        public string? Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Instructor { get; set; }
        public int DurationHours { get; set; }
    }
}