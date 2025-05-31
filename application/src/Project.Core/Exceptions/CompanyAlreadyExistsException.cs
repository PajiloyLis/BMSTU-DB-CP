namespace Project.Core.Exceptions;

/// <summary>
/// Exception when company with same unique fields already exists
/// </summary>
public class CompanyAlreadyExistsException : Exception
{
    public CompanyAlreadyExistsException()
    {
    }

    public CompanyAlreadyExistsException(string message) : base(message)
    {
    }

    public CompanyAlreadyExistsException(string message, Exception? inner) : base(message, inner)
    {
    }
}