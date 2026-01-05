namespace KeremProject1backend.Models.DTOs.Requests;

public class ReviewReportRequest
{
    public string Action { get; set; } = string.Empty; // resolve, reject
    public string? Notes { get; set; }
}

