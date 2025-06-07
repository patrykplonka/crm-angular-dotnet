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

namespace crm.Server.Controllers
{
    [Route("api/attendance")]
    [ApiController]
    [Authorize]
    public class AttendanceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{courseId}")]
        [Authorize(Roles = "Tutor,Admin")]
        public async Task<ActionResult<List<AttendanceDto>>> GetAttendance(string courseId, [FromQuery] DateTime date)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
                return NotFound("Course not found");

            var enrollments = await _context.CourseEnrollments
                .Where(e => e.CourseId == courseId)
                .Include(e => e.User)
                .ToListAsync();

            var attendances = await _context.Attendances
                .Where(a => a.CourseId == courseId && a.MeetingDate.Date == date.Date)
                .ToListAsync();

            var result = enrollments.Select(e => {
                var attendance = attendances.FirstOrDefault(a => a.UserId == e.UserId && a.MeetingDate.Date == date.Date);
                return new AttendanceDto
                {
                    Id = attendance?.Id ?? Guid.NewGuid().ToString(),
                    UserId = e.UserId,
                    CourseId = courseId,
                    MeetingDate = date,
                    Present = attendance?.Present ?? false,
                    UserName = $"{e.User.FirstName} {e.User.LastName}"
                };
            }).ToList();

            return Ok(result);
        }

        [HttpPut("{courseId}")]
        [Authorize(Roles = "Tutor,Admin")]
        public async Task<IActionResult> UpdateAttendance(string courseId, [FromBody] AttendanceDto attendanceDto)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
                return NotFound("Course not found");

            var enrollment = await _context.CourseEnrollments
                .FirstOrDefaultAsync(e => e.CourseId == courseId && e.UserId == attendanceDto.UserId);
            if (enrollment == null)
                return BadRequest("User not enrolled in course");

            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.CourseId == courseId && a.UserId == attendanceDto.UserId && a.MeetingDate.Date == attendanceDto.MeetingDate.Date);

            if (attendance == null)
            {
                attendance = new Attendance
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = attendanceDto.UserId,
                    CourseId = courseId,
                    MeetingDate = attendanceDto.MeetingDate,
                    Present = attendanceDto.Present
                };
                _context.Attendances.Add(attendance);
            }
            else
            {
                attendance.Present = attendanceDto.Present;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

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