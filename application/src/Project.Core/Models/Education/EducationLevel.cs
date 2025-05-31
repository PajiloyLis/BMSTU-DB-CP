using Project.Core.Exceptions;

namespace Project.Core.Models.Education;

/// <summary>
/// Уровни образования
/// </summary>
public enum EducationLevel
{
    /// <summary>
    /// Высшее (бакалавриат)
    /// </summary>
    Bachelor = 1,

    /// <summary>
    /// Высшее (магистратура)
    /// </summary>
    Master = 2,

    /// <summary>
    /// Высшее (специалитет)
    /// </summary>
    Specialist = 3,

    /// <summary>
    /// Среднее профессиональное (ПКР)
    /// </summary>
    SecondaryProfessionalPKR = 4,

    /// <summary>
    /// Среднее профессиональное (ПССЗ)
    /// </summary>
    SecondaryProfessionalPSSZ = 5,

    /// <summary>
    /// Программы переподготовки
    /// </summary>
    Retraining = 6,

    /// <summary>
    /// Курсы повышения квалификации
    /// </summary>
    AdvancedTraining = 7
}

/// <summary>
/// Методы расширения для EducationLevel
/// </summary>
public static class EducationLevelExtensions
{
    /// <summary>
    /// Получить строковое представление уровня образования
    /// </summary>
    public static string ToStringVal(this EducationLevel? level)
    {
        if (level is null)
            throw new ArgumentNullException(nameof(level));
        return level switch
        {
            EducationLevel.Bachelor => "Высшее (бакалавриат)",
            EducationLevel.Master => "Высшее (магистратура)",
            EducationLevel.Specialist => "Высшее (специалитет)",
            EducationLevel.SecondaryProfessionalPKR => "Среднее профессиональное (ПКР)",
            EducationLevel.SecondaryProfessionalPSSZ => "Среднее профессиональное (ПССЗ)",
            EducationLevel.Retraining => "Программы переподготовки",
            EducationLevel.AdvancedTraining => "Курсы повышения квалификации",
            _ => throw new EducationLevelNotFoundException($"Неизвестный уровень образования: {level}")
        };
    }


    /// <summary>
    /// Получить уровень образования из строкового представления
    /// </summary>
    public static EducationLevel ToEducationLevel(this string displayString)
    {
        return displayString switch
        {
            "Высшее (бакалавриат)" => EducationLevel.Bachelor,
            "Высшее (магистратура)" => EducationLevel.Master,
            "Высшее (специалитет)" => EducationLevel.Specialist,
            "Среднее профессиональное (ПКР)" => EducationLevel.SecondaryProfessionalPKR,
            "Среднее профессиональное (ПССЗ)" => EducationLevel.SecondaryProfessionalPSSZ,
            "Программы переподготовки" => EducationLevel.Retraining,
            "Курсы повышения квалификации" => EducationLevel.AdvancedTraining,
            _ => throw new EducationLevelNotFoundException($"Неизвестный уровень образования: {displayString}")
        };
    }
}