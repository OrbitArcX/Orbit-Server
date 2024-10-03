using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    // Get all users
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userService.GetUsersAsync();
        return Ok(users);
    }

    // Get user by id
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(string id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }

    // Create a new user
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] User user)
    {
        if (user == null)
        {
            return BadRequest("User data is missing.");
        }

        if (user.Role == "Customer")
        {
            user.ApproveStatus = false;
        }
        else
        {
            user.ApproveStatus = true;
        }

        user.Status = true;
        await _userService.CreateUserAsync(user);

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    // Update an existing user
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] User user)
    {
        var existingUser = await _userService.GetUserByIdAsync(id);
        if (existingUser == null)
        {
            return NotFound();
        }

        existingUser.Status = user.Status;
        existingUser.ApproveStatus = user.ApproveStatus;
        existingUser.Name = user.Name;

        existingUser.UpdatedAt = DateTime.Now;
        await _userService.UpdateUserAsync(id, existingUser);

        return Ok(existingUser);
    }

    // Delete user by id
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var existingUser = await _userService.GetUserByIdAsync(id);
        if (existingUser == null)
        {
            return NotFound();
        }

        await _userService.DeleteUserAsync(id);
        return Ok($"Successfully deleted user with id: {id}");
    }

    // User login
    [HttpPost("login")]
    public async Task<IActionResult> UserLogin(LoginDto loginDto)
    {
        var user = await _userService.UserLogin(loginDto);
        if (user == null)
        {
            return StatusCode(404, "User does not exist with the provided email");
        }

        if (user.Status == false)
        {
            return StatusCode(401, "User account is deactivated");
        }

        if (user.Password != loginDto.Password)
        {
            return StatusCode(401, "Incorrect Password");
        }

        if (user.ApproveStatus == false)
        {
            return StatusCode(403, "Waiting for account activation by CSR... Try again later");
        }

        return Ok(user);
    }

    // Get customer accounts to activate
    [HttpGet("activate/account")]
    public async Task<IActionResult> GetUsersToApproveLogin()
    {
        var users = await _userService.GetUsersToApproveLoginAsync();
        return Ok(users);
    }

    // Get all users by role
    [HttpGet("role/{role}")]
    public async Task<IActionResult> GetUsersByRole(string role)
    {
        var users = await _userService.GetUsersByRoleAsync(role);
        return Ok(users);
    }
}
