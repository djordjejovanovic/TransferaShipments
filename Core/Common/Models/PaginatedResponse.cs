namespace AppServices.Common.Models;

public class PaginatedResponse<T>
{
    public IEnumerable<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }


    public PaginatedResponse(IEnumerable<T> items, int totalCount, int page, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize > 0 ? pageSize : 1;
        TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
    }
}
