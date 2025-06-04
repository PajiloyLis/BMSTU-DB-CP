using System.Text.RegularExpressions;

namespace Project.Core.Models.User;

public class AuthorizationData
{
    public AuthorizationData(string email, string token)
    {
        if (!Regex.IsMatch(email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$") || email.Length > 254)
            throw new ArgumentException("Invalid employee email", nameof(email));
        Email = email;
        Token = token;
    }

    public string Token { get; set; }

    public string Email { get; set; }
}