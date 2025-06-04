namespace Project.Service.AuthorizationService.Configuration;

public class PasswordHashingConfiguration
{
    public int Iterations { get; set; }

    public int KeyLen { get; set; }
}