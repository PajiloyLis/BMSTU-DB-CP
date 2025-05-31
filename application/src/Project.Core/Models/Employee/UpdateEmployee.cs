using System.Text.Json;
using System.Text.RegularExpressions;

namespace Project.Core.Models.Employee;

public class UpdateEmployee
{
    public UpdateEmployee(Guid employeeId,
        string? fullName,
        string? phoneNumber,
        string? email,
        DateOnly? birthDate,
        string? photo,
        string? duties
    )
    {
        if (!Guid.TryParse(employeeId.ToString(), out _))
            throw new ArgumentException("Employee Id is invalid", nameof(employeeId));
        EmployeeId = employeeId;

        if (fullName is not null && !Regex.IsMatch(fullName, @"^[A-ZА-ЯЁ][a-zа-яё]+(?: [A-ZА-ЯЁ][a-zа-яё]+){1,2}$"))
            throw new ArgumentException("Invalid employee name", nameof(fullName));
        FullName = fullName;

        if (phoneNumber is not null && !Regex.IsMatch(phoneNumber, @"^\+\d{5,17}$"))
            throw new ArgumentException("Invalid phone number", nameof(phoneNumber));
        PhoneNumber = phoneNumber;

        if (email is not null && (!Regex.IsMatch(email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$") ||
                                  email.Length > 254))
            throw new ArgumentException("Invalid employee email", nameof(email));
        Email = email;

        if (birthDate is not null && birthDate > DateOnly.FromDateTime(DateTime.Today))
            throw new ArgumentException("Invalid employee birth date", nameof(birthDate));
        BirthDate = birthDate;
        Photo = photo;
        try
        {
            if (duties is not null)
            {
                JsonDocument.Parse(duties);
                Duties = duties;
            }
            else
            {
                Duties = duties;
            }
        }
        catch (JsonException e)
        {
            throw new ArgumentException("Invalid duties JSON exception", nameof(duties));
        }
    }

    public UpdateEmployee()
    {
    }

    public Guid EmployeeId { get; set; }

    public string? FullName { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public DateOnly? BirthDate { get; set; }

    public string? Photo { get; set; }

    public string? Duties { get; set; }
}