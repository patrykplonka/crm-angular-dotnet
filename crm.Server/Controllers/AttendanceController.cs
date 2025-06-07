using crm.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using crm.Server.Data; 
using crm.Server.Models; 

namespace YourApp.Controllers
{
    [Route("api/courses/{courseId}/attendance")]
    [ApiController]
    [Authorize(Roles = "Tutor")]
    public class AttendanceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/courses/{courseId}/attendance?meetingDate=2025-06-07
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Attendance>>> GetAttendance(string courseId, [FromQuery] DateTime meetingDate)
        {
            var course = await _context.Courses
                .Include(c => c.EnrolledStudents)
                .Include(c => c.Attendances)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
                return NotFound("Kurs nie znaleziony.");

            if (course.Instructor != User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value)
                return Forbid();

            var attendances = await _context.Attendances
                .Where(a => a.CourseId == courseId && a.MeetingDate.Date == meetingDate.Date)
                .ToListAsync();

            // Jeśli brak rekordów obecności, utwórz domyślne dla zapisanych uczniów
            if (!attendances.Any())
            {
                foreach (var student in course.EnrolledStudents)
                {
                    var attendance = new Attendance
                    {
                        CourseId = courseId,
                        StudentId = student.Id,
                        MeetingDate = meetingDate,
                        IsPresent = false
                    };
                    _context.Attendances.Add(attendance);
                    attendances.Add(attendance);
                }
                await _context.SaveChangesAsync();
            }

            return Ok(attendances);
        }

        // POST: api/courses/{courseId}/attendance
        [HttpPost]
        public async Task<IActionResult> SaveAttendance(string courseId, [FromBody] List<Attendance> attendances)
        {
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
                return NotFound("Kurs nie znaleziony.");

            if (course.Instructor != User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value)
                return Forbid();

            foreach (var attendance in attendances)
            {
                if (attendance.CourseId != courseId)
                    return BadRequest("Nieprawidłowy CourseId.");

                var existing = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.CourseId == courseId && a.StudentId == attendance.StudentId && a.MeetingDate.Date == attendance.MeetingDate.Date);

                if (existing != null)
                {
                    existing.IsPresent = attendance.IsPresent;
                    _context.Attendances.Update(existing);
                }
                else
                {
                    _context.Attendances.Add(attendance);
                }
            }

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}