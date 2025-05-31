using System.Text.RegularExpressions;

namespace Project.Core.Models.Company;

/// <summary>
/// Update company model
/// </summary>
public class UpdateCompany
{
    public UpdateCompany(Guid companyId,
        string? title,
        DateOnly? registrationDate,
        string? phoneNumber,
        string? email,
        string? inn,
        string? kpp,
        string? ogrn,
        string? address
    )
    {
        if (!Guid.TryParse(companyId.ToString(), out _))
            throw new ArgumentException("Company Id is invalid", nameof(companyId));
        CompanyId = companyId;
        Title = title;
        if (registrationDate is not null && registrationDate > DateOnly.FromDateTime(DateTime.Today))
            throw new ArgumentException("Registration Date is invalid", nameof(registrationDate));
        RegistrationDate = registrationDate;
        if (phoneNumber is not null && !Regex.IsMatch(phoneNumber, @"^\+\d{5,17}$"))
            throw new ArgumentException("Phone Number is invalid", nameof(phoneNumber));
        PhoneNumber = phoneNumber;
        if (email is not null && !Regex.IsMatch(email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
            throw new ArgumentException("Email is invalid", nameof(email));
        Email = email;
        if (inn is not null && !Regex.IsMatch(inn, @"^[0-9]{10}$"))
            throw new ArgumentException("Inn is invalid", nameof(inn));
        Inn = inn;
        if (kpp is not null && !Regex.IsMatch(kpp, @"^[0-9]{9}$"))
            throw new ArgumentException("Kpp is invalid", nameof(kpp));
        Kpp = kpp;
        if (ogrn is not null && !Regex.IsMatch(ogrn, @"^[0-9]{13}$"))
            throw new ArgumentException("OGRN is invalid", nameof(ogrn));
        Ogrn = ogrn;
        Address = address;
    }

    /// <summary>
    /// Company's id
    /// </summary>
    public Guid CompanyId { get; init; }

    /// <summary>
    /// Company's name
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Company's registration date
    /// </summary>
    public DateOnly? RegistrationDate { get; set; }

    /// <summary>
    /// Company's contact phone number
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Company's contact email
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Company's inn
    /// </summary>
    public string? Inn { get; set; }

    /// <summary>
    /// Company's kpp
    /// </summary>
    public string? Kpp { get; set; }

    /// <summary>
    /// Company's ogrn
    /// </summary>
    public string? Ogrn { get; set; }

    /// <summary>
    /// Company's registered address
    /// </summary>
    public string? Address { get; set; }
}