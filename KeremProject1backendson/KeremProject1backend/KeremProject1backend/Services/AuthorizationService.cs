using KeremProject1backend.Core.Constants;
using KeremProject1backend.Infrastructure;
using KeremProject1backend.Models.DTOs;
using KeremProject1backend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace KeremProject1backend.Services;

/// <summary>
/// Merkezi yetki kontrolü servisi. Tüm yetki kontrolleri bu servis üzerinden yapılır.
/// </summary>
public class AuthorizationService
{
    private readonly ApplicationContext _context;
    private readonly SessionService _sessionService;

    public AuthorizationService(ApplicationContext context, SessionService sessionService)
    {
        _context = context;
        _sessionService = sessionService;
    }

    /// <summary>
    /// Kullanıcının global role'ünü kontrol eder
    /// </summary>
    public bool IsInGlobalRole(UserRole role)
    {
        return _sessionService.IsInGlobalRole(role);
    }

    /// <summary>
    /// Kullanıcının AdminAdmin veya Admin olduğunu kontrol eder
    /// </summary>
    public bool IsAdmin()
    {
        return IsInGlobalRole(UserRole.AdminAdmin) || IsInGlobalRole(UserRole.Admin);
    }

    /// <summary>
    /// Kullanıcının belirli bir kurumda belirli bir role sahip olup olmadığını kontrol eder
    /// </summary>
    public async Task<bool> HasInstitutionRoleAsync(int institutionId, params InstitutionRole[] roles)
    {
        var userId = _sessionService.GetUserId();
        
        // Admin her şeyi yapabilir
        if (IsAdmin())
            return true;

        var hasRole = await _context.InstitutionUsers
            .AnyAsync(iu => iu.InstitutionId == institutionId &&
                           iu.UserId == userId &&
                           roles.Contains(iu.Role));

        return hasRole;
    }

    /// <summary>
    /// Kullanıcının belirli bir kurumda Manager olup olmadığını kontrol eder
    /// </summary>
    public async Task<bool> IsManagerAsync(int institutionId)
    {
        return await HasInstitutionRoleAsync(institutionId, InstitutionRole.Manager);
    }

    /// <summary>
    /// Kullanıcının belirli bir kurumda Teacher olup olmadığını kontrol eder
    /// </summary>
    public async Task<bool> IsTeacherAsync(int institutionId)
    {
        return await HasInstitutionRoleAsync(institutionId, InstitutionRole.Teacher);
    }

    /// <summary>
    /// Kullanıcının belirli bir kurumda Student olup olmadığını kontrol eder
    /// </summary>
    public async Task<bool> IsStudentAsync(int institutionId)
    {
        return await HasInstitutionRoleAsync(institutionId, InstitutionRole.Student);
    }

    /// <summary>
    /// Kullanıcının belirli bir kurumda Manager veya Teacher olup olmadığını kontrol eder
    /// </summary>
    public async Task<bool> IsManagerOrTeacherAsync(int institutionId)
    {
        return await HasInstitutionRoleAsync(institutionId, InstitutionRole.Manager, InstitutionRole.Teacher);
    }

    /// <summary>
    /// Kullanıcının belirli bir sınıfın öğretmeni olup olmadığını kontrol eder
    /// </summary>
    public async Task<bool> IsClassroomTeacherAsync(int classroomId)
    {
        var userId = _sessionService.GetUserId();

        if (IsAdmin())
            return true;

        var classroom = await _context.Classrooms
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == classroomId);

        if (classroom == null)
            return false;

        // Sınıf başkanı öğretmeni kontrolü
        if (classroom.HeadTeacherId == userId)
            return true;

        // Kurumda öğretmen mi?
        return await IsTeacherAsync(classroom.InstitutionId);
    }

    /// <summary>
    /// Kullanıcının belirli bir sınıfta öğrenci olup olmadığını kontrol eder
    /// </summary>
    public async Task<bool> IsClassroomStudentAsync(int classroomId)
    {
        var userId = _sessionService.GetUserId();

        var isStudent = await _context.ClassroomStudents
            .AsNoTracking()
            .AnyAsync(cs => cs.ClassroomId == classroomId &&
                          cs.Student.UserId == userId &&
                          cs.RemovedAt == null);

        return isStudent;
    }

    /// <summary>
    /// Kullanıcının belirli bir içeriğin sahibi olup olmadığını kontrol eder
    /// </summary>
    public async Task<bool> IsContentOwnerAsync(int contentId)
    {
        var userId = _sessionService.GetUserId();

        if (IsAdmin())
            return true;

        var isOwner = await _context.Contents
            .AsNoTracking()
            .AnyAsync(c => c.Id == contentId && c.AuthorId == userId && !c.IsDeleted);

        return isOwner;
    }

    /// <summary>
    /// Kullanıcının belirli bir kullanıcının profilini görüntüleme yetkisi olup olmadığını kontrol eder
    /// </summary>
    public async Task<bool> CanViewProfileAsync(int targetUserId)
    {
        var userId = _sessionService.GetUserId();

        // Kendi profilini her zaman görebilir
        if (userId == targetUserId)
            return true;

        if (IsAdmin())
            return true;

        var targetUser = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == targetUserId);

        if (targetUser == null)
            return false;

        // Public profil kontrolü
        if (targetUser.ProfileVisibility == ProfileVisibility.PublicToAll)
            return true;

        // TeachersOnly kontrolü
        if (targetUser.ProfileVisibility == ProfileVisibility.TeachersOnly)
        {
            // Kullanıcının herhangi bir kurumda öğretmen olup olmadığını kontrol et
            var isTeacher = await _context.InstitutionUsers
                .AsNoTracking()
                .AnyAsync(iu => iu.UserId == userId && iu.Role == InstitutionRole.Teacher);
            return isTeacher;
        }

        // Private profil - sadece kendisi görebilir (zaten yukarıda kontrol edildi)
        return false;
    }

    /// <summary>
    /// Öğrencilerin belirli bir işlemi yapıp yapamayacağını kontrol eder
    /// </summary>
    public async Task<bool> CanStudentPerformActionAsync(int institutionId, string action)
    {
        // Öğrenci değilse true döner
        if (!await IsStudentAsync(institutionId))
            return true;

        // Öğrencilerin yapabileceği işlemler
        var allowedActions = new[]
        {
            "view_own_exam_results",
            "view_own_profile",
            "update_own_profile",
            "view_classroom",
            "view_classmates",
            "send_message",
            "create_content",
            "like_content",
            "comment_content",
            "follow_user",
            "save_content"
        };

        return allowedActions.Contains(action.ToLower());
    }

    /// <summary>
    /// Öğrencilerin yapamayacağı işlemler için kontrol
    /// </summary>
    public async Task<bool> CanPerformActionAsync(int? institutionId, string action)
    {
        if (IsAdmin())
            return true;

        // InstitutionId yoksa sadece admin kontrolü yeterli
        if (!institutionId.HasValue)
            return true;

        // Öğrenci kontrolü
        if (await IsStudentAsync(institutionId.Value))
        {
            return await CanStudentPerformActionAsync(institutionId.Value, action);
        }

        // Öğretmen ve Manager'lar çoğu işlemi yapabilir
        if (await IsManagerOrTeacherAsync(institutionId.Value))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Kullanıcının belirli bir sınavı görüntüleme yetkisi olup olmadığını kontrol eder
    /// </summary>
    public async Task<bool> CanViewExamAsync(int examId)
    {
        var userId = _sessionService.GetUserId();

        if (IsAdmin())
            return true;

        var exam = await _context.Exams
            .AsNoTracking()
            .Include(e => e.Classroom)
            .FirstOrDefaultAsync(e => e.Id == examId);

        if (exam == null)
            return false;

        // Sınavı oluşturan öğretmen/manager görebilir
        // TODO: Exam tablosuna CreatedByUserId eklenmeli
        // Şimdilik sınıf öğretmeni kontrolü yapıyoruz
        if (await IsClassroomTeacherAsync(exam.ClassroomId))
            return true;

        // Sınıf öğrencisi kendi sonuçlarını görebilir
        if (await IsClassroomStudentAsync(exam.ClassroomId))
            return true;

        return false;
    }

    /// <summary>
    /// Kullanıcının belirli bir sınav sonucunu görüntüleme yetkisi olup olmadığını kontrol eder
    /// </summary>
    public async Task<bool> CanViewExamResultAsync(int examResultId)
    {
        var userId = _sessionService.GetUserId();

        if (IsAdmin())
            return true;

        var examResult = await _context.ExamResults
            .AsNoTracking()
            .Include(er => er.Exam)
                .ThenInclude(e => e.Classroom)
            .FirstOrDefaultAsync(er => er.Id == examResultId);

        if (examResult == null)
            return false;

        // Kendi sonucunu görebilir
        if (examResult.StudentId == userId)
            return true;

        // Öğretmen/Manager görebilir
        if (examResult.Exam != null && examResult.Exam.Classroom != null)
        {
            return await IsClassroomTeacherAsync(examResult.Exam.ClassroomId);
        }

        return false;
    }

    // ========== KAPSAMLI YETKİ KONTROLLERİ ==========

    /// <summary>
    /// Kullanıcının kurum oluşturma yetkisi var mı?
    /// </summary>
    public bool CanCreateInstitution()
    {
        return IsAdmin();
    }

    /// <summary>
    /// Kullanıcının kurum onaylama yetkisi var mı?
    /// </summary>
    public bool CanApproveInstitution()
    {
        return IsInGlobalRole(UserRole.AdminAdmin);
    }

    /// <summary>
    /// Kullanıcının belirli bir kurumu yönetme yetkisi var mı?
    /// </summary>
    public async Task<bool> CanManageInstitutionAsync(int institutionId)
    {
        if (IsAdmin())
            return true;

        return await IsManagerAsync(institutionId);
    }

    /// <summary>
    /// Kullanıcının kurum içi kullanıcı ekleme yetkisi var mı?
    /// </summary>
    public async Task<bool> CanAddUserToInstitutionAsync(int institutionId)
    {
        if (IsAdmin())
            return true;

        return await IsManagerAsync(institutionId);
    }

    /// <summary>
    /// Kullanıcının kullanıcı silme yetkisi var mı?
    /// </summary>
    public async Task<bool> CanDeleteUserAsync(int targetUserId, int? institutionId = null)
    {
        if (IsAdmin())
            return true;

        // Kendi hesabını silebilir
        var userId = _sessionService.GetUserId();
        if (userId == targetUserId)
            return true;

        // Kurum yöneticisi kurum içi kullanıcıları silebilir
        if (institutionId.HasValue && await IsManagerAsync(institutionId.Value))
        {
            var targetUserInInstitution = await _context.InstitutionUsers
                .AnyAsync(iu => iu.UserId == targetUserId && iu.InstitutionId == institutionId.Value);
            return targetUserInInstitution;
        }

        return false;
    }

    /// <summary>
    /// Kullanıcının sınıf oluşturma yetkisi var mı?
    /// </summary>
    public async Task<bool> CanCreateClassroomAsync(int? institutionId)
    {
        if (IsAdmin())
            return true;

        // Standalone öğretmenler kendi sınıflarını oluşturabilir
        if (!institutionId.HasValue)
        {
            // Kullanıcının herhangi bir kurumda öğretmen olup olmadığını kontrol et
            var userId = _sessionService.GetUserId();
            var isTeacher = await _context.InstitutionUsers
                .AnyAsync(iu => iu.UserId == userId && iu.Role == InstitutionRole.Teacher);
            return isTeacher;
        }

        // Kurum içi: Sadece Manager sınıf oluşturabilir
        return await IsManagerAsync(institutionId.Value);
    }

    /// <summary>
    /// Kullanıcının belirli bir sınıfı yönetme yetkisi var mı?
    /// </summary>
    public async Task<bool> CanManageClassroomAsync(int classroomId)
    {
        if (IsAdmin())
            return true;

        var classroom = await _context.Classrooms
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == classroomId);

        if (classroom == null)
            return false;

        // Manager kurum içi sınıfları yönetebilir
        if (await IsManagerAsync(classroom.InstitutionId))
            return true;

        // Öğretmen kendi sınıfını yönetebilir
        return await IsClassroomTeacherAsync(classroomId);
    }

    /// <summary>
    /// Kullanıcının sınıfa öğrenci ekleme yetkisi var mı?
    /// </summary>
    public async Task<bool> CanAddStudentToClassroomAsync(int classroomId)
    {
        return await CanManageClassroomAsync(classroomId);
    }

    /// <summary>
    /// Kullanıcının sınav oluşturma yetkisi var mı?
    /// </summary>
    public async Task<bool> CanCreateExamAsync(int? institutionId)
    {
        if (IsAdmin())
            return true;

        // Standalone öğretmenler sınav oluşturabilir
        if (!institutionId.HasValue)
        {
            var userId = _sessionService.GetUserId();
            var isTeacher = await _context.InstitutionUsers
                .AnyAsync(iu => iu.UserId == userId && iu.Role == InstitutionRole.Teacher);
            return isTeacher;
        }

        // Kurum içi: Manager ve Teacher sınav oluşturabilir
        return await IsManagerOrTeacherAsync(institutionId.Value);
    }

    /// <summary>
    /// Kullanıcının belirli bir sınavı yönetme yetkisi var mı?
    /// </summary>
    public async Task<bool> CanManageExamAsync(int examId)
    {
        if (IsAdmin())
            return true;

        var exam = await _context.Exams
            .AsNoTracking()
            .Include(e => e.Classroom)
            .FirstOrDefaultAsync(e => e.Id == examId);

        if (exam == null)
            return false;

        // Manager kurum içi sınavları yönetebilir
        if (await IsManagerAsync(exam.InstitutionId))
            return true;

        // Teacher kendi sınavlarını yönetebilir
        // TODO: Exam tablosuna CreatedByUserId eklenmeli
        // Şimdilik sınıf öğretmeni kontrolü yapıyoruz
        return await IsClassroomTeacherAsync(exam.ClassroomId);
    }

    /// <summary>
    /// Kullanıcının optik sonuç işleme yetkisi var mı?
    /// </summary>
    public async Task<bool> CanProcessOpticalResultsAsync(int examId)
    {
        return await CanManageExamAsync(examId);
    }

    /// <summary>
    /// Kullanıcının sınav sonuçlarını onaylama yetkisi var mı?
    /// </summary>
    public async Task<bool> CanConfirmExamResultsAsync(int examId)
    {
        return await CanManageExamAsync(examId);
    }

    /// <summary>
    /// Kullanıcının başka bir öğrencinin sınav sonucunu görüntüleme yetkisi var mı?
    /// </summary>
    public async Task<bool> CanViewOtherStudentResultAsync(int examResultId)
    {
        if (IsAdmin())
            return true;

        var examResult = await _context.ExamResults
            .AsNoTracking()
            .Include(er => er.Exam)
                .ThenInclude(e => e.Classroom)
            .FirstOrDefaultAsync(er => er.Id == examResultId);

        if (examResult == null)
            return false;

        // Kendi sonucunu her zaman görebilir
        var userId = _sessionService.GetUserId();
        if (examResult.StudentId == userId)
            return true;

        // Öğretmen/Manager kendi sınıfının öğrencilerinin sonuçlarını görebilir
        if (examResult.Exam != null && examResult.Exam.Classroom != null)
        {
            return await IsClassroomTeacherAsync(examResult.Exam.ClassroomId) ||
                   await IsManagerAsync(examResult.Exam.InstitutionId);
        }

        return false;
    }

    /// <summary>
    /// Kullanıcının sınıf karnesini görüntüleme yetkisi var mı?
    /// </summary>
    public async Task<bool> CanViewClassroomReportAsync(int classroomId)
    {
        if (IsAdmin())
            return true;

        var classroom = await _context.Classrooms
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == classroomId);

        if (classroom == null)
            return false;

        // Manager ve Teacher sınıf karnesini görebilir
        if (await IsManagerAsync(classroom.InstitutionId))
            return true;

        return await IsClassroomTeacherAsync(classroomId);
    }

    /// <summary>
    /// Kullanıcının içerik oluşturma yetkisi var mı? (Herkes yapabilir)
    /// </summary>
    public bool CanCreateContent()
    {
        return true; // Tüm kullanıcılar içerik oluşturabilir
    }

    /// <summary>
    /// Kullanıcının belirli bir içeriği yönetme yetkisi var mı?
    /// </summary>
    public async Task<bool> CanManageContentAsync(int contentId)
    {
        if (IsAdmin())
            return true;

        return await IsContentOwnerAsync(contentId);
    }

    /// <summary>
    /// Kullanıcının sınıf grubuna mesaj gönderme yetkisi var mı?
    /// </summary>
    public async Task<bool> CanSendMessageToClassroomAsync(int classroomId)
    {
        if (IsAdmin())
            return true;

        var classroom = await _context.Classrooms
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == classroomId);

        if (classroom == null)
            return false;

        // Manager, Teacher ve Student sınıf grubuna mesaj gönderebilir
        if (await IsManagerAsync(classroom.InstitutionId))
            return true;

        if (await IsClassroomTeacherAsync(classroomId))
            return true;

        return await IsClassroomStudentAsync(classroomId);
    }

    /// <summary>
    /// Kullanıcının toplu mesaj gönderme yetkisi var mı?
    /// </summary>
    public async Task<bool> CanSendBulkMessageAsync(int? institutionId, int? classroomId)
    {
        if (IsAdmin())
            return true;

        // Standalone öğretmenler kendi sınıflarına toplu mesaj gönderebilir
        if (!institutionId.HasValue && classroomId.HasValue)
        {
            var userId = _sessionService.GetUserId();
            var classroom = await _context.Classrooms
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == classroomId.Value);
            
            if (classroom != null && classroom.HeadTeacherId == userId)
                return true;
        }

        // Kurum içi: Manager ve Teacher toplu mesaj gönderebilir
        if (institutionId.HasValue)
        {
            if (await IsManagerAsync(institutionId.Value))
                return true;

            if (classroomId.HasValue)
                return await IsClassroomTeacherAsync(classroomId.Value);
        }

        return false;
    }

    /// <summary>
    /// Kullanıcının konuşma güncelleme yetkisi var mı?
    /// </summary>
    public async Task<bool> CanUpdateConversationAsync(int conversationId)
    {
        if (IsAdmin())
            return true;

        var conversation = await _context.Conversations
            .AsNoTracking()
            .Include(c => c.Classroom)
            .FirstOrDefaultAsync(c => c.Id == conversationId);

        if (conversation == null)
            return false;

        // Sınıf grubu ise sadece Manager ve Teacher güncelleyebilir
        if (conversation.Type == Models.Enums.ConversationType.ClassGroup && conversation.Classroom != null)
        {
            if (await IsManagerAsync(conversation.Classroom.InstitutionId))
                return true;

            return await IsClassroomTeacherAsync(conversation.ClassroomId ?? 0);
        }

        // Standalone öğretmen kendi sınıfını güncelleyebilir
        if (conversation.Classroom != null && conversation.Classroom.HeadTeacherId == _sessionService.GetUserId())
            return true;

        // Özel konuşmalar güncellenemez
        return false;
    }

    /// <summary>
    /// Kullanıcının hesap bağlama isteği onaylama yetkisi var mı?
    /// </summary>
    public async Task<bool> CanApproveAccountLinkAsync(int institutionId)
    {
        if (IsAdmin())
            return true;

        return await IsManagerAsync(institutionId);
    }

    /// <summary>
    /// Kullanıcının özel ders bilgisi oluşturma yetkisi var mı?
    /// </summary>
    public async Task<bool> CanCreatePrivateTutoringInfoAsync()
    {
        if (IsAdmin())
            return true;

        var userId = _sessionService.GetUserId();
        
        // Herhangi bir kurumda öğretmen mi?
        var isTeacher = await _context.InstitutionUsers
            .AnyAsync(iu => iu.UserId == userId && iu.Role == InstitutionRole.Teacher);

        return isTeacher;
    }

    /// <summary>
    /// Kullanıcının özel ders bilgisi yönetme yetkisi var mı?
    /// </summary>
    public async Task<bool> CanManagePrivateTutoringInfoAsync(int tutoringInfoId)
    {
        if (IsAdmin())
            return true;

        // TODO: PrivateTutoringInfo tablosu oluşturulduğunda bu kontrolü ekle
        // Şimdilik öğretmen kontrolü yapıyoruz
        return await CanCreatePrivateTutoringInfoAsync();
    }

    /// <summary>
    /// Kullanıcının rapor oluşturma yetkisi var mı?
    /// </summary>
    public async Task<bool> CanCreateReportAsync(int? institutionId)
    {
        if (IsAdmin())
            return true;

        // Standalone öğretmenler kendi raporlarını oluşturabilir
        if (!institutionId.HasValue)
        {
            return await CanCreatePrivateTutoringInfoAsync(); // Öğretmen kontrolü
        }

        // Kurum içi: Manager ve Teacher rapor oluşturabilir
        return await IsManagerOrTeacherAsync(institutionId.Value);
    }

    /// <summary>
    /// Kullanıcının kurum raporunu görüntüleme yetkisi var mı?
    /// </summary>
    public async Task<bool> CanViewInstitutionReportAsync(int institutionId)
    {
        if (IsAdmin())
            return true;

        // Manager ve Teacher kurum raporunu görebilir
        return await IsManagerOrTeacherAsync(institutionId);
    }

    /// <summary>
    /// Kullanıcının standalone (bağımsız) kullanıcı olup olmadığını kontrol eder
    /// </summary>
    public async Task<bool> IsStandaloneUserAsync()
    {
        var userId = _sessionService.GetUserId();
        
        // Herhangi bir kurumda üye mi?
        var hasInstitution = await _context.InstitutionUsers
            .AnyAsync(iu => iu.UserId == userId);

        return !hasInstitution;
    }

    /// <summary>
    /// Kullanıcının standalone öğretmen olup olmadığını kontrol eder
    /// </summary>
    public async Task<bool> IsStandaloneTeacherAsync()
    {
        if (await IsStandaloneUserAsync())
        {
            // TODO: User tablosuna bir alan eklenebilir veya başka bir kontrol yapılabilir
            // Şimdilik standalone kullanıcılar öğretmen olabilir
            return true;
        }

        return false;
    }

    /// <summary>
    /// Genel yetki kontrolü - işlem adına göre yetki kontrolü yapar
    /// </summary>
    public async Task<BaseResponse<bool>> CheckPermissionAsync(string action, params object[] parameters)
    {
        try
        {
            bool hasPermission = false;

            switch (action.ToLower())
            {
                case "create_institution":
                    hasPermission = CanCreateInstitution();
                    break;

                case "approve_institution":
                    hasPermission = CanApproveInstitution();
                    break;

                case "manage_institution":
                    if (parameters.Length > 0 && parameters[0] is int institutionId)
                        hasPermission = await CanManageInstitutionAsync(institutionId);
                    break;

                case "create_classroom":
                    int? instId = parameters.Length > 0 && parameters[0] is int id ? id : null;
                    hasPermission = await CanCreateClassroomAsync(instId);
                    break;

                case "manage_classroom":
                    if (parameters.Length > 0 && parameters[0] is int classroomId)
                        hasPermission = await CanManageClassroomAsync(classroomId);
                    break;

                case "create_exam":
                    int? examInstId = parameters.Length > 0 && parameters[0] is int eid ? eid : null;
                    hasPermission = await CanCreateExamAsync(examInstId);
                    break;

                case "manage_exam":
                    if (parameters.Length > 0 && parameters[0] is int examId)
                        hasPermission = await CanManageExamAsync(examId);
                    break;

                case "create_content":
                    hasPermission = CanCreateContent();
                    break;

                case "manage_content":
                    if (parameters.Length > 0 && parameters[0] is int contentId)
                        hasPermission = await CanManageContentAsync(contentId);
                    break;

                default:
                    return Models.DTOs.BaseResponse<bool>.ErrorResponse($"Unknown action: {action}", "100400");
            }

            if (!hasPermission)
                return Models.DTOs.BaseResponse<bool>.ErrorResponse("Unauthorized", Core.Constants.ErrorCodes.AccessDenied);

            return Models.DTOs.BaseResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            return Models.DTOs.BaseResponse<bool>.ErrorResponse($"Authorization check failed: {ex.Message}", "100500");
        }
    }
}

