# Kapsamlı Yetki Kontrolü Sistemi Kullanım Kılavuzu

## 📋 Genel Bakış

Bu doküman, uygulamadaki tüm yetki kontrollerinin nasıl yapılacağını detaylı bir şekilde açıklar. Her Operations metodunda yetki kontrolü yapılması zorunludur.

## 🎯 Kullanıcı Tipleri

1. **AdminAdmin** - Sistem kurucusu, her şeye yetkisi var, admin tanımlayabilir
2. **Admin** - Sistem yöneticisi, her şeye yetkisi var (admin tanımlama hariç)
3. **Institution Manager** - Dershane yöneticisi, kurumunu yönetir
4. **Institution Teacher** - Dershane öğretmeni, sınıfları ve öğrencileri yönetir
5. **Institution Student** - Dershane öğrencisi, sınırlı yetkiler
6. **Standalone Teacher** - Dershaneye bağlı olmayan öğretmen
7. **Standalone Student** - Dershaneye bağlı olmayan öğrenci

## 🔧 AuthorizationService Kullanımı

### 1. Operations Sınıfına Ekleme

```csharp
public class YourOperations
{
    private readonly ApplicationContext _context;
    private readonly SessionService _sessionService;
    private readonly AuthorizationService _authorizationService;
    // ... diğer servisler

    public YourOperations(
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

### 2. Her Metodun Başında Yetki Kontrolü

**KURAL: Her Operations metodunun EN BAŞINDA yetki kontrolü yapılmalıdır!**

```csharp
public async Task<BaseResponse<T>> YourMethodAsync(int someId)
{
    // 1. YETKİ KONTROLÜ (EN BAŞTA!)
    if (!await _authorizationService.CanDoSomethingAsync(someId))
    {
        return BaseResponse<T>.ErrorResponse("Bu işlemi yapma yetkiniz yok", ErrorCodes.AccessDenied);
    }

    // 2. Validation
    if (someId <= 0)
        return BaseResponse<T>.ErrorResponse("Invalid ID", ErrorCodes.ValidationFailed);

    // 3. İş mantığı
    // ...
}
```

## 📝 İşlem Kategorilerine Göre Yetki Kontrolleri

### 1. KURUM YÖNETİMİ

#### Kurum Oluşturma
```csharp
if (!_authorizationService.CanCreateInstitution())
{
    return BaseResponse<T>.ErrorResponse("Kurum oluşturma yetkiniz yok", ErrorCodes.AccessDenied);
}
```

#### Kurum Onaylama
```csharp
if (!_authorizationService.CanApproveInstitution())
{
    return BaseResponse<T>.ErrorResponse("Kurum onaylama yetkiniz yok", ErrorCodes.AccessDenied);
}
```

#### Kurum Yönetimi
```csharp
if (!await _authorizationService.CanManageInstitutionAsync(institutionId))
{
    return BaseResponse<T>.ErrorResponse("Bu kurumu yönetme yetkiniz yok", ErrorCodes.AccessDenied);
}
```

#### Kurum İçi Kullanıcı Ekleme
```csharp
if (!await _authorizationService.CanAddUserToInstitutionAsync(institutionId))
{
    return BaseResponse<T>.ErrorResponse("Kuruma kullanıcı ekleme yetkiniz yok", ErrorCodes.AccessDenied);
}
```

### 2. SINIF YÖNETİMİ

#### Sınıf Oluşturma
```csharp
if (!await _authorizationService.CanCreateClassroomAsync(institutionId))
{
    return BaseResponse<T>.ErrorResponse("Sınıf oluşturma yetkiniz yok", ErrorCodes.AccessDenied);
}
```

#### Sınıf Yönetimi
```csharp
if (!await _authorizationService.CanManageClassroomAsync(classroomId))
{
    return BaseResponse<T>.ErrorResponse("Bu sınıfı yönetme yetkiniz yok", ErrorCodes.AccessDenied);
}
```

#### Öğrenci Ekleme
```csharp
if (!await _authorizationService.CanAddStudentToClassroomAsync(classroomId))
{
    return BaseResponse<T>.ErrorResponse("Sınıfa öğrenci ekleme yetkiniz yok", ErrorCodes.AccessDenied);
}
```

### 3. SINAV YÖNETİMİ

#### Sınav Oluşturma
```csharp
if (!await _authorizationService.CanCreateExamAsync(institutionId))
{
    return BaseResponse<T>.ErrorResponse("Sınav oluşturma yetkiniz yok", ErrorCodes.AccessDenied);
}
```

#### Sınav Yönetimi
```csharp
if (!await _authorizationService.CanManageExamAsync(examId))
{
    return BaseResponse<T>.ErrorResponse("Bu sınavı yönetme yetkiniz yok", ErrorCodes.AccessDenied);
}
```

#### Optik Sonuç İşleme
```csharp
if (!await _authorizationService.CanProcessOpticalResultsAsync(examId))
{
    return BaseResponse<T>.ErrorResponse("Optik sonuç işleme yetkiniz yok", ErrorCodes.AccessDenied);
}
```

#### Sınav Sonuçlarını Onaylama
```csharp
if (!await _authorizationService.CanConfirmExamResultsAsync(examId))
{
    return BaseResponse<T>.ErrorResponse("Sınav sonuçlarını onaylama yetkiniz yok", ErrorCodes.AccessDenied);
}
```

#### Başka Öğrencinin Sonucunu Görüntüleme
```csharp
if (!await _authorizationService.CanViewOtherStudentResultAsync(examResultId))
{
    return BaseResponse<T>.ErrorResponse("Bu sonucu görüntüleme yetkiniz yok", ErrorCodes.AccessDenied);
}
```

### 4. İÇERİK YÖNETİMİ (Sosyal Medya)

#### İçerik Oluşturma
```csharp
// Herkes içerik oluşturabilir, kontrol gerekmez
// Ama öğrenci kısıtlamaları için:
var userId = _sessionService.GetUserId();
var userInstitutions = await _context.InstitutionUsers
    .Where(iu => iu.UserId == userId)
    .Select(iu => iu.InstitutionId)
    .ToListAsync();

foreach (var instId in userInstitutions)
{
    if (await _authorizationService.IsStudentAsync(instId))
    {
        // Öğrenciler içerik oluşturabilir (sosyal medya)
        break;
    }
}
```

#### İçerik Yönetimi
```csharp
if (!await _authorizationService.CanManageContentAsync(contentId))
{
    return BaseResponse<T>.ErrorResponse("Bu içeriği yönetme yetkiniz yok", ErrorCodes.AccessDenied);
}
```

### 5. MESAJLAŞMA

#### Sınıf Grubuna Mesaj
```csharp
if (!await _authorizationService.CanSendMessageToClassroomAsync(classroomId))
{
    return BaseResponse<T>.ErrorResponse("Bu sınıfa mesaj gönderme yetkiniz yok", ErrorCodes.AccessDenied);
}
```

#### Toplu Mesaj
```csharp
if (!await _authorizationService.CanSendBulkMessageAsync(institutionId, classroomId))
{
    return BaseResponse<T>.ErrorResponse("Toplu mesaj gönderme yetkiniz yok", ErrorCodes.AccessDenied);
}
```

#### Konuşma Güncelleme
```csharp
if (!await _authorizationService.CanUpdateConversationAsync(conversationId))
{
    return BaseResponse<T>.ErrorResponse("Bu konuşmayı güncelleme yetkiniz yok", ErrorCodes.AccessDenied);
}
```

### 6. HESAP BAĞLAMA

#### Hesap Bağlama İsteği Onaylama
```csharp
if (!await _authorizationService.CanApproveAccountLinkAsync(institutionId))
{
    return BaseResponse<T>.ErrorResponse("Hesap bağlama isteği onaylama yetkiniz yok", ErrorCodes.AccessDenied);
}
```

### 7. ÖZEL DERS

#### Özel Ders Bilgisi Oluşturma
```csharp
if (!await _authorizationService.CanCreatePrivateTutoringInfoAsync())
{
    return BaseResponse<T>.ErrorResponse("Özel ders bilgisi oluşturma yetkiniz yok", ErrorCodes.AccessDenied);
}
```

### 8. RAPORLAMA

#### Rapor Oluşturma
```csharp
if (!await _authorizationService.CanCreateReportAsync(institutionId))
{
    return BaseResponse<T>.ErrorResponse("Rapor oluşturma yetkiniz yok", ErrorCodes.AccessDenied);
}
```

#### Kurum Raporu Görüntüleme
```csharp
if (!await _authorizationService.CanViewInstitutionReportAsync(institutionId))
{
    return BaseResponse<T>.ErrorResponse("Kurum raporunu görüntüleme yetkiniz yok", ErrorCodes.AccessDenied);
}
```

#### Sınıf Raporu Görüntüleme
```csharp
if (!await _authorizationService.CanViewClassroomReportAsync(classroomId))
{
    return BaseResponse<T>.ErrorResponse("Sınıf raporunu görüntüleme yetkiniz yok", ErrorCodes.AccessDenied);
}
```

## 🔍 Özel Durumlar

### Standalone Kullanıcılar

Standalone (bağımsız) kullanıcılar için özel kontroller:

```csharp
// Standalone öğretmen mi?
if (await _authorizationService.IsStandaloneTeacherAsync())
{
    // Standalone öğretmenler kendi sınıflarını oluşturabilir
    // Kurum yönetimi yapamazlar
}
```

### Öğrenci Kısıtlamaları

Öğrenciler için özel kısıtlamalar:

```csharp
// Öğrenci kontrolü
var userId = _sessionService.GetUserId();
var userInstitutions = await _context.InstitutionUsers
    .Where(iu => iu.UserId == userId && iu.Role == InstitutionRole.Student)
    .Select(iu => iu.InstitutionId)
    .ToListAsync();

if (userInstitutions.Any())
{
    // Öğrenci kısıtlamaları
    // Öğrenciler şunları YAPAMAZ:
    // - Sınav oluşturma
    // - Sınıf oluşturma
    // - Öğrenci ekleme/çıkarma
    // - Kurum yönetimi
    // - Toplu mesaj gönderme
    // - Rapor oluşturma (kendi raporları hariç)
}
```

## 📋 Tüm Yetki Kontrolü Metodları

| Metod | Açıklama | Parametreler |
|-------|----------|--------------|
| `CanCreateInstitution()` | Kurum oluşturma | - |
| `CanApproveInstitution()` | Kurum onaylama | - |
| `CanManageInstitutionAsync(int)` | Kurum yönetimi | institutionId |
| `CanAddUserToInstitutionAsync(int)` | Kuruma kullanıcı ekleme | institutionId |
| `CanDeleteUserAsync(int, int?)` | Kullanıcı silme | targetUserId, institutionId? |
| `CanCreateClassroomAsync(int?)` | Sınıf oluşturma | institutionId? |
| `CanManageClassroomAsync(int)` | Sınıf yönetimi | classroomId |
| `CanAddStudentToClassroomAsync(int)` | Öğrenci ekleme | classroomId |
| `CanCreateExamAsync(int?)` | Sınav oluşturma | institutionId? |
| `CanManageExamAsync(int)` | Sınav yönetimi | examId |
| `CanProcessOpticalResultsAsync(int)` | Optik sonuç işleme | examId |
| `CanConfirmExamResultsAsync(int)` | Sonuç onaylama | examId |
| `CanViewOtherStudentResultAsync(int)` | Başka öğrencinin sonucu | examResultId |
| `CanViewClassroomReportAsync(int)` | Sınıf raporu | classroomId |
| `CanCreateContent()` | İçerik oluşturma | - |
| `CanManageContentAsync(int)` | İçerik yönetimi | contentId |
| `CanSendMessageToClassroomAsync(int)` | Sınıfa mesaj | classroomId |
| `CanSendBulkMessageAsync(int?, int?)` | Toplu mesaj | institutionId?, classroomId? |
| `CanUpdateConversationAsync(int)` | Konuşma güncelleme | conversationId |
| `CanApproveAccountLinkAsync(int)` | Hesap bağlama onaylama | institutionId |
| `CanCreatePrivateTutoringInfoAsync()` | Özel ders bilgisi | - |
| `CanCreateReportAsync(int?)` | Rapor oluşturma | institutionId? |
| `CanViewInstitutionReportAsync(int)` | Kurum raporu | institutionId |

## ⚠️ ÖNEMLİ KURALLAR

1. **Her Operations metodunun EN BAŞINDA yetki kontrolü yapılmalıdır**
2. **Admin her şeyi yapabilir (zaten kontrol ediliyor)**
3. **Öğrenci kısıtlamaları unutulmamalıdır**
4. **Standalone kullanıcılar için özel kontroller yapılmalıdır**
5. **Hata mesajları açıklayıcı olmalıdır**
6. **Yetki kontrolü yapılmadan veritabanı işlemi yapılmamalıdır**

## 🔄 Migration (Eski Kodları Güncelleme)

Tüm Operations sınıflarındaki manuel yetki kontrollerini `AuthorizationService` kullanacak şekilde güncelleyin.

**Örnek:**
```csharp
// ÖNCE (Manuel kontrol):
var isManager = await _context.Institutions.AnyAsync(i => i.Id == institutionId && i.ManagerUserId == currentUserId);
if (!isManager && !_sessionService.IsInGlobalRole(UserRole.AdminAdmin))
{
    return BaseResponse<T>.ErrorResponse("Unauthorized", ErrorCodes.AccessDenied);
}

// SONRA (AuthorizationService ile):
if (!await _authorizationService.CanManageInstitutionAsync(institutionId))
{
    return BaseResponse<T>.ErrorResponse("Bu kurumu yönetme yetkiniz yok", ErrorCodes.AccessDenied);
}
```

## 📊 Yetki Matrisi

Detaylı yetki matrisi için: `DOC/permission_matrix.md` dosyasına bakınız.

