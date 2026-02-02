namespace KeremProject1backend.Models.DTOs.Responses;

public class ClassroomDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int Grade { get; set; }
    public int StudentCount { get; set; }
}

public class ClassroomDetailDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int Grade { get; set; }
    public int InstitutionId { get; set; }
    public List<ClassroomStudentDto> Students { get; set; } = new();
}

public class ClassroomStudentDto
{
    public int InstitutionUserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? StudentNumber { get; set; }
    public DateTime AssignedAt { get; set; }
}
