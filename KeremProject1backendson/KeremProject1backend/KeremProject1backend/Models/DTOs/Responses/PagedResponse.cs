namespace KeremProject1backend.Models.DTOs.Responses;

public class PagedResponse<T>
{
    public List<T> Data { get; set; } = new List<T>();
    public int Page { get; set; }
    public int Limit { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => Limit > 0 ? (int)Math.Ceiling(TotalCount / (double)Limit) : 0;

    public PagedResponse() { }

    public PagedResponse(List<T> data, int totalCount, int page, int limit)
    {
        Data = data;
        TotalCount = totalCount;
        Page = page;
        Limit = limit;
    }
}

