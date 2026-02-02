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
    public class CategoriesController : ControllerBase
    {
        private readonly PortalActividadesDbContext _context;

        public CategoriesController(PortalActividadesDbContext context)
        {
            _context = context;
        }

        // GET: api/Categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            return await _context.Categories
                .OrderByDescending(c => c.Active)
                .ThenBy(c => c.Id)
                .ToListAsync();
        }

        // GET: api/Categories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return category;
        }

        // PUT: api/Categories/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(int id, Category category)
        {
            if (id != category.Id)
            {
                return BadRequest("El ID de la categoría no coincide");
            }

            bool exists = await _context.Categories
                .AnyAsync(c => c.Name.ToLower() == category.Name.ToLower() && c.Id != id);
            if (exists)
            {
                return BadRequest(new { Name = new[] { "Ya existe otra categoría con ese nombre." } });
            }

            _context.Entry(category).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
                    return NotFound("Categoría no encontrada");
                else
                    throw;
            }

            return Ok(new { message = "Categoría actualizada correctamente" });

        }

        // POST: api/Categories
        [HttpPost]
        public async Task<ActionResult<Category>> PostCategory(Category category)
        {
            if (_context.Categories.Any(c => c.Name == category.Name))
            {
                return BadRequest(new { Name = new[] { "Ya existe otra categoría con ese nombre." } });
            }

            _context.Categories.Add(category);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Error al crear la categoría.");
            }

            return CreatedAtAction("GetCategory", new { id = category.Id }, category);
        }

        // PUT: api/Categories/deactivate/{id}
        [HttpPut("deactivate/{id}")]
        public async Task<IActionResult> DeactivateCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound("Categoría no encontrada.");

            category.Active = false;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "Error al desactivar la categoría.");
            }

            return NoContent();
        }

        // GET: api/Categories/search
        [HttpGet("search")]
        public async Task<IActionResult> SearchCategories(string? name)
        {
            var query = _context.Categories
                .Where(c => c.Active == true)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(c => c.Name.Contains(name));

            var categories = await query
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Active
                })
                .ToListAsync();

            if (!categories.Any())
                return NotFound("No se encontraron categorías con los criterios dados.");

            return Ok(categories);
        }


        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
