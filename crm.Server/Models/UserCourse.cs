using System.ComponentModel.DataAnnotations;

namespace crm.Server.Models
{
    public class UserCourse
    {
        [Key]
        public int Id { get; set; } 
        [Required]
        public string UserId { get; set; }
        [Required]
        public string CourseId { get; set; }
        public DateTime EnrolledAt { get; set; }

        public ApplicationUser User { get; set; }
        public Course Course { get; set; }
    }
}