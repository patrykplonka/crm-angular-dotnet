using crm.Server.Models.Dto;
using crm.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace TutoringCRM.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager; 
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            Console.WriteLine($"Register request: {System.Text.Json.JsonSerializer.Serialize(model)}");
            if (!ModelState.IsValid)
            {
                var modelStateErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                Console.WriteLine($"ModelState errors: {string.Join(", ", modelStateErrors)}");
                return BadRequest(new { Errors = modelStateErrors });
            }

            var validRoles = new[] { "Student", "Tutor", "Admin" };
            if (!validRoles.Contains(model.Role))
            {
                Console.WriteLine($"Invalid role: {model.Role}");
                return BadRequest(new { Errors = new[] { $"Invalid role: {model.Role}. Allowed roles: {string.Join(", ", validRoles)}" } });
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Role = model.Role 
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var identityErrors = result.Errors.Select(e => e.Description).ToList();
                Console.WriteLine($"Identity errors: {string.Join(", ", identityErrors)}");
                return BadRequest(new { Errors = identityErrors });
            }

            if (!await _roleManager.RoleExistsAsync(model.Role))
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole(model.Role));
                if (!roleResult.Succeeded)
                {
                    var roleErrors = roleResult.Errors.Select(e => e.Description).ToList();
                    Console.WriteLine($"Role creation errors: {string.Join(", ", roleErrors)}");
                    return BadRequest(new { Errors = roleErrors });
                }
            }

            var addToRoleResult = await _userManager.AddToRoleAsync(user, model.Role);
            if (!addToRoleResult.Succeeded)
            {
                var roleErrors = addToRoleResult.Errors.Select(e => e.Description).ToList();
                Console.WriteLine($"Role assignment errors: {string.Join(", ", roleErrors)}");
                return BadRequest(new { Errors = roleErrors });
            }

            Console.WriteLine($"User {user.Email} registered with role {model.Role}");
            return Ok(new { Message = "User registered successfully" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto model)
        {
            Console.WriteLine($"Login request: Email={model.Email}");
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                var token = await GenerateJwtToken(user);
                Console.WriteLine($"Login successful for {user.Email}");
                return Ok(new { Token = token });
            }
            Console.WriteLine($"Login failed for {model.Email}");
            return Unauthorized(new { Errors = new[] { "Invalid login attempt" } });
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "Student"; 

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}