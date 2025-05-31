using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Project.Core.Exceptions;
using Project.Core.Services;
using Project.Dto.Http;
using Project.Dto.Http.Converters;
using Project.Dto.Http.PositionHistory;
using Swashbuckle.AspNetCore.Annotations;

namespace Project.HttpServer.Controllers;

[ApiController]
[Route("api/positionHistory")]
public class PositionHistoryController : ControllerBase
{
    private readonly IPositionHistoryService _positionHistoryService;
    private readonly ILogger<PositionHistoryController> _logger;

    public PositionHistoryController(ILogger<PositionHistoryController> logger,
        IPositionHistoryService positionHistoryService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _positionHistoryService = positionHistoryService ?? throw new ArgumentNullException(nameof(positionHistoryService));
    }

    [HttpGet("{employeeId:guid}/{positionId:guid}")]
    [SwaggerOperation("getPositionHistoryByEmployeeAndPositionId")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(PositionHistoryDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status404NotFound, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> GetPositionHistory([FromRoute] [Required] Guid employeeId, [FromRoute] [Required] Guid positionId)
    {
        try
        {
            var positionHistory = await _positionHistoryService.GetPositionHistoryAsync(positionId, employeeId);

            return Ok(PositionHistoryConverter.Convert(positionHistory));
        }
        catch (PositionHistoryNotFoundException e)
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
    [SwaggerOperation("createPositionHistory")]
    [SwaggerResponse(StatusCodes.Status201Created, type: typeof(PositionHistoryDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> CreatePositionHistory([FromBody] [Required] CreatePositionHistoryDto newPositionHistory)
    {
        try
        {
            var createdPositionHistory = await _positionHistoryService.AddPositionHistoryAsync(newPositionHistory.PositionId,
                newPositionHistory.EmployeeId,
                newPositionHistory.StartDate,
                newPositionHistory.EndDate);

            return StatusCode(StatusCodes.Status201Created, PositionHistoryConverter.Convert(createdPositionHistory));
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
    [SwaggerOperation("updatePositionHistory")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(PositionHistoryDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> UpdatePositionHistory([FromBody] [Required] UpdatePositionHistoryDto updatePositionHistory)
    {
        try
        {
            var updatedPositionHistory = await _positionHistoryService.UpdatePositionHistoryAsync(updatePositionHistory.PositionId,
                updatePositionHistory.EmployeeId,
                updatePositionHistory.StartDate,
                updatePositionHistory.EndDate);

            return Ok(PositionHistoryConverter.Convert(updatedPositionHistory));
        }
        catch (PositionHistoryNotFoundException e)
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

    [HttpDelete("{employeeId:guid}/{positionId:guid}")]
    [SwaggerOperation("deletePositionHistory")]
    [SwaggerResponse(StatusCodes.Status204NoContent, type: typeof(bool))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> DeletePositionHistory([FromRoute] [Required] Guid positionId, [FromRoute] [Required] Guid employeeId)
    {
        try
        {
            await _positionHistoryService.DeletePositionHistoryAsync(positionId, employeeId);

            return StatusCode(StatusCodes.Status204NoContent);
        }
        catch (PositionHistoryNotFoundException e)
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

    [HttpGet("/positionHistory/{employeeId:guid}")]
    [SwaggerOperation("getPositionHistoriesByEmployeeId")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(PositionHistoryDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> GetPositionHistorysByEmployeeId([FromRoute] [Required] Guid employeeId,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] DateOnly? startDate = null, [FromQuery] DateOnly? endDate = null)
    {
        try
        {
            var positionHistories = await _positionHistoryService.GetPositionHistoryByEmployeeIdAsync(employeeId, pageNumber, pageSize, startDate, endDate);

            return Ok(positionHistories.Items.Select(PositionHistoryConverter.Convert));
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorDto(e.GetType().Name, e.Message));
        }
    }
 
    [HttpGet("/subordinatesPositionHistory/{employeeId:guid}")]
    [SwaggerOperation("getSubordinatesPositionHistoriesByHeadEmployeeId")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(PositionHistoryDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> GetSubordinatesPositionHistoriesByHeadEmployeeId([FromRoute] [Required] Guid employeeId,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] DateOnly? startDate = null, [FromQuery] DateOnly? endDate = null)
    {
        try
        {
            var positionHistories = await _positionHistoryService.GetCurrentSubordinatesPositionHistoryAsync(employeeId, pageNumber, pageSize, startDate, endDate);

            return Ok(positionHistories.Items.Select(PositionHistoryConverter.Convert));
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorDto(e.GetType().Name, e.Message));
        }
    }
    
    [HttpGet("/currentSubordinates/{employeeId:guid}")]
    [SwaggerOperation("getCurrentSubordinatesByHeadEmployeeId")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(PositionHierarchyWithEmployeeDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> GetCurrentSubordinatesByHeadEmployeeId([FromRoute] [Required] Guid employeeId,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var positionHistories = await _positionHistoryService.GetCurrentSubordinatesAsync(employeeId, pageNumber, pageSize);

            return Ok(positionHistories.Items.Select(PositionHierarchyWithEmployeeWithEmployeeConverter.Convert));
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorDto(e.GetType().Name, e.Message));
        }
    }
}