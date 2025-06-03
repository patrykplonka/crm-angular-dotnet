using Microsoft.AspNetCore.Mvc;
using crm.Server.Data;
using crm.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace crm.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ScheduleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Pobierz wszystkie wydarzenia
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.ScheduleEvents
                .Include(e => e.Course)
                .Include(e => e.User)
                .ToListAsync();
            return Ok(events);
        }

        // Dodaj nowe wydarzenie
        [HttpPost]
        public async Task<IActionResult> AddEvent([FromBody] ScheduleEvent eventData)
        {
            if (eventData == null || eventData.StartDateTime >= eventData.EndDateTime)
            {
                return BadRequest("Invalid event data");
            }

            _context.ScheduleEvents.Add(eventData);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetEvents), new { id = eventData.Id }, eventData);
        }

        // Usuń wydarzenie (opcjonalne)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var eventToDelete = await _context.ScheduleEvents.FindAsync(id);
            if (eventToDelete == null)
            {
                return NotFound();
            }

            _context.ScheduleEvents.Remove(eventToDelete);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}