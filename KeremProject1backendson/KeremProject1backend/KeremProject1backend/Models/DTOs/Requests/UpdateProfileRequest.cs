using KeremProject1backend.Models.Enums;

namespace KeremProject1backend.Models.DTOs.Requests;

public class UpdateProfileRequest
{
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public ProfileVisibility? ProfileVisibility { get; set; }
}

