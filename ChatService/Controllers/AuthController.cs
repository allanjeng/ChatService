using ChatService.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService, ILogger<AuthController> logger) : ControllerBase
{
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
                logger.LogWarning("Login attempt with empty username");
                return BadRequest("Username is required");
            }

            var user = await authService.AuthenticateAsync(username);
            if (user == null)
            {
                logger.LogInformation("New user registration: {Username}", username);
                user = await authService.RegisterAsync(username);
            }
            else
            {
                logger.LogInformation("User authenticated: {Username}", username);
            }
            
            return Ok(new { userId = user.Id, username = user.Username });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during login");
            return Problem(
                detail: "An error occurred during authentication",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
} 