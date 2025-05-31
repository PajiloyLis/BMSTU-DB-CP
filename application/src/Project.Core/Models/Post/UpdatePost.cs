namespace Project.Core.Models.Post;

public class UpdatePost
{
    public UpdatePost(Guid id, Guid companyId, string? title = null, decimal? salary = null)
    {
        if (!Guid.TryParse(id.ToString(), out _))
            throw new ArgumentException("Id cannot be empty", nameof(id));

        if (!Guid.TryParse(companyId.ToString(), out _))
            throw new ArgumentException("CompanyId cannot be empty", nameof(companyId));

        if (salary is not null && salary <= 0)
            throw new ArgumentException("Salary must be greater than zero", nameof(salary));

        Id = id;
        CompanyId = companyId;
        Title = title;
        Salary = salary;
    }

    public Guid Id { get; set; }
    public string? Title { get; set; }
    public decimal? Salary { get; set; }
    public Guid CompanyId { get; set; }
}