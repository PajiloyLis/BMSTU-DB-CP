using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Project.Core.Exceptions;
using Project.Core.Models.Score;
using Project.Core.Services;
using Project.Dto.Http;
using Project.Dto.Http.Converters;
using Project.Dto.Http.Score;
using Swashbuckle.AspNetCore.Annotations;

namespace Project.HttpServer.Controllers;

[ApiController]
[Route("api/score")]
public class ScoreController : ControllerBase
{
    private readonly IScoreService _scoreService;
    private readonly ILogger<ScoreController> _logger;

    public ScoreController(ILogger<ScoreController> logger,
        IScoreService scoreService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _scoreService = scoreService ?? throw new ArgumentNullException(nameof(scoreService));
    }

    [HttpGet("{scoreId:guid}")]
    [SwaggerOperation("getScoreById")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(ScoreDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status404NotFound, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> GetScore([FromRoute] [Required] Guid scoreId)
    {
        try
        {
            var score = await _scoreService.GetScoreAsync(scoreId);

            return Ok(ScoreConverter.Convert(score));
        }
        catch (ScoreNotFoundException e)
        {
            _logger.LogWarning(e, e.Message);
            return StatusCode(StatusCodes.Status404NotFound, new ErrorDto(e.GetType().Name, e.Message));
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorDto(e.GetType().Name, e.Message));
        }
    }

    [HttpPost]
    [SwaggerOperation("createScore")]
    [SwaggerResponse(StatusCodes.Status201Created, type: typeof(ScoreDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> CreateScore([FromBody] [Required] CreateScoreDto newScore)
    {
        try
        {
            var createdScore = await _scoreService.AddScoreAsync(newScore.EmployeeId,
                newScore.AuthorId,
                newScore.PositionId,
                newScore.CreatedAt,
                newScore.EfficiencyScore,
                newScore.EngagementScore,
                newScore.CompetencyScore);

            return StatusCode(StatusCodes.Status201Created, ScoreConverter.Convert(createdScore));
        }
        catch (ArgumentException e)
        {
            _logger.LogWarning(e, e.Message);
            return StatusCode(StatusCodes.Status400BadRequest, new ErrorDto(e.GetType().Name, e.Message));
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorDto(e.GetType().Name, e.Message));
        }
    }

    [HttpPut]
    [SwaggerOperation("updateScore")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(ScoreDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> UpdateScore([FromBody] [Required] UpdateScoreDto updateScore)
    {
        try
        {
            var updatedScore = await _scoreService.UpdateScoreAsync(updateScore.Id,
                updateScore.CreatedAt,
                updateScore.EfficiencyScore,
                updateScore.EngagementScore,
                updateScore.CompetencyScore);

            return Ok(ScoreConverter.Convert(updatedScore));
        }
        catch (ScoreNotFoundException e)
        {
            _logger.LogWarning(e, e.Message);
            return StatusCode(StatusCodes.Status404NotFound, new ErrorDto(e.GetType().Name, e.Message));
        }
        catch (ArgumentException e)
        {
            _logger.LogWarning(e, e.Message);
            return StatusCode(StatusCodes.Status400BadRequest, new ErrorDto(e.GetType().Name, e.Message));
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorDto(e.GetType().Name, e.Message));
        }
    }

    [HttpDelete("{scoreId:guid}")]
    [SwaggerOperation("deleteScore")]
    [SwaggerResponse(StatusCodes.Status204NoContent, type: typeof(bool))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> DeleteScore([FromRoute] [Required] Guid scoreId)
    {
        try
        {
            await _scoreService.DeleteScoreAsync(scoreId);

            return StatusCode(StatusCodes.Status204NoContent);
        }
        catch (ScoreNotFoundException e)
        {
            _logger.LogWarning(e, e.Message);
            return StatusCode(StatusCodes.Status404NotFound, new ErrorDto(e.GetType().Name, e.Message));
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorDto(e.GetType().Name, e.Message));
        }
    }

    [HttpGet("/employeeScores/{employeeId:guid}")]
    [SwaggerOperation("getScoresByEmployeeId")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(ScoreDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> GetScoresByEmployeeId([FromRoute] [Required] Guid employeeId,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] DateTimeOffset? startDate = null, [FromQuery] DateTimeOffset? endDate = null)
    {
        try
        {
            var scores = await _scoreService.GetScoresByEmployeeIdAsync(employeeId, pageNumber, pageSize, startDate, endDate);

            return Ok(scores.Items.Select(ScoreConverter.Convert));
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorDto(e.GetType().Name, e.Message));
        }
    }
    
    [HttpGet("{authorId:guid}")]
    [SwaggerOperation("getScoresByAuthorId")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(ScoreDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> GetScoresByAuthorId([FromRoute] [Required] Guid authorId,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] DateTimeOffset? startDate = null, [FromQuery] DateTimeOffset? endDate = null)
    {
        try
        {
            var scores = await _scoreService.GetScoresByAuthorIdAsync(authorId, pageNumber, pageSize, startDate, endDate);

            return Ok(scores.Items.Select(ScoreConverter.Convert));
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorDto(e.GetType().Name, e.Message));
        }
    }
    
    [HttpGet("{positionId:guid}")]
    [SwaggerOperation("getScoresByPositionId")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(ScoreDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> GetScoresByPositionId([FromRoute] [Required] Guid positionId,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] DateTimeOffset? startDate = null, [FromQuery] DateTimeOffset? endDate = null)
    {
        try
        {
            var scores = await _scoreService.GetScoresByPositionIdAsync(positionId, pageNumber, pageSize, startDate, endDate);

            return Ok(scores.Items.Select(ScoreConverter.Convert));
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorDto(e.GetType().Name, e.Message));
        }
    }
    
    [HttpGet("/subordinatesScores/{employeeId:guid}")]
    [SwaggerOperation("getSubordinatesScoresByHeadEmployeeId")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(ScoreDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> GetSubordinatesScoresByHeadEmployeeId([FromRoute] [Required] Guid employeeId,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] DateTimeOffset? startDate = null, [FromQuery] DateTimeOffset? endDate = null)
    {
        try
        {
            var scores = await _scoreService.GetScoresSubordinatesByEmployeeAsync(employeeId, pageNumber, pageSize, startDate, endDate);

            return Ok(scores.Items.Select(ScoreConverter.Convert));
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorDto(e.GetType().Name, e.Message));
        }
    }
}