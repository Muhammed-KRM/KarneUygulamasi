namespace KeremProject1backend.Models.DTOs.Responses;

public class TopEngagerDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public int Interactions { get; set; }
}

