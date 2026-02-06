namespace KeremProject1backend.Models.DTOs.Requests;

public class UpdateUserRequest
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}
