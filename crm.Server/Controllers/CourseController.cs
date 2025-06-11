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

            var courseDtos = courses.Select(course => new CourseDto
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                Instructor = course.Instructor,
                DurationHours = course.DurationHours,
                Link = course.Link,
                Enrolled = enrollments.Contains(course.Id),
                StartDate = course.StartDate,
                EndDate = course.EndDate,
                RecurrencePattern = course.RecurrencePattern,
                MeetingDates = course.MeetingDates,
                RecurrenceDays = ParseRecurrenceDays(course.RecurrencePattern),
                RecurrenceWeeks = ParseRecurrenceWeeks(course.RecurrencePattern),
                StartTime = ParseTime(course.RecurrencePattern, "StartTime"),
                EndTime = ParseTime(course.RecurrencePattern, "EndTime")
            }).ToList();

            return Ok(courseDtos);
        }

        [HttpGet("{courseId}")]
        [Authorize(Roles = "Tutor,Admin")]
        public async Task<ActionResult<CourseDto>> GetCourse(string courseId)
        {
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
            {
                return NotFound("Kurs nie znaleziony.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (course.Instructor != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var courseDto = new CourseDto
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                Instructor = course.Instructor,
                DurationHours = course.DurationHours,
                Link = course.Link,
                Enrolled = false,
                StartDate = course.StartDate,
                EndDate = course.EndDate,
                RecurrencePattern = course.RecurrencePattern,
                MeetingDates = course.MeetingDates,
                RecurrenceDays = ParseRecurrenceDays(course.RecurrencePattern),
                RecurrenceWeeks = ParseRecurrenceWeeks(course.RecurrencePattern),
                StartTime = ParseTime(course.RecurrencePattern, "StartTime"),
                EndTime = ParseTime(course.RecurrencePattern, "EndTime")
            };

            return Ok(courseDto);
        }

        [HttpGet("{courseId}/enrollments")]
        [Authorize(Roles = "Tutor,Admin")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetEnrollments(string courseId)
        {
            var course = await _context.Courses
                .Include(c => c.EnrolledStudents)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
                return NotFound("Kurs nie znaleziony.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (course.Instructor != userId && !User.IsInRole("Admin"))
                return Forbid();

            var userDtos = course.EnrolledStudents.Select(user => new UserDto
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                Role = user.Role,
                FirstName = user.FirstName,
                LastName = user.LastName
            }).ToList();

            return Ok(userDtos);
        }

        [HttpGet("{courseId}/attendance")]
        [Authorize(Roles = "Tutor,Admin")]
        [HttpGet("{courseId}/attendance")]
        [Authorize(Roles = "Tutor,Admin")]
        public async Task<ActionResult<IEnumerable<Attendance>>> GetAttendance(string courseId, [FromQuery] string meetingDate)
        {
            if (!DateTime.TryParse(meetingDate, out var date))
                return BadRequest("Nieprawidłowy format daty.");

            var course = await _context.Courses
                .Include(c => c.EnrolledStudents)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
                return NotFound("Kurs nie znaleziony.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (course.Instructor != userId && !User.IsInRole("Admin"))
                return Forbid();

            var attendances = await _context.Attendances
                .Where(a => a.CourseId == courseId && a.MeetingDate.Date == date.Date)
                .ToListAsync();

            if (!attendances.Any() && course.EnrolledStudents.Any())
            {
                foreach (var student in course.EnrolledStudents)
                {
                    var attendance = new Attendance
                    {
                        Id = Guid.NewGuid().ToString(),
                        CourseId = courseId,
                        StudentId = student.Id,
                        MeetingDate = date,
                        IsPresent = false
                    };
                    _context.Attendances.Add(attendance);
                    attendances.Add(attendance);
                }
                await _context.SaveChangesAsync();
                Console.WriteLine($"Created {attendances.Count} attendance records for {date}");
            }

            return Ok(attendances);
        }

        [HttpPost("{courseId}/attendance")]
        [Authorize(Roles = "Tutor,Admin")]
        public async Task<IActionResult> SaveAttendance(string courseId, [FromBody] List<Attendance> attendances)
        {
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
            {
                return NotFound("Kurs nie znaleziony.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (course.Instructor != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            foreach (var attendance in attendances)
            {
                if (attendance.CourseId != courseId)
                {
                    return BadRequest("Nieprawidłowy CourseId.");
                }

                var existingAttendance = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.Id == attendance.Id && a.CourseId == courseId && a.StudentId == attendance.StudentId && a.MeetingDate.Date == attendance.MeetingDate.Date);

                if (existingAttendance == null)
                {
                    attendance.Id = Guid.NewGuid().ToString();
                    _context.Attendances.Add(attendance);
                }
                else
                {
                    existingAttendance.IsPresent = attendance.IsPresent;
                    existingAttendance.MeetingDate = attendance.MeetingDate;
                    _context.Attendances.Update(existingAttendance);
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Obecność zapisana pomyślnie." });
        }

        [HttpGet("tutor")]
        [Authorize(Roles = "Tutor")]
        public async Task<ActionResult<List<CourseDto>>> GetTutorCourses()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var courses = await _context.Courses
                .Where(c => c.Instructor == userId)
                .ToListAsync();

            var courseDtos = courses.Select(course => new CourseDto
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                Instructor = course.Instructor,
                DurationHours = course.DurationHours,
                Link = course.Link,
                Enrolled = false,
                StartDate = course.StartDate,
                EndDate = course.EndDate,
                RecurrencePattern = course.RecurrencePattern,
                MeetingDates = course.MeetingDates,
                RecurrenceDays = ParseRecurrenceDays(course.RecurrencePattern),
                RecurrenceWeeks = ParseRecurrenceWeeks(course.RecurrencePattern),
                StartTime = ParseTime(course.RecurrencePattern, "StartTime"),
                EndTime = ParseTime(course.RecurrencePattern, "EndTime")
            }).ToList();

            return Ok(courseDtos);
        }

        [HttpPost("{courseId}/assign-tutor")]
        [Authorize(Roles = "Tutor")]
        public async Task<IActionResult> AssignTutor(string courseId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var course = await _context.Courses.FindAsync(courseId);

            if (course == null)
            {
                return NotFound("Kurs nie znaleziony.");
            }

            if (!string.IsNullOrEmpty(course.Instructor))
            {
                return BadRequest("Kurs ma już przypisanego korepetytora.");
            }

            course.Instructor = userId;
            await _context.SaveChangesAsync();

            return Ok(new { CourseId = course.Id, Message = "Korepetytor przypisany do kursu." });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CourseDto>> AddCourse([FromBody] CourseDto courseDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrEmpty(courseDto.Title) || string.IsNullOrEmpty(courseDto.Description) ||
                courseDto.DurationHours <= 0 || string.IsNullOrEmpty(courseDto.RecurrenceDays) ||
                courseDto.RecurrenceWeeks <= 0 || string.IsNullOrEmpty(courseDto.StartTime) ||
                string.IsNullOrEmpty(courseDto.EndTime))
            {
                return BadRequest("Wszystkie wymagane pola muszą być wypełnione.");
            }

            if (!TimeSpan.TryParse(courseDto.StartTime, out _) || !TimeSpan.TryParse(courseDto.EndTime, out _))
            {
                return BadRequest("Nieprawidłowy format dla StartTime lub EndTime.");
            }

            var validDays = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
            var days = courseDto.RecurrenceDays.Split(',').Select(d => d.Trim()).ToList();
            if (days.Any(d => !validDays.Contains(d)))
            {
                return BadRequest("Podano nieprawidłowe dni powtarzania.");
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
        public IActionResult Test()
        {
            return Ok("CourseController is working");
        }

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
                return NotFound("Kurs nie znaleziony");
            }

            var enrollment = await _context.CourseEnrollments
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

            if (action.Action == "enroll")
            {
                if (enrollment != null)
                {
                    return BadRequest("Użytkownik już zapisany na kurs");
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
                    return BadRequest("Użytkownik nie jest zapisany na kurs");
                }

                _context.CourseEnrollments.Remove(enrollment);
            }
            else
            {
                return BadRequest("Nieprawidłowa akcja");
            }

            await _context.SaveChangesAsync();
            return Ok(new { Enrolled = action.Action == "enroll" });
        }

        private List<DateTime> GenerateMeetingDates(CourseDto courseDto)
        {
            var meetingDates = new List<DateTime>();
            if (string.IsNullOrEmpty(courseDto.RecurrenceDays) || courseDto.RecurrenceWeeks == null || courseDto.RecurrenceWeeks <= 0)
            {
                return meetingDates;
            }

            var days = courseDto.RecurrenceDays.Split(',').Select(d => Enum.Parse<DayOfWeek>(d.Trim())).ToList();
            var startDate = courseDto.StartDate;
            var startTime = TimeSpan.Parse(courseDto.StartTime);
            var weeks = courseDto.RecurrenceWeeks.Value;

            for (int i = 0; i < weeks; i++)
            {
                foreach (var day in days)
                {
                    var nextDate = startDate.AddDays(i * 7);
                    while (nextDate.DayOfWeek != day)
                    {
                        nextDate = nextDate.AddDays(1);
                    }
                    meetingDates.Add(nextDate + startTime);
                }
            }

            return meetingDates.OrderBy(d => d).ToList();
        }

        private static string ParseRecurrenceDays(string pattern)
        {
            if (string.IsNullOrEmpty(pattern)) return null;
            var parts = pattern.Split(';');
            var daysPart = parts.FirstOrDefault(p => p.StartsWith("Days:"));
            return daysPart?.Replace("Days:", "") ?? "";
        }

        private static int? ParseRecurrenceWeeks(string pattern)
        {
            if (string.IsNullOrEmpty(pattern)) return null;
            var parts = pattern.Split(';');
            var weeksPart = parts.FirstOrDefault(p => p.StartsWith("DurationWeeks:"));
            if (weeksPart != null && int.TryParse(weeksPart.Replace("DurationWeeks:", ""), out int weeks))
            {
                return weeks;
            }
            return null;
        }

        private static string ParseTime(string pattern, string timeType)
        {
            if (string.IsNullOrEmpty(pattern)) return "";
            var parts = pattern.Split(';');
            var timePart = parts.FirstOrDefault(p => p.StartsWith($"{timeType}:"));
            return timePart != null ? timePart.Replace($"{timeType}:", "") : null;
        }
    }

    public class EnrollmentActionDto
    {
        public string Action { get; set; }
    }

    public class UserDto
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
    public class AttendanceDto
    {
        public string Id { get; set; }
        public string CourseId { get; set; }
        public string StudentId { get; set; }
        public DateTime MeetingDateTime { get; set; }
        public bool IsPresent { get; set; }
    }
}