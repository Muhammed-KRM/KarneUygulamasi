# Tek Fonksiyon Yetki Kontrolü Sistemi

## 🎯 Amaç

Sadece **tek bir fonksiyon** ile basit ve anlaşılır yetki kontrolü. User'ın `GlobalRole`'üne bakarak kontrol yapılır.

---

## 📋 UserRole Enum'u

```csharp
public enum UserRole : byte
{
    AdminAdmin = 0,        // Sistem kurucusu
    Admin = 1,             // Sistem yöneticisi
    Manager = 2,           // Kurum yöneticisi (Dershane yöneticisi)
    Teacher = 3,           // Öğretmen (kuruma bağlı veya bağımsız)
    Student = 4,           // Öğrenci (kuruma bağlı veya bağımsız)
    StandaloneTeacher = 5, // Bağımsız öğretmen (kuruma bağlı değil)
    StandaloneStudent = 6  // Bağımsız öğrenci (kuruma bağlı değil)
}
```

---

## 🔧 TEK FONKSİYON: `RequireGlobalRole`

### Tanım
```csharp
public BaseResponse<string>? RequireGlobalRole(params UserRole[] requiredRoles)
```

### Açıklama
- Session'dan `UserId` alınır
- `User` tablosundan `GlobalRole` kontrol edilir
- Yetki yoksa `BaseResponse<string>` hatası döndürür
- Yetki varsa `null` döndürür

### Kullanım Şablonu
```csharp
public async Task<BaseResponse<T>> YourFunctionAsync(...)
{
    // 1. YETKİ KONTROLÜ (EN BAŞTA - ZORUNLU!)
    var authError = _authorizationService.RequireGlobalRole(
        UserRole.Manager, 
        UserRole.AdminAdmin, 
        UserRole.Admin);
    if (authError != null)
        return BaseResponse<T>.ErrorResponse(
            authError.Error ?? "Yetkiniz yok", 
            authError.ErrorCode ?? ErrorCodes.AccessDenied);

    // 2. Validation ve iş mantığı...
}
```

---

## 📝 KULLANIM ÖRNEKLERİ

### Örnek 1: Sınıf Oluşturma (Manager, AdminAdmin veya Admin)
```csharp
public async Task<BaseResponse<int>> CreateClassroomAsync(int institutionId, string name, int grade)
{
    // Manager, AdminAdmin veya Admin olmalı
    var authError = _authorizationService.RequireGlobalRole(
        UserRole.Manager, 
        UserRole.AdminAdmin, 
        UserRole.Admin);
    if (authError != null)
        return BaseResponse<int>.ErrorResponse(
            authError.Error ?? "Yetkiniz yok", 
            authError.ErrorCode ?? ErrorCodes.AccessDenied);

    // İş mantığı...
}
```

### Örnek 2: Sınav Oluşturma (Teacher, Manager, AdminAdmin veya Admin)
```csharp
public async Task<BaseResponse<int>> CreateExamAsync(CreateExamDto dto)
{
    // Teacher, Manager, AdminAdmin veya Admin olmalı
    var authError = _authorizationService.RequireGlobalRole(
        UserRole.Teacher,
        UserRole.StandaloneTeacher,
        UserRole.Manager, 
        UserRole.AdminAdmin, 
        UserRole.Admin);
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

### Örnek 4: Herkes Yapabilir (Öğrenci dahil)
```csharp
public async Task<BaseResponse<bool>> LikeContentAsync(int contentId)
{
    // Herkes yapabilir, yetki kontrolü yok
    // İş mantığı...
}
```

### Örnek 5: Öğrenci Olamaz
```csharp
public async Task<BaseResponse<int>> CreateExamAsync(CreateExamDto dto)
{
    // Student veya StandaloneStudent olamaz
    var authError = _authorizationService.RequireGlobalRole(
        UserRole.Teacher,
        UserRole.StandaloneTeacher,
        UserRole.Manager, 
        UserRole.AdminAdmin, 
        UserRole.Admin);
    if (authError != null)
        return BaseResponse<int>.ErrorResponse(
            authError.Error ?? "Öğrenciler bu işlemi yapamaz", 
            authError.ErrorCode ?? ErrorCodes.AccessDenied);

    // İş mantığı...
}
```

---

## ✅ ÖNEMLİ NOTLAR

1. **Her fonksiyonun başında kontrol yapılmalı** - İş mantığından önce
2. **Null kontrolü** - `authError == null` ise yetki var, devam et
3. **Hata mesajı** - `authError.Error` ve `authError.ErrorCode` kullanılır
4. **Birden fazla rol** - `params UserRole[]` ile birden fazla rol belirtilebilir (en az biri olmalı)

---

## 🔄 Roller ve Kullanım Senaryoları

| Senaryo | Gerekli Roller |
|---------|----------------|
| Sınıf oluşturma | Manager, AdminAdmin, Admin |
| Sınav oluşturma | Teacher, StandaloneTeacher, Manager, AdminAdmin, Admin |
| İçerik oluşturma | Herkes (Student dahil) |
| Admin işlemleri | AdminAdmin |
| Sistem yönetimi | AdminAdmin, Admin |
| Profil görüntüleme | Herkes |
| Profil güncelleme | Kendi profili veya AdminAdmin, Admin |

---

## 🗑️ KALDIRILAN FONKSİYONLAR

Artık sadece **tek bir fonksiyon** var:
- ✅ `RequireGlobalRole(params UserRole[] requiredRoles)`

Kaldırılan fonksiyonlar:
- ❌ `RequireInstitutionRoleAsync`
- ❌ `RequireManagerByInstitutionAsync`
- ❌ `RequireNotStudentAsync`
- ❌ `RequireContentOwnerAsync`
- ❌ `RequireOwnOperation`
- ❌ Tüm `Can...` fonksiyonları
- ❌ Tüm `Is...` fonksiyonları

**Yerine:** Sadece `RequireGlobalRole` kullanılacak!

