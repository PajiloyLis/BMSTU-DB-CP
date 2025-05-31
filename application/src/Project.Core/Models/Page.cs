namespace Project.Core.Models;

/// <summary>
/// Base pagination base model
/// </summary>
public class Page
{
    public Page(int pageNumber, int totalPages, int totalItems)
    {
        PageNumber = pageNumber;
        TotalPages = totalPages;
        TotalItems = totalItems;
    }

    public Page()
    {
    }

    /// <summary>
    /// Page number
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Total pages with currnet page size
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Total items on all pages
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// Check if exist previous page for current page
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Check if exist next page for current page
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;
}