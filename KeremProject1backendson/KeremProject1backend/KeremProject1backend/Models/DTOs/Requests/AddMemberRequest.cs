using KeremProject1backend.Models.Enums;

namespace KeremProject1backend.Models.DTOs.Requests;

public class AddMemberRequest
{
    public int UserId { get; set; }
    public InstitutionRole Role { get; set; }
    public string? Number { get; set; } // StudentNumber or EmployeeNumber
}

