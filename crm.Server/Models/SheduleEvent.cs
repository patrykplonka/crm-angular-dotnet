namespace crm.Server.Models
{
    public class ScheduleEvent
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string Description { get; set; } = string.Empty;
        public int CourseId { get; set; } // Powiązanie z kursem
        public string UserId { get; set; } = string.Empty; // Powiązanie z ApplicationUser (IdentityUser)
        public Course Course { get; set; } // Nawigacyjna właściwość
        public ApplicationUser User { get; set; } // Nawigacyjna właściwość
    }
}