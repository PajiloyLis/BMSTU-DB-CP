namespace Project.Dto.Http.User;

public class LoginDto
{
    public LoginDto(string password, string email)
    {
        Email = email;
        Password = password;
    }

    public string Password { get; set; }

    public string Email { get; set; }
}