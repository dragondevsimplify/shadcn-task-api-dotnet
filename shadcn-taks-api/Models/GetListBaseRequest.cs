namespace shadcn_taks_api.Models;

public class GetListBaseRequest
{
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    public string? SortBy { get; set; }
    public SortOrder? SortOrder { get; set; }
}

public enum SortOrder
{
    Asc,
    Desc
}