using KeremProject1backend.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace KeremProject1backend.Models.DTOs.Requests;

public class CreateClassroomRequest
{
    public int InstitutionId { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;
    public int Grade { get; set; }
}

public class BulkAddStudentsRequest
{
    public int ClassroomId { get; set; }
    public List<int> StudentIds { get; set; } = new();
}

public class AddStudentRequest
{
    public int ClassroomId { get; set; }
    public int StudentId { get; set; }
}
