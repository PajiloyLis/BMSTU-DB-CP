namespace Project.Core.Exceptions;

public class EducationNotFoundException : Exception
{
    public EducationNotFoundException() : base("Education not found")
    {
    }

    public EducationNotFoundException(string message) : base(message)
    {
    }

    public EducationNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}