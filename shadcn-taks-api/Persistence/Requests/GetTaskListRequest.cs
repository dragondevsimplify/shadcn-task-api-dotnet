namespace shadcn_taks_api.Persistence.Requests;

public class GetTaskListRequest : GetListBaseRequest
{
    public string? Name { get; set; }
    public string? Title { get; set; }
    public int[]? TagIds { get; set; }
}