using System.Text.Json.Serialization;

namespace Project.Dto.Http;

/// <summary>
/// Dto for representing errors
/// </summary>
public class ErrorDto
{
    public ErrorDto(string errorType, string message)
    {
        ErrorType = errorType;
        Message = message;
    }

    /// <summary>
    /// Error type name
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("errorType")]
    public string ErrorType { get; set; }

    /// <summary>
    /// Error message
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("message")]
    public string Message { get; set; }
}