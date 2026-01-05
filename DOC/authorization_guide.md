# Yetki Kontrolü (Authorization) Sistemi Kullanım Kılavuzu

## 📋 Genel Bakış

Projede merkezi bir yetki kontrolü sistemi bulunmaktadır. Tüm yetki kontrolleri `AuthorizationService` üzerinden yapılır.

## 🔐 Mevcut Durum

### ✅ Var Olanlar:
- JWT Authentication (Token tabanlı kimlik doğrulama)
- `SessionService` (Kullanıcı bilgilerini alma)
- `AuthorizationService` (Merkezi yetki kontrolü servisi)
- Role-based access control (GlobalRole ve InstitutionRole)

### ❌ Eksikler (Önceden):
- Merkezi yetki kontrolü servisi yoktu
- Her Operations'ta farklı şekilde yetki kontrolü yapılıyordu
- Öğrenci kısıtlamaları yoktu
- InstitutionRole kontrolü eksikti

## 🎯 AuthorizationService Kullanımı

### 1. Dependency Injection

Operations sınıflarınıza `AuthorizationService` ekleyin:

```csharp
public class SocialOperations
{
    private readonly ApplicationContext _context;
    private readonly SessionService _sessionService;
    private readonly AuthorizationService _authorizationService;
    // ... diğer servisler

    public SocialOperations(
        ApplicationContext context,
        SessionService sessionService,
        AuthorizationService authorizationService,
        // ... diğer servisler)
    {
        _context = context;
        _sessionService = sessionService;
        _authorizationService = authorizationService;
        // ...
    }
}
```

### 2. Temel Yetki Kontrolleri

#### Global Role Kontrolü

```csharp
// Admin mi?
if (!_authorizationService.IsAdmin())
{
    return BaseResponse<T>.ErrorResponse("Unauthorized", ErrorCodes.AccessDenied);
}

// Belirli bir role sahip mi?
if (!_authorizationService.IsInGlobalRole(UserRole.AdminAdmin))
{
    return BaseResponse<T>.ErrorResponse("Unauthorized", ErrorCodes.AccessDenied);
}
```

#### Institution Role Kontrolü

```csharp
// Kurumda Manager mı?
if (!await _authorizationService.IsManagerAsync(institutionId))
{
    return BaseResponse<T>.ErrorResponse("Unauthorized", ErrorCodes.AccessDenied);
}

// Kurumda Teacher mı?
if (!await _authorizationService.IsTeacherAsync(institutionId))
{
    return BaseResponse<T>.ErrorResponse("Unauthorized", ErrorCodes.AccessDenied);
}

// Kurumda Manager veya Teacher mı?
if (!await _authorizationService.IsManagerOrTeacherAsync(institutionId))
{
    return BaseResponse<T>.ErrorResponse("Unauthorized", ErrorCodes.AccessDenied);
}

// Belirli rollerden biri var mı?
if (!await _authorizationService.HasInstitutionRoleAsync(institutionId, InstitutionRole.Manager, InstitutionRole.Teacher))
{
    return BaseResponse<T>.ErrorResponse("Unauthorized", ErrorCodes.AccessDenied);
}
```

### 3. Öğrenci Kısıtlamaları

```csharp
// Öğrenciler bu işlemi yapabilir mi?
if (!await _authorizationService.CanPerformActionAsync(institutionId, "create_exam"))
{
    return BaseResponse<T>.ErrorResponse("Öğrenciler bu işlemi yapamaz", ErrorCodes.AccessDenied);
}

// Öğrenci kontrolü (spesifik)
if (await _authorizationService.IsStudentAsync(institutionId))
{
    if (!await _authorizationService.CanStudentPerformActionAsync(institutionId, "create_exam"))
    {
        return BaseResponse<T>.ErrorResponse("Öğrenciler sınav oluşturamaz", ErrorCodes.AccessDenied);
    }
}
```

### 4. Özel Yetki Kontrolleri

#### Sınıf Öğretmeni Kontrolü

```csharp
// Kullanıcı bu sınıfın öğretmeni mi?
if (!await _authorizationService.IsClassroomTeacherAsync(classroomId))
{
    return BaseResponse<T>.ErrorResponse("Bu sınıfın öğretmeni değilsiniz", ErrorCodes.AccessDenied);
}
```

#### İçerik Sahibi Kontrolü

```csharp
// Kullanıcı bu içeriğin sahibi mi?
if (!await _authorizationService.IsContentOwnerAsync(contentId))
{
    return BaseResponse<T>.ErrorResponse("Bu içeriğin sahibi değilsiniz", ErrorCodes.AccessDenied);
}
```

#### Profil Görüntüleme Kontrolü

```csharp
// Kullanıcı bu profili görüntüleyebilir mi?
if (!await _authorizationService.CanViewProfileAsync(targetUserId))
{
    return BaseResponse<T>.ErrorResponse("Bu profili görüntüleme yetkiniz yok", ErrorCodes.AccessDenied);
}
```

#### Sınav Görüntüleme Kontrolü

```csharp
// Kullanıcı bu sınavı görüntüleyebilir mi?
if (!await _authorizationService.CanViewExamAsync(examId))
{
    return BaseResponse<T>.ErrorResponse("Bu sınavı görüntüleme yetkiniz yok", ErrorCodes.AccessDenied);
}

// Kullanıcı bu sınav sonucunu görüntüleyebilir mi?
if (!await _authorizationService.CanViewExamResultAsync(examResultId))
{
    return BaseResponse<T>.ErrorResponse("Bu sınav sonucunu görüntüleme yetkiniz yok", ErrorCodes.AccessDenied);
}
```

## 📝 Örnek Kullanım Senaryoları

### Senaryo 1: Sınav Oluşturma

```csharp
public async Task<BaseResponse<int>> CreateExamAsync(CreateExamDto dto)
{
    var userId = _sessionService.GetUserId();
    
    // Yetki kontrolü: Sadece Manager veya Teacher sınav oluşturabilir
    if (!await _authorizationService.IsManagerOrTeacherAsync(dto.InstitutionId))
    {
        return BaseResponse<int>.ErrorResponse("Sadece öğretmenler ve yöneticiler sınav oluşturabilir", ErrorCodes.AccessDenied);
    }

    // Öğrenci kontrolü (ekstra güvenlik)
    if (await _authorizationService.IsStudentAsync(dto.InstitutionId))
    {
        return BaseResponse<int>.ErrorResponse("Öğrenciler sınav oluşturamaz", ErrorCodes.AccessDenied);
    }

    // ... sınav oluşturma işlemi
}
```

### Senaryo 2: İçerik Silme

```csharp
public async Task<BaseResponse<bool>> DeleteContentAsync(int contentId)
{
    var userId = _sessionService.GetUserId();
    
    // Yetki kontrolü: Sadece içerik sahibi veya admin silebilir
    if (!await _authorizationService.IsContentOwnerAsync(contentId) && !_authorizationService.IsAdmin())
    {
        return BaseResponse<bool>.ErrorResponse("Bu içeriği silme yetkiniz yok", ErrorCodes.AccessDenied);
    }

    // ... içerik silme işlemi
}
```

### Senaryo 3: Sınıf Yönetimi

```csharp
public async Task<BaseResponse<bool>> AddStudentToClassroomAsync(int classroomId, int studentId)
{
    // Yetki kontrolü: Sadece sınıf öğretmeni veya Manager öğrenci ekleyebilir
    var classroom = await _context.Classrooms.FindAsync(classroomId);
    if (classroom == null)
        return BaseResponse<bool>.ErrorResponse("Sınıf bulunamadı", ErrorCodes.NotFound);

    if (!await _authorizationService.IsClassroomTeacherAsync(classroomId) && 
        !await _authorizationService.IsManagerAsync(classroom.InstitutionId))
    {
        return BaseResponse<bool>.ErrorResponse("Bu sınıfa öğrenci ekleme yetkiniz yok", ErrorCodes.AccessDenied);
    }

    // ... öğrenci ekleme işlemi
}
```

### Senaryo 4: Öğrenci Kısıtlamaları

```csharp
public async Task<BaseResponse<bool>> DeleteUserAsync(int targetUserId)
{
    // Öğrenciler kullanıcı silemez
    var targetUser = await _context.Users.FindAsync(targetUserId);
    if (targetUser == null)
        return BaseResponse<bool>.ErrorResponse("Kullanıcı bulunamadı", ErrorCodes.NotFound);

    // Kullanıcının kurumlarını bul
    var institutionIds = await _context.InstitutionUsers
        .Where(iu => iu.UserId == targetUserId)
        .Select(iu => iu.InstitutionId)
        .ToListAsync();

    // Herhangi bir kurumda öğrenci mi?
    foreach (var institutionId in institutionIds)
    {
        if (await _authorizationService.IsStudentAsync(institutionId))
        {
            return BaseResponse<bool>.ErrorResponse("Öğrenciler kullanıcı silemez", ErrorCodes.AccessDenied);
        }
    }

    // ... kullanıcı silme işlemi
}
```

## 🚫 Öğrencilerin Yapamayacağı İşlemler

Aşağıdaki işlemler öğrenciler için kısıtlanmıştır:

- ❌ Sınav oluşturma
- ❌ Sınıf oluşturma
- ❌ Öğrenci ekleme/çıkarma
- ❌ Kurum yönetimi
- ❌ Kullanıcı silme
- ❌ Admin işlemleri
- ❌ Rapor oluşturma (kendi raporları hariç)

## ✅ Öğrencilerin Yapabileceği İşlemler

- ✅ Kendi profilini görüntüleme/güncelleme
- ✅ Kendi sınav sonuçlarını görüntüleme
- ✅ Sınıf bilgilerini görüntüleme
- ✅ Mesaj gönderme
- ✅ İçerik oluşturma (sosyal medya)
- ✅ İçerik beğenme/yorumlama
- ✅ Kullanıcı takip etme
- ✅ İçerik kaydetme

## 🔧 AuthorizationService Metodları

| Metod | Açıklama |
|-------|----------|
| `IsInGlobalRole(UserRole role)` | Global role kontrolü |
| `IsAdmin()` | Admin (AdminAdmin veya Admin) kontrolü |
| `HasInstitutionRoleAsync(int institutionId, params InstitutionRole[] roles)` | Kurum role kontrolü |
| `IsManagerAsync(int institutionId)` | Manager kontrolü |
| `IsTeacherAsync(int institutionId)` | Teacher kontrolü |
| `IsStudentAsync(int institutionId)` | Student kontrolü |
| `IsManagerOrTeacherAsync(int institutionId)` | Manager veya Teacher kontrolü |
| `IsClassroomTeacherAsync(int classroomId)` | Sınıf öğretmeni kontrolü |
| `IsClassroomStudentAsync(int classroomId)` | Sınıf öğrencisi kontrolü |
| `IsContentOwnerAsync(int contentId)` | İçerik sahibi kontrolü |
| `CanViewProfileAsync(int targetUserId)` | Profil görüntüleme yetkisi |
| `CanStudentPerformActionAsync(int institutionId, string action)` | Öğrenci işlem yetkisi |
| `CanPerformActionAsync(int? institutionId, string action)` | Genel işlem yetkisi |
| `CanViewExamAsync(int examId)` | Sınav görüntüleme yetkisi |
| `CanViewExamResultAsync(int examResultId)` | Sınav sonucu görüntüleme yetkisi |

## 📌 Best Practices

1. **Her Operations metodunun başında yetki kontrolü yapın**
2. **Öğrenci kısıtlamalarını unutmayın**
3. **Admin her şeyi yapabilir (zaten kontrol ediliyor)**
4. **Hata mesajlarını açıklayıcı yapın**
5. **Yetki kontrolü yapılmadan veritabanı işlemi yapmayın**

## 🔄 Migration (Eski Kodları Güncelleme)

Eski kodlardaki manuel yetki kontrollerini `AuthorizationService` kullanacak şekilde güncelleyin:

**Önce:**
```csharp
var isManager = await _context.Institutions.AnyAsync(i => i.Id == institutionId && i.ManagerUserId == currentUserId);
if (!isManager && !_sessionService.IsInGlobalRole(UserRole.AdminAdmin))
{
    return BaseResponse<T>.ErrorResponse("Unauthorized", ErrorCodes.AccessDenied);
}
```

**Sonra:**
```csharp
if (!await _authorizationService.IsManagerAsync(institutionId) && !_authorizationService.IsAdmin())
{
    return BaseResponse<T>.ErrorResponse("Unauthorized", ErrorCodes.AccessDenied);
}
```

## ⚠️ Önemli Notlar

1. `AuthorizationService` tüm metodlarını `async` olarak kullanın (InstitutionRole kontrolleri için)
2. Admin kontrolü genellikle en başta yapılır (admin her şeyi yapabilir)
3. Öğrenci kısıtlamaları özellikle kritik işlemlerde kontrol edilmelidir
4. Hata mesajları kullanıcı dostu olmalıdır

