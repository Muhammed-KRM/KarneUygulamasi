namespace KeremProject1backend.Models.DTOs.Requests;

public class CreateStoryRequest
{
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    public string? Text { get; set; } // Max 200 karakter
}

