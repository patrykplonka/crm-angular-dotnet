using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
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
    [Authorize]
    public class CourseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CourseController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Student,Tutor,Admin")]
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
                Enrolled = enrollments.Contains(c.Id),
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                RecurrencePattern = c.RecurrencePattern,
                MeetingDates = c.MeetingDates,
                RecurrenceDays = ParseRecurrenceDays(c.RecurrencePattern),
                RecurrenceWeeks = ParseRecurrenceWeeks(c.RecurrencePattern),
                StartTime = ParseTime(c.RecurrencePattern, "StartTime"),
                EndTime = ParseTime(c.RecurrencePattern, "EndTime")
            }).ToList();

            return Ok(courseDtos);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CourseDto>> AddCourse([FromBody] CourseDto courseDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate required fields
            if (string.IsNullOrEmpty(courseDto.Title) || string.IsNullOrEmpty(courseDto.Description) ||
                courseDto.DurationHours <= 0 || string.IsNullOrEmpty(courseDto.RecurrenceDays) ||
                courseDto.RecurrenceWeeks <= 0 || string.IsNullOrEmpty(courseDto.StartTime) ||
                string.IsNullOrEmpty(courseDto.EndTime))
            {
                return BadRequest("All required fields must be provided.");
            }

            // Validate time format
            if (!TimeSpan.TryParse(courseDto.StartTime, out _) || !TimeSpan.TryParse(courseDto.EndTime, out _))
            {
                return BadRequest("Invalid time format for StartTime or EndTime.");
            }

            // Validate recurrence days
            var validDays = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
            var days = courseDto.RecurrenceDays.Split(',').Select(d => d.Trim()).ToList();
            if (days.Any(d => !validDays.Contains(d)))
            {
                return BadRequest("Invalid recurrence days provided.");
            }

            var course = new Course
            {
                Id = Guid.NewGuid().ToString(),
                Title = courseDto.Title,
                Description = courseDto.Description,
                Instructor = courseDto.Instructor,
                DurationHours = courseDto.DurationHours,
                Link = courseDto.Link,
                StartDate = courseDto.StartDate,
                EndDate = courseDto.EndDate,
                RecurrencePattern = $"Weekly;Days:{courseDto.RecurrenceDays};DurationWeeks:{courseDto.RecurrenceWeeks};StartTime:{courseDto.StartTime};EndTime:{courseDto.EndTime}",
                MeetingDates = GenerateMeetingDates(courseDto)
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            courseDto.Id = course.Id;
            courseDto.Enrolled = false;
            courseDto.MeetingDates = course.MeetingDates;

            return CreatedAtAction(nameof(GetCourses), new { id = course.Id }, courseDto);
        }

        [HttpGet("test")]
        [AllowAnonymous]
        public IActionResult Test() => Ok("CourseController is working");

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Student")]
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

        private List<DateTime> GenerateMeetingDates(CourseDto courseDto)
        {
            var meetingDates = new List<DateTime>();
            if (string.IsNullOrEmpty(courseDto.RecurrenceDays) || courseDto.RecurrenceWeeks == null || courseDto.RecurrenceWeeks <= 0)
                return meetingDates;

            var days = courseDto.RecurrenceDays.Split(',').Select(d => Enum.Parse<DayOfWeek>(d.Trim())).ToList();
            var startDate = courseDto.StartDate.Date;
            var startTime = TimeSpan.Parse(courseDto.StartTime);
            var weeks = courseDto.RecurrenceWeeks.Value;

            for (int week = 0; week < weeks; week++)
            {
                foreach (var day in days)
                {
                    var nextDate = startDate.AddDays(week * 7);
                    while (nextDate.DayOfWeek != day)
                        nextDate = nextDate.AddDays(1);
                    meetingDates.Add(nextDate + startTime);
                }
            }

            return meetingDates.OrderBy(d => d).ToList();
        }

        private string ParseRecurrenceDays(string pattern)
        {
            if (string.IsNullOrEmpty(pattern)) return "";
            var parts = pattern.Split(';');
            var daysPart = parts.FirstOrDefault(p => p.StartsWith("Days:"));
            return daysPart?.Replace("Days:", "") ?? "";
        }

        private int? ParseRecurrenceWeeks(string pattern)
        {
            if (string.IsNullOrEmpty(pattern)) return null;
            var parts = pattern.Split(';');
            var weeksPart = parts.FirstOrDefault(p => p.StartsWith("DurationWeeks:"));
            if (weeksPart != null && int.TryParse(weeksPart.Replace("DurationWeeks:", ""), out int weeks))
                return weeks;
            return null;
        }

        private string ParseTime(string pattern, string timeType)
        {
            if (string.IsNullOrEmpty(pattern)) return "";
            var parts = pattern.Split(';');
            var timePart = parts.FirstOrDefault(p => p.StartsWith($"{timeType}:"));
            return timePart?.Replace($"{timeType}:", "") ?? "";
        }
    }

    public class EnrollmentActionDto
    {
        public string Action { get; set; }
    }
}