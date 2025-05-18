using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using crm.Server.Models;
using crm.Server.Data;
using crm.Server.Models.Dto;

namespace crm.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CourseController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<CourseDto>>> GetCourses()
        {
            var courses = await _context.Courses.ToListAsync();
            var courseDtos = courses.Select(c => new CourseDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                Instructor = c.Instructor,
                DurationHours = c.DurationHours
            }).ToList();
            return Ok(courseDtos);
        }

        [HttpPost("{id}/enroll")]
        public async Task<IActionResult> Enroll(string id, [FromBody] EnrollRequest request)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();
            // Logika zapisu/wypisu użytkownika może być dodana tutaj
            return Ok();
        }
    }

    public class EnrollRequest
    {
        public string Action { get; set; }
    }
}