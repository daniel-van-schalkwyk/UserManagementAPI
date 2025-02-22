using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using System;
using UserManagementAPI.Services;

namespace UserManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private static ConcurrentDictionary<int, User> users = new ConcurrentDictionary<int, User>();
        private static int nextId = 1;
        private readonly ILogger<UserController> _logger;
        private readonly ApiCallTrackingService _trackingService;

        public UserController(ILogger<UserController> logger, ApiCallTrackingService trackingService)
        {
            _logger = logger;
            _trackingService = trackingService;
        }

        [HttpPost("create")]
        public ActionResult<User> CreateUser([FromBody] User user)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("{id}")]
        public ActionResult<User> GetUser(int id)
        {
            try
            {
                if (users.TryGetValue(id, out var user))
                {
                    return user;
                }
                _logger.LogWarning($"User not found with ID: {id}");
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id, [FromBody] User updatedUser)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            try
            {
                if (users.TryRemove(id, out var user))
                {
                    _logger.LogInformation($"User deleted with ID: {id}");
                    return NoContent();
                }
                _logger.LogWarning($"User not found with ID: {id}");
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet]
        public ActionResult<List<User>> GetAllUsers()
        {
            try
            {
                return users.Values.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("apicallcounts")]
        public ActionResult<ConcurrentDictionary<string, int>> GetApiCallCounts()
        {
            return _trackingService.GetApiCallCounts();
        }
    }
}