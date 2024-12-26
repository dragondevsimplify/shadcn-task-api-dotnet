namespace shadcn_taks_api.Models;

public class ColumnSorting
{
    public string Column { get; set; } = string.Empty;
    public string Order { get; set; } = string.Empty;
}

public class ColumnFiltering
{
    public string Column { get; set; } = string.Empty;
    public string SearchValue { get; set; } = string.Empty;
}

public interface IOrderingParams
{
    List<ColumnSorting> Orders { get; set; }
}

public interface IFilteringParams
{
    List<ColumnFiltering> Filters { get; set; }
}

public interface IPagingParams
{
    int? PageNumber { get; set; }
    int? PageSize { get; set; }
}

public interface IPaginationParams : IOrderingParams, IPagingParams;

public class PaginationParams : IPaginationParams
{
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public List<ColumnSorting> Orders { get; set; } = [];
}