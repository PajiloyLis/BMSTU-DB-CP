namespace Project.Dto.Http.Post;

public class CreatePostDto
{
    public CreatePostDto(string title, decimal salary, Guid companyId)
    {
        Title = title;
        Salary = salary;
        CompanyId = companyId;
    }

   public string Title { get; set; }
    public decimal Salary { get; set; }
    public Guid CompanyId { get; set; }
}