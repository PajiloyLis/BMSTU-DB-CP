namespace Project.Core.Models.Company;

/// <summary>
/// Companies page model
/// </summary>
public class CompanyPage
{
    public CompanyPage(List<Models.Company.BaseCompany> companies, Page page)
    {
        Companies = companies;
        Page = page;
    }

    public CompanyPage()
    {
        Companies = new List<Models.Company.BaseCompany>();
        Page = new Page();
    }

    /// <summary>
    /// Companies on the page
    /// </summary>
    public List<Models.Company.BaseCompany> Companies { get; set; }

    /// <summary>
    /// Page model
    /// </summary>
    public Page Page { get; set; }
}