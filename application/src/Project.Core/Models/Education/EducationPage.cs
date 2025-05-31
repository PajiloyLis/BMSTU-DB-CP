namespace Project.Core.Models.Education;

public class EducationPage
{
    public EducationPage()
    {
        Educations = new List<BaseEducation>();
        Page = new Page(1, 0, 10);
    }

    public EducationPage(IReadOnlyList<BaseEducation> educations, Page page)
    {
        Educations = educations;
        Page = page;
    }

    public IReadOnlyList<BaseEducation> Educations { get; set; }
    public Page Page { get; set; }
}