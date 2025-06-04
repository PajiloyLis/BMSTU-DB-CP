using System.Text.RegularExpressions;

namespace Project.Core.Models.User;

public class BaseUser
{
    public BaseUser(string email, string password, string salt, string role="")
    {
        if (!Regex.IsMatch(email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$") || email.Length > 254)
            throw new ArgumentException("Invalid employee email", nameof(email));

        Email = email;
        Password = password;
        Salt = salt;
        Role = role;
    }

    public string Salt { get; set; }

    public string Password { get; set; }

    public string Email { get; set; }
    
    public string Role { get; set; }
}