using KeremProject1backend.Models.Enums;

namespace KeremProject1backend.Models.DTOs.Responses;

public class ContentReportDto
{
    public int Id { get; set; }
    public int ContentId { get; set; }
    public string ContentTitle { get; set; } = string.Empty;
    public int ReporterId { get; set; }
    public string ReporterName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ReportStatus Status { get; set; }
    public int? ReviewedById { get; set; }
    public string? ReviewerName { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ResolutionNotes { get; set; }
    public DateTime CreatedAt { get; set; }
}

