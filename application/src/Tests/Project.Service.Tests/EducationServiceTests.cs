using Microsoft.Extensions.Logging;
using Moq;
using Project.Core.Exceptions;
using Project.Core.Models;
using Project.Core.Models.Education;
using Project.Core.Repositories;
using Project.Services.EducationService;
using Xunit;

namespace Project.Service.Tests;

public class EducationServiceTests
{
    private readonly EducationService _educationService;
    private readonly Mock<ILogger<EducationService>> _mockLogger;
    private readonly Mock<IEducationRepository> _mockRepository;

    public EducationServiceTests()
    {
        _mockRepository = new Mock<IEducationRepository>();
        _mockLogger = new Mock<ILogger<EducationService>>();
        _educationService = new EducationService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task AddEducation_Successful()
    {
        //Arrange
        var employeeId = Guid.NewGuid();
        var educationToAdd = new CreateEducation(
            employeeId,
            "МГУ",
            "Высшее (бакалавриат)",
            "Информатика",
            new DateOnly(2020, 9, 1),
            new DateOnly(2024, 6, 30)
        );

        var expectedEducation = new BaseEducation(
            Guid.NewGuid(),
            employeeId,
            "МГУ",
            "Высшее (бакалавриат)",
            "Информатика",
            new DateOnly(2020, 9, 1),
            new DateOnly(2024, 6, 30)
        );

        _mockRepository.Setup(x => x.AddEducationAsync(It.IsAny<CreateEducation>()))
            .ReturnsAsync(expectedEducation);

        //Act
        var result = await _educationService.AddEducationAsync(
            employeeId,
            "МГУ",
            "Высшее (бакалавриат)",
            "Информатика",
            new DateOnly(2020, 9, 1),
            new DateOnly(2024, 6, 30));

        //Assert
        Assert.NotNull(result);
        Assert.Equal(expectedEducation.Id, result.Id);
        Assert.Equal(expectedEducation.EmployeeId, result.EmployeeId);
        Assert.Equal(expectedEducation.Institution, result.Institution);
        Assert.Equal(expectedEducation.Level, result.Level);
        Assert.Equal(expectedEducation.StudyField, result.StudyField);
        Assert.Equal(expectedEducation.StartDate, result.StartDate);
        Assert.Equal(expectedEducation.EndDate, result.EndDate);
        _mockRepository.Verify(x => x.AddEducationAsync(It.IsAny<CreateEducation>()), Times.Once);
    }

    [Fact]
    public async Task GetEducationById_Successful()
    {
        //Arrange
        var educationId = Guid.NewGuid();
        var expectedEducation = new BaseEducation(
            educationId,
            Guid.NewGuid(),
            "МГУ",
            "Высшее (бакалавриат)",
            "Информатика",
            new DateOnly(2020, 9, 1),
            new DateOnly(2024, 6, 30)
        );

        _mockRepository.Setup(x => x.GetEducationByIdAsync(educationId))
            .ReturnsAsync(expectedEducation);

        //Act
        var result = await _educationService.GetEducationByIdAsync(educationId);

        //Assert
        Assert.NotNull(result);
        Assert.Equal(expectedEducation.Id, result.Id);
        Assert.Equal(expectedEducation.Institution, result.Institution);
        _mockRepository.Verify(x => x.GetEducationByIdAsync(educationId), Times.Once);
    }

    [Fact]
    public async Task GetEducationById_NotFound()
    {
        //Arrange
        var educationId = Guid.NewGuid();
        _mockRepository.Setup(x => x.GetEducationByIdAsync(educationId))
            .ThrowsAsync(new EducationNotFoundException());

        //Act & Assert
        await Assert.ThrowsAsync<EducationNotFoundException>(() =>
            _educationService.GetEducationByIdAsync(educationId));
        _mockRepository.Verify(x => x.GetEducationByIdAsync(educationId), Times.Once);
    }

    [Fact]
    public async Task UpdateEducation_Successful()
    {
        //Arrange
        var educationId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var updateEducation = new UpdateEducation(
            educationId,
            employeeId,
            "СПбГУ",
            "Высшее (магистратура)",
            "Программная инженерия",
            new DateOnly(2018, 9, 1),
            new DateOnly(2024, 6, 30)
        );

        var expectedEducation = new BaseEducation(
            educationId,
            employeeId,
            updateEducation.Institution!,
            updateEducation.Level.ToStringVal(),
            updateEducation.StudyField!,
            updateEducation.StartDate!.Value,
            updateEducation.EndDate
        );

        _mockRepository.Setup(x => x.UpdateEducationAsync(It.IsAny<UpdateEducation>()))
            .ReturnsAsync(expectedEducation);

        //Act
        var result = await _educationService.UpdateEducationAsync(
            educationId,
            employeeId,
            updateEducation.Institution,
            updateEducation.Level.ToStringVal(),
            updateEducation.StudyField,
            updateEducation.StartDate,
            updateEducation.EndDate
        );

        //Assert
        Assert.NotNull(result);
        Assert.Equal(expectedEducation.Institution, result.Institution);
        Assert.Equal(expectedEducation.Level, result.Level);
        Assert.Equal(expectedEducation.StudyField, result.StudyField);
        Assert.Equal(expectedEducation.StartDate, result.StartDate);
        Assert.Equal(expectedEducation.EndDate, result.EndDate);
        _mockRepository.Verify(x => x.UpdateEducationAsync(It.IsAny<UpdateEducation>()), Times.Once);
    }

    [Fact]
    public async Task UpdateEducation_NotFound()
    {
        //Arrange
        var educationId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        _mockRepository.Setup(x => x.UpdateEducationAsync(It.IsAny<UpdateEducation>()))
            .ThrowsAsync(new EducationNotFoundException());

        //Act & Assert
        await Assert.ThrowsAsync<EducationNotFoundException>(() =>
            _educationService.UpdateEducationAsync(educationId, employeeId, "New Institution"));
        _mockRepository.Verify(x => x.UpdateEducationAsync(It.IsAny<UpdateEducation>()), Times.Once);
    }

    [Fact]
    public async Task GetEducations_Successful()
    {
        //Arrange
        var employeeId = Guid.NewGuid();
        var pageNumber = 1;
        var pageSize = 10;
        var expectedEducations = new List<BaseEducation>
        {
            new(
                Guid.NewGuid(),
                employeeId,
                "МГУ",
                "Высшее (бакалавриат)",
                "Информатика",
                new DateOnly(2020, 9, 1),
                new DateOnly(2024, 6, 3)
            ),
            new(
                Guid.NewGuid(),
                employeeId,
                "СПбГУ",
                "Высшее (магистратура)",
                "Программная инженерия",
                new DateOnly(2018, 9, 1),
                new DateOnly(2024, 6, 30)
            )
        };

        var expectedPage = new EducationPage(
            expectedEducations,
            new Page(pageNumber, 2, pageSize)
        );

        _mockRepository.Setup(x => x.GetEducationsAsync(employeeId, pageNumber, pageSize))
            .ReturnsAsync(expectedPage);

        //Act
        var result = await _educationService.GetEducationsByEmployeeIdAsync(employeeId, pageNumber, pageSize);

        //Assert
        Assert.NotNull(result);
        Assert.Equal(expectedPage.Page.TotalItems, result.Page.TotalItems);
        Assert.Equal(expectedPage.Educations.Count, result.Educations.Count);
        _mockRepository.Verify(x => x.GetEducationsAsync(employeeId, pageNumber, pageSize), Times.Once);
    }

    [Fact]
    public async Task DeleteEducation_Successful()
    {
        //Arrange
        var educationId = Guid.NewGuid();
        _mockRepository.Setup(x => x.DeleteEducationAsync(educationId))
            .Returns(Task.CompletedTask);

        //Act
        await _educationService.DeleteEducationAsync(educationId);

        //Assert
        _mockRepository.Verify(x => x.DeleteEducationAsync(educationId), Times.Once);
    }

    [Fact]
    public async Task DeleteEducation_NotFound()
    {
        //Arrange
        var educationId = Guid.NewGuid();
        _mockRepository.Setup(x => x.DeleteEducationAsync(educationId))
            .ThrowsAsync(new EducationNotFoundException());

        //Act & Assert
        await Assert.ThrowsAsync<EducationNotFoundException>(() =>
            _educationService.DeleteEducationAsync(educationId));
        _mockRepository.Verify(x => x.DeleteEducationAsync(educationId), Times.Once);
    }

    [Fact]
    public async Task AddEducation_WithNullEndDate_Successful()
    {
        //Arrange
        var employeeId = Guid.NewGuid();
        var educationToAdd = new CreateEducation(
            employeeId,
            "МГУ",
            "Высшее (бакалавриат)",
            "Информатика",
            new DateOnly(2020, 9, 1)
        );

        var expectedEducation = new BaseEducation(
            Guid.NewGuid(),
            employeeId,
            "МГУ",
            "Высшее (бакалавриат)",
            "Информатика",
            new DateOnly(2020, 9, 1),
            null
        );

        _mockRepository.Setup(x => x.AddEducationAsync(It.IsAny<CreateEducation>()))
            .ReturnsAsync(expectedEducation);

        //Act
        var result = await _educationService.AddEducationAsync(
            employeeId,
            "МГУ",
            "Высшее (бакалавриат)",
            "Информатика",
            new DateOnly(2020, 9, 1));

        //Assert
        Assert.NotNull(result);
        Assert.Equal(expectedEducation.Id, result.Id);
        Assert.Equal(expectedEducation.EmployeeId, result.EmployeeId);
        Assert.Equal(expectedEducation.Institution, result.Institution);
        Assert.Equal(expectedEducation.Level, result.Level);
        Assert.Equal(expectedEducation.StudyField, result.StudyField);
        Assert.Equal(expectedEducation.StartDate, result.StartDate);
        Assert.Null(result.EndDate);
        _mockRepository.Verify(x => x.AddEducationAsync(It.IsAny<CreateEducation>()), Times.Once);
    }

    [Fact]
    public async Task AddEducation_StartDateAfterEndDate_ThrowsException()
    {
        //Arrange
        var employeeId = Guid.NewGuid();
        var startDate = new DateOnly(2024, 9, 1);
        var endDate = new DateOnly(2020, 6, 30);

        //Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _educationService.AddEducationAsync(
                employeeId,
                "МГУ",
                "Высшее (бакалавриат)",
                "Информатика",
                startDate,
                endDate));

        _mockRepository.Verify(x => x.AddEducationAsync(It.IsAny<CreateEducation>()), Times.Never);
    }

    [Fact]
    public async Task AddEducation_StartDateInFuture_ThrowsException()
    {
        //Arrange
        var employeeId = Guid.NewGuid();
        var startDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1));
        var endDate = DateOnly.FromDateTime(DateTime.Now.AddYears(4));

        //Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _educationService.AddEducationAsync(
                employeeId,
                "МГУ",
                "Высшее (бакалавриат)",
                "Информатика",
                startDate,
                endDate));

        _mockRepository.Verify(x => x.AddEducationAsync(It.IsAny<CreateEducation>()), Times.Never);
    }

    [Fact]
    public async Task AddEducation_EndDateInFuture_ThrowsException()
    {
        //Arrange
        var employeeId = Guid.NewGuid();
        var startDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-1));
        var endDate = DateOnly.FromDateTime(DateTime.Now.AddYears(4));

        //Act
        await Assert.ThrowsAsync<ArgumentException>(() => _educationService.AddEducationAsync(
            employeeId,
            "МГУ",
            "Высшее (бакалавриат)",
            "Информатика",
            startDate,
            endDate)
        );

        _mockRepository.Verify(x => x.AddEducationAsync(It.IsAny<CreateEducation>()), Times.Never);
    }
}