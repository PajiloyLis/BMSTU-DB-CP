namespace Database.Models;

public class UserDb
{
    public UserDb(string email, string password, string salt, string role)
    {
        Email = email;
        Password = password;
        Salt = salt;
        Role = role;
    }
    public string Password { get; set; }

    public string Email { get; set; }
    
    public string Salt { get; set; }
    
    public string Role { get; set; }
}