using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NoCrastinator.Api.Domain;


namespace NoCrastinator.Api.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            // ✅ basic validation
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest("Email is required");

            if (request.Password.Length < 6) // ❌ weak rule intentionally
                return BadRequest("Password too short");

            //  missing proper email format validation (intentional)
            //  missing phone validation (intentional)

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new UserResponse
            {
                Id = user.Id,
                Email = user.Email,
                TotalPoints = user.TotalPoints,
                PhoneNumber = user.PhoneNumber
            });
        }

        [HttpGet]
        public IActionResult GetUsers()
        {
            var users = _userManager.Users
                .Select(u => new UserResponse
                {
                    Id = u.Id,
                    Email = u.Email,
                    TotalPoints = u.TotalPoints,
                    PhoneNumber = u.PhoneNumber
                })
                .ToList();

            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null) return NotFound();

            return Ok(new UserResponse
            {
                Id = user.Id,
                Email = user.Email,
                TotalPoints = user.TotalPoints,
                PhoneNumber = user.PhoneNumber
            });
        }

        //[HttpGet("me")]
        //public IActionResult GetMeDebug()
        //{
        //    var claims = User.Claims.Select(c => $"{c.Type}={c.Value}").ToList();

        //    return Ok(claims);
        //}
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst("userId")?.Value
                ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null) return NotFound();

            return Ok(new UserResponse
            {
                Id = user.Id,
                Email = user.Email,
                TotalPoints = user.TotalPoints
            });
        }
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // INTENTIONAL BUG:
            // goals for this user still exist

            return NoContent();
        }

        [HttpPatch("{id}/phone")]
        public async Task<IActionResult> UpdatePhone(string id, string phone)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // no validation
            user.PhoneNumber = phone;

            await _userManager.UpdateAsync(user);

            return Ok();
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(RegisterRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
                return Unauthorized();

            var valid = await _userManager.CheckPasswordAsync(user, request.Password);

            if (!valid)
                return Unauthorized();

            var claims = new[]
            {
                new System.Security.Claims.Claim("userId", user.Id),
                new System.Security.Claims.Claim("email", user.Email)
            };

            var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes("this_is_a_secure_demo_key_that_is_long_enough_for_hs256"));

            var creds = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                key, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

            var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);
            try
            {
                var jwt = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler()
                    .WriteToken(token);
                return Ok(new { token = jwt });
            }
            catch (Exception ex) {
                return BadRequest(ex);
            }
           
        }


    }

}
