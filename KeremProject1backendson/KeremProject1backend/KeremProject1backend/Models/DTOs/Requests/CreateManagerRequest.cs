namespace KeremProject1backend.Models.DTOs.Requests;

public class CreateManagerRequest
{
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public string? Phone { get; set; }
}
