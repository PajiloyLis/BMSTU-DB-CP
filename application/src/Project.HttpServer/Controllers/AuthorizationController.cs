using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Project.Core.Exceptions;
using Project.Dto.Http;
using Project.Dto.Http.Converters;
using Project.Dto.Http.User;
using Swashbuckle.AspNetCore.Annotations;
using IAuthorizationService = Project.Core.Services.IAuthorizationService;

namespace Project.HttpServer.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthorizationController : ControllerBase
{
    private readonly ILogger<AuthorizationController> _logger;

    private readonly IAuthorizationService _authorizationService;

    public AuthorizationController(IAuthorizationService authorizationService,
        ILogger<AuthorizationController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    /// <summary>
    /// Logs the user in by validating their credentials and generating access and refresh tokens.
    /// </summary>
    [HttpPost("login")]
    [SwaggerOperation("login")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(AuthorizationDataDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status404NotFound, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> Login([FromBody][Required] LoginDto dto)
    {
        try
        {
            var authData = await _authorizationService.LoginAsync(dto.Email, dto.Password);
            return Ok(AuthorizationDataConverter.Convert(authData));
        }
        catch (InvalidPasswordException ex)
        {
            _logger.LogWarning(ex, $"Invalid password for user: {dto.Email}");
            return BadRequest(new ErrorDto(ex.GetType().Name, ex.Message));
        }
        catch (UserNotFoundException ex)
        {
            _logger.LogWarning(ex, $"User with email: {dto.Email} not found");
            return NotFound(new ErrorDto(ex.GetType().Name, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during login");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorDto(ex.GetType().Name, ex.Message));
        }
    }

    /// <summary>
    /// Registers a new user and returns authentication data including access and refresh tokens.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register")]
    [SwaggerOperation("register")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(AuthorizationDataDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> Register([FromBody] LoginDto dto)
    {
        try
        {
            var authData = await _authorizationService.RegisterAsync(dto.Password,
                dto.Email);
            return Ok(AuthorizationDataConverter.Convert(authData));
        }
        catch (UserNotFoundException ex)
        {
            _logger.LogWarning(ex, $"User with email {dto.Email} already exists");
            return BadRequest(new ErrorDto(ex.GetType().Name, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during registration");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorDto(ex.GetType().Name, ex.Message));
        }
    }
    
    [Authorize(Roles = "employee")]
    [HttpGet("currentUser")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(Guid))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status404NotFound, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> GetCurrentUserId()
    {
        try
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (email is null)
            {
                throw new SecurityTokenException("Invalid token");
            }

            var authData = await _authorizationService.GetCurrentUserIdAsync(email);
            return Ok(authData);
        }
        catch (SecurityTokenException e)
        {
            _logger.LogWarning(e, "Token invalid");
            return BadRequest(new ErrorDto(e.GetType().Name, e.Message));
        }
        catch (UserNotFoundException ex)
        {
            _logger.LogWarning(ex, $"User with not found");
            return NotFound(new ErrorDto(ex.GetType().Name, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during login");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorDto(ex.GetType().Name, ex.Message));
        }
    }
}