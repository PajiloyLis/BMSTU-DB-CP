namespace Project.Core.Exceptions;

/// <summary>
/// Исключение, возникающее при отсутствии уровня образования
/// </summary>
public class EducationLevelNotFoundException : Exception
{
    /// <summary>
    /// Создает новый экземпляр исключения с сообщением по умолчанию
    /// </summary>
    public EducationLevelNotFoundException() : base("Уровень образования не найден")
    {
    }

    /// <summary>
    /// Создает новый экземпляр исключения с указанным сообщением
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    public EducationLevelNotFoundException(string message) : base(message)
    {
    }

    /// <summary>
    /// Создает новый экземпляр исключения с указанным сообщением и ссылкой на внутреннее исключение
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    /// <param name="innerException">Внутреннее исключение</param>
    public EducationLevelNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}