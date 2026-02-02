using ApiPortalActividades.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalActividades.Data.Contexts;
using PortalActividades.Data.Models;

namespace ApiPortalActividades.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizersController : ControllerBase
    {
        private readonly PortalActividadesDbContext _context;

        public OrganizersController(PortalActividadesDbContext context)
        {
            _context = context;
        }

        // GET: api/Organizers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Organizer>>> GetOrganizers()
        {
            return await _context.Organizers.ToListAsync();
        }

        // GET: api/Organizers2
        [HttpGet("Organizers2")]
        public async Task<ActionResult<IEnumerable<OrganizerDto>>> GetOrganizers2()
        {
            var organizers = await _context.Organizers
                .Include(o => o.User)
                .OrderByDescending(o => o.User.Active)
                .ThenBy(o => o.User.Id)
                .Select(o => new OrganizerDto
                {
                    Id = o.User.Id,
                    Name = o.User.Name,
                    Email = o.User.Email,
                    Phone = o.User.Phone,
                    Active = o.User.Active,
                    Department = o.Department,
                    Position = o.Position,
                    Bio = o.Bio,
                    Shifts = o.Shifts,
                    WorkDays = o.WorkDays
                })
                .ToListAsync();

            return Ok(organizers);
        }

        // GET: api/Organizers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<OrganizerDto>> GetOrganizer(int id)
        {
            var organizer = await _context.Organizers
                .Include(o => o.User)
                .Where(o => o.UserId == id)
                .Select(o => new OrganizerDto
                {
                    Id = o.User.Id,
                    Name = o.User.Name,
                    Email = o.User.Email,
                    Phone = o.User.Phone,
                    Active = o.User.Active,
                    Department = o.Department,
                    Position = o.Position,
                    Bio = o.Bio,
                    Shifts = o.Shifts,
                    WorkDays = o.WorkDays
                })
                .FirstOrDefaultAsync();

            if (organizer == null)
                return NotFound("Organizador no encontrado.");

            return Ok(organizer);
        }

        // PUT: api/Organizers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrganizer(int id, [FromBody] OrganizerUpdateDto dto)
        {
            var organizer = await _context.Organizers
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.UserId == id);

            if (organizer == null)
                return NotFound(new { message = "Organizador no encontrado" });

            var user = organizer.User;

            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                bool nameExists = await _context.Users
                    .AnyAsync(u => u.Name.ToLower() == dto.Name.ToLower() && u.Id != user.Id);
                if (nameExists)
                    return BadRequest(new { Name = new[] { "Ya existe otro organizador con ese nombre." } });

                user.Name = dto.Name;
            }

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                bool emailExists = await _context.Users
                    .AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower() && u.Id != user.Id);
                if (emailExists)
                    return BadRequest(new { Email = new[] { "Ya existe otro organizador con ese correo." } });

                user.Email = dto.Email;
            }

            if (!string.IsNullOrWhiteSpace(dto.Phone))
                user.Phone = dto.Phone;

            if (dto.Active.HasValue)
                user.Active = dto.Active.Value;

            if (!string.IsNullOrWhiteSpace(dto.Department))
                organizer.Department = dto.Department;

            if (!string.IsNullOrWhiteSpace(dto.Position))
                organizer.Position = dto.Position;

            if (!string.IsNullOrWhiteSpace(dto.Bio))
                organizer.Bio = dto.Bio;

            if (dto.Shifts != null)
                organizer.Shifts = dto.Shifts;

            if (dto.WorkDays != null)
                organizer.WorkDays = dto.WorkDays;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Organizers.Any(o => o.UserId == id))
                    return NotFound(new { message = "Organizador no encontrado" });
                else
                    throw;
            }

            return Ok(new { message = "Organizador actualizado correctamente" });
        }

        // POST: api/Organizers
        [HttpPost]
        public async Task<IActionResult> PostOrganizer([FromBody] OrganizerCreateDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower()))
                return BadRequest(new { Email = new[] { "Ya existe otro organizador con ese correo." } });

            if (await _context.Users.AnyAsync(u => u.Name.ToLower() == dto.Name.ToLower()))
                return BadRequest(new { Name = new[] { "Ya existe otro organizador con ese nombre." } });

            var plainPassword = dto.Name.Replace(" ", "").ToLower() + "123";
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword);

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                Role = "Organizador",
                Active = true,
                Password = passwordHash
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var organizer = new Organizer
            {
                UserId = user.Id,
                Department = dto.Department,
                Position = dto.Position,
                Bio = dto.Bio,
                Shifts = dto.Shifts,
                WorkDays = dto.WorkDays
            };

            _context.Organizers.Add(organizer);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Organizador creado correctamente",
                defaultPassword = plainPassword
            });
        }

        // PUT: api/Organizers/deactivate/5
        [HttpPut("deactivate/{id}")]
        public async Task<IActionResult> DeactivateOrganizer(int id)
        {
            var organizer = await _context.Organizers
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.UserId == id);

            if (organizer == null)
                return NotFound("Organizador no encontrado.");

            organizer.User.Active = false;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "Error al desactivar el organizador.");
            }

            return NoContent();
        }

        // GET: api/Organizers/search
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<OrganizerDto>>> SearchOrganizers([FromQuery] OrganizerSearchDto filters)
        {
            var query = _context.Organizers
                .Include(o => o.User)
                .Where(o => o.User.Active == true);

            if (!string.IsNullOrWhiteSpace(filters.Name))
                query = query.Where(o => o.User.Name.Contains(filters.Name));

            if (!string.IsNullOrWhiteSpace(filters.Email))
                query = query.Where(o => o.User.Email.Contains(filters.Email));

            if (!string.IsNullOrWhiteSpace(filters.Department))
                query = query.Where(o => o.Department != null && o.Department.Contains(filters.Department));

            if (!string.IsNullOrWhiteSpace(filters.Position))
                query = query.Where(o => o.Position != null && o.Position.Contains(filters.Position));

            if (!string.IsNullOrWhiteSpace(filters.Shift))
                query = query.Where(o => o.Shifts != null && o.Shifts.Contains(filters.Shift));

            var organizers = await query
                .Select(o => new OrganizerDto
                {
                    Id = o.User.Id,
                    Name = o.User.Name,
                    Email = o.User.Email,
                    Phone = o.User.Phone,
                    Active = o.User.Active,
                    Department = o.Department,
                    Position = o.Position,
                    Bio = o.Bio,
                    Shifts = o.Shifts,
                    WorkDays = o.WorkDays
                })
                .ToListAsync();

            return Ok(organizers);
        }

        private bool OrganizerExists(int id)
        {
            return _context.Organizers.Any(o => o.UserId == id);
        }
    }
}
