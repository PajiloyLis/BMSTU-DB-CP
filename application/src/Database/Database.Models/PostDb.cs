using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models;

/// <summary>
/// Database post model.
/// </summary>
public class PostDb
{
    public PostDb()
    {
        Title = string.Empty;
        PostHistories = new List<PostHistoryDb>();
    }
    
    public PostDb(Guid id, string title, decimal salary, Guid companyId)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty");

        if (salary <= 0)
            throw new ArgumentException("Salary must be greater than zero");

        Id = id;
        Title = title;
        Salary = salary;
        CompanyId = companyId;
        PostHistories = new List<PostHistoryDb>();
    }

    /// <summary>
    /// Post id.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Post title.
    /// </summary>
    [Required]
    public string Title { get; set; }

    /// <summary>
    /// Post salary.
    /// </summary>
    [Required]
    public decimal Salary { get; set; }

    /// <summary>
    /// Company id.
    /// </summary>
    [ForeignKey(nameof(CompanyDb))]
    public Guid CompanyId { get; set; }

    public ICollection<PostHistoryDb> PostHistories { get; set; }
}