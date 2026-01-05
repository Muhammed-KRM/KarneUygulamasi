namespace KeremProject1backend.Models.DTOs.Requests;

public class DeleteAccountRequest
{
    public string Password { get; set; } = string.Empty;
    public bool HardDelete { get; set; } = false;
}

