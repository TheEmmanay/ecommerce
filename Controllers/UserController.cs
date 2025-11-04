using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ecommerce.Entities;
using ecommerce.Services;

namespace ecommerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: api/User
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ecommerce.DTOs.UserResponse>>> GetUsers()
        {
            var users = await _userService.GetAllUsers();
            var dto = users.Select(u => new ecommerce.DTOs.UserResponse(u.Id, u.FullName, u.Email, u.Role, u.CreatedAt));
            return Ok(dto);
        }

        // GET: api/User/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ecommerce.DTOs.UserResponse>> GetUser(int id)
        {
            var user = await _userService.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }
            var dto = new ecommerce.DTOs.UserResponse(user.Id, user.FullName, user.Email, user.Role, user.CreatedAt);
            return Ok(dto);
        }

        // POST: api/User
        [HttpPost]
        public async Task<ActionResult<ecommerce.DTOs.UserResponse>> CreateUser([FromBody] ecommerce.DTOs.CreateUserRequest request)
        {
            try
            {
                var user = new ecommerce.Entities.User
                {
                    FullName = request.FullName,
                    Email = request.Email.ToLowerInvariant(),
                    PasswordHash = request.Password,
                    Role = request.Role,
                    CreatedAt = DateTime.UtcNow
                };

                var createdUser = await _userService.CreateUser(user);
                var dto = new ecommerce.DTOs.UserResponse(createdUser.Id, createdUser.FullName, createdUser.Email, createdUser.Role, createdUser.CreatedAt);
                return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, dto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/User/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] ecommerce.DTOs.UpdateUserRequest request)
        {
            try
            {
                var user = await _userService.GetUserById(id);
                if (user == null) return NotFound();

                user.FullName = request.FullName;
                user.Email = request.Email.ToLowerInvariant();
                user.Role = request.Role;

                if (!string.IsNullOrEmpty(request.Password))
                {
                    user.PasswordHash = request.Password;
                }
                await _userService.UpdateUser(user, !string.IsNullOrEmpty(request.Password));
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        // DELETE: api/User/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                await _userService.DeleteUser(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}