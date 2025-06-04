namespace Project.Dto.Http.User;

public class AuthorizationDataDto
{
    public AuthorizationDataDto(string email, string token)
    {
        Email = email;
        Token = token;
    }

    public string Token { get; set; }

    public string Email { get; set; }
}