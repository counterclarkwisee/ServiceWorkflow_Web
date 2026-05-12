using Microsoft.AspNetCore.Mvc;
using server.Interfaces;
using server.Models;

namespace server.Controllers;

[ApiController]
[Route("api/[controller]")] // This makes the URL: api/users
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    // Dependency Injection: The constructor asks for the Interface, not the Database.
    public UsersController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        try
        {
            var users = await _userRepository.GetAllUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            // Standard practice: Don't expose internal errors to the frontend
            return StatusCode(500, "Internal server error while fetching users.");
        }
    }
}