using System.Text.Json.Serialization;

namespace Project.Dto.Http.Company;

public class UpdateCompanyDto
{
    public UpdateCompanyDto(Guid companyId,
        string? title,
        DateOnly? registrationDate,
        string? phoneNumber,
        string? email,
        string? inn,
        string? kpp,
        string? ogrn,
        string? address)
    {
        CompanyId = companyId;
        Title = title;
        RegistrationDate = registrationDate;
        PhoneNumber = phoneNumber;
        Email = email;
        Inn = inn;
        Kpp = kpp;
        Ogrn = ogrn;
        Address = address;
    }
    
    /// <summary>
    /// Company's id
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("companyId")]
    public Guid CompanyId { get; init; }

    /// <summary>
    /// Company's name
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// Company's registration date
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("registrationDate")]
    public DateOnly? RegistrationDate { get; set; }

    /// <summary>
    /// Company's contact phone number
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("phoneNumber")]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Company's contact email
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    /// <summary>
    /// Company's inn
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("inn")]
    public string? Inn { get; set; }

    /// <summary>
    /// Company's kpp
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("kpp")]
    public string? Kpp { get; set; }

    /// <summary>
    /// Company's ogrn
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("ogrn")]
    public string? Ogrn { get; set; }

    /// <summary>
    /// Company's registered address
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("address")]
    public string? Address { get; set; }
}