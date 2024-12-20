namespace shadcn_taks_api.Persistence.Responses;

public class PaginationResponse<T>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public List<T>? List { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
}