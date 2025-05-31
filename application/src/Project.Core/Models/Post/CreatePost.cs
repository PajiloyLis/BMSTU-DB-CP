namespace Project.Core.Models.Post;

public class CreatePost
{
    public CreatePost(string title, decimal salary, Guid companyId)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));

        if (salary <= 0)
            throw new ArgumentException("Salary must be greater than zero", nameof(salary));

        if (!Guid.TryParse(companyId.ToString(), out _))
            throw new ArgumentException("CompanyId cannot be empty", nameof(companyId));

        Id = Guid.NewGuid();
        Title = title;
        Salary = salary;
        CompanyId = companyId;
    }

    public Guid Id { get; set; }
    public string Title { get; set; }
    public decimal Salary { get; set; }
    public Guid CompanyId { get; set; }
}