# Basitleştirilmiş Yetki Kontrolü Sistemi

## 🎯 Amaç

Sadece **temel fonksiyonlar** kullanarak basit ve anlaşılır yetki kontrolü.

---

## 📋 KALAN FONKSİYONLAR (Sadece 6 Tane!)

### 1. `RequireGlobalRole(params UserRole[] requiredRoles)`
**Kullanım:** Global role kontrolü (AdminAdmin, Admin, User)

**Örnek:**
```csharp
// Sadece AdminAdmin
var error = _authorizationService.RequireGlobalRole(UserRole.AdminAdmin);
if (error != null)
    return BaseResponse<T>.ErrorResponse(error.Error, error.ErrorCode);

// AdminAdmin veya Admin
var error = _authorizationService.RequireGlobalRole(UserRole.AdminAdmin, UserRole.Admin);
```

### 2. `RequireInstitutionRoleAsync(int institutionId, params InstitutionRole[] requiredRoles)`
**Kullanım:** Kurumsal role kontrolü (Manager, Teacher, Student)

**Örnek:**
```csharp
// Manager
var error = await _authorizationService.RequireInstitutionRoleAsync(institutionId, InstitutionRole.Manager);
if (error != null)
    return BaseResponse<T>.ErrorResponse(error.Error, error.ErrorCode);

// Manager veya Teacher
var error = await _authorizationService.RequireInstitutionRoleAsync(
    institutionId, 
    InstitutionRole.Manager, 
    InstitutionRole.Teacher);
```

### 3. `RequireManagerByInstitutionAsync(int institutionId)`
**Kullanım:** Institution.ManagerUserId kontrolü (özel durumlar için)

**Örnek:**
```csharp
var error = await _authorizationService.RequireManagerByInstitutionAsync(institutionId);
if (error != null)
    return BaseResponse<T>.ErrorResponse(error.Error, error.ErrorCode);
```

### 4. `RequireNotStudentAsync(int? institutionId)`
**Kullanım:** Öğrenci olamaz kontrolü

**Örnek:**
```csharp
var error = await _authorizationService.RequireNotStudentAsync(institutionId);
if (error != null)
    return BaseResponse<T>.ErrorResponse(error.Error, error.ErrorCode);
```

### 5. `RequireContentOwnerAsync(int contentId)`
**Kullanım:** İçerik sahibi kontrolü

**Örnek:**
```csharp
var error = await _authorizationService.RequireContentOwnerAsync(contentId);
if (error != null)
    return BaseResponse<T>.ErrorResponse(error.Error, error.ErrorCode);
```

### 6. `RequireOwnOperation(int targetUserId)`
**Kullanım:** Kendi işlemi kontrolü (kendi profili, kendi hesabı vb.)

**Örnek:**
```csharp
var error = _authorizationService.RequireOwnOperation(userId);
if (error != null)
    return BaseResponse<T>.ErrorResponse(error.Error, error.ErrorCode);
```

---

## ✅ ÖNEMLİ NOTLAR

1. **Tüm fonksiyonlar Admin'i otomatik geçer** - Ayrıca Admin kontrolü yapmaya gerek yok
2. **Null kontrolü** - `error == null` ise yetki var, devam et
3. **Hata mesajı** - `error.Error` ve `error.ErrorCode` kullanılır

---

## 📝 KULLANIM ÖRNEKLERİ

### Örnek 1: Sınıf Oluşturma (Manager veya Admin)
```csharp
public async Task<BaseResponse<int>> CreateClassroomAsync(int institutionId, string name, int grade)
{
    // Manager veya Admin (RequireManagerByInstitutionAsync zaten Admin kontrolü yapıyor)
    var authError = await _authorizationService.RequireManagerByInstitutionAsync(institutionId);
    if (authError != null)
        return BaseResponse<int>.ErrorResponse(
            authError.Error ?? "Yetkiniz yok", 
            authError.ErrorCode ?? ErrorCodes.AccessDenied);

    // İş mantığı...
}
```

### Örnek 2: Sınav Oluşturma (Manager veya Teacher veya Admin)
```csharp
public async Task<BaseResponse<int>> CreateExamAsync(CreateExamDto dto)
{
    // Manager veya Teacher veya Admin
    var authError = await _authorizationService.RequireInstitutionRoleAsync(
        dto.InstitutionId, 
        InstitutionRole.Manager, 
        InstitutionRole.Teacher);
    if (authError != null)
        return BaseResponse<int>.ErrorResponse(
            authError.Error ?? "Yetkiniz yok", 
            authError.ErrorCode ?? ErrorCodes.AccessDenied);

    // İş mantığı...
}
```

### Örnek 3: Admin İşlemi (Sadece AdminAdmin)
```csharp
public async Task<BaseResponse<string>> CreateAdminAsync(CreateAdminRequest request)
{
    // Sadece AdminAdmin
    var authError = _authorizationService.RequireGlobalRole(UserRole.AdminAdmin);
    if (authError != null)
        return BaseResponse<string>.ErrorResponse(
            authError.Error ?? "Bu işlem için AdminAdmin yetkisi gereklidir", 
            authError.ErrorCode ?? ErrorCodes.AccessDenied);

    // İş mantığı...
}
```

### Örnek 4: Kendi Profilini Güncelleme
```csharp
public async Task<BaseResponse<string>> UpdateProfileAsync(int userId, UpdateProfileRequest request)
{
    // Kendi işlemi veya Admin
    var authError = _authorizationService.RequireOwnOperation(userId);
    if (authError != null)
        return BaseResponse<string>.ErrorResponse(
            authError.Error ?? "Bu işlemi sadece kendi hesabınız için yapabilirsiniz", 
            authError.ErrorCode ?? ErrorCodes.AccessDenied);

    // İş mantığı...
}
```

### Örnek 5: İçerik Güncelleme (Sahibi veya Admin)
```csharp
public async Task<BaseResponse<bool>> UpdateContentAsync(int contentId, UpdateContentRequest request)
{
    // İçerik sahibi veya Admin
    var authError = await _authorizationService.RequireContentOwnerAsync(contentId);
    if (authError != null)
        return BaseResponse<bool>.ErrorResponse(
            authError.Error ?? "Bu içeriği yönetme yetkiniz yok", 
            authError.ErrorCode ?? ErrorCodes.AccessDenied);

    // İş mantığı...
}
```

### Örnek 6: Öğrenci Kısıtlaması
```csharp
public async Task<BaseResponse<int>> CreateExamAsync(CreateExamDto dto)
{
    // Öğrenci olamaz (Admin geçer)
    var authError = await _authorizationService.RequireNotStudentAsync(dto.InstitutionId);
    if (authError != null)
        return BaseResponse<int>.ErrorResponse(
            authError.Error ?? "Öğrenciler bu işlemi yapamaz", 
            authError.ErrorCode ?? ErrorCodes.AccessDenied);

    // İş mantığı...
}
```

---

## 🗑️ KALDIRILAN FONKSİYONLAR

Aşağıdaki fonksiyonlar kaldırıldı (artık yok):
- ❌ `CanCreateInstitution`
- ❌ `CanManageInstitutionAsync`
- ❌ `CanCreateClassroomAsync`
- ❌ `CanManageClassroomAsync`
- ❌ `CanCreateExamAsync`
- ❌ `CanManageExamAsync`
- ❌ `CanCreateContent`
- ❌ `CanManageContentAsync`
- ❌ `IsManagerAsync`
- ❌ `IsTeacherAsync`
- ❌ `IsStudentAsync`
- ❌ `IsManagerOrTeacherAsync`
- ❌ `RequireManagerAsync`
- ❌ `RequireTeacherAsync`
- ❌ `RequireManagerOrTeacherAsync`
- ❌ Ve diğer tüm `Can...` ve `Is...` fonksiyonları

**Yerine:** Sadece yukarıdaki 6 temel fonksiyon kullanılacak!

