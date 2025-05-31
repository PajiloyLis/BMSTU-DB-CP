using Database.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Project.Core.Exceptions;
using Project.Core.Models;
using Project.Core.Models.Company;
using Project.Core.Repositories;
using Project.Services.CompanyService;
using StackExchange.Redis;
using Xunit;

namespace Project.Service.Tests;

public class CompanyServiceTests
{
    private readonly CompanyService _companyService;
    private readonly Mock<ILogger<CompanyService>> _mockLogger;
    private readonly Mock<ICompanyRepository> _mockRepository;
    private readonly Mock<IConnectionMultiplexer> _mockRedis;

    public CompanyServiceTests()
    {
        _mockRepository = new Mock<ICompanyRepository>();
        _mockLogger = new Mock<ILogger<CompanyService>>();
        _mockRedis = new Mock<IConnectionMultiplexer>();
        _companyService = new CompanyService(_mockRepository.Object, _mockLogger.Object, _mockRedis.Object);
    }


    [Fact]
    public async Task AddCompany_Successful_1()
    {
        //Arrange
        var companyToAdd = new CreationCompany(
            "Roga  i kopita",
            new DateOnly(1995, 10, 12),
            "+79771234567",
            "roga_i_kopita@mail.ru",
            "1234567890",
            "123456789",
            "1234567890123",
            "г. Москва, ул. Пушкина, д. 57"
        );

        var expectedCompany = new BaseCompany(
            Guid.NewGuid(),
            "Roga  i kopita",
            new DateOnly(1995, 10, 12),
            "+79771234567",
            "roga_i_kopita@mail.ru",
            "1234567890",
            "123456789",
            "1234567890123",
            "г. Москва, ул. Пушкина, д. 57"
        );

        _mockRepository.Setup(expr => expr.AddCompanyAsync(It.IsAny<CreationCompany>())).ReturnsAsync(expectedCompany);
        //Act
        var result = await _companyService.AddCompanyAsync("Roga  i kopita",
            new DateOnly(1995, 10, 12),
            "+79771234567",
            "roga_i_kopita@mail.ru",
            "1234567890",
            "123456789",
            "1234567890123",
            "г. Москва, ул. Пушкина, д. 57");

        //Assert
        Assert.NotNull(result);
        Assert.Equal(expectedCompany.CompanyId, result.CompanyId);
        Assert.Equal(expectedCompany.Title, result.Title);
        Assert.Equal(expectedCompany.RegistrationDate, result.RegistrationDate);
        Assert.Equal(expectedCompany.PhoneNumber, result.PhoneNumber);
        Assert.Equal(expectedCompany.Email, result.Email);
        Assert.Equal(expectedCompany.Address, result.Address);
        Assert.Equal(expectedCompany.Inn, result.Inn);
        Assert.Equal(expectedCompany.Kpp, result.Kpp);
        Assert.Equal(expectedCompany.Ogrn, result.Ogrn);
        _mockRepository.Verify(expr => expr.AddCompanyAsync(It.IsAny<CreationCompany>()), Times.Once);
    }

    [Fact]
    public async Task GetCompanyById_Successful()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var expectedCompany = new BaseCompany(
            companyId,
            "Test Company",
            new DateOnly(2020, 1, 1),
            "+79771234567",
            "test@mail.ru",
            "1234567890",
            "123456789",
            "1234567890123",
            "Test Address"
        );

        _mockRepository.Setup(x => x.GetCompanyByIdAsync(companyId))
            .ReturnsAsync(expectedCompany);

        // Act
        var result = await _companyService.GetCompanyByIdAsync(companyId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(companyId, result.CompanyId);
        Assert.Equal(expectedCompany.Title, result.Title);
        _mockRepository.Verify(x => x.GetCompanyByIdAsync(companyId), Times.Once);
    }

    [Fact]
    public async Task GetCompanyById_NotFound()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        _mockRepository.Setup(x => x.GetCompanyByIdAsync(companyId))
            .ThrowsAsync(new CompanyNotFoundException());

        // Act & Assert
        await Assert.ThrowsAsync<CompanyNotFoundException>(() =>
            _companyService.GetCompanyByIdAsync(companyId));
        _mockRepository.Verify(x => x.GetCompanyByIdAsync(companyId), Times.Once);
    }

    [Fact]
    public async Task UpdateCompany_Successful()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var updateCompany = new UpdateCompany(
            companyId,
            "Updated Company",
            new DateOnly(2021, 1, 1),
            "+79771234568",
            "updated@mail.ru",
            "0987654321",
            "987654321",
            "3210987654321",
            "Updated Address"
        );

        var expectedCompany = new BaseCompany(
            companyId,
            updateCompany.Title!,
            updateCompany.RegistrationDate!.Value,
            updateCompany.PhoneNumber!,
            updateCompany.Email!,
            updateCompany.Inn!,
            updateCompany.Kpp!,
            updateCompany.Ogrn!,
            updateCompany.Address!
        );

        _mockRepository.Setup(x => x.UpdateCompanyAsync(It.IsAny<UpdateCompany>()))
            .ReturnsAsync(expectedCompany);

        // Act
        var result = await _companyService.UpdateCompanyAsync(
            companyId,
            updateCompany.Title,
            updateCompany.RegistrationDate,
            updateCompany.PhoneNumber,
            updateCompany.Email,
            updateCompany.Inn,
            updateCompany.Kpp,
            updateCompany.Ogrn,
            updateCompany.Address
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedCompany.Title, result.Title);
        Assert.Equal(expectedCompany.RegistrationDate, result.RegistrationDate);
        Assert.Equal(expectedCompany.PhoneNumber, result.PhoneNumber);
        _mockRepository.Verify(x => x.UpdateCompanyAsync(It.IsAny<UpdateCompany>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCompany_NotFound()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        _mockRepository.Setup(x => x.UpdateCompanyAsync(It.IsAny<UpdateCompany>()))
            .ThrowsAsync(new CompanyNotFoundException());

        // Act & Assert
        await Assert.ThrowsAsync<CompanyNotFoundException>(() =>
            _companyService.UpdateCompanyAsync(companyId, "New Title", null, null, null, null, null, null, null));
        _mockRepository.Verify(x => x.UpdateCompanyAsync(It.IsAny<UpdateCompany>()), Times.Once);
    }

    [Fact]
    public async Task GetCompanies_Successful()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 10;
        var expectedCompanies = new List<BaseCompany>
        {
            new(
                Guid.NewGuid(),
                "Company 1",
                new DateOnly(2020, 1, 1),
                "+79771234567",
                "company1@mail.ru",
                "1234567890",
                "123456789",
                "1234567890123",
                "Address 1"
            ),
            new(
                Guid.NewGuid(),
                "Company 2",
                new DateOnly(2020, 2, 1),
                "+79771234568",
                "company2@mail.ru",
                "0987654321",
                "987654321",
                "3210987654321",
                "Address 2"
            )
        };


        var expectedPage = new CompanyPage
        (
            expectedCompanies,
            new Page(
                pageNumber,
                2,
                10
            )
        );
        _mockRepository.Setup(x => x.GetCompaniesAsync(pageNumber, pageSize))
            .ReturnsAsync(expectedPage);

        // Act
        var result = await _companyService.GetCompaniesAsync(pageNumber, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedPage.Page.TotalItems, result.Page.TotalItems);
        Assert.Equal(expectedPage.Companies.Count, result.Companies.Count);
        _mockRepository.Verify(x => x.GetCompaniesAsync(pageNumber, pageSize), Times.Once);
    }

    [Fact]
    public async Task DeleteCompany_Successful()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        _mockRepository.Setup(x => x.DeleteCompanyAsync(companyId))
            .Returns(Task.CompletedTask);

        // Act
        await _companyService.DeleteCompanyAsync(companyId);

        // Assert
        _mockRepository.Verify(x => x.DeleteCompanyAsync(companyId), Times.Once);
    }

    [Fact]
    public async Task AddCompany_AlreadyExists()
    {
        // Arrange
        var companyToAdd = new CreationCompany(
            "Existing Company",
            new DateOnly(1995, 10, 12),
            "+79771234567",
            "existing@mail.ru",
            "1234567890",
            "123456789",
            "1234567890123",
            "Test Address"
        );

        _mockRepository.Setup(x => x.AddCompanyAsync(It.IsAny<CreationCompany>()))
            .ThrowsAsync(new CompanyAlreadyExistsException());

        // Act & Assert
        await Assert.ThrowsAsync<CompanyAlreadyExistsException>(() =>
            _companyService.AddCompanyAsync(
                companyToAdd.Title,
                companyToAdd.RegistrationDate,
                companyToAdd.PhoneNumber,
                companyToAdd.Email,
                companyToAdd.Inn,
                companyToAdd.Kpp,
                companyToAdd.Ogrn,
                companyToAdd.Address
            ));
        _mockRepository.Verify(x => x.AddCompanyAsync(It.IsAny<CreationCompany>()), Times.Once);
    }
}