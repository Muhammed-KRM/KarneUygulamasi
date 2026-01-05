namespace KeremProject1backend.Models.DTOs.Requests;

public class UpdateEmailRequest
{
    public string NewEmail { get; set; } = string.Empty;
    public string CurrentPassword { get; set; } = string.Empty;
}

