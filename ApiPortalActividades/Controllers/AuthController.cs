using ApiPortalActividades.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.IdentityModel.Tokens;
using PortalActividades.Data.Contexts;
using PortalActividades.Data.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApiPortalActividades.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly PortalActividadesDbContext _context;

        public AuthController(IConfiguration configuration, PortalActividadesDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel login)
        {
            var usuario = _context.Users.FirstOrDefault(u => u.Email == login.Email);
            if (usuario == null)
                return NotFound(new { message = "Credenciales inválidas" });

            if (!BCrypt.Net.BCrypt.Verify(login.Password, usuario.Password))
                return NotFound(new { message = "Credenciales inválidas" });

            var token = GenerateToken(usuario);
            return Ok(new { token });
        }

        private string GenerateToken(User usuario)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings").
                Get<JwtSettings>();
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("id", usuario.Id.ToString()),
                new Claim("username", usuario.Name),
                new Claim("role", usuario.Role)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings.Issuer,
                audience: jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(jwtSettings.ExpireMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

}