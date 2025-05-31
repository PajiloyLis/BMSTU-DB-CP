using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Project.Core.Services;
using Project.Dto.Http;
using Project.HttpServer.Controllers;
using Project.Core.Models.Company;
using Project.Dto.Http.Company;
using Project.Core.Exceptions;
using Project.Core.Models;
using Xunit;

namespace Project.Tests.Controllers;

public class CompanyControllerTests
{
    private readonly Mock<ICompanyService> _mockService;
    private readonly Mock<ILogger<CompanyController>> _mockLogger;
    private readonly CompanyController _controller;

    public CompanyControllerTests()
    {
        _mockService = new Mock<ICompanyService>();
        _mockLogger = new Mock<ILogger<CompanyController>>();
        _controller = new CompanyController(_mockLogger.Object, _mockService.Object);
    }

    #region GetCompany Tests
    [Fact]
    public async Task GetCompany_ValidId_ReturnsCompany()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var expectedCompany = new BaseCompany(
            companyId,
            "Test Company",
            new DateOnly(2020, 1, 1),
            "+1234567890",
            "test@test.com",
            "1234567890",
            "123456789",
            "1234567890123",
            "Test Address");

        _mockService.Setup(x => x.GetCompanyByIdAsync(companyId))
            .ReturnsAsync(expectedCompany);

        // Act
        var result = await _controller.GetCompany(companyId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<CompanyDto>(okResult.Value);
        Assert.Equal(companyId, dto.CompanyId);
        Assert.Equal("Test Company", dto.Title);
    }

    [Fact]
    public async Task GetCompany_NotFound_Returns404()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        _mockService.Setup(x => x.GetCompanyByIdAsync(companyId))
            .ThrowsAsync(new CompanyNotFoundException("Not found"));

        // Act
        var result = await _controller.GetCompany(companyId);

        // Assert
        var notFoundResult = Assert.IsType<ObjectResult>(result);
        var errorDto = Assert.IsType<ErrorDto>(notFoundResult.Value);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        Assert.Equal("CompanyNotFoundException", errorDto.ErrorType);
    }
    #endregion

    #region CreateCompany Tests
    [Fact]
    public async Task CreateCompany_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var request = new CreateCompanyDto(
            "New Company",
            new DateOnly(2020, 1, 1),
            "+1234567890",
            "test@test.com",
            "1234567890",
            "123456789",
            "1234567890123",
            "Test Address");

        var expectedCompany = new BaseCompany(
            Guid.NewGuid(),
            request.Title,
            request.RegistrationDate,
            request.PhoneNumber,
            request.Email,
            request.Inn,
            request.Kpp,
            request.Ogrn,
            request.Address);

        _mockService.Setup(x => x.AddCompanyAsync(
            request.Title,
            request.RegistrationDate,
            request.PhoneNumber,
            request.Email,
            request.Inn,
            request.Kpp,
            request.Ogrn,
            request.Address))
            .ReturnsAsync(expectedCompany);

        // Act
        var result = await _controller.CreateCompany(request);

        // Assert
        var createdResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
        var dto = Assert.IsType<CompanyDto>(createdResult.Value);
        Assert.Equal(expectedCompany.CompanyId, dto.CompanyId);
    }

    [Fact]
    public async Task CreateCompany_Duplicate_Returns400()
    {
        // Arrange
        var request = new CreateCompanyDto(
            "New Company",
            new DateOnly(2020, 1, 1),
            "+1234567890",
            "test@test.com",
            "1234567890",
            "123456789",
            "1234567890123",
            "Test Address");

        _mockService.Setup(x => x.AddCompanyAsync(
            It.IsAny<string>(),
            It.IsAny<DateOnly>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()))
            .ThrowsAsync(new CompanyAlreadyExistsException("Duplicate"));

        // Act
        var result = await _controller.CreateCompany(request);

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        var errorDto = Assert.IsType<ErrorDto>(badRequestResult.Value);
    }
    #endregion

    #region UpdateCompany Tests
    [Fact]
    public async Task UpdateCompany_ValidRequest_ReturnsOk()
    {
        // Arrange
        var request = new UpdateCompanyDto(
            Guid.NewGuid(),
            "Updated Name",
            new DateOnly(2020, 1, 1),
            "+1234567890",
            "test@test.com",
            "1234567890",
            "123456789",
            "1234567890123",
            "Updated Address");

        var expectedCompany = new BaseCompany(
            request.CompanyId,
            request.Title!,
            request.RegistrationDate!.Value,
            request.PhoneNumber!,
            request.Email!,
            request.Inn!,
            request.Kpp!,
            request.Ogrn!,
            request.Address!);

        _mockService.Setup(x => x.UpdateCompanyAsync(
            request.CompanyId,
            request.Title,
            request.RegistrationDate,
            request.PhoneNumber,
            request.Email,
            request.Inn,
            request.Kpp,
            request.Ogrn,
            request.Address))
            .ReturnsAsync(expectedCompany);

        // Act
        var result = await _controller.UpdateCompany(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<CompanyDto>(okResult.Value);
        Assert.Equal(request.CompanyId, dto.CompanyId);
        Assert.Equal("Updated Name", dto.Title);
    }

    [Fact]
    public async Task UpdateCompany_NotFound_Returns404()
    {
        // Arrange
        var request = new UpdateCompanyDto(
            Guid.NewGuid(),
            "Updated Name",
            null, null, null, null, null, null, null);

        _mockService.Setup(x => x.UpdateCompanyAsync(
            request.CompanyId,
            request.Title,
            request.RegistrationDate,
            request.PhoneNumber,
            request.Email,
            request.Inn,
            request.Kpp,
            request.Ogrn,
            request.Address))
            .ThrowsAsync(new CompanyNotFoundException("Not found"));

        // Act
        var result = await _controller.UpdateCompany(request);

        // Assert
        var notFoundResult = Assert.IsType<ObjectResult>(result);
        var errorDto = Assert.IsType<ErrorDto>(notFoundResult.Value);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        Assert.Equal("CompanyNotFoundException", errorDto.ErrorType);
    }
    #endregion

    #region DeleteCompany Tests
    [Fact]
    public async Task DeleteCompany_ValidId_Returns204()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        _mockService.Setup(x => x.DeleteCompanyAsync(companyId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteCompany(companyId);

        // Assert
        var okResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, okResult.StatusCode);
    }

    [Fact]
    public async Task DeleteCompany_NotFound_Returns404()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        _mockService.Setup(x => x.DeleteCompanyAsync(companyId))
            .ThrowsAsync(new CompanyNotFoundException("Not found"));

        // Act
        var result = await _controller.DeleteCompany(companyId);

        // Assert
        var notFoundResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        var errorDto = Assert.IsType<ErrorDto>(notFoundResult.Value);
    }
    #endregion

    #region GetCompanies Tests
    [Fact]
    public async Task GetCompanies_ValidRequest_ReturnsPagedList()
    {
        // Arrange
        var companies = new List<BaseCompany>
        {
            new BaseCompany(
                Guid.NewGuid(),
                "Company 1",
                new DateOnly(2020, 1, 1),
                "+1111111111",
                "company1@test.com",
                "1111111111",
                "111111111",
                "1111111111111",
                "Address 1"),
            new BaseCompany(
                Guid.NewGuid(),
                "Company 2",
                new DateOnly(2021, 1, 1),
                "+2222222222",
                "company2@test.com",
                "2222222222",
                "222222222",
                "2222222222222",
                "Address 2")
        };

        var pagedResult = new CompanyPage(companies, new Page(1, 1, 2));

        _mockService.Setup(x => x.GetCompaniesAsync(1, 2))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.GetCompanies(pageNumber: 1, pageSize: 2);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var dtos = Assert.IsAssignableFrom<IEnumerable<CompanyDto>>(okResult.Value);
        Assert.Equal(2, dtos.Count());
    }
    #endregion
}