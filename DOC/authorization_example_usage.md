# Yetki Kontrolü Örnek Kullanımları

## Örnek 1: ClassroomOperations - CreateClassroomAsync

### ÖNCE (Manuel Kontrol):
```csharp
public async Task<BaseResponse<int>> CreateClassroomAsync(int institutionId, string name, int grade)
{
    var currentUserId = _sessionService.GetUserId();
    var isManager = await _context.Institutions.AnyAsync(i => i.Id == institutionId && i.ManagerUserId == currentUserId);

    if (!isManager && !_sessionService.IsInGlobalRole(UserRole.AdminAdmin))
    {
        return BaseResponse<int>.ErrorResponse("Unauthorized", ErrorCodes.AccessDenied);
    }

    // ... iş mantığı
}
```

### SONRA (AuthorizationService ile):
```csharp
public async Task<BaseResponse<int>> CreateClassroomAsync(int institutionId, string name, int grade)
{
    // 1. YETKİ KONTROLÜ (EN BAŞTA!)
    if (!await _authorizationService.CanCreateClassroomAsync(institutionId))
    {
        return BaseResponse<int>.ErrorResponse("Sınıf oluşturma yetkiniz yok", ErrorCodes.AccessDenied);
    }

    // 2. Validation
    if (string.IsNullOrWhiteSpace(name))
        return BaseResponse<int>.ErrorResponse("Sınıf adı gereklidir", ErrorCodes.ValidationFailed);

    // 3. İş mantığı
    var classroom = new Classroom
    {
        InstitutionId = institutionId,
        Name = name,
        Grade = grade,
        CreatedAt = DateTime.UtcNow
    };

    _context.Classrooms.Add(classroom);
    await _context.SaveChangesAsync();

    // Cache invalidation
    await _cacheService.RemoveByPatternAsync($"Inst:{institutionId}:Classrooms");

    return BaseResponse<int>.SuccessResponse(classroom.Id);
}
```

## Örnek 2: ExamOperations - CreateExamAsync

### ÖNCE:
```csharp
public async Task<BaseResponse<int>> CreateExamAsync(CreateExamDto dto)
{
    var userId = _sessionService.GetUserId();
    var canCreate = await _context.InstitutionUsers.AnyAsync(iu =>
        iu.InstitutionId == dto.InstitutionId &&
        iu.UserId == userId &&
        (iu.Role == InstitutionRole.Manager || iu.Role == InstitutionRole.Teacher));

    if (!canCreate && !_sessionService.IsInGlobalRole(UserRole.AdminAdmin))
    {
        return BaseResponse<int>.ErrorResponse("Unauthorized", ErrorCodes.AccessDenied);
    }

    // ... iş mantığı
}
```

### SONRA:
```csharp
public async Task<BaseResponse<int>> CreateExamAsync(CreateExamDto dto)
{
    // 1. YETKİ KONTROLÜ
    if (!await _authorizationService.CanCreateExamAsync(dto.InstitutionId))
    {
        return BaseResponse<int>.ErrorResponse("Sınav oluşturma yetkiniz yok", ErrorCodes.AccessDenied);
    }

    // 2. Öğrenci kontrolü (ekstra güvenlik)
    if (dto.InstitutionId.HasValue)
    {
        if (await _authorizationService.IsStudentAsync(dto.InstitutionId.Value))
        {
            return BaseResponse<int>.ErrorResponse("Öğrenciler sınav oluşturamaz", ErrorCodes.AccessDenied);
        }
    }

    // 3. Validation
    if (string.IsNullOrWhiteSpace(dto.Title))
        return BaseResponse<int>.ErrorResponse("Sınav başlığı gereklidir", ErrorCodes.ValidationFailed);

    // 4. İş mantığı
    var exam = new Exam
    {
        InstitutionId = dto.InstitutionId,
        ClassroomId = dto.ClassroomId,
        Title = dto.Title,
        Type = dto.Type,
        ExamDate = dto.ExamDate,
        AnswerKeyJson = dto.AnswerKeyJson,
        LessonConfigJson = dto.LessonConfigJson,
        CreatedAt = DateTime.UtcNow
    };

    _context.Exams.Add(exam);
    await _context.SaveChangesAsync();

    await _cacheService.InvalidateExamCacheAsync();
    return BaseResponse<int>.SuccessResponse(exam.Id);
}
```

## Örnek 3: SocialOperations - CreateContentAsync

### ÖNCE:
```csharp
public async Task<BaseResponse<ContentDto>> CreateContentAsync(CreateContentRequest request)
{
    var userId = _sessionService.GetUserId();
    
    // Validation
    if (string.IsNullOrWhiteSpace(request.Title))
        return BaseResponse<ContentDto>.ErrorResponse("Title is required", ErrorCodes.ValidationFailed);

    // ... iş mantığı (yetki kontrolü yok!)
}
```

### SONRA:
```csharp
public async Task<BaseResponse<ContentDto>> CreateContentAsync(CreateContentRequest request)
{
    // 1. YETKİ KONTROLÜ
    // Not: Herkes içerik oluşturabilir, ama öğrenci kısıtlamaları kontrol edilebilir
    // İçerik oluşturma genelde herkese açık olduğu için kontrol gerekmez
    // Ama eğer özel bir durum varsa:
    
    // Örnek: Sadece öğretmenler Announcement oluşturabilir
    if (request.ContentType == ContentType.Announcement)
    {
        var userId = _sessionService.GetUserId();
        var isTeacher = await _context.InstitutionUsers
            .AnyAsync(iu => iu.UserId == userId && iu.Role == InstitutionRole.Teacher);
        
        if (!isTeacher && !_authorizationService.IsAdmin())
        {
            return BaseResponse<ContentDto>.ErrorResponse("Duyuru oluşturma yetkiniz yok", ErrorCodes.AccessDenied);
        }
    }

    // 2. Validation
    if (string.IsNullOrWhiteSpace(request.Title))
        return BaseResponse<ContentDto>.ErrorResponse("Başlık gereklidir", ErrorCodes.ValidationFailed);

    // 3. İş mantığı
    var userId = _sessionService.GetUserId();
    var content = new Content
    {
        AuthorId = userId,
        ContentType = request.ContentType,
        Title = request.Title,
        // ...
    };

    _context.Contents.Add(content);
    await _context.SaveChangesAsync();

    // Cache invalidation
    await _cacheService.RemoveByPatternAsync("Content:*");
    await _cacheService.RemoveByPatternAsync("Feed:*");

    return BaseResponse<ContentDto>.SuccessResponse(MapToContentDto(content));
}
```

## Örnek 4: ExamOperations - GetStudentReportAsync

### ÖNCE:
```csharp
public async Task<BaseResponse<StudentReportDto>> GetStudentReportAsync(int studentId, int? institutionId = null)
{
    var userId = _sessionService.GetUserId();
    
    // Karmaşık manuel kontrol
    var isOwner = studentId == userId;
    var isTeacher = institutionId.HasValue && await _context.InstitutionUsers.AnyAsync(iu =>
        iu.InstitutionId == institutionId.Value &&
        iu.UserId == userId &&
        iu.Role == InstitutionRole.Teacher);
    
    if (!isOwner && !isTeacher && !_sessionService.IsInGlobalRole(UserRole.AdminAdmin))
    {
        return BaseResponse<StudentReportDto>.ErrorResponse("Yetkiniz yok", ErrorCodes.AccessDenied);
    }

    // ... iş mantığı
}
```

### SONRA:
```csharp
public async Task<BaseResponse<StudentReportDto>> GetStudentReportAsync(int studentId, int? institutionId = null)
{
    // 1. YETKİ KONTROLÜ
    var userId = _sessionService.GetUserId();
    
    // Kendi raporunu herkes görebilir
    if (studentId == userId)
    {
        // İzin var, devam et
    }
    else
    {
        // Başka öğrencinin raporunu görüntüleme yetkisi
        // Öğretmen/Manager kendi sınıfının öğrencilerinin raporlarını görebilir
        if (institutionId.HasValue)
        {
            if (!await _authorizationService.IsManagerOrTeacherAsync(institutionId.Value))
            {
                return BaseResponse<StudentReportDto>.ErrorResponse("Bu öğrencinin raporunu görüntüleme yetkiniz yok", ErrorCodes.AccessDenied);
            }
        }
        else
        {
            // Standalone kullanıcılar başka öğrencilerin raporlarını göremez
            return BaseResponse<StudentReportDto>.ErrorResponse("Bu raporu görüntüleme yetkiniz yok", ErrorCodes.AccessDenied);
        }
    }

    // 2. Validation
    var student = await _context.Users.FindAsync(studentId);
    if (student == null)
        return BaseResponse<StudentReportDto>.ErrorResponse("Öğrenci bulunamadı", ErrorCodes.NotFound);

    // 3. İş mantığı
    // ... rapor oluşturma
}
```

## Örnek 5: InstitutionOperations - AddUserToInstitutionAsync

### ÖNCE:
```csharp
public async Task<BaseResponse<bool>> AddUserToInstitutionAsync(int institutionId, int userId, InstitutionRole role, string? number = null)
{
    var currentUserId = _sessionService.GetUserId();
    var isManager = await _context.Institutions.AnyAsync(i => i.Id == institutionId && i.ManagerUserId == currentUserId);

    if (!isManager && !_sessionService.IsInGlobalRole(UserRole.AdminAdmin))
    {
        return BaseResponse<bool>.ErrorResponse("Unauthorized", ErrorCodes.AccessDenied);
    }

    // ... iş mantığı
}
```

### SONRA:
```csharp
public async Task<BaseResponse<bool>> AddUserToInstitutionAsync(int institutionId, int userId, InstitutionRole role, string? number = null)
{
    // 1. YETKİ KONTROLÜ
    if (!await _authorizationService.CanAddUserToInstitutionAsync(institutionId))
    {
        return BaseResponse<bool>.ErrorResponse("Kuruma kullanıcı ekleme yetkiniz yok", ErrorCodes.AccessDenied);
    }

    // 2. Validation
    var institution = await _context.Institutions.FindAsync(institutionId);
    if (institution == null)
        return BaseResponse<bool>.ErrorResponse("Kurum bulunamadı", ErrorCodes.NotFound);

    var user = await _context.Users.FindAsync(userId);
    if (user == null)
        return BaseResponse<bool>.ErrorResponse("Kullanıcı bulunamadı", ErrorCodes.NotFound);

    // 3. İş mantığı
    var existing = await _context.InstitutionUsers
        .AnyAsync(iu => iu.UserId == userId && iu.InstitutionId == institutionId);
    
    if (existing)
        return BaseResponse<bool>.ErrorResponse("Kullanıcı zaten kurumda", ErrorCodes.ValidationFailed);

    var institutionUser = new InstitutionUser
    {
        InstitutionId = institutionId,
        UserId = userId,
        Role = role,
        StudentNumber = role == InstitutionRole.Student ? number : null,
        EmployeeNumber = role != InstitutionRole.Student ? number : null,
        AssignedAt = DateTime.UtcNow
    };

    _context.InstitutionUsers.Add(institutionUser);
    await _context.SaveChangesAsync();

    // Cache invalidation
    await _cacheService.RemoveByPatternAsync($"Inst:{institutionId}:Users:*");

    return BaseResponse<bool>.SuccessResponse(true);
}
```

## Örnek 6: MessageOperations - SendMessageAsync

### ÖNCE:
```csharp
public async Task<BaseResponse<bool>> SendMessageAsync(int conversationId, string text)
{
    var userId = _sessionService.GetUserId();
    
    // Konuşma üyesi mi?
    var isMember = await _context.ConversationMembers
        .AnyAsync(cm => cm.ConversationId == conversationId && cm.UserId == userId);
    
    if (!isMember)
        return BaseResponse<bool>.ErrorResponse("Unauthorized", ErrorCodes.AccessDenied);

    // ... iş mantığı
}
```

### SONRA:
```csharp
public async Task<BaseResponse<bool>> SendMessageAsync(int conversationId, string text)
{
    // 1. YETKİ KONTROLÜ
    var userId = _sessionService.GetUserId();
    
    // Konuşma üyesi mi?
    var isMember = await _context.ConversationMembers
        .AnyAsync(cm => cm.ConversationId == conversationId && cm.UserId == userId);
    
    if (!isMember && !_authorizationService.IsAdmin())
    {
        return BaseResponse<bool>.ErrorResponse("Bu konuşmaya mesaj gönderme yetkiniz yok", ErrorCodes.AccessDenied);
    }

    // Sınıf grubu ise özel kontrol
    var conversation = await _context.Conversations
        .Include(c => c.Classroom)
        .FirstOrDefaultAsync(c => c.Id == conversationId);
    
    if (conversation?.Type == ConversationType.ClassGroup && conversation.Classroom != null)
    {
        if (!await _authorizationService.CanSendMessageToClassroomAsync(conversation.ClassroomId ?? 0))
        {
            return BaseResponse<bool>.ErrorResponse("Bu sınıfa mesaj gönderme yetkiniz yok", ErrorCodes.AccessDenied);
        }
    }

    // 2. Validation
    if (string.IsNullOrWhiteSpace(text))
        return BaseResponse<bool>.ErrorResponse("Mesaj metni gereklidir", ErrorCodes.ValidationFailed);

    // 3. İş mantığı
    var message = new Message
    {
        ConversationId = conversationId,
        SenderId = userId,
        Text = text,
        CreatedAt = DateTime.UtcNow
    };

    _context.Messages.Add(message);
    await _context.SaveChangesAsync();

    // SignalR notification
    await _notificationHub.Clients.Group($"Conversation_{conversationId}")
        .SendAsync("NewMessage", MapToMessageDto(message));

    return BaseResponse<bool>.SuccessResponse(true);
}
```

## 📌 Genel Şablon

Her Operations metodunda şu sırayı takip edin:

```csharp
public async Task<BaseResponse<T>> YourMethodAsync(parameters...)
{
    // 1. YETKİ KONTROLÜ (EN BAŞTA - ZORUNLU!)
    if (!await _authorizationService.CanDoSomethingAsync(...))
    {
        return BaseResponse<T>.ErrorResponse("Açıklayıcı hata mesajı", ErrorCodes.AccessDenied);
    }

    // 2. VALIDATION
    if (parameter == null)
        return BaseResponse<T>.ErrorResponse("Validation hatası", ErrorCodes.ValidationFailed);

    // 3. İŞ MANTIĞI
    // ... veritabanı işlemleri
    // ... cache işlemleri
    // ... SignalR bildirimleri
    // ... audit log

    // 4. RESPONSE
    return BaseResponse<T>.SuccessResponse(result);
}
```

## ⚠️ ÖNEMLİ NOTLAR

1. **Her metodun EN BAŞINDA yetki kontrolü yapılmalıdır**
2. **Hata mesajları Türkçe ve açıklayıcı olmalıdır**
3. **Admin kontrolü genellikle AuthorizationService içinde yapılıyor, tekrar kontrol etmeye gerek yok**
4. **Öğrenci kısıtlamaları özellikle kritik işlemlerde kontrol edilmelidir**
5. **Standalone kullanıcılar için özel durumlar göz önünde bulundurulmalıdır**

