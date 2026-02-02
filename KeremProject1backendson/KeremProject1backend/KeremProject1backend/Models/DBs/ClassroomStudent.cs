namespace KeremProject1backend.Models.DBs;

public class ClassroomStudent
{
    public int Id { get; set; }

    public int ClassroomId { get; set; }
    public required Classroom Classroom { get; set; }

    public int InstitutionUserId { get; set; } // points to InstitutionUser ID
    public required InstitutionUser Student { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RemovedAt { get; set; }
}
