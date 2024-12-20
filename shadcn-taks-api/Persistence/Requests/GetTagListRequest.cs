namespace shadcn_taks_api.Persistence.Requests;

public class GetTagListRequest : GetListRequest
{
    public string? Title { get; set; }
    public string? Types { get; set; }
    public string? Statuses { get; set; }
    public string? Priorities { get; set; }
}