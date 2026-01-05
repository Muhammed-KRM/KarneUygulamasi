namespace KeremProject1backend.Models.DTOs.Responses;

public class UserSearchResultDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public string GlobalRole { get; set; } = string.Empty;
    public List<InstitutionSummaryDto> Institutions { get; set; } = new();
    public bool IsVisible { get; set; } // ProfileVisibility kontrolü sonucu
}

