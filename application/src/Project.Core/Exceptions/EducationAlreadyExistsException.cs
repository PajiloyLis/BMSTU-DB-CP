namespace Project.Core.Exceptions;

public class EducationAlreadyExistsException : Exception
{
    public EducationAlreadyExistsException() : base("Education already exists")
    {
    }

    public EducationAlreadyExistsException(string message) : base(message)
    {
    }

    public EducationAlreadyExistsException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}