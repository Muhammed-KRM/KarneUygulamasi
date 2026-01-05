namespace KeremProject1backend.Models.DTOs.Requests;

public class ReportContentRequest
{
    public string Reason { get; set; } = string.Empty; // spam, inappropriate, harassment, copyright, other
    public string? Description { get; set; }
}

