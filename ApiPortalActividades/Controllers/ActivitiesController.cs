using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalActividades.Data.Contexts;
using PortalActividades.Data.Models;

namespace ApiPortalActividades.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivitiesController : ControllerBase
    {
        private readonly PortalActividadesDbContext _context;

        public ActivitiesController(PortalActividadesDbContext context)
        {
            _context = context;
        }

        // GET: api/Activities
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Activity>>> GetActivities()
        {
            return await _context.Activities.ToListAsync();
        }

        // GET: api/Activities/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Activity>> GetActivity(int id)
        {
            var activity = await _context.Activities.FindAsync(id);

            if (activity == null)
            {
                return NotFound();
            }

            return activity;
        }

        // PUT: api/Activities/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutActivity(int id, Activity activity)
        {
            if (id != activity.Id)
            {
                return BadRequest();
            }

            _context.Entry(activity).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ActivityExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Activities
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Activity>> PostActivity(Activity activity)
        {
            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetActivity", new { id = activity.Id }, activity);
        }

        // PUT: api/Activities/deactivate/{id}
        [HttpPut("deactivate/{id}")]
        public async Task<IActionResult> DeactivateActivity(int id)
        {
            var activity = await _context.Activities.FindAsync(id);
            if (activity == null)
                return NotFound("Actividad no encontrada.");

            activity.Active = false;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "Error al desactivar la actividad.");
            }

            return NoContent();
        }

        // GET: api/Activities/public/{id}
        [HttpGet("public/{id}")]
        public async Task<ActionResult<object>> GetActivityPublic(int id)
        {
            var activity = await _context.Activities
                .Where(a => a.Id == id && a.Active == true)
                .Select(a => new
                {
                    a.Id,
                    a.Title,
                    a.Date,
                    a.StartTime,
                    a.EndTime,
                    a.Location,
                    a.Capacity,
                    Category = new
                    {
                        a.Category.Id,
                        a.Category.Name
                    },
                    Organizer = new
                    {
                        a.Organizer.Id,
                        a.Organizer.Name
                    }
                })
                .FirstOrDefaultAsync();

            if (activity == null)
                return NotFound();

            return Ok(activity);
        }

        // GET: api/Activities/search
        [HttpGet("search")]
        public async Task<IActionResult> SearchActivities(
            string? title,
            int? categoryId,
            int? organizerId,
            string? location,
            DateOnly? date)
        {
            var query = _context.Activities
                .Where(a => a.Active == true)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(title))
                query = query.Where(a => a.Title.Contains(title));

            if (categoryId.HasValue)
                query = query.Where(a => a.CategoryId == categoryId.Value);

            if (organizerId.HasValue)
                query = query.Where(a => a.OrganizerId == organizerId.Value);

            if (!string.IsNullOrWhiteSpace(location))
                query = query.Where(a => a.Location!.Contains(location));

            if (date.HasValue)
                query = query.Where(a => a.Date == date.Value);

            var activities = await query
                .Select(a => new
                {
                    a.Id,
                    a.Title,
                    a.Date,
                    a.StartTime,
                    a.EndTime,
                    a.Location,
                    a.Capacity,
                    Category = new
                    {
                        a.Category.Id,
                        a.Category.Name
                    },
                    Organizer = new
                    {
                        a.Organizer.Id,
                        a.Organizer.Name
                    }
                })
                .ToListAsync();

            if (!activities.Any())
                return NotFound("No se encontraron actividades con los criterios dados.");

            return Ok(activities);
        }


        private bool ActivityExists(int id)
        {
            return _context.Activities.Any(e => e.Id == id);
        }
    }
}
