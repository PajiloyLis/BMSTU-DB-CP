using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Project.Core.Exceptions;
using Project.Core.Models.Position;
using Project.Core.Services;
using Project.Dto.Http;
using Project.Dto.Http.Converters;
using Project.Dto.Http.Position;
using Swashbuckle.AspNetCore.Annotations;

namespace Project.HttpServer.Controllers;

[ApiController]
[Route("api/position")]
public class PositionController : ControllerBase
{
    private readonly IPositionService _positionService;
    private readonly ILogger<PositionController> _logger;

    public PositionController(ILogger<PositionController> logger,
        IPositionService positionService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _positionService = positionService ?? throw new ArgumentNullException(nameof(positionService));
    }

    [HttpGet("{positionId:guid}")]
    [SwaggerOperation("getPositionById")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(PositionDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status404NotFound, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> GetPosition([FromRoute] [Required] Guid positionId)
    {
        try
        {
            var position = await _positionService.GetPositionByIdAsync(positionId);

            return Ok(PositionConverter.Convert(position));
        }
        catch (PositionNotFoundException e)
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
    [SwaggerOperation("createPosition")]
    [SwaggerResponse(StatusCodes.Status201Created, type: typeof(PositionDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> CreatePosition([FromBody] [Required] CreatePositionDto newPosition)
    {
        try
        {
            var createdPosition = await _positionService.AddPositionAsync(newPosition.ParentId,
                newPosition.Title,
                newPosition.CompanyId);

            return StatusCode(StatusCodes.Status201Created, PositionConverter.Convert(createdPosition));
        }
        catch (PositionAlreadyExistsException e)
        {
            _logger.LogWarning(e, e.Message);
            return StatusCode(StatusCodes.Status400BadRequest, new ErrorDto(e.GetType().Name, e.Message));
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
    [SwaggerOperation("updatePosition")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(PositionDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> UpdatePosition([FromBody] [Required] UpdatePositionDto updatePosition)
    {
        try
        {
            var updatedPosition = await _positionService.UpdatePositionAsync(updatePosition.Id,
                updatePosition.CompanyId,
                updatePosition.ParentId,
                updatePosition.Title);

            return Ok(PositionConverter.Convert(updatedPosition));
        }
        catch (PositionNotFoundException e)
        {
            _logger.LogWarning(e, e.Message);
            return StatusCode(StatusCodes.Status404NotFound, new ErrorDto(e.GetType().Name, e.Message));
        }
        catch (PositionAlreadyExistsException e)
        {
            _logger.LogWarning(e, e.Message);
            return StatusCode(StatusCodes.Status400BadRequest, new ErrorDto(e.GetType().Name, e.Message));
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

    [HttpDelete("{positionId:guid}")]
    [SwaggerOperation("deletePosition")]
    [SwaggerResponse(StatusCodes.Status204NoContent, type: typeof(bool))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> DeletePosition([FromRoute] [Required] Guid positionId)
    {
        try
        {
            await _positionService.DeletePositionAsync(positionId);

            return StatusCode(StatusCodes.Status204NoContent);
        }
        catch (PositionNotFoundException e)
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

    [HttpGet("/subordinates/{positionId:guid}")]
    [SwaggerOperation("getSubordinatesPositionsByHeadPositionId")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(PositionHierarchyDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> GetSubordinatesPositionsByHeadPositionId([FromRoute] [Required] Guid positionId,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var positions = await _positionService.GetSubordinatesAsync(positionId, pageNumber, pageSize);

            return Ok(positions.Items.Select(PositionHierarchyConverter.Convert));
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorDto(e.GetType().Name, e.Message));
        }
    }
}