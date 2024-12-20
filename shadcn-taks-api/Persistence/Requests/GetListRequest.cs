namespace shadcn_taks_api.Persistence.Requests;

public class GetListRequest
{
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    public bool? GetAll { get; set; }
    public string? SortBy { get; set; }
    public SortOrder? SortOrder { get; set; }
}

public enum SortOrder
{
    Asc,
    Desc
}