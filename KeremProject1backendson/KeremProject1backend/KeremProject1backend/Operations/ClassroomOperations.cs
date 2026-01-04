using KeremProject1backend.Core.Constants;
using KeremProject1backend.Infrastructure;
using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.DTOs;
using KeremProject1backend.Models.Enums;
using KeremProject1backend.Services;
using Microsoft.EntityFrameworkCore;

namespace KeremProject1backend.Operations;

public class ClassroomOperations
{
    private readonly ApplicationContext _context;
    private readonly SessionService _sessionService;

    public ClassroomOperations(ApplicationContext context, SessionService sessionService)
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<BaseResponse<int>> CreateClassroomAsync(int institutionId, string name, int grade)
    {
        var currentUserId = _sessionService.GetUserId();
        var isManager = await _context.Institutions.AnyAsync(i => i.Id == institutionId && i.ManagerUserId == currentUserId);

        if (!isManager && !_sessionService.IsInGlobalRole(UserRole.AdminAdmin))
        {
            return BaseResponse<int>.ErrorResponse("Unauthorized", ErrorCodes.AccessDenied);
        }

        var classroom = new Classroom
        {
            InstitutionId = institutionId,
            Name = name,
            Grade = grade,
            CreatedAt = DateTime.UtcNow,
            Institution = null! // Satisfy required, EF will handle mapping
        };

        _context.Classrooms.Add(classroom);
        await _context.SaveChangesAsync();

        // 3. Create Class Conversation
        var conversation = new Conversation
        {
            Type = ConversationType.ClassGroup,
            ClassroomId = classroom.Id,
            Title = $"{classroom.Name} Sınıf Grubu",
            IsGroup = true,
            CreatedAt = DateTime.UtcNow,
            InstitutionId = institutionId,
            Institution = null!
        };
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        // 4. Conversation is already linked via conversation.ClassroomId
        return BaseResponse<int>.SuccessResponse(classroom.Id);
    }

    public async Task<BaseResponse<bool>> AddStudentToClassroomAsync(int classroomId, int studentInstitutionUserId)
    {
        // studentInstitutionUserId is the ID from InstitutionUsers table
        var classroom = await _context.Classrooms.FindAsync(classroomId);
        if (classroom == null) return BaseResponse<bool>.ErrorResponse("Classroom not found", ErrorCodes.Unauthorized); // Using existing code for now or will add Generic

        var existing = await _context.ClassroomStudents.AnyAsync(cs => cs.ClassroomId == classroomId && cs.StudentId == studentInstitutionUserId);
        if (existing) return BaseResponse<bool>.ErrorResponse("Student already in class", "202001");

        var cs = new ClassroomStudent
        {
            ClassroomId = classroomId,
            StudentId = studentInstitutionUserId,
            AssignedAt = DateTime.UtcNow,
            Classroom = null!,
            Student = null!
        };

        _context.ClassroomStudents.Add(cs);
        await _context.SaveChangesAsync();

        return BaseResponse<bool>.SuccessResponse(true);
    }

    public async Task<BaseResponse<bool>> AddStudentsToClassroomAsync(int classroomId, List<int> studentInstitutionUserIds)
    {
        var classroom = await _context.Classrooms.FindAsync(classroomId);
        if (classroom == null) return BaseResponse<bool>.ErrorResponse("Classroom not found", ErrorCodes.GenericError);

        foreach (var studentId in studentInstitutionUserIds)
        {
            var existing = await _context.ClassroomStudents.AnyAsync(cs => cs.ClassroomId == classroomId && cs.StudentId == studentId);
            if (existing) continue;

            var cs = new ClassroomStudent
            {
                ClassroomId = classroomId,
                StudentId = studentId,
                AssignedAt = DateTime.UtcNow, // Renamed from JoinedAt if following guide
                Classroom = null!,
                Student = null!
            };
            _context.ClassroomStudents.Add(cs);
        }

        await _context.SaveChangesAsync();
        return BaseResponse<bool>.SuccessResponse(true);
    }

    public async Task<BaseResponse<ClassroomDetailDto>> GetClassroomDetailsAsync(int classroomId)
    {
        // Fetch with join for better mapping
        var details = await _context.Classrooms
            .Where(c => c.Id == classroomId)
            .Select(c => new ClassroomDetailDto
            {
                Id = c.Id,
                Name = c.Name,
                Grade = c.Grade,
                InstitutionId = c.InstitutionId,
                Students = _context.ClassroomStudents
                    .Where(cs => cs.ClassroomId == c.Id)
                    .Select(cs => new ClassroomStudentDto
                    {
                        InstitutionUserId = cs.StudentId,
                        FullName = cs.Student.User.FullName,
                        StudentNumber = cs.Student.StudentNumber
                    }).ToList()
            }).FirstOrDefaultAsync();

        if (details == null) return BaseResponse<ClassroomDetailDto>.ErrorResponse("Classroom not found", ErrorCodes.GenericError);

        return BaseResponse<ClassroomDetailDto>.SuccessResponse(details);
    }

    public async Task<BaseResponse<List<ClassroomDto>>> GetClassroomsAsync(int institutionId)
    {
        var classrooms = await _context.Classrooms
            .Where(c => c.InstitutionId == institutionId)
            .Select(c => new ClassroomDto
            {
                Id = c.Id,
                Name = c.Name,
                Grade = c.Grade,
                StudentCount = _context.ClassroomStudents.Count(cs => cs.ClassroomId == c.Id)
            }).ToListAsync();

        return BaseResponse<List<ClassroomDto>>.SuccessResponse(classrooms);
    }
}

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
}
