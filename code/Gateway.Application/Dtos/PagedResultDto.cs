namespace Gateway.Application.Dtos;

public class PagedResultDto<T>
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalElements { get; init; }
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
}
