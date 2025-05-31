using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices.JavaScript;
using Database.Models;
using Microsoft.AspNetCore.Mvc;
using Project.Core.Exceptions;
using Project.Core.Models.PostHistory;
using Project.Core.Services;
using Project.Dto.Http;
using Project.Dto.Http.Converters;
using Project.Dto.Http.PostHistory;
using Swashbuckle.AspNetCore.Annotations;

namespace Project.HttpServer.Controllers;

[ApiController]
[Route("api/postHistory")]
public class PostHistoryController : ControllerBase
{
    private readonly IPostHistoryService _postHistoryService;
    private readonly ILogger<PostHistoryController> _logger;

    public PostHistoryController(ILogger<PostHistoryController> logger,
        IPostHistoryService postHistoryService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _postHistoryService = postHistoryService ?? throw new ArgumentNullException(nameof(postHistoryService));
    }

    [HttpGet("{employeeId:guid}/{postId:guid}")]
    [SwaggerOperation("getPostHistoryByEmployeeAndPostId")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(PostHistoryDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status404NotFound, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> GetPostHistory([FromRoute] [Required] Guid employeeId, [FromRoute] [Required] Guid postId)
    {
        try
        {
            var postHistory = await _postHistoryService.GetPostHistoryAsync(employeeId, postId);

            return Ok(PostHistoryConverter.Convert(postHistory));
        }
        catch (PostHistoryNotFoundException e)
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
    [SwaggerOperation("createPostHistory")]
    [SwaggerResponse(StatusCodes.Status201Created, type: typeof(PostHistoryDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> CreatePostHistory([FromBody] [Required] CreatePostHistoryDto newPostHistory)
    {
        try
        {
            var createdPostHistory = await _postHistoryService.AddPostHistoryAsync(newPostHistory.PostId,
                newPostHistory.EmployeeId,
                newPostHistory.StartDate,
                newPostHistory.EndDate);

            return StatusCode(StatusCodes.Status201Created, PostHistoryConverter.Convert(createdPostHistory));
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
    [SwaggerOperation("updatePostHistory")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(PostHistoryDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> UpdatePostHistory([FromBody] [Required] UpdatePostHistoryDto updatePostHistory)
    {
        try
        {
            var updatedPostHistory = await _postHistoryService.UpdatePostHistoryAsync(updatePostHistory.PostId,
                updatePostHistory.EmployeeId,
                updatePostHistory.StartDate,
                updatePostHistory.EndDate);

            return Ok(PostHistoryConverter.Convert(updatedPostHistory));
        }
        catch (PostHistoryNotFoundException e)
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

    [HttpDelete("{employeeId:guid}/{postId:guid}")]
    [SwaggerOperation("deletePostHistory")]
    [SwaggerResponse(StatusCodes.Status204NoContent, type: typeof(bool))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> DeletePostHistory([FromRoute] [Required] Guid postId, [FromRoute] [Required] Guid employeeId)
    {
        try
        {
            await _postHistoryService.DeletePostHistoryAsync(postId, employeeId);

            return StatusCode(StatusCodes.Status204NoContent);
        }
        catch (PostHistoryNotFoundException e)
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

    [HttpGet("/postHistory/{employeeId:guid}")]
    [SwaggerOperation("getPostHistoriesByEmployeeId")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(PostHistoryDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> GetPostHistorysByEmployeeId([FromRoute] [Required] Guid employeeId,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] DateOnly? startDate = null, [FromQuery] DateOnly? endDate = null)
    {
        try
        {
            var postHistories = await _postHistoryService.GetPostHistoryByEmployeeIdAsync(employeeId, pageNumber, pageSize, startDate, endDate);

            return Ok(postHistories.Items.Select(PostHistoryConverter.Convert));
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorDto(e.GetType().Name, e.Message));
        }
    }
 
    [HttpGet("/subordinatesPostHistory/{employeeId:guid}")]
    [SwaggerOperation("getSubordinatesPostHistoriesByHeadEmployeeId")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(PostHistoryDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> GetSubordinatesPostHistoriesByHeadEmployeeId([FromRoute] [Required] Guid employeeId,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] DateOnly? startDate = null, [FromQuery] DateOnly? endDate = null)
    {
        try
        {
            var postHistories = await _postHistoryService.GetSubordinatesPostHistoryAsync(employeeId, pageNumber, pageSize, startDate, endDate);

            return Ok(postHistories.Items.Select(PostHistoryConverter.Convert));
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorDto(e.GetType().Name, e.Message));
        }
    }
}