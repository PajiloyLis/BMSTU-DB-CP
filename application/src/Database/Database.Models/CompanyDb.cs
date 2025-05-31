namespace Database.Models;

/// <summary>
/// Database company model
/// </summary>
public class CompanyDb
{
    public CompanyDb(Guid id,
        string title,
        DateOnly registrationDate,
        string phoneNumber,
        string email,
        string inn,
        string kpp,
        string ogrn,
        string address)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty");

        if (registrationDate > DateOnly.FromDateTime(DateTime.Today))
            throw new ArgumentException("RegistrationDate cannot be later than today");

        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("PhoneNumber cannot be empty");

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty");

        if (string.IsNullOrWhiteSpace(inn))
            throw new ArgumentException("Inn cannot be empty");

        if (string.IsNullOrWhiteSpace(kpp))
            throw new ArgumentException("Kpp cannot be empty");

        if (string.IsNullOrWhiteSpace(ogrn))
            throw new ArgumentException("Ogrn cannot be empty");

        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address cannot be empty");

        Id = id;
        Title = title;
        RegistrationDate = registrationDate;
        PhoneNumber = phoneNumber;
        Email = email;
        Inn = inn;
        Kpp = kpp;
        Ogrn = ogrn;
        Address = address;
        Posts = new List<PostDb>();
        Positions = new List<PositionDb>();
    }

    public CompanyDb(Guid id,
        string title,
        DateTime registrationDate,
        string phoneNumber,
        string email,
        string inn,
        string kpp,
        string ogrn,
        string address)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty");

        if (registrationDate > DateTime.Today)
            throw new ArgumentException("RegistrationDate cannot be later than today");

        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("PhoneNumber cannot be empty");

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty");

        if (string.IsNullOrWhiteSpace(inn))
            throw new ArgumentException("Inn cannot be empty");

        if (string.IsNullOrWhiteSpace(kpp))
            throw new ArgumentException("Kpp cannot be empty");

        if (string.IsNullOrWhiteSpace(ogrn))
            throw new ArgumentException("Ogrn cannot be empty");

        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address cannot be empty");

        Id = id;
        Title = title;
        RegistrationDate = DateOnly.FromDateTime(registrationDate);
        PhoneNumber = phoneNumber;
        Email = email;
        Inn = inn;
        Kpp = kpp;
        Ogrn = ogrn;
        Address = address;
        Posts = new List<PostDb>();
        Positions = new List<PositionDb>();
    }

    /// <summary>
    /// Company id
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Company name
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Company registration date
    /// </summary>
    public DateOnly RegistrationDate { get; set; }

    /// <summary>
    /// Company contact phone number
    /// </summary>
    public string PhoneNumber { get; set; }

    /// <summary>
    /// Company contact email
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Company inn
    /// </summary>
    public string Inn { get; set; }

    /// <summary>
    /// Company kpp
    /// </summary>
    public string Kpp { get; set; }

    /// <summary>
    /// Company ogrn
    /// </summary>
    public string Ogrn { get; set; }

    /// <summary>
    /// Company registered address
    /// </summary>
    public string Address { get; set; }

    public ICollection<PostDb> Posts { get; set; }

    public ICollection<PositionDb> Positions { get; set; }

}