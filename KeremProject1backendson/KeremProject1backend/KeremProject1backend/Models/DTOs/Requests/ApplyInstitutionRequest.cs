namespace KeremProject1backend.Models.DTOs.Requests;

public class ApplyInstitutionRequest
{
    public string Name { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Phone { get; set; }
}
