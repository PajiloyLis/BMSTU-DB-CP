using System.Text.Json.Serialization;

namespace Project.Dto.Http.Employee;

/// <summary>
/// Update employee dto
/// </summary>
public class UpdateEmployeeDto
{
    public UpdateEmployeeDto(Guid id,
        string? fullName,
        string? phoneNumber,
        string? email,
        DateOnly? birthday,
        string? photoPath,
        string? duties)
    {
        Id = id;
        FullName = fullName;
        PhoneNumber = phoneNumber;
        Email = email;
        Birthday = birthday;
        PhotoPath = photoPath;
        Duties = duties;
    }

    /// <summary>
    /// Employee's id
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    /// <summary>
    /// Employee's full name
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("fullName")]
    public string? FullName { get; set; }

    /// <summary>
    /// Employee's business phone
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("phoneNumber")]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Employee's business email
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    /// <summary>
    /// Employee's birthday
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("birthday")]
    public DateOnly? Birthday { get; set; }

    /// <summary>
    /// Employee's photo filesystem path
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("photoPath")]
    public string? PhotoPath { get; set; }

    /// <summary>
    /// Employee's duties json formated
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("duties")]
    public string? Duties { get; set; }
}