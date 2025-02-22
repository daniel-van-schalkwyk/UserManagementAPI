using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace UserManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private static ConcurrentDictionary<int, User> users = new ConcurrentDictionary<int, User>();
        private static int nextId = 1;
        private readonly ILogger<UserController> _logger;

        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        [HttpPost("create")]
        public ActionResult<User> CreateUser([FromBody] User user)
        {
            if (user == null || string.IsNullOrEmpty(user.Name))
            {
                return BadRequest("Invalid user data.");
            }

            user.Id = nextId++;
            users[user.Id] = user;
            _logger.LogInformation($"User created with ID: {user.Id}");
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        [HttpGet("{id}")]
        public ActionResult<User> GetUser(int id)
        {
            if (users.TryGetValue(id, out var user))
            {
                return user;
            }
            _logger.LogWarning($"User not found with ID: {id}");
            return NotFound();
        }

        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id, [FromBody] User updatedUser)
        {
            if (updatedUser == null || string.IsNullOrEmpty(updatedUser.Name))
            {
                return BadRequest("Invalid user data.");
            }

            if (users.TryGetValue(id, out var user))
            {
                user.Name = updatedUser.Name;
                _logger.LogInformation($"User updated with ID: {id}");
                return NoContent();
            }
            _logger.LogWarning($"User not found with ID: {id}");
            return NotFound();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            if (users.TryRemove(id, out var user))
            {
                _logger.LogInformation($"User deleted with ID: {id}");
                return NoContent();
            }
            _logger.LogWarning($"User not found with ID: {id}");
            return NotFound();
        }

        [HttpGet]
        public ActionResult<List<User>> GetAllUsers()
        {
            return users.Values.ToList();
        }
    }
}