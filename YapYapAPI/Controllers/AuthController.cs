using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YapYapAPI.Data;
using YapYapAPI.Models;
using YapYapAPI.Services;
using BCrypt.Net;

namespace YapYapAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly YapYapDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;

        public AuthController(YapYapDbContext context, ITokenService tokenService, IConfiguration configuration)
        {
            _context = context;
            _tokenService = tokenService;
            _configuration = configuration;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if user already exists
            if (await _context.Users.AnyAsync(u => u.Name == registerDto.Name))
            {
                return BadRequest(new { message = "Username already exists" });
            }

            // Hash the password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            var user = new User
            {
                Name = registerDto.Name,
                Password = hashedPassword,
                BIO = registerDto.BIO,
                status_id = registerDto.status_id,
                created_at = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Generate token
            var token = _tokenService.GenerateToken(user);
            var jwtSettings = _configuration.GetSection("JwtSettings").Get<JwtSettings>();

            var response = new AuthResponseDto
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddMinutes(jwtSettings.ExpirationInMinutes),
                User = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    BIO = user.BIO,
                    status_id = user.status_id,
                    created_at = user.created_at
                }
            };

            return Ok(response);
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Name == loginDto.Name);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            // Generate token
            var token = _tokenService.GenerateToken(user);
            var jwtSettings = _configuration.GetSection("JwtSettings").Get<JwtSettings>();

            var response = new AuthResponseDto
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddMinutes(jwtSettings.ExpirationInMinutes),
                User = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    BIO = user.BIO,
                    status_id = user.status_id,
                    created_at = user.created_at
                }
            };

            return Ok(response);
        }
    }
}