using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace crm.Server.Models
{
    public class Course
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        public string Instructor { get; set; }
        public int DurationHours { get; set; }
    }
}
