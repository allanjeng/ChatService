using ChatService.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var username = await reader.ReadToEndAsync();
            
            if (string.IsNullOrWhiteSpace(username))
            {
                _logger.LogWarning("Login attempt with empty username");
                return BadRequest("Username is required");
            }

            var user = await _authService.AuthenticateAsync(username);
            if (user == null)
            {
                _logger.LogInformation("New user registration: {Username}", username);
                user = await _authService.RegisterAsync(username);
            }
            else
            {
                _logger.LogInformation("User authenticated: {Username}", username);
            }
            
            return Ok(new { userId = user.Id, username = user.Username });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return Problem(
                detail: "An error occurred during authentication",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
} 