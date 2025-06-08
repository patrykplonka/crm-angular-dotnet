using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
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
    [Authorize] // Wymaga autoryzacji dla wszystkich endpointów
    public class CourseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CourseController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous] // Publiczny dostęp do listy kursów
        public async Task<ActionResult<List<CourseDto>>> GetCourses()
        {
            try
            {
                var courses = await _context.Courses
                    .Select(c => new CourseDto
                    {
                        Id = c.Id,
                        Title = c.Title,
                        Description = c.Description,
                        Instructor = c.Instructor,
                        DurationHours = c.DurationHours
                    })
                    .ToListAsync();
                return Ok(courses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Błąd serwera: {ex.Message}");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")] // Tylko Admin może dodawać kursy
        public async Task<ActionResult<CourseDto>> AddCourse([FromBody] CourseDto courseDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
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
            catch (Exception ex)
            {
                return StatusCode(500, $"Błąd podczas dodawania kursu: {ex.Message}");
            }
        }

        [HttpPost("{id}/enroll")]
        public async Task<IActionResult> Enroll(string id, [FromBody] EnrollRequest request)
        {
            if (string.IsNullOrEmpty(request.Action) || !new[] { "enroll", "unenroll" }.Contains(request.Action.ToLower()))
            {
                return BadRequest("Nieprawidłowa akcja. Użyj 'enroll' lub 'unenroll'.");
            }

            try
            {
                var course = await _context.Courses.FindAsync(id);
                if (course == null)
                {
                    return NotFound("Kurs nie znaleziony.");
                }

                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Użytkownik nie jest zalogowany.");
                }

                var userCourse = await _context.UserCourses
                    .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CourseId == id);

                if (request.Action.ToLower() == "enroll")
                {
                    if (userCourse != null)
                    {
                        return BadRequest("Użytkownik jest już zapisany na ten kurs.");
                    }

                    _context.UserCourses.Add(new UserCourse
                    {
                        UserId = userId,
                        CourseId = id,
                        EnrolledAt = DateTime.UtcNow
                    });
                }
                else // unenroll
                {
                    if (userCourse == null)
                    {
                        return BadRequest("Użytkownik nie jest zapisany na ten kurs.");
                    }

                    _context.UserCourses.Remove(userCourse);
                }

                await _context.SaveChangesAsync();
                return Ok(new { Message = $"Akcja '{request.Action}' zakończona sukcesem." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Błąd podczas zapisu/wypisu: {ex.Message}");
            }
        }

        [HttpGet("test")]
        [AllowAnonymous]
        public IActionResult Test() => Ok("CourseController is working");

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Tylko Admin może usuwać kursy
        public async Task<IActionResult> DeleteCourse(string id)
        {
            try
            {
                var course = await _context.Courses.FindAsync(id);
                if (course == null)
                {
                    return NotFound("Kurs nie znaleziony.");
                }

                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Błąd podczas usuwania kursu: {ex.Message}");
            }
        }
    }

    public class EnrollRequest
    {
        public string Action { get; set; } = string.Empty;
    }
}