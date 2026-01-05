namespace KeremProject1backend.Models.DTOs.Requests;

public class UserSearchRequest
{
    public string Query { get; set; } = string.Empty;
    public int? Role { get; set; } // UserRole enum value
    public int? InstitutionId { get; set; }
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 20;
}

