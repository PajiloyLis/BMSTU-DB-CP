using System.ComponentModel.DataAnnotations;
using Database.Models;
using Project.Dto.Http.Converters;
using Microsoft.AspNetCore.Mvc;
using Project.Core.Exceptions;
using Project.Core.Services;
using Project.Dto.Http;
using Project.Dto.Http.Education;
using Swashbuckle.AspNetCore.Annotations;

namespace Project.HttpServer.Controllers;

[ApiController]
[Route("api/education")]
public class EducationController :ControllerBase
{
    private readonly IEducationService _educationService;
    private readonly ILogger<EducationController> _logger;

    public EducationController(ILogger<EducationController> logger,
        IEducationService educationService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _educationService = educationService ?? throw new ArgumentNullException(nameof(educationService));
    }

    [HttpGet("/education/{educationId:guid}")]
    [SwaggerOperation("getEducationById")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(EducationDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status404NotFound, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> GetEducation([FromRoute] [Required] Guid educationId)
    {
        try
        {
            var education = await _educationService.GetEducationByIdAsync(educationId);

            return Ok(EducationConverter.Convert(education));
        }
        catch (EducationNotFoundException e)
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
    [SwaggerOperation("createEducation")]
    [SwaggerResponse(StatusCodes.Status201Created, type: typeof(EducationDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> CreateEducation([FromBody] [Required] CreateEducationDto newEducation)
    {
        try
        {
            var createdEducation = await _educationService.AddEducationAsync(newEducation.EmployeeId,
                newEducation.Institution,
                newEducation.Level,
                newEducation.StudyField,
                newEducation.StartDate,
                newEducation.EndDate);

            return StatusCode(StatusCodes.Status201Created, EducationConverter.Convert(createdEducation));
        }
        catch (EducationAlreadyExistsException e)
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
    [SwaggerOperation("updateEducation")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(EducationDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> UpdateEducation([FromBody] [Required] UpdateEducationDto updateEducation)
    {
        try
        {
            var updatedEducation = await _educationService.UpdateEducationAsync(updateEducation.Id,
                updateEducation.EmployeeId,
                updateEducation.Institution,
                updateEducation.Level,
                updateEducation.StudyField,
                updateEducation.StartDate,
                updateEducation.EndDate);

            return Ok(EducationConverter.Convert(updatedEducation));
        }
        catch (EducationNotFoundException e)
        {
            _logger.LogWarning(e, e.Message);
            return StatusCode(StatusCodes.Status404NotFound, new ErrorDto(e.GetType().Name, e.Message));
        }
        catch (EducationAlreadyExistsException e)
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

    [HttpDelete("{educationId:guid}")]
    [SwaggerOperation("deleteEducation")]
    [SwaggerResponse(StatusCodes.Status204NoContent, type: typeof(bool))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> DeleteEducation([FromRoute] [Required] Guid educationId)
    {
        try
        {
            await _educationService.DeleteEducationAsync(educationId);

            return StatusCode(StatusCodes.Status204NoContent);
        }
        catch (EducationNotFoundException e)
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

    [HttpGet("/employeeEducations/{employeeId:guid}")]
    [SwaggerOperation("getEducationsByEmployeeId")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(EducationDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> GetEducationsByEmployeeId([FromRoute] [Required] Guid employeeId,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var educations = await _educationService.GetEducationsByEmployeeIdAsync(employeeId, pageNumber, pageSize);

            return Ok(educations.Educations.Select(EducationConverter.Convert));
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorDto(e.GetType().Name, e.Message));
        }
    }
}