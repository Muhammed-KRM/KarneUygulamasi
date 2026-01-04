namespace KeremProject1backend.Models.DBs;

public class ClassroomStudent
{
    public int Id { get; set; }

    public int ClassroomId { get; set; }
    public required Classroom Classroom { get; set; }

    public int StudentId { get; set; } // points to InstitutionUser ID (or User ID?)
    // In Phase 1 structure, InstitutionUser represents the student role within the institution.
    // Linking to InstitutionUser is better to ensure the student belongs to the same institution.
    public required InstitutionUser Student { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}
