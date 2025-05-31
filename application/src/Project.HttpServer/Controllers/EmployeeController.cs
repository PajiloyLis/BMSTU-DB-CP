using System.ComponentModel.DataAnnotations;
using Database.Models;
using Microsoft.AspNetCore.Mvc;
using Project.Core.Exceptions;
using Project.Core.Services;
using Project.Dto.Http;
using Project.Dto.Http.Converters;
using Project.Dto.Http.Employee;
using Swashbuckle.AspNetCore.Annotations;

namespace Project.HttpServer.Controllers;

[ApiController]
[Route("api/employees")]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<EmployeeController> _logger;

    public EmployeeController(ILogger<EmployeeController> logger,
        IEmployeeService emploteeService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _employeeService = emploteeService ?? throw new ArgumentNullException(nameof(emploteeService));
    }

    [HttpGet("{employeeId:guid}")]
    [SwaggerOperation("getEmployeeById")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(EmployeeDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status404NotFound, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> GetEmployee([FromRoute] [Required] Guid employeeId)
    {
        try
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);

            return Ok(EmployeeConverter.Convert(employee));
        }
        catch (EmployeeNotFoundException e)
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
    [SwaggerOperation("createEmployee")]
    [SwaggerResponse(StatusCodes.Status201Created, type: typeof(EmployeeDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> CreateEmployee([FromBody] [Required] CreateEmployeeDto newEmployee)
    {
        try
        {
            var createdEmployee = await _employeeService.AddEmployeeAsync(newEmployee.FullName,
                newEmployee.PhoneNumber,
                newEmployee.Email,
                newEmployee.Birthday,
                newEmployee.PhotoPath,
                newEmployee.Duties);

            return StatusCode(StatusCodes.Status201Created, EmployeeConverter.Convert(createdEmployee));
        }
        catch (EmployeeAlreadyExistsException e)
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
    [SwaggerOperation("updateEmployee")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(EmployeeDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> UpdateEmployee([FromBody] [Required] UpdateEmployeeDto updateEmployee)
    {
        try
        {
            var updatedEmployee = await _employeeService.UpdateEmployeeAsync(updateEmployee.Id,
                updateEmployee.FullName,
                updateEmployee.PhoneNumber,
                updateEmployee.Email,
                updateEmployee.Birthday,
                updateEmployee.PhotoPath,
                updateEmployee.Duties);

            return Ok(EmployeeConverter.Convert(updatedEmployee));
        }
        catch (EmployeeNotFoundException e)
        {
            _logger.LogWarning(e, e.Message);
            return StatusCode(StatusCodes.Status404NotFound, new ErrorDto(e.GetType().Name, e.Message));
        }
        catch (EmployeeAlreadyExistsException e)
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

    [HttpDelete("{employeeId:guid}")]
    [SwaggerOperation("deleteEmployee")]
    [SwaggerResponse(StatusCodes.Status204NoContent, type: typeof(bool))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> DeleteEmployee([FromRoute] [Required] Guid employeeId)
    {
        try
        {
            await _employeeService.DeleteEmployeeAsync(employeeId);

            return StatusCode(StatusCodes.Status204NoContent);
        }
        catch (EmployeeNotFoundException e)
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
}