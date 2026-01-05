# Basit Yetki Kontrolü Kullanım Kılavuzu

## 🎯 Amaç

Her fonksiyonun başında basit bir yetki kontrolü yapmak. Kullanıcının `UserId`'sinden yetkisine bakıp, yetki yoksa hata döndürmek.

---

## 📝 ÖRNEK KULLANIM

### Örnek 1: CreateClassroomAsync (Manager veya AdminAdmin)

```csharp
public async Task<BaseResponse<int>> CreateClassroomAsync(int institutionId, string name, int grade)
{
    // 1. YETKİ KONTROLÜ (EN BAŞTA - ZORUNLU!)
    // Manager veya AdminAdmin olmalı
    var authError = await _authorizationService.RequireManagerByInstitutionAsync(institutionId);
    if (authError != null)
    {
        // Manager değilse AdminAdmin kontrolü yap
        var adminCheck = _authorizationService.RequireGlobalRole(UserRole.AdminAdmin);
        if (adminCheck != null)
            return BaseResponse<int>.ErrorResponse(
                authError.Error ?? "Yetkiniz yok", 
                authError.ErrorCode ?? ErrorCodes.AccessDenied);
    }

    // 2. Validation
    if (string.IsNullOrWhiteSpace(name))
        return BaseResponse<int>.ErrorResponse("Sınıf adı gereklidir", ErrorCodes.ValidationFailed);

    // 3. İş mantığı...
    var classroom = new Classroom { ... };
    // ...
}
```

### Örnek 2: Admin İşlemi (Sadece AdminAdmin)

```csharp
public async Task<BaseResponse<string>> CreateAdminAsync(CreateAdminRequest request, int adminId)
{
    // 1. YETKİ KONTROLÜ
    var authError = _authorizationService.RequireGlobalRole(UserRole.AdminAdmin);
    if (authError != null)
        return BaseResponse<string>.ErrorResponse(
            authError.Error ?? "Bu işlem için AdminAdmin yetkisi gereklidir", 
            authError.ErrorCode ?? ErrorCodes.AccessDenied);

    // 2. İş mantığı...
}
```

### Örnek 3: Teacher veya Manager İşlemi

```csharp
public async Task<BaseResponse<int>> CreateExamAsync(CreateExamDto dto)
{
    // 1. YETKİ KONTROLÜ
    var authError = await _authorizationService.RequireManagerOrTeacherAsync(dto.InstitutionId);
    if (authError != null)
    {
        // Admin kontrolü
        var adminCheck = _authorizationService.RequireGlobalRole(UserRole.AdminAdmin, UserRole.Admin);
        if (adminCheck != null)
            return BaseResponse<int>.ErrorResponse(
                authError.Error ?? "Yetkiniz yok", 
                authError.ErrorCode ?? ErrorCodes.AccessDenied);
    }

    // 2. İş mantığı...
}
```

### Örnek 4: Kendi İşlemi (Kendi Profilini Güncelleme)

```csharp
public async Task<BaseResponse<string>> UpdateProfileAsync(int userId, UpdateProfileRequest request)
{
    // 1. YETKİ KONTROLÜ - Kendi profili mi?
    var authError = _authorizationService.RequireOwnOperation(userId);
    if (authError != null)
        return BaseResponse<string>.ErrorResponse(
            authError.Error ?? "Bu işlemi sadece kendi hesabınız için yapabilirsiniz", 
            authError.ErrorCode ?? ErrorCodes.AccessDenied);

    // 2. İş mantığı...
}
```

### Örnek 5: Öğrenci Kısıtlaması

```csharp
public async Task<BaseResponse<int>> CreateExamAsync(CreateExamDto dto)
{
    // 1. YETKİ KONTROLÜ - Öğrenci olamaz
    var authError = await _authorizationService.RequireNotStudentAsync(dto.InstitutionId);
    if (authError != null)
    {
        // Admin kontrolü
        var adminCheck = _authorizationService.RequireGlobalRole(UserRole.AdminAdmin, UserRole.Admin);
        if (adminCheck != null)
            return BaseResponse<int>.ErrorResponse(
                authError.Error ?? "Öğrenciler bu işlemi yapamaz", 
                authError.ErrorCode ?? ErrorCodes.AccessDenied);
    }

    // 2. İş mantığı...
}
```

---

## 🔧 KULLANILABİLİR METODLAR

### Global Role Kontrolü
```csharp
// Tek rol
var error = _authorizationService.RequireGlobalRole(UserRole.AdminAdmin);

// Birden fazla rol (en az biri olmalı)
var error = _authorizationService.RequireGlobalRole(UserRole.AdminAdmin, UserRole.Admin);
```

### Kurumsal Role Kontrolü
```csharp
// Manager
var error = await _authorizationService.RequireManagerAsync(institutionId);

// Teacher
var error = await _authorizationService.RequireTeacherAsync(institutionId);

// Manager veya Teacher
var error = await _authorizationService.RequireManagerOrTeacherAsync(institutionId);

// Manuel Manager kontrolü (Institution.ManagerUserId)
var error = await _authorizationService.RequireManagerByInstitutionAsync(institutionId);

// Özel roller
var error = await _authorizationService.RequireInstitutionRoleAsync(
    institutionId, 
    InstitutionRole.Manager, 
    InstitutionRole.Teacher);
```

### Özel Kontroller
```csharp
// Kendi işlemi
var error = _authorizationService.RequireOwnOperation(targetUserId);

// İçerik sahibi
var error = await _authorizationService.RequireContentOwnerAsync(contentId);

// Öğrenci olamaz
var error = await _authorizationService.RequireNotStudentAsync(institutionId);
```

---

## 📋 TÜM FONKSİYONLAR İÇİN YETKİ KONTROLLERİ

### 1. CLASSROOM OPERATIONS

| Fonksiyon | Yetki Kontrolü |
|-----------|----------------|
| `CreateClassroomAsync` | `RequireManagerByInstitutionAsync` + `RequireGlobalRole(AdminAdmin)` ✅ ÖRNEK |
| `AddStudentToClassroomAsync` | `RequireManagerOrTeacherAsync` (classroom'dan institutionId al) + Admin |
| `UpdateClassroomAsync` | `RequireManagerByInstitutionAsync` (classroom'dan institutionId al) + Admin |
| `DeleteClassroomAsync` | `RequireManagerByInstitutionAsync` (classroom'dan institutionId al) + Admin |
| `RemoveStudentAsync` | `RequireManagerOrTeacherAsync` (classroom'dan institutionId al) + Admin |
| `GetClassroomDetailsAsync` | - (Herkes görebilir, ama sınıf üyesi kontrolü opsiyonel) |
| `GetClassroomsAsync` | - (Kurum üyeleri görebilir) |
| `GetStudentsAsync` | - (Sınıf üyeleri görebilir) |

### 2. EXAM OPERATIONS

| Fonksiyon | Yetki Kontrolü |
|-----------|----------------|
| `CreateExamAsync` | `RequireManagerOrTeacherAsync(institutionId)` + Admin |
| `ProcessOpticalResultsAsync` | `RequireManagerOrTeacherAsync` (exam'dan institutionId al) + Admin |
| `ConfirmResultsAndNotifyAsync` | `RequireManagerOrTeacherAsync` (exam'dan institutionId al) + Admin |
| `GetExamAsync` | - (Sınıf üyeleri görebilir) |
| `GetExamsAsync` | - (Kurum üyeleri görebilir) |
| `DeleteExamAsync` | `RequireManagerOrTeacherAsync` (exam'dan institutionId al) + Admin |
| `GetStudentReportAsync` | Kendi sonucu mu kontrolü + `RequireManagerOrTeacherAsync` + Admin |
| `GetClassroomReportAsync` | `RequireManagerOrTeacherAsync` (classroom'dan institutionId al) + Admin |
| `GetInstitutionReportAsync` | `RequireManagerOrTeacherAsync(institutionId)` + Admin |

### 3. INSTITUTION OPERATIONS

| Fonksiyon | Yetki Kontrolü |
|-----------|----------------|
| `AddUserToInstitutionAsync` | `RequireManagerByInstitutionAsync(institutionId)` + Admin |
| `RemoveUserFromInstitutionAsync` | `RequireManagerByInstitutionAsync(institutionId)` + Admin |
| `UpdateInstitutionAsync` | `RequireManagerByInstitutionAsync(institutionId)` + Admin |
| `UpdateUserRoleAsync` | `RequireManagerByInstitutionAsync(institutionId)` + Admin |
| `GetInstitutionDetailAsync` | - (Kurum üyeleri görebilir) |
| `GetInstitutionUsersAsync` | `RequireManagerOrTeacherAsync(institutionId)` + Admin |
| `GetInstitutionStatisticsAsync` | `RequireManagerOrTeacherAsync(institutionId)` + Admin |

### 4. SOCIAL OPERATIONS

| Fonksiyon | Yetki Kontrolü |
|-----------|----------------|
| `CreateContentAsync` | - (Herkes yapabilir) |
| `UpdateContentAsync` | `RequireContentOwnerAsync(contentId)` + Admin |
| `DeleteContentAsync` | `RequireContentOwnerAsync(contentId)` + Admin |
| `GetContentAnalyticsAsync` | `RequireContentOwnerAsync(contentId)` + Admin |
| `GetContentReportsAsync` | `RequireGlobalRole(AdminAdmin, Admin)` |
| `ReviewContentReportAsync` | `RequireGlobalRole(AdminAdmin, Admin)` |
| `CreatePollAsync` | `RequireContentOwnerAsync(contentId)` + Admin |
| `GetPollResultsAsync` | `RequireContentOwnerAsync(contentId)` + Admin |
| `PinContentAsync` | `RequireContentOwnerAsync(contentId)` + Admin |
| Diğerleri | - (Herkes yapabilir: beğenme, yorumlama, takip etme vb.) |

### 5. MESSAGE OPERATIONS

| Fonksiyon | Yetki Kontrolü |
|-----------|----------------|
| `SendMessageAsync` | - (Konuşma üyesi kontrolü iş mantığında) |
| `SendToClassAsync` | `RequireManagerOrTeacherAsync` (classroom'dan institutionId al) + Admin |
| `UpdateConversationAsync` | `RequireManagerOrTeacherAsync` (conversation'dan institutionId al) + Admin |
| `DeleteMessageAsync` | - (Kendi mesajı kontrolü iş mantığında) |
| `UpdateMessageAsync` | - (Kendi mesajı kontrolü iş mantığında) |
| Diğerleri | - (Konuşma üyesi kontrolü iş mantığında) |

### 6. USER OPERATIONS

| Fonksiyon | Yetki Kontrolü |
|-----------|----------------|
| `GetProfileAsync` | - (Profil görünürlük kontrolü iş mantığında) |
| `UpdateProfileAsync` | `RequireOwnOperation(userId)` |
| `ChangePasswordAsync` | `RequireOwnOperation(userId)` |
| `UploadProfileImageAsync` | `RequireOwnOperation(userId)` |
| `GetStatisticsAsync` | `RequireOwnOperation(userId)` |
| `UpdatePreferencesAsync` | `RequireOwnOperation(userId)` |
| Diğerleri | `RequireOwnOperation(userId)` veya profil görünürlük kontrolü |

### 7. ACCOUNT OPERATIONS

| Fonksiyon | Yetki Kontrolü |
|-----------|----------------|
| `ApproveAccountLinkAsync` | `RequireManagerByInstitutionAsync(institutionId)` + Admin |
| `RejectAccountLinkAsync` | `RequireManagerByInstitutionAsync(institutionId)` + Admin |
| Diğerleri | - (Kendi bağlantıları) |

### 8. ADMIN OPERATIONS

| Fonksiyon | Yetki Kontrolü |
|-----------|----------------|
| `GetAllUsersAsync` | `RequireGlobalRole(AdminAdmin, Admin)` |
| `UpdateUserAsync` | `RequireGlobalRole(AdminAdmin, Admin)` |
| `DeleteUserAsync` | `RequireGlobalRole(AdminAdmin, Admin)` |
| `GetAllInstitutionsAsync` | `RequireGlobalRole(AdminAdmin, Admin)` |
| `RejectInstitutionAsync` | `RequireGlobalRole(AdminAdmin)` |
| `CreateAdminAsync` | `RequireGlobalRole(AdminAdmin)` |
| `GetStatisticsAsync` | `RequireGlobalRole(AdminAdmin, Admin)` |
| `GetAuditLogsAsync` | `RequireGlobalRole(AdminAdmin, Admin)` |
| Diğerleri | `RequireGlobalRole(AdminAdmin, Admin)` |

### 9. AUTH OPERATIONS

| Fonksiyon | Yetki Kontrolü |
|-----------|----------------|
| `RegisterAsync` | - (Herkes yapabilir) |
| `LoginAsync` | - (Herkes yapabilir) |
| `RefreshTokenAsync` | - (Token kontrolü) |
| `ApplyInstitutionAsync` | - (Herkes yapabilir) |

---

## ⚠️ ÖNEMLİ NOTLAR

1. **Her fonksiyonun başında kontrol yapılmalı** - İş mantığından önce
2. **Admin her zaman geçer** - Admin kontrolü genellikle ikinci kontrol olarak yapılır
3. **Null kontrolü** - `authError == null` ise yetki var, devam et
4. **Hata mesajı** - `authError.Error` ve `authError.ErrorCode` kullanılır
5. **Kendi işlemleri** - `RequireOwnOperation` kullanılır
6. **Öğrenci kısıtlamaları** - `RequireNotStudentAsync` kullanılır

---

## 🔄 Uygulama Sırası

1. **Önce:** Tüm Operations sınıflarına `AuthorizationService` ekle (zaten var)
2. **Sonra:** Her fonksiyonun başına uygun yetki kontrolü ekle
3. **Son olarak:** Eski manuel kontrolleri kaldır

