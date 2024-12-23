namespace shadcn_taks_api.Persistence.Requests;

public class GetTagListRequest : GetListBaseRequest
{
    public string? Name { get; set; }
}