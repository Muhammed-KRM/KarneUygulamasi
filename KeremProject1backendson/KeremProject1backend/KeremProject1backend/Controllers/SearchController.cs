using KeremProject1backend.Infrastructure;
using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.DTOs;
using KeremProject1backend.Models.DTOs.Responses;
using KeremProject1backend.Models.Enums;
using KeremProject1backend.Operations;
using KeremProject1backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KeremProject1backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SearchController : BaseController
{
    private readonly ApplicationContext _context;
    private readonly SessionService _sessionService;
    private readonly CacheService _cacheService;

    public SearchController(ApplicationContext context, SessionService sessionService, CacheService cacheService) : base(sessionService)
    {
        _context = context;
        _sessionService = sessionService;
        _cacheService = cacheService;
    }

    [HttpGet("users")]
    public async Task<IActionResult> SearchUsers(
        [FromQuery] string query,
        [FromQuery] UserRole? role = null,
        [FromQuery] int? institutionId = null,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] bool forceRefresh = false)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest(BaseResponse<List<UserSearchResultDto>>.ErrorResponse("Query is required", "300001"));

        // Cache key
        var cacheKey = $"search_users_{query}_{role}_{institutionId}_{page}_{limit}";
        if (!forceRefresh)
        {
            var cached = await _cacheService.GetAsync<List<UserSearchResultDto>>(cacheKey);
            if (cached != null)
                return Ok(BaseResponse<List<UserSearchResultDto>>.SuccessResponse(cached));
        }

        var currentUserId = GetCurrentUserId();
        var userQuery = _context.Users
            .AsNoTracking()
            .Where(u => 
                u.Username.Contains(query) ||
                u.FullName.Contains(query) ||
                u.Email.Contains(query))
            .AsQueryable();

        if (role.HasValue)
            userQuery = userQuery.Where(u => u.GlobalRole == role.Value);

        if (institutionId.HasValue)
        {
            var institutionUserIds = await _context.InstitutionUsers
                .Where(iu => iu.InstitutionId == institutionId.Value)
                .Select(iu => iu.UserId)
                .ToListAsync();
            userQuery = userQuery.Where(u => institutionUserIds.Contains(u.Id));
        }

        // ProfileVisibility kontrolü
        var isTeacher = await _context.InstitutionUsers
            .AsNoTracking()
            .AnyAsync(iu =>
                iu.UserId == currentUserId &&
                iu.Role == InstitutionRole.Teacher);

        var users = await userQuery
            .Include(u => u.InstitutionMemberships)
                .ThenInclude(im => im.Institution)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(u => new
            {
                u.Id,
                u.FullName,
                u.Username,
                u.ProfileImageUrl,
                u.GlobalRole,
                u.ProfileVisibility,
                Institutions = u.InstitutionMemberships.Select(im => new InstitutionSummaryDto
                {
                    Id = im.InstitutionId,
                    Name = im.Institution.Name,
                    Role = im.Role.ToString()
                }).ToList()
            })
            .ToListAsync();

        var results = users.Select(u => new UserSearchResultDto
        {
            Id = u.Id,
            FullName = u.FullName,
            Username = u.Username,
            ProfileImageUrl = u.ProfileImageUrl,
            GlobalRole = u.GlobalRole.ToString(),
            Institutions = u.Institutions,
            IsVisible = u.Id == currentUserId ||
                       u.ProfileVisibility == ProfileVisibility.PublicToAll ||
                       (u.ProfileVisibility == ProfileVisibility.TeachersOnly && isTeacher)
        }).ToList();

        // Cache for 2 minutes
        if (!forceRefresh)
        {
            await _cacheService.SetAsync(cacheKey, results, TimeSpan.FromMinutes(2));
        }

        return Ok(BaseResponse<List<UserSearchResultDto>>.SuccessResponse(results));
    }

    [HttpGet("institutions")]
    public async Task<IActionResult> SearchInstitutions(
        [FromQuery] string query,
        [FromQuery] InstitutionStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] bool forceRefresh = false)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest(BaseResponse<List<InstitutionSearchResultDto>>.ErrorResponse("Query is required", "300001"));

        // Cache key
        var cacheKey = $"search_institutions_{query}_{status}_{page}_{limit}";
        if (!forceRefresh)
        {
            var cached = await _cacheService.GetAsync<List<InstitutionSearchResultDto>>(cacheKey);
            if (cached != null)
                return Ok(BaseResponse<List<InstitutionSearchResultDto>>.SuccessResponse(cached));
        }

        var institutionQuery = _context.Institutions
            .Include(i => i.Manager)
            .AsNoTracking()
            .Where(i =>
                i.Name.Contains(query) ||
                i.LicenseNumber.Contains(query))
            .AsQueryable();

        if (status.HasValue)
            institutionQuery = institutionQuery.Where(i => i.Status == status.Value);

        var institutions = await institutionQuery
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(i => new InstitutionSearchResultDto
            {
                Id = i.Id,
                Name = i.Name,
                LicenseNumber = i.LicenseNumber,
                ManagerName = i.Manager.FullName,
                Status = i.Status.ToString(),
                CreatedAt = i.CreatedAt
            })
            .ToListAsync();

        // Cache for 5 minutes
        if (!forceRefresh)
        {
            await _cacheService.SetAsync(cacheKey, institutions, TimeSpan.FromMinutes(5));
        }

        return Ok(BaseResponse<List<InstitutionSearchResultDto>>.SuccessResponse(institutions));
    }

    [HttpGet("classrooms")]
    public async Task<IActionResult> SearchClassrooms(
        [FromQuery] string query,
        [FromQuery] int? institutionId = null,
        [FromQuery] int? grade = null,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] bool forceRefresh = false)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest(BaseResponse<List<ClassroomSearchResultDto>>.ErrorResponse("Query is required", "300001"));

        // Cache key
        var cacheKey = $"search_classrooms_{query}_{institutionId}_{grade}_{page}_{limit}";
        if (!forceRefresh)
        {
            var cached = await _cacheService.GetAsync<List<ClassroomSearchResultDto>>(cacheKey);
            if (cached != null)
                return Ok(BaseResponse<List<ClassroomSearchResultDto>>.SuccessResponse(cached));
        }

        var currentUserId = GetCurrentUserId();
        var classroomQuery = _context.Classrooms
            .Include(c => c.Institution)
            .AsNoTracking()
            .Where(c => c.Name.Contains(query))
            .AsQueryable();

        // Authorization: User can only see classrooms from their institutions
        if (!_sessionService.IsInGlobalRole(UserRole.AdminAdmin))
        {
            var userInstitutionIds = await _context.InstitutionUsers
                .Where(iu => iu.UserId == currentUserId)
                .Select(iu => iu.InstitutionId)
                .ToListAsync();
            classroomQuery = classroomQuery.Where(c => userInstitutionIds.Contains(c.InstitutionId));
        }

        if (institutionId.HasValue)
            classroomQuery = classroomQuery.Where(c => c.InstitutionId == institutionId.Value);

        if (grade.HasValue)
            classroomQuery = classroomQuery.Where(c => c.Grade == grade.Value);

        var classrooms = await classroomQuery
            .OrderBy(c => c.Name)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(c => new ClassroomSearchResultDto
            {
                Id = c.Id,
                Name = c.Name,
                Grade = c.Grade,
                InstitutionId = c.InstitutionId,
                InstitutionName = c.Institution.Name,
                StudentCount = _context.ClassroomStudents.Count(cs => cs.ClassroomId == c.Id && cs.RemovedAt == null)
            })
            .ToListAsync();

        // Cache for 3 minutes
        if (!forceRefresh)
        {
            await _cacheService.SetAsync(cacheKey, classrooms, TimeSpan.FromMinutes(3));
        }

        return Ok(BaseResponse<List<ClassroomSearchResultDto>>.SuccessResponse(classrooms));
    }

    [HttpGet("exams")]
    public async Task<IActionResult> SearchExams(
        [FromQuery] string query,
        [FromQuery] int? institutionId = null,
        [FromQuery] ExamType? type = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] bool forceRefresh = false)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest(BaseResponse<List<ExamSearchResultDto>>.ErrorResponse("Query is required", "300001"));

        // Cache key
        var cacheKey = $"search_exams_{query}_{institutionId}_{type}_{dateFrom?.ToString("yyyyMMdd")}_{dateTo?.ToString("yyyyMMdd")}_{page}_{limit}";
        if (!forceRefresh)
        {
            var cached = await _cacheService.GetAsync<List<ExamSearchResultDto>>(cacheKey);
            if (cached != null)
                return Ok(BaseResponse<List<ExamSearchResultDto>>.SuccessResponse(cached));
        }

        var currentUserId = GetCurrentUserId();
        var examQuery = _context.Exams
            .Include(e => e.Institution)
            .Include(e => e.Classroom)
            .AsNoTracking()
            .Where(e => e.Title.Contains(query))
            .AsQueryable();

        // Authorization: User can only see exams from their institutions
        if (!_sessionService.IsInGlobalRole(UserRole.AdminAdmin))
        {
            var userInstitutionIds = await _context.InstitutionUsers
                .Where(iu => iu.UserId == currentUserId)
                .Select(iu => iu.InstitutionId)
                .ToListAsync();
            examQuery = examQuery.Where(e => userInstitutionIds.Contains(e.InstitutionId));
        }

        if (institutionId.HasValue)
            examQuery = examQuery.Where(e => e.InstitutionId == institutionId.Value);

        if (type.HasValue)
            examQuery = examQuery.Where(e => e.Type == type.Value);

        if (dateFrom.HasValue)
            examQuery = examQuery.Where(e => e.ExamDate >= dateFrom.Value);

        if (dateTo.HasValue)
            examQuery = examQuery.Where(e => e.ExamDate <= dateTo.Value);

        var exams = await examQuery
            .OrderByDescending(e => e.ExamDate)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(e => new ExamSearchResultDto
            {
                Id = e.Id,
                Title = e.Title,
                Type = e.Type.ToString(),
                ExamDate = e.ExamDate,
                InstitutionId = e.InstitutionId,
                InstitutionName = e.Institution.Name,
                ClassroomId = e.ClassroomId,
                ClassroomName = e.Classroom != null ? e.Classroom.Name : null,
                ResultCount = _context.ExamResults.Count(er => er.ExamId == e.Id)
            })
            .ToListAsync();

        // Cache for 2 minutes
        if (!forceRefresh)
        {
            await _cacheService.SetAsync(cacheKey, exams, TimeSpan.FromMinutes(2));
        }

        return Ok(BaseResponse<List<ExamSearchResultDto>>.SuccessResponse(exams));
    }
}

public class InstitutionSearchResultDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string ManagerName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ClassroomSearchResultDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Grade { get; set; }
    public int InstitutionId { get; set; }
    public string InstitutionName { get; set; } = string.Empty;
    public int StudentCount { get; set; }
}

public class ExamSearchResultDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime ExamDate { get; set; }
    public int InstitutionId { get; set; }
    public string InstitutionName { get; set; } = string.Empty;
    public int? ClassroomId { get; set; }
    public string? ClassroomName { get; set; }
    public int ResultCount { get; set; }
}

