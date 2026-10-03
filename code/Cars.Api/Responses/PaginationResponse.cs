namespace Cars.Api.Responses;

public class PaginationResponse
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalElements { get; init; }
    public IReadOnlyList<CarResponse> Items { get; init; } = Array.Empty<CarResponse>();
}
