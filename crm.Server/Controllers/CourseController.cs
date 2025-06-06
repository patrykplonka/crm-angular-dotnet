using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using crm.Server.Models;
using crm.Server.Data;
using crm.Server.Models.Dto;

namespace crm.Server.Controllers
{
    [Route("api/courses")]
    [ApiController]
    [Authorize] // Require authentication for all endpoints
    public class CourseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CourseController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Student,Tutor,Admin")] // All roles can view courses
        public async Task<ActionResult<List<CourseDto>>> GetCourses()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var courses = await _context.Courses.ToListAsync();
            var enrollments = await _context.CourseEnrollments
                .Where(e => e.UserId == userId)
                .Select(e => e.CourseId)
                .ToListAsync();

            var courseDtos = courses.Select(c => new CourseDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                Instructor = c.Instructor,
                DurationHours = c.DurationHours,
                Link = c.Link,
                Enrolled = enrollments.Contains(c.Id)
            }).ToList();

            return Ok(courseDtos);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")] // Only Admins can add courses
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
                DurationHours = courseDto.DurationHours,
                Link = courseDto.Link
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            courseDto.Id = course.Id;
            courseDto.Enrolled = false;

            return CreatedAtAction(nameof(GetCourses), new { id = course.Id }, courseDto);
        }

        [HttpGet("test")]
        [AllowAnonymous] // Public access for testing
        public IActionResult Test() => Ok("CourseController is working");

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Only Admins can delete courses
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

        [HttpPost("{courseId}/enroll")]
        [Authorize(Roles = "Student")] // Only Students can enroll/unenroll
        public async Task<IActionResult> ToggleEnrollment(string courseId, [FromBody] EnrollmentActionDto action)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var course = await _context.Courses.FindAsync(courseId);

            if (course == null)
            {
                return NotFound("Course not found");
            }

            var enrollment = await _context.CourseEnrollments
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

            if (action.Action == "enroll")
            {
                if (enrollment != null)
                {
                    return BadRequest("User already enrolled");
                }

                enrollment = new CourseEnrollment
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    CourseId = courseId
                };

                _context.CourseEnrollments.Add(enrollment);
            }
            else if (action.Action == "unenroll")
            {
                if (enrollment == null)
                {
                    return BadRequest("User not enrolled");
                }

                _context.CourseEnrollments.Remove(enrollment);
            }
            else
            {
                return BadRequest("Invalid action");
            }

            await _context.SaveChangesAsync();
            return Ok(new { Enrolled = action.Action == "enroll" });
        }
    }

    public class EnrollmentActionDto
    {
        public string Action { get; set; }
    }
}