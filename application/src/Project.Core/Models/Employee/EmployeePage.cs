namespace Project.Core.Models.Employee;

public class EmployeePage
{
    public EmployeePage(List<UpdateEmployee> employees, Page page)
    {
        Employees = employees;
        Page = page;
    }

    public EmployeePage()
    {
        Employees = new List<UpdateEmployee>();
        Page = new Page();
    }

    public List<UpdateEmployee> Employees { get; set; }

    public Page Page { get; set; }
}