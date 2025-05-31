namespace Project.Dto.Http.Post;

public class PostDto
{
    public PostDto(Guid id, string title, decimal salary, Guid companyId)
    {
        Id = id;
        Title = title;
        Salary = salary;
        CompanyId = companyId;
    }

    public Guid Id { get; set; }
    public string Title { get; set; }
    public decimal Salary { get; set; }
    public Guid CompanyId { get; set; }
}