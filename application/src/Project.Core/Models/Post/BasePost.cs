namespace Project.Core.Models.Post;

public class BasePost
{
    public BasePost(Guid id, string title, decimal salary, Guid companyId)
    {
        if (!Guid.TryParse(id.ToString(), out _))
            throw new ArgumentException("Id cannot be empty", nameof(id));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));

        if (salary <= 0)
            throw new ArgumentException("Salary must be greater than zero", nameof(salary));

        if (!Guid.TryParse(companyId.ToString(), out _))
            throw new ArgumentException("CompanyId cannot be empty", nameof(companyId));

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