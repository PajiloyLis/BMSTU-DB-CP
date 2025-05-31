using System.Text.Json.Serialization;

namespace Project.Dto.Http.Employee;

/// <summary>
/// Dto for employee creation request
/// </summary>
public class CreateEmployeeDto
{
    public CreateEmployeeDto(string fullName,
        string phoneNumber,
        string email,
        DateOnly birthday,
        string? photoPath,
        string? duties
    )
    {
        FullName = fullName;
        PhoneNumber = phoneNumber;
        Email = email;
        Birthday = birthday;
        PhotoPath = photoPath;
        Duties = duties;
    }

    /// <summary>
    /// Employee's full name
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("fullName")]
    public string FullName { get; set; }

    /// <summary>
    /// Employee's business phone number
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("phoneNumber")]
    public string PhoneNumber { get; set; }

    /// <summary>
    /// Employee's business email address
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("email")]
    public string Email { get; set; }

    /// <summary>
    /// Employee's birthday
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("birthday")]
    public DateOnly Birthday { get; set; }

    /// <summary>
    /// Employee's photo filesystem path
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("photoPath")]
    public string? PhotoPath { get; set; }

    /// <summary>
    /// Employee's duties json formatted
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("duties")]
    public string? Duties { get; set; }
}