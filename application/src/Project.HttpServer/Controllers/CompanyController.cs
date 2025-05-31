using System.ComponentModel.DataAnnotations;
using Project.Dto.Http.Converters;
using Microsoft.AspNetCore.Mvc;
using Project.Core.Exceptions;
using Project.Core.Services;
using Project.Dto.Http;
using Project.Dto.Http.Company;
using Swashbuckle.AspNetCore.Annotations;

namespace Project.HttpServer.Controllers;

[ApiController]
[Route("api/companies")]
public class CompanyController : ControllerBase
{
    private readonly ICompanyService _companyService;
    private readonly ILogger<CompanyController> _logger;

    public CompanyController(ILogger<CompanyController> logger,
        ICompanyService companyService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _companyService = companyService ?? throw new ArgumentNullException(nameof(companyService));
    }

    [HttpGet("{companyId:guid}")]
    [SwaggerOperation("getCompanyById")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(CompanyDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status404NotFound, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> GetCompany([FromRoute] [Required] Guid companyId)
    {
        try
        {
            var company = await _companyService.GetCompanyByIdAsync(companyId);

            return Ok(CompanyConverter.Convert(company));
        }
        catch (CompanyNotFoundException e)
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
    [SwaggerOperation("createCompany")]
    [SwaggerResponse(StatusCodes.Status201Created, type: typeof(CompanyDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> CreateCompany([FromBody] [Required] CreateCompanyDto newCompany)
    {
        try
        {
            var createdCompany = await _companyService.AddCompanyAsync(newCompany.Title,
                newCompany.RegistrationDate,
                newCompany.PhoneNumber,
                newCompany.Email,
                newCompany.Inn,
                newCompany.Kpp,
                newCompany.Ogrn,
                newCompany.Address);

            return StatusCode(StatusCodes.Status201Created, CompanyConverter.Convert(createdCompany));
        }
        catch (CompanyAlreadyExistsException e)
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
    [SwaggerOperation("updateCompany")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(CompanyDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> UpdateCompany([FromBody] [Required] UpdateCompanyDto updateCompany)
    {
        try
        {
            var updatedCompany = await _companyService.UpdateCompanyAsync(updateCompany.CompanyId,
                updateCompany.Title,
                updateCompany.RegistrationDate,
                updateCompany.PhoneNumber,
                updateCompany.Email,
                updateCompany.Inn,
                updateCompany.Kpp,
                updateCompany.Ogrn,
                updateCompany.Address);

            return Ok(CompanyConverter.Convert(updatedCompany));
        }
        catch (CompanyNotFoundException e)
        {
            _logger.LogWarning(e, e.Message);
            return StatusCode(StatusCodes.Status404NotFound, new ErrorDto(e.GetType().Name, e.Message));
        }
        catch (CompanyAlreadyExistsException e)
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

    [HttpDelete("{companyId:guid}")]
    [SwaggerOperation("deleteCompany")]
    [SwaggerResponse(StatusCodes.Status204NoContent, type: typeof(bool))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> DeleteCompany([FromRoute] [Required] Guid companyId)
    {
        try
        {
            await _companyService.DeleteCompanyAsync(companyId);

            return StatusCode(StatusCodes.Status204NoContent);
        }
        catch (CompanyNotFoundException e)
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
    
    [HttpGet]
    [SwaggerOperation("getCompanies")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(IEnumerable<CompanyDto>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> GetCompanies([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var companies = await _companyService.GetCompaniesAsync(pageNumber, pageSize);

            return Ok(companies.Companies.Select(CompanyConverter.Convert));
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorDto(e.GetType().Name, e.Message));
        }
    }
}