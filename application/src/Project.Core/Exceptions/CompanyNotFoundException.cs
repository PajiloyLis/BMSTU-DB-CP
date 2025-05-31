namespace Project.Core.Exceptions;

/// <summary>
/// Exception when company with required id doesn't exist
/// </summary>
public class CompanyNotFoundException : Exception
{
    public CompanyNotFoundException()
    {
    }

    public CompanyNotFoundException(string message) : base(message)
    {
    }

    public CompanyNotFoundException(string message, Exception? inner) : base(message, inner)
    {
    }
}