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
    [Route("api/courses")]
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

        [HttpPost]
        public async Task<ActionResult<CourseDto>> AddCourse([FromBody] CourseDto courseDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var course = new Course
            {
                Id = Guid.NewGuid().ToString(),
                Title = courseDto.Title,
                Description = courseDto.Description,
                Instructor = courseDto.Instructor,
                DurationHours = courseDto.DurationHours
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCourses), new { id = course.Id }, courseDto);
        }

        [HttpPost("{id}/enroll")]
        public async Task<IActionResult> Enroll(string id, [FromBody] EnrollRequest request)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();
            // Logika zapisu/wypisu użytkownika może być dodana tutaj
            return Ok();
        }

        [HttpGet("test")]
        public IActionResult Test() => Ok("CourseController is working");

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(string id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }


    public class EnrollRequest
    {
        public string Action { get; set; }
    }
}