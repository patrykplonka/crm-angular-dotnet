namespace crm.Server.Models.Dto
{
    public class AttendanceDto
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string CourseId { get; set; }
        public DateTime MeetingDate { get; set; }
        public bool Present { get; set; }
        public string UserName { get; set; }
    }
}