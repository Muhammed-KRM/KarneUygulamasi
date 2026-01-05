# 📘 KarneProject - Kapsamlı Backend Geliştirme Kılavuzu (v5.0 FINAL)

Bu döküman, **KarneProject** platformunun tüm teknik mimarisini, iş akışlarını, veri modellerini, API endpointlerini ve UI entegrasyonlarını **en ince detayına kadar** açıklayan nihai referans dokümanıdır.

> **💡 Not:** Bu rehber, hem backend geliştiriciler hem de frontend geliştiriciler için hazırlanmıştır. Her özellik için "Backend İş Mantığı" ve "Frontend Kullanım Senaryosu" ayrı ayrı açıklanmıştır.

---

## 📋 İÇİNDEKİLER

1. [Proje Vizyonu ve Kullanıcı Hikayeleri](#1-proje-vizyonu)
2. [Teknik Mimari ve Standartlar](#2-teknik-mimari)
3. [Faz 1: Foundation (Temel Altyapı)](#3-faz-1-foundation)
4. [Faz 2: Kurum Yönetimi](#4-faz-2-kurum-yonetimi)
5. [Faz 3: Sosyal Ağ ve Keşfet](#5-faz-3-sosyal-ag)
6. [Faz 4: Marketplace ve Ödeme](#6-faz-4-marketplace)
7. [Faz 5: Araçlar ve Zamanlayıcı](#7-faz-5-araclar)
8. [UI Akış Senaryoları](#8-ui-akis-senaryolari)
9. [Nihai Teknoloji Stack](#9-teknoloji-stack)
10. [Proje Yapısı](#10-proje-yapisi)

---

## 🎯 1. PROJE VİZYONU VE KULLANICI HİKAYELERİ

### 1.1. Proje Amacı

**KarneProject**, eğitim kurumlarını dijitalleştiren ve bireysel öğretmen/öğrencileri bir araya getiren **hibrit bir eğitim platformudur**.

**İki ana ekosistem:**

1. **B2B (Dershaneler):** Öğrenci/öğretmen yönetimi, sınav değerlendirme, karne oluşturma, mesajlaşma.
2. **B2C (Bireysel):** Soru/sınav paylaşımı, öğretmen bulma, sosyal öğrenme.

### 1.2. Kullanıcı Rolleri ve Hikayeleri

#### 👨‍💼 AdminAdmin (Sistem Kurucusu)

> "Ben sistemin sahibiyim. Admin hesapları ben tanımlarım ve tüm sisteme erişimim vardır."

**Yetkiler:**

- Admin hesabı oluşturma/silme
- Tüm veritabanı kayıtlarına tam erişim
- Sistem konfigürasyonu

#### 👨‍💻 Admin (Sistem Yöneticisi)

> "Dershane başvurularını ben onaylıyorum. Kullanıcı sorunlarını çözüyorum."

**Yetkiler:**

- Dershane başvurularını onaylama/reddetme
- Kullanıcı hesaplarını yönetme (CRUD)
- Her sınıfa/içeriğe erişim
- **AdminAdmin olamaz**

#### 🏢 Kurum Yöneticisi (Institution Manager)

> "Dershanenin sahibiyim. Öğretmen ve öğrenci ekler, sınıf oluştururum."

**Yetkiler:**

- Kendi kurumunda öğretmen/öğrenci ekleme
- Sınıf oluşturma ve yönetme
- Hesap bağlama taleplerini onaylama

#### 👨‍🏫 Kurum Öğretmeni (Institution Teacher)

> "Sınıfıma sınav giriyorum. Optik form yüklüyüp karne oluşturuyorum. Öğrencilerime hem sınıf grubundan hem özel mesaj gönderebiliyorum."

**Yetkiler:**

- Sınıflarına sınav tanımlama
- Optik TXT dosyası yükleme
- Karne oluşturma ve dağıtma (toplu veya tekil)
- Sınıf grubu mesajlaşma
- Öğrenci profillerini görüntüleme

#### 👨‍🎓 Kurum Öğrencisi (Institution Student)

> "Dershanede öğrenciyim. Sınav sonuçlarımı ve karnelerimi görüyorum. Ana hesabımı dershane hesabıma bağlayabilirim."

**Yetkiler:**

- Kendi sınav sonuçlarını görme
- Sınıf grup mesajlarına katılma
- Profilini açık/gizli yapma
- Ana hesaba bağlama talebi gönderme

#### 👨‍🏫 Bağımsız Öğretmen (Independent Teacher)

> "Özel ders veriyorum. Sorularımı paylaşıyorum. Öğrenci arıyorum."

**Yetkiler:**

- Soru/sınav paylaşma
- Özel ders ilanı oluşturma
- Kendi sınıflarını yönetme
- Feed'i kullanma

#### 👨‍🎓 Bağımsız Öğrenci (Independent Student)

> "Paylaşılan soruları çözüyorum. Öğretmen arıyorum. Performansımı takip ediyorum."

**Yetkiler:**

- Soru/sınav paylaşma ve çözme
- Öğretmen arama
- Zamanlayıcı ile çalışma süresi tutma
- Feed kullanma

---

## 🏗️ 2. TEKNİK MİMARİ VE STANDARTLAR

### 2.1. Katmanlı Mimari (Layered Architecture)

Proje **ASP.NET Core** üzerine inşa edilmiş **3 katmanlı** bir yapıdadır:

```
┌─────────────────────────────────────┐
│       CONTROLLER LAYER              │  ← HTTP Endpoint'ler
│  (Sadece Routing & Validation)      │
└────────────┬────────────────────────┘
             │
             ▼
┌─────────────────────────────────────┐
│      OPERATION LAYER                │  ← İş Mantığı
│  (Business Logic & Orchestration)   │
└────────────┬────────────────────────┘
             │
             ▼
┌─────────────────────────────────────┐
│        DATA LAYER                   │  ← Veritabanı
│   (EF Core + Repository Pattern)    │
└─────────────────────────────────────┘
```

#### 2.1.1. Controller Katmanı

**Sorumluluklar:**

- HTTP isteğini karşılama
- JWT token'dan `UserId` çıkarma
- FluentValidation ile input doğrulama
- Operation çağırma
- `BaseResponse<T>` döndürme

**Yasak:**

- İş mantığı yazmak
- Veritabanına doğrudan erişmek
- Karmaşık if/else blokları

**Örnek Controller:**

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExamController : BaseController
{
    [HttpPost("upload-optical")]
    public async Task<IActionResult> UploadOptical([FromForm] OpticalUploadRequest request)
    {
        int userId = GetCurrentUserId(); // BaseController'dan
        var result = await ExamOperations.UploadOpticalAsync(request, userId, _context);
        return Ok(result);
    }
}
```

#### 2.1.2. Operation Katmanı (The Brain)

Tüm iş mantığı buradadır.

**Prensipler:**

1. **Atomik Fonksiyonlar:** Her metod 50 satırı geçmemeli
2. **Helper Metodlar:** Tekrar eden kod private helper'lara ayrılmalı
3. **Stateless:** Hiçbir state tutulmamalı
4. **Async:** Tüm I/O işlemleri async

**Örnek Operation Yapısı:**

```csharp
public static class ExamOperations
{
    // Ana metod (Public)
    public static async Task<BaseResponse<UploadSummary>> UploadOpticalAsync(
        OpticalUploadRequest request,
        int userId,
        ApplicationContext context)
    {
        // 1. Yetki kontrolü
        if (!await HasPermissionAsync(userId, request.ClassroomId, context))
            return BaseResponse<UploadSummary>.Error("Yetkiniz yok");

        // 2. Dosya parse
        var parsedData = ParseOpticalFile(request.File);

        // 3. Hesaplama
        var results = CalculateResults(parsedData, request.AnswerKey);

        // 4. Kaydetme
        await SaveResultsAsync(results, context);

        // 5. Bildirim gönderme
        await NotificationService.SendToStudentsAsync(results, context);

        // 6. Audit log
        await AuditService.LogAsync(userId, "OpticalUploaded", $"ExamId: {request.ExamId}");

        return BaseResponse<UploadSummary>.Success(new UploadSummary { ... });
    }

    // Helper metodlar (Private)
    private static List<StudentAnswer> ParseOpticalFile(IFormFile file) { ... }
    private static List<ExamResult> CalculateResults(...) { ... }
    private static async Task SaveResultsAsync(...) { ... }
}
```

### 2.2. Cross-Cutting Concerns

#### A. Authentication & Authorization

**Teknoloji:** JWT Bearer Token

**Token İçeriği (Claims):**

```json
{
  "sub": "105", // UserId
  "role": "User", // Global Role
  "inst_1": "Teacher", // Kurum 1'deki rolü
  "inst_2": "Student", // Kurum 2'deki rolü
  "exp": 1738761600
}
```

**SessionService Metodları:**

```csharp
public class SessionService
{
    public string GenerateToken(User user, List<InstitutionUser> memberships);
    public bool ValidateToken(string token);
    public UserContext GetCurrentContext(ClaimsPrincipal user);
    public bool HasRole(int userId, string role, int? institutionId = null);
}
```

#### B. Logging & Auditing

**İki tür log:**

1. **System Logs:** Hata/performans logları (File/Elasticsearch)
2. **Audit Logs:** İş aksiyonları (Database)

**AuditLog Modeli:**

```csharp
public class AuditLog
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Action { get; set; } // "ExamCreated", "StudentDeleted"
    public string Details { get; set; } // JSON
    public string IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

**Zorunlu Loglama:**
Her `Create`, `Update`, `Delete` işleminde:

```csharp
await AuditService.LogAsync(userId, "ActionName", detailsJson, ipAddress);
```

#### C. Caching (Redis)

**Cache Stratejisi:**
| Veri Tipi | Cache Süresi | Örnek Key |
|-----------|--------------|-----------|
| Statik Veriler (İl, Ders) | 24 saat | `App:Cities` |
| Feed | 10 dakika | `User:105:Feed` |
| Bildirimler | 5 dakika | `User:105:Notifications` |
| Sınıf Listesi | 30 dakika | `Inst:5:Classes` |

**Kullanım:**

```csharp
// Read-Through Pattern
var cities = await _cacheService.GetOrSetAsync("App:Cities", async () =>
{
    return await _context.Cities.ToListAsync();
}, TimeSpan.FromHours(24));

// Cache Invalidation
await _cacheService.RemoveAsync($"Inst:{institutionId}:Classes");
```

#### D. Validation (FluentValidation)

**Her Request için Validator:**

```csharp
public class OpticalUploadRequestValidator : AbstractValidator<OpticalUploadRequest>
{
    public OpticalUploadRequestValidator()
    {
        RuleFor(x => x.File)
            .NotNull()
            .Must(f => f.Length < 5 * 1024 * 1024).WithMessage("Dosya 5MB'dan küçük olmalı")
            .Must(f => f.FileName.EndsWith(".txt")).WithMessage("Sadece .txt dosyası");

        RuleFor(x => x.ExamId).GreaterThan(0);
    }
}
```

### 2.3. Yanıt Formatı ve Hata Kodları (6 Haneli Sistem)

Platformdaki tüm API yanıtları `BaseResponse<T>` tipindedir. Hataların istemci tarafında (Frontend/Mobil) daha spesifik olarak ele alınabilmesi için **6 haneli sayısal hata kodları** kullanılır.

#### [Model] BaseResponse<T>

```csharp
public class BaseResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
    public string? ErrorCode { get; set; } // 6 Haneli Hata Kodu (Success=true ise null)
}
```

#### Hata Kodu Standartları

| Kod Aralığı | Kategori            | Açıklama                                                           |
| ----------- | ------------------- | ------------------------------------------------------------------ |
| **100XXX**  | **Sistem / Global** | Yetki, Kaynak Bulunamadı, Sunucu Hatası vb. genel sistem hataları. |
| **001XXX**  | **Auth / User**     | Kayıt, Giriş, Profil ve Bireysel kullanıcı işlemleri hataları.     |
| **002XXX**  | **Admin**           | Sistem yönetimi ve Kurum onay/red süreçleri hataları.              |
| **003XXX**  | **Institution**     | Kurum içi (Sınıf, Mevcut Öğrenci, Öğretmen) yönetim hataları.      |

#### Önemli Sabitler (Global)

- `100000`: **Yetkisiz İşlem (No Session)** - Frontend bu kodu alınca doğrudan Login sayfasına yönlendirmelidir.
- `100403`: **Erişim Engellendi** - Kullanıcının bu işlemi yapmaya yetkisi (rolü) yok.

---

## 🔧 3. FAZ 1: FOUNDATION (Temel Altyapı)

Bu faz tamamlanmadan diğer fazlara geçilemez.

### 3.1. Veritabanı Modelleri

#### [Model] User

Ana kullanıcı tablosu. **Herkes** bir User'dır.

```csharp
public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } // varchar(100)
    public string Username { get; set; } // Unique, Index
    public string Email { get; set; } // Unique
    public string? Phone { get; set; }
    public byte[] PasswordHash { get; set; }
    public byte[] PasswordSalt { get; set; }

    public UserRole GlobalRole { get; set; } // Admin, User
    public UserStatus Status { get; set; } // Active, Suspended

    public string? ProfileImageUrl { get; set; }
    public ProfileVisibility ProfileVisibility { get; set; } // Public, TeachersOnly, Private

    // Denormalized Counts (Performans için)
    public int FollowerCount { get; set; } = 0;
    public int FollowingCount { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    // Navigation Properties
    public ICollection<InstitutionUser> InstitutionMemberships { get; set; }
    public ICollection<AccountLink> AccountLinks { get; set; }
}
```

#### [Enum] UserRole

```csharp
public enum UserRole : byte
{
    AdminAdmin = 0, // Sistem kurucusu
    Admin = 1,      // Sistem yöneticisi
    User = 2        // Normal kullanıcı (Öğretmen/Öğrenci rolleri InstitutionUser'da)
}
```

#### [Enum] ProfileVisibility

```csharp
public enum ProfileVisibility : byte
{
    PublicToAll = 1,    // Herkes görebilir
    TeachersOnly = 2,   // Sadece öğretmenler
    Private = 3         // Sadece kendisi
}
```

#### [Model] Institution

Dershane/Kurum kayıtları.

```csharp
public class Institution
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string LicenseNumber { get; set; } // MEB Ruhsat No
    public string Address { get; set; }
    public string? Phone { get; set; }

    public int ManagerUserId { get; set; } // Kurucu/Yönetici
    public User Manager { get; set; }

    public InstitutionStatus Status { get; set; } // PendingApproval, Active, Suspended
    public DateTime? SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public int? ApprovedByAdminId { get; set; }
}
```

#### [Enum] InstitutionStatus

```csharp
public enum InstitutionStatus : byte
{
    PendingApproval = 0,
    Active = 1,
    Suspended = 2,
    Expired = 3
}
```

#### [Model] InstitutionUser

Kullanıcı-Kurum çoktan-çoğa ilişkisi. **Bir kullanıcı birden fazla kurumda farklı rollerde olabilir.**

```csharp
public class InstitutionUser
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }

    public int InstitutionId { get; set; }
    public Institution Institution { get; set; }

    public InstitutionRole Role { get; set; } // Manager, Teacher, Student, Parent

    // Öğrenci için
    public string? StudentNumber { get; set; } // Kurum öğrenci numarası

    // Öğretmen için
    public string? EmployeeNumber { get; set; }

    public DateTime JoinedAt { get; set; }
}
```

#### [Enum] InstitutionRole

```csharp
public enum InstitutionRole : byte
{
    Manager = 1,  // Kurum yöneticisi
    Teacher = 2,  // Öğretmen
    Student = 3,  // Öğrenci
    Parent = 4    // Veli (gelecek faz)
}
```

#### [Model] AccountLink

**Kritik:** Bağımsız hesabın kurum hesabına bağlanması.

```csharp
public class AccountLink
{
    public int Id { get; set; }

    public int MainUserId { get; set; } // Bağımsız ana hesap
    public User MainUser { get; set; }

    public int InstitutionUserId { get; set; } // Kurum hesabı
    public InstitutionUser InstitutionUser { get; set; }

    public LinkStatus Status { get; set; } // Pending, Approved, Rejected
    public DateTime RequestedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public int? ProcessedByUserId { get; set; } // Onaylayan kurum yöneticisi
}
```

#### [Enum] LinkStatus

```csharp
public enum LinkStatus : byte
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}
```

### 3.2. Authentication Endpoints

#### [POST] `/api/auth/register`

Bireysel kullanıcı kaydı.

**Request:**

```json
{
  "fullName": "Ahmet Yılmaz",
  "username": "ahmet123",
  "email": "ahmet@example.com",
  "password": "SecurePass123!"
}
```

**Validation:**

- Username: Min 5, max 20 karakter, alfanumerik
- Password: Min 8 karakter, en az 1 büyük, 1 küçük, 1 rakam
- Email: Valid format

**Operation Logic:**

```csharp
public static async Task<BaseResponse<string>> RegisterAsync(RegisterRequest request, ApplicationContext context)
{
    // 1. Username/Email benzersizliği kontrolü
    if (await context.Users.AnyAsync(u => u.Username == request.Username))
        return BaseResponse<string>.Error("Username kullanımda");

    // 2. Password hash
    PasswordHelper.CreateHash(request.Password, out byte[] hash, out byte[] salt);

    // 3. User oluştur
    var user = new User
    {
        FullName = request.FullName,
        Username = request.Username,
        Email = request.Email,
        PasswordHash = hash,
        PasswordSalt = salt,
        GlobalRole = UserRole.User,
        Status = UserStatus.Active,
        ProfileVisibility = ProfileVisibility.PublicToAll
    };

    context.Users.Add(user);
    await context.SaveChangesAsync();

    // 4. Audit log
    await AuditService.LogAsync(user.Id, "UserRegistered", $"Username: {user.Username}");

    return BaseResponse<string>.Success("Kayıt başarılı. Giriş yapabilirsiniz.");
}
```

**Response:**

```json
{
  "success": true,
  "data": "Kayıt başarılı",
  "error": null,
  "errorCode": null
}
```

**Frontend Kullanımı:**

1. Kullanıcı form doldurur
2. `POST /api/auth/register` çağrılır
3. Success: Login sayfasına yönlendir
4. Error: Hata mesajını göster (örn: "Username kullanımda")

---

#### [POST] `/api/auth/login`

Kullanıcı girişi.

**Request:**

```json
{
  "username": "ahmet123",
  "password": "SecurePass123!"
}
```

**Operation Logic:**

```csharp
public static async Task<BaseResponse<LoginResponse>> LoginAsync(LoginRequest request, ApplicationContext context)
{
    // 1. Kullanıcı bul
    var user = await context.Users
        .Include(u => u.InstitutionMemberships)
            .ThenInclude(im => im.Institution)
        .FirstOrDefaultAsync(u => u.Username == request.Username);

    if (user == null)
        return BaseResponse<LoginResponse>.Error("Kullanıcı adı veya şifre hatalı");

    // 2. Password doğrula
    if (!PasswordHelper.VerifyHash(request.Password, user.PasswordHash, user.PasswordSalt))
        return BaseResponse<LoginResponse>.Error("Kullanıcı adı veya şifre hatalı");

    // 3. Status kontrolü
    if (user.Status == UserStatus.Suspended)
        return BaseResponse<LoginResponse>.Error("Hesabınız askıya alınmış");

    // 4. Token oluştur
    var token = SessionService.GenerateToken(user, user.InstitutionMemberships.ToList());

    // 5. LastLogin güncelle
    user.LastLoginAt = DateTime.UtcNow;
    await context.SaveChangesAsync();

    // 6. Audit log
    await AuditService.LogAsync(user.Id, "UserLoggedIn", null);

    var response = new LoginResponse
    {
        Token = token,
        ExpiresAt = DateTime.UtcNow.AddDays(7),
        User = new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Username = user.Username,
            Email = user.Email,
            GlobalRole = user.GlobalRole.ToString(),
            Institutions = user.InstitutionMemberships.Select(im => new InstitutionSummaryDto
            {
                Id = im.InstitutionId,
                Name = im.Institution.Name,
                Role = im.Role.ToString()
            }).ToList()
        }
    };

    return BaseResponse<LoginResponse>.Success(response);
}
```

**Response:**

```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5...",
    "expiresAt": "2026-01-11T16:00:00Z",
    "user": {
      "id": 105,
      "fullName": "Ahmet Yılmaz",
      "username": "ahmet123",
      "email": "ahmet@example.com",
      "globalRole": "User",
      "institutions": [
        { "id": 1, "name": "ABC Dershanesi", "role": "Teacher" },
        { "id": 2, "name": "XYZ Dershanesi", "role": "Student" }
      ]
    }
  },
  "error": null,
  "errorCode": null
}
```

**Frontend Kullanımı:**

1. Token'ı `localStorage` veya `sessionStorage`'a kaydet
2. User bilgilerini state'e al
3. Eğer `institutions` listesi varsa, kullanıcıya "Hangi kurumda çalışmak istersiniz?" seçimi sun
4. Ana sayfaya yönlendir

---

**(Devam edecek - Faz 1'in geri kalanı...)**

---

#### [POST] `/api/auth/apply-institution`

Dershane başvurusu yapma.

**Auth Required:** `[Authorize]` (Sadece giriş yapmış kullanıcılar)

**Request:**

```json
{
  "name": "ABC Dershanesi",
  "licenseNumber": "34-12345",
  "address": "Kadıköy, İstanbul",
  "phone": "0216 123 45 67"
}
```

**Operation Logic:**

```csharp
public static async Task<BaseResponse<int>> ApplyInstitutionAsync(
    InstitutionApplicationRequest request,
    int currentUserId,
    ApplicationContext context)
{
    // 1. Kullanıcı zaten bir kurumun yöneticisi mi?
    var existingManager = await context.Institutions
        .AnyAsync(i => i.ManagerUserId == currentUserId);

    if (existingManager)
        return BaseResponse<int>.Error("Zaten bir kurum başvurunuz var");

    // 2. Institution kaydı oluştur
    var institution = new Institution
    {
        Name = request.Name,
        LicenseNumber = request.LicenseNumber,
        Address = request.Address,
        Phone = request.Phone,
        ManagerUserId = currentUserId,
        Status = InstitutionStatus.PendingApproval,
        CreatedAt = DateTime.UtcNow
    };

    context.Institutions.Add(institution);
    await context.SaveChangesAsync();

    // 3. Adminlere bildirim gönder
    await NotificationService.SendToAdminsAsync(
        "Yeni Kurum Başvurusu",
        $"{institution.Name} başvuru yaptı",
        $"/admin/institution/{institution.Id}"
    );

    // 4. Audit log
    await AuditService.LogAsync(currentUserId, "InstitutionApplicationCreated",
        JsonSerializer.Serialize(new { InstitutionId = institution.Id, Name = institution.Name }));

    return BaseResponse<int>.Success(institution.Id);
}
```

**Response:**

```json
{
  "success": true,
  "data": 15, // Institution ID
  "error": null,
  "errorCode": null
}
```

**Frontend Kullanımı:**

1. Başvuru formu doldurulur
2. Success mesajı: "Başvurunuz alındı. Admin onayından sonra bilgilendirileceksiniz."
3. Kullanıcıyı ana sayfaya yönlendir

---

### 3.3. Admin İşlemleri

#### [GET] `/api/admin/institution-applications`

Bekleyen kurum başvurularını listele.

**Auth:** Sadece `Admin` veya `AdminAdmin`

**Response:**

```json
{
  "success": true,
  "data": [
    {
      "id": 15,
      "name": "ABC Dershanesi",
      "licenseNumber": "34-12345",
      "managerName": "Ahmet Yılmaz",
      "createdAt": "2026-01-04T10:00:00Z",
      "status": "PendingApproval"
    }
  ]
}
```

#### [POST] `/api/admin/institution/approve/{id}`

Kurum başvurusunu onayla.

**Request:**

```json
{
  "subscriptionMonths": 12
}
```

**Operation Logic:**

```csharp
public static async Task<BaseResponse<string>> ApproveInstitutionAsync(
    int institutionId,
    int subscriptionMonths,
    int adminId,
    ApplicationContext context)
{
    var institution = await context.Institutions.FindAsync(institutionId);
    if (institution == null)
        return BaseResponse<string>.Error("Kurum bulunamadı");

    // 1. Status güncelle
    institution.Status = InstitutionStatus.Active;
    institution.SubscriptionStartDate = DateTime.UtcNow;
    institution.SubscriptionEndDate = DateTime.UtcNow.AddMonths(subscriptionMonths);
    institution.ApprovedAt = DateTime.UtcNow;
    institution.ApprovedByAdminId = adminId;

    // 2. Kurum yöneticisine InstitutionUser kaydı oluştur
    var institutionUser = new InstitutionUser
    {
        UserId = institution.ManagerUserId,
        InstitutionId = institution.Id,
        Role = InstitutionRole.Manager,
        JoinedAt = DateTime.UtcNow
    };

    context.InstitutionUsers.Add(institutionUser);
    await context.SaveChangesAsync();

    // 3. Yöneticiye bildirim
    await NotificationService.SendAsync(
        institution.ManagerUserId,
        "Kurum Başvurunuz Onaylandı",
        $"{institution.Name} kurumunuz aktif edildi!",
        $"/institution/{institution.Id}/dashboard"
    );

    // 4. Audit log
    await AuditService.LogAsync(adminId, "InstitutionApproved",
        JsonSerializer.Serialize(new { InstitutionId = institutionId }));

    return BaseResponse<string>.Success("Kurum onaylandı");
}
```

---

### 3.4. AccountLink (Hesap Bağlama) İşlemleri

#### [POST] `/api/account/link-request`

Bağımsız hesabı kurum hesabına bağlama talebi.

**Auth Required:** `[Authorize]`

**Request:**

```json
{
  "studentNumber": "2024001",
  "institutionId": 1
}
```

**Operation Logic:**

```csharp
public static async Task<BaseResponse<int>> RequestAccountLinkAsync(
    AccountLinkRequest request,
    int currentUserId,
    ApplicationContext context)
{
    // 1. InstitutionUser'ı bul
    var institutionUser = await context.InstitutionUsers
        .FirstOrDefaultAsync(iu =>
            iu.InstitutionId == request.InstitutionId &&
            iu.StudentNumber == request.StudentNumber);

    if (institutionUser == null)
        return BaseResponse<int>.Error("Öğrenci numarası bulunamadı");

    // 2. Zaten bağlı mı?
    var existingLink = await context.AccountLinks
        .AnyAsync(al =>
            al.MainUserId == currentUserId &&
            al.InstitutionUserId == institutionUser.Id);

    if (existingLink)
        return BaseResponse<int>.Error("Zaten bağlantı talebi gönderdiniz");

    // 3. Link oluştur
    var accountLink = new AccountLink
    {
        MainUserId = currentUserId,
        InstitutionUserId = institutionUser.Id,
        Status = LinkStatus.Pending,
        RequestedAt = DateTime.UtcNow
    };

    context.AccountLinks.Add(accountLink);
    await context.SaveChangesAsync();

    // 4. Kurum yöneticisine bildirim
    var manager = await context.InstitutionUsers
        .Where(iu => iu.InstitutionId == request.InstitutionId && iu.Role == InstitutionRole.Manager)
        .Select(iu => iu.UserId)
        .FirstOrDefaultAsync();

    if (manager > 0)
    {
        await NotificationService.SendAsync(
            manager,
            "Hesap Bağlama Talebi",
            $"Yeni bir hesap bağlama talebi var",
            $"/institution/link-requests"
        );
    }

    return BaseResponse<int>.Success(accountLink.Id);
}
```

**Frontend Kullanımı:**

1. Öğrenci dershane seçer ve öğrenci numarasını girer
2. Talep gönderilir
3. "Talebiniz gönderildi. Kurum yöneticisi onayından sonra hesaplar bağlanacak." mesajı gösterilir

---

#### [POST] `/api/auth/refresh-token`

JWT token'ı yenileme (Refresh Token kullanarak).

**Request:**

```json
{
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Operation Logic:**

```csharp
public static async Task<BaseResponse<LoginResponse>> RefreshTokenAsync(
    RefreshTokenRequest request,
    ApplicationContext context)
{
    // 1. Refresh token'ı bul ve doğrula
    var refreshToken = await context.RefreshTokens
        .Include(rt => rt.User)
            .ThenInclude(u => u.InstitutionMemberships)
                .ThenInclude(im => im.Institution)
        .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && rt.IsActive);

    if (refreshToken == null || refreshToken.ExpiresAt < DateTime.UtcNow)
        return BaseResponse<LoginResponse>.Error("Geçersiz veya süresi dolmuş refresh token");

    // 2. Yeni JWT token oluştur
    var newToken = SessionService.GenerateToken(refreshToken.User, refreshToken.User.InstitutionMemberships.ToList());

    // 3. Yeni refresh token oluştur (eski token'ı devre dışı bırak)
    refreshToken.IsActive = false;
    var newRefreshToken = new RefreshToken
    {
        UserId = refreshToken.UserId,
        Token = Guid.NewGuid().ToString(),
        ExpiresAt = DateTime.UtcNow.AddDays(30),
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    context.RefreshTokens.Add(newRefreshToken);
    await context.SaveChangesAsync();

    var response = new LoginResponse
    {
        Token = newToken,
        RefreshToken = newRefreshToken.Token,
        ExpiresAt = DateTime.UtcNow.AddDays(7),
        User = MapToUserDto(refreshToken.User)
    };

    return BaseResponse<LoginResponse>.Success(response);
}
```

**Frontend Kullanımı:**

1. Token süresi dolduğunda otomatik olarak refresh token ile yeni token al
2. Yeni token'ı localStorage'a kaydet
3. İstekleri yeni token ile devam ettir

---

#### [POST] `/api/auth/forgot-password`

Şifre sıfırlama talebi gönderme.

**Request:**

```json
{
  "email": "ahmet@example.com"
}
```

**Operation Logic:**

```csharp
public static async Task<BaseResponse<string>> ForgotPasswordAsync(
    ForgotPasswordRequest request,
    ApplicationContext context)
{
    var user = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
    if (user == null)
        return BaseResponse<string>.Success("Eğer bu email kayıtlıysa, şifre sıfırlama linki gönderildi"); // Güvenlik için aynı mesaj

    // 1. Token oluştur
    var token = Guid.NewGuid().ToString();
    var resetToken = new PasswordResetToken
    {
        UserId = user.Id,
        Token = token,
        ExpiresAt = DateTime.UtcNow.AddHours(24),
        IsUsed = false,
        CreatedAt = DateTime.UtcNow
    };

    context.PasswordResetTokens.Add(resetToken);
    await context.SaveChangesAsync();

    // 2. Email gönder (EmailService kullanılmalı)
    var resetLink = $"https://karneproject.com/reset-password?token={token}";
    await EmailService.SendPasswordResetEmailAsync(user.Email, resetLink);

    return BaseResponse<string>.Success("Şifre sıfırlama linki email adresinize gönderildi");
}
```

---

#### [POST] `/api/auth/reset-password`

Şifre sıfırlama işlemini tamamlama.

**Request:**

```json
{
  "token": "guid-token-here",
  "newPassword": "NewSecurePass123!"
}
```

**Operation Logic:**

```csharp
public static async Task<BaseResponse<string>> ResetPasswordAsync(
    ResetPasswordRequest request,
    ApplicationContext context)
{
    // 1. Token'ı bul ve doğrula
    var resetToken = await context.PasswordResetTokens
        .Include(rt => rt.User)
        .FirstOrDefaultAsync(rt => rt.Token == request.Token && !rt.IsUsed && rt.ExpiresAt > DateTime.UtcNow);

    if (resetToken == null)
        return BaseResponse<string>.Error("Geçersiz veya süresi dolmuş token");

    // 2. Yeni şifreyi hash'le
    PasswordHelper.CreateHash(request.NewPassword, out byte[] hash, out byte[] salt);

    // 3. Kullanıcı şifresini güncelle
    resetToken.User.PasswordHash = hash;
    resetToken.User.PasswordSalt = salt;

    // 4. Token'ı kullanıldı olarak işaretle
    resetToken.IsUsed = true;
    resetToken.UsedAt = DateTime.UtcNow;

    await context.SaveChangesAsync();

    // 5. Audit log
    await AuditService.LogAsync(resetToken.UserId, "PasswordReset", null);

    return BaseResponse<string>.Success("Şifreniz başarıyla güncellendi");
}
```

---

### 3.5. User Profile Management (Kullanıcı Profil Yönetimi)

#### [Model] UserPreferences

Kullanıcının UI tercihlerini saklar.

```csharp
public class UserPreferences
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }

    // UI Ayarları
    public string Theme { get; set; } = "light"; // "light", "dark", "auto"
    public string Language { get; set; } = "tr"; // "tr", "en"
    public string DateFormat { get; set; } = "dd/MM/yyyy";
    public string TimeFormat { get; set; } = "24h"; // "12h", "24h"

    // Bildirim Ayarları (JSON)
    public string NotificationSettingsJson { get; set; } = "{}";

    // Layout Ayarları (JSON)
    public string ProfileLayoutJson { get; set; } = "{}";
    public string DashboardLayoutJson { get; set; } = "{}";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
```

#### [GET] `/api/user/profile`

Kullanıcının kendi profil bilgilerini getir.

**Query Parameters:**

- `forceRefresh`: Cache'i bypass et (default: false)

**Response:**

```json
{
  "success": true,
  "data": {
    "id": 105,
    "fullName": "Ahmet Yılmaz",
    "username": "ahmet123",
    "email": "ahmet@example.com",
    "phone": "0555 123 45 67",
    "profileImageUrl": "https://cdn.../profile.jpg",
    "profileVisibility": "PublicToAll",
    "followerCount": 45,
    "followingCount": 23,
    "createdAt": "2025-01-01T10:00:00Z",
    "lastLoginAt": "2026-01-05T14:30:00Z"
  }
}
```

**Operation Logic:**

```csharp
public async Task<BaseResponse<UserProfileDto>> GetProfileAsync(int userId, bool forceRefresh = false)
{
    var cacheKey = $"user_profile_{userId}";
    if (!forceRefresh)
    {
        var cached = await _cacheService.GetAsync<UserProfileDto>(cacheKey);
        if (cached != null)
            return BaseResponse<UserProfileDto>.SuccessResponse(cached);
    }

    var user = await _context.Users
        .AsNoTracking()
        .FirstOrDefaultAsync(u => u.Id == userId);

    if (user == null)
        return BaseResponse<UserProfileDto>.ErrorResponse("User not found", ErrorCodes.AuthUserNotFound);

    var profile = MapToUserProfileDto(user);

    if (!forceRefresh)
    {
        await _cacheService.SetAsync(cacheKey, profile, TimeSpan.FromMinutes(15));
    }

    return BaseResponse<UserProfileDto>.SuccessResponse(profile);
}
```

---

#### [PUT] `/api/user/profile`

Profil bilgilerini güncelle.

**Request:**

```json
{
  "fullName": "Ahmet Yılmaz",
  "phone": "0555 123 45 67",
  "profileVisibility": "TeachersOnly"
}
```

**Operation Logic:**

```csharp
public async Task<BaseResponse<string>> UpdateProfileAsync(int userId, UpdateProfileRequest request)
{
    var user = await _context.Users.FindAsync(userId);
    if (user == null)
        return BaseResponse<string>.ErrorResponse("User not found", ErrorCodes.AuthUserNotFound);

    if (!string.IsNullOrEmpty(request.FullName))
        user.FullName = request.FullName;

    if (request.Phone != null)
        user.Phone = request.Phone;

    if (request.ProfileVisibility.HasValue)
        user.ProfileVisibility = request.ProfileVisibility.Value;

    await _context.SaveChangesAsync();

    // Cache invalidation
    await _cacheService.InvalidateUserCacheAsync(userId);

    // Audit log
    await _auditService.LogAsync(userId, "ProfileUpdated", null);

    return BaseResponse<string>.SuccessResponse("Profile updated successfully");
}
```

---

#### [POST] `/api/user/change-password`

Kullanıcı şifresini değiştirme.

**Request:**

```json
{
  "currentPassword": "OldPass123!",
  "newPassword": "NewPass123!"
}
```

**Operation Logic:**

```csharp
public async Task<BaseResponse<string>> ChangePasswordAsync(int userId, ChangePasswordRequest request)
{
    var user = await _context.Users.FindAsync(userId);
    if (user == null)
        return BaseResponse<string>.ErrorResponse("User not found", ErrorCodes.AuthUserNotFound);

    // Mevcut şifreyi doğrula
    if (!PasswordHelper.VerifyHash(request.CurrentPassword, user.PasswordHash, user.PasswordSalt))
        return BaseResponse<string>.ErrorResponse("Current password is incorrect", ErrorCodes.AuthInvalidPassword);

    // Yeni şifreyi hash'le
    PasswordHelper.CreateHash(request.NewPassword, out byte[] hash, out byte[] salt);
    user.PasswordHash = hash;
    user.PasswordSalt = salt;

    await _context.SaveChangesAsync();

    // Cache invalidation
    await _cacheService.InvalidateUserCacheAsync(userId);

    // Audit log
    await _auditService.LogAsync(userId, "PasswordChanged", null);

    return BaseResponse<string>.SuccessResponse("Password changed successfully");
}
```

---

#### [POST] `/api/user/upload-profile-image`

Profil fotoğrafı yükleme.

**Request:**

- `IFormFile file`: Resim dosyası (max 5MB, jpg/png)

**Operation Logic:**

```csharp
public async Task<BaseResponse<string>> UploadProfileImageAsync(int userId, IFormFile file)
{
    // 1. Dosya validasyonu
    if (file == null || file.Length == 0)
        return BaseResponse<string>.ErrorResponse("File is required", ErrorCodes.ValidationFailed);

    if (file.Length > 5 * 1024 * 1024) // 5MB
        return BaseResponse<string>.ErrorResponse("File size must be less than 5MB", ErrorCodes.ValidationFailed);

    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
    var extension = Path.GetExtension(file.FileName).ToLower();
    if (!allowedExtensions.Contains(extension))
        return BaseResponse<string>.ErrorResponse("Only JPG and PNG files are allowed", ErrorCodes.ValidationFailed);

    // 2. Dosyayı yükle (FileService kullanılmalı)
    var imageUrl = await _fileService.UploadImageAsync(file, $"profile_{userId}");

    // 3. Kullanıcı profil resmini güncelle
    var user = await _context.Users.FindAsync(userId);
    user.ProfileImageUrl = imageUrl;
    await _context.SaveChangesAsync();

    // Cache invalidation
    await _cacheService.InvalidateUserCacheAsync(userId);

    return BaseResponse<string>.SuccessResponse(imageUrl);
}
```

---

#### [POST] `/api/user/logout`

Kullanıcı çıkışı (Token blacklist'e ekleme).

**Operation Logic:**

```csharp
public async Task<BaseResponse<string>> LogoutAsync(int userId, string token)
{
    // 1. Refresh token'ları devre dışı bırak
    var refreshTokens = await _context.RefreshTokens
        .Where(rt => rt.UserId == userId && rt.IsActive)
        .ToListAsync();

    foreach (var rt in refreshTokens)
    {
        rt.IsActive = false;
    }

    await _context.SaveChangesAsync();

    // 2. JWT token'ı blacklist'e ekle (TokenBlacklistService kullanılmalı)
    await _tokenBlacklistService.BlacklistTokenAsync(token);

    // Cache invalidation
    await _cacheService.InvalidateUserCacheAsync(userId);

    // Audit log
    await _auditService.LogAsync(userId, "UserLoggedOut", null);

    return BaseResponse<string>.SuccessResponse("Logged out successfully");
}
```

---

#### [POST] `/api/user/send-verification-email`

Email doğrulama linki gönderme.

**Operation Logic:**

```csharp
public async Task<BaseResponse<string>> SendVerificationEmailAsync(int userId)
{
    var user = await _context.Users.FindAsync(userId);
    if (user == null)
        return BaseResponse<string>.ErrorResponse("User not found", ErrorCodes.AuthUserNotFound);

    // 1. Token oluştur
    var token = Guid.NewGuid().ToString();
    var emailVerification = new EmailVerification
    {
        UserId = userId,
        Token = token,
        ExpiresAt = DateTime.UtcNow.AddDays(7),
        IsUsed = false,
        CreatedAt = DateTime.UtcNow
    };

    _context.EmailVerifications.Add(emailVerification);
    await _context.SaveChangesAsync();

    // 2. Email gönder
    var verificationLink = $"https://karneproject.com/verify-email?token={token}";
    await EmailService.SendVerificationEmailAsync(user.Email, verificationLink);

    return BaseResponse<string>.SuccessResponse("Verification email sent");
}
```

---

#### [POST] `/api/user/verify-email`

Email doğrulama işlemini tamamlama.

**Request:**

```json
{
  "token": "guid-token-here"
}
```

**Operation Logic:**

```csharp
public async Task<BaseResponse<string>> VerifyEmailAsync(string token)
{
    var emailVerification = await _context.EmailVerifications
        .Include(ev => ev.User)
        .FirstOrDefaultAsync(ev => ev.Token == token && !ev.IsUsed && ev.ExpiresAt > DateTime.UtcNow);

    if (emailVerification == null)
        return BaseResponse<string>.ErrorResponse("Invalid or expired token", ErrorCodes.ValidationFailed);

    // Email'i doğrula
    emailVerification.User.EmailVerified = true;
    emailVerification.IsUsed = true;
    emailVerification.UsedAt = DateTime.UtcNow;

    await _context.SaveChangesAsync();

    // Cache invalidation
    await _cacheService.InvalidateUserCacheAsync(emailVerification.UserId);

    return BaseResponse<string>.SuccessResponse("Email verified successfully");
}
```

---

#### [GET] `/api/user/profile/{userId}`

Başka bir kullanıcının profilini görüntüleme (privacy ayarlarına göre).

**Query Parameters:**

- `forceRefresh`: Cache'i bypass et (default: false)

**Operation Logic:**

```csharp
public async Task<BaseResponse<UserProfileDto>> GetUserProfileAsync(int targetUserId, int currentUserId, bool forceRefresh = false)
{
    var cacheKey = $"user_profile_{targetUserId}_{currentUserId}";
    if (!forceRefresh)
    {
        var cached = await _cacheService.GetAsync<UserProfileDto>(cacheKey);
        if (cached != null)
            return BaseResponse<UserProfileDto>.SuccessResponse(cached);
    }

    var targetUser = await _context.Users
        .AsNoTracking()
        .FirstOrDefaultAsync(u => u.Id == targetUserId);

    if (targetUser == null)
        return BaseResponse<UserProfileDto>.ErrorResponse("User not found", ErrorCodes.AuthUserNotFound);

    // Privacy kontrolü
    var isOwner = targetUserId == currentUserId;
    var canView = CanViewProfile(targetUser, currentUserId, _context);

    if (!canView)
        return BaseResponse<UserProfileDto>.ErrorResponse("You don't have permission to view this profile", ErrorCodes.AuthAccessDenied);

    var profile = MapToUserProfileDto(targetUser, isOwner);

    if (!forceRefresh)
    {
        await _cacheService.SetAsync(cacheKey, profile, TimeSpan.FromMinutes(10));
    }

    return BaseResponse<UserProfileDto>.SuccessResponse(profile);
}
```

---

#### [PUT] `/api/user/email`

Email adresini güncelleme.

**Request:**

```json
{
  "newEmail": "newemail@example.com"
}
```

**Operation Logic:**

```csharp
public async Task<BaseResponse<string>> UpdateEmailAsync(int userId, UpdateEmailRequest request)
{
    var user = await _context.Users.FindAsync(userId);
    if (user == null)
        return BaseResponse<string>.ErrorResponse("User not found", ErrorCodes.AuthUserNotFound);

    // Email benzersizlik kontrolü
    var emailExists = await _context.Users.AnyAsync(u => u.Email == request.NewEmail && u.Id != userId);
    if (emailExists)
        return BaseResponse<string>.ErrorResponse("Email already in use", ErrorCodes.ValidationFailed);

    user.Email = request.NewEmail;
    user.EmailVerified = false; // Yeni email doğrulanmalı

    await _context.SaveChangesAsync();

    // Yeni email doğrulama linki gönder
    await SendVerificationEmailAsync(userId);

    // Cache invalidation
    await _cacheService.InvalidateUserCacheAsync(userId);

    return BaseResponse<string>.SuccessResponse("Email updated. Please verify your new email.");
}
```

---

#### [DELETE] `/api/user/account`

Hesap silme (soft delete).

**Request:**

```json
{
  "password": "SecurePass123!"
}
```

**Operation Logic:**

```csharp
public async Task<BaseResponse<string>> DeleteAccountAsync(int userId, DeleteAccountRequest request)
{
    var user = await _context.Users.FindAsync(userId);
    if (user == null)
        return BaseResponse<string>.ErrorResponse("User not found", ErrorCodes.AuthUserNotFound);

    // Şifre doğrulama
    if (!PasswordHelper.VerifyHash(request.Password, user.PasswordHash, user.PasswordSalt))
        return BaseResponse<string>.ErrorResponse("Password is incorrect", ErrorCodes.AuthInvalidPassword);

    // Soft delete
    user.Status = UserStatus.Deleted;
    user.DeletedAt = DateTime.UtcNow;

    // Refresh token'ları devre dışı bırak
    var refreshTokens = await _context.RefreshTokens
        .Where(rt => rt.UserId == userId && rt.IsActive)
        .ToListAsync();

    foreach (var rt in refreshTokens)
    {
        rt.IsActive = false;
    }

    await _context.SaveChangesAsync();

    // Cache invalidation
    await _cacheService.InvalidateUserCacheAsync(userId);

    // Audit log
    await _auditService.LogAsync(userId, "AccountDeleted", null);

    return BaseResponse<string>.SuccessResponse("Account deleted successfully");
}
```

---

#### [GET] `/api/user/statistics`

Kullanıcı istatistiklerini getir.

**Query Parameters:**

- `forceRefresh`: Cache'i bypass et (default: false)

**Response:**

```json
{
  "success": true,
  "data": {
    "totalExams": 15,
    "averageScore": 78.5,
    "totalReports": 12,
    "classRank": 3,
    "institutionRank": 15,
    "totalStudyHours": 120.5,
    "completedTasks": 45,
    "pendingTasks": 8
  }
}
```

---

#### [GET] `/api/user/activity`

Kullanıcı aktivite geçmişini getir.

**Query Parameters:**

- `page`: Sayfa numarası (default: 1)
- `limit`: Sayfa başına kayıt (default: 20)
- `forceRefresh`: Cache'i bypass et (default: false)

**Response:**

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "action": "ExamCompleted",
        "description": "TYT Deneme-1 sınavını tamamladı",
        "timestamp": "2026-01-05T10:30:00Z",
        "link": "/exam/123"
      }
    ],
    "totalCount": 150,
    "page": 1,
    "limit": 20
  }
}
```

---

#### [GET] `/api/user/search`

Kullanıcı arama.

**Query Parameters:**

- `query`: Arama metni
- `role`: UserRole filtresi (opsiyonel)
- `page`: Sayfa numarası (default: 1)
- `limit`: Sayfa başına kayıt (default: 20)
- `forceRefresh`: Cache'i bypass et (default: false)

**Response:**

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 105,
        "fullName": "Ahmet Yılmaz",
        "username": "ahmet123",
        "profileImageUrl": "https://...",
        "followerCount": 45
      }
    ],
    "totalCount": 50,
    "page": 1,
    "limit": 20
  }
}
```

---

#### [GET] `/api/user/preferences`

Kullanıcı tercihlerini getir.

**Query Parameters:**

- `forceRefresh`: Cache'i bypass et (default: false)

**Response:**

```json
{
  "success": true,
  "data": {
    "theme": "dark",
    "language": "tr",
    "dateFormat": "dd/MM/yyyy",
    "timeFormat": "24h",
    "notificationSettings": {
      "emailNotifications": true,
      "pushNotifications": true,
      "messageNotifications": true
    },
    "profileLayout": {
      "showStatistics": true,
      "showActivity": true,
      "widgetOrder": ["stats", "activity", "reports"]
    },
    "dashboardLayout": {
      "showQuickActions": true,
      "showRecentExams": true,
      "widgetOrder": ["exams", "notifications", "calendar"]
    }
  }
}
```

---

#### [PUT] `/api/user/preferences`

Kullanıcı tercihlerini güncelle.

**Request:**

```json
{
  "theme": "dark",
  "language": "en",
  "dateFormat": "MM/dd/yyyy",
  "timeFormat": "12h",
  "notificationSettings": {
    "emailNotifications": false,
    "pushNotifications": true
  }
}
```

**Operation Logic:**

```csharp
public async Task<BaseResponse<string>> UpdatePreferencesAsync(int userId, UserPreferencesDto request)
{
    var preferences = await _context.UserPreferences
        .FirstOrDefaultAsync(up => up.UserId == userId);

    if (preferences == null)
    {
        preferences = new UserPreferences
        {
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };
        _context.UserPreferences.Add(preferences);
    }

    if (!string.IsNullOrEmpty(request.Theme))
        preferences.Theme = request.Theme;

    if (!string.IsNullOrEmpty(request.Language))
        preferences.Language = request.Language;

    if (!string.IsNullOrEmpty(request.DateFormat))
        preferences.DateFormat = request.DateFormat;

    if (!string.IsNullOrEmpty(request.TimeFormat))
        preferences.TimeFormat = request.TimeFormat;

    if (request.NotificationSettings != null)
        preferences.NotificationSettingsJson = JsonSerializer.Serialize(request.NotificationSettings);

    preferences.UpdatedAt = DateTime.UtcNow;

    await _context.SaveChangesAsync();

    // Cache invalidation
    await _cacheService.InvalidateUserCacheAsync(userId);

    // Audit log
    await _auditService.LogAsync(userId, "PreferencesUpdated", null);

    return BaseResponse<string>.SuccessResponse("Preferences updated successfully");
}
```

---

#### [PUT] `/api/user/preferences/profile-layout`

Profil sayfası layout'unu güncelle.

**Request:**

```json
{
  "showStatistics": true,
  "showActivity": false,
  "widgetOrder": ["reports", "stats", "activity"]
}
```

---

#### [PUT] `/api/user/preferences/dashboard-layout`

Dashboard layout'unu güncelle.

**Request:**

```json
{
  "showQuickActions": true,
  "showRecentExams": true,
  "widgetOrder": ["exams", "calendar", "notifications"]
}
```

---

### 3.6. Admin Operations (Genişletilmiş)

#### [GET] `/api/admin/users`

Tüm kullanıcıları listele (pagination, filtreleme, arama).

**Query Parameters:**

- `page`: Sayfa numarası (default: 1)
- `limit`: Sayfa başına kayıt (default: 20)
- `status`: UserStatus filtresi (opsiyonel)
- `role`: UserRole filtresi (opsiyonel)
- `search`: Arama metni (opsiyonel)
- `forceRefresh`: Cache'i bypass et (default: false)

**Response:**

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 105,
        "fullName": "Ahmet Yılmaz",
        "username": "ahmet123",
        "email": "ahmet@example.com",
        "role": "User",
        "status": "Active",
        "createdAt": "2025-01-01T10:00:00Z",
        "lastLoginAt": "2026-01-05T14:30:00Z"
      }
    ],
    "totalCount": 1500,
    "page": 1,
    "limit": 20
  }
}
```

---

#### [GET] `/api/admin/users/{id}`

Belirli bir kullanıcının detaylarını getir.

**Query Parameters:**

- `forceRefresh`: Cache'i bypass et (default: false)

---

#### [PUT] `/api/admin/users/{id}`

Kullanıcı bilgilerini admin tarafından güncelleme.

**Request:**

```json
{
  "fullName": "Ahmet Yılmaz",
  "email": "ahmet@example.com",
  "phone": "0555 123 45 67",
  "role": "User",
  "status": "Active"
}
```

---

#### [PUT] `/api/admin/users/{id}/status`

Kullanıcı durumunu değiştirme (Active, Suspended, Deleted).

**Request:**

```json
{
  "status": "Suspended"
}
```

---

#### [DELETE] `/api/admin/users/{id}`

Kullanıcıyı silme (soft delete).

---

#### [POST] `/api/admin/users/{id}/reset-password`

Admin tarafından kullanıcı şifresini sıfırlama.

**Request:**

```json
{
  "newPassword": "TempPass123!"
}
```

**Operation Logic:**

```csharp
public async Task<BaseResponse<string>> ResetUserPasswordAsync(int userId, string newPassword, int adminId)
{
    var user = await _context.Users.FindAsync(userId);
    if (user == null)
        return BaseResponse<string>.ErrorResponse("User not found", ErrorCodes.AuthUserNotFound);

    // Yeni şifreyi hash'le
    PasswordHelper.CreateHash(newPassword, out byte[] hash, out byte[] salt);
    user.PasswordHash = hash;
    user.PasswordSalt = salt;

    // Tüm refresh token'ları devre dışı bırak (güvenlik)
    var refreshTokens = await _context.RefreshTokens
        .Where(rt => rt.UserId == userId && rt.IsActive)
        .ToListAsync();

    foreach (var rt in refreshTokens)
    {
        rt.IsActive = false;
    }

    await _context.SaveChangesAsync();

    // Cache invalidation
    await _cacheService.InvalidateUserCacheAsync(userId);

    // Audit log
    await _auditService.LogAsync(adminId, "UserPasswordReset", JsonSerializer.Serialize(new { TargetUserId = userId }));

    return BaseResponse<string>.SuccessResponse("Password reset successfully");
}
```

---

#### [GET] `/api/admin/institutions`

Tüm kurumları listele.

**Query Parameters:**

- `page`: Sayfa numarası (default: 1)
- `limit`: Sayfa başına kayıt (default: 20)
- `status`: InstitutionStatus filtresi (opsiyonel)
- `search`: Arama metni (opsiyonel)
- `forceRefresh`: Cache'i bypass et (default: false)

---

#### [GET] `/api/admin/institutions/{id}`

Belirli bir kurumun detaylarını getir.

**Query Parameters:**

- `forceRefresh`: Cache'i bypass et (default: false)

---

#### [POST] `/api/admin/institutions/{id}/reject`

Kurum başvurusunu reddetme.

**Request:**

```json
{
  "reason": "Eksik belgeler"
}
```

---

#### [PUT] `/api/admin/institutions/{id}/status`

Kurum durumunu değiştirme.

**Request:**

```json
{
  "status": "Suspended"
}
```

---

#### [PUT] `/api/admin/institutions/{id}/subscription`

Kurum aboneliğini uzatma.

**Request:**

```json
{
  "months": 12
}
```

---

#### [POST] `/api/admin/create-admin`

Yeni admin hesabı oluşturma (Sadece AdminAdmin).

**Request:**

```json
{
  "fullName": "Admin User",
  "username": "admin123",
  "email": "admin@karneproject.com",
  "password": "SecurePass123!",
  "role": "Admin"
}
```

---

#### [GET] `/api/admin/admins`

Tüm admin hesaplarını listele.

**Query Parameters:**

- `forceRefresh`: Cache'i bypass et (default: false)

---

#### [GET] `/api/admin/statistics`

Admin paneli istatistikleri.

**Query Parameters:**

- `forceRefresh`: Cache'i bypass et (default: false)

**Response:**

```json
{
  "success": true,
  "data": {
    "totalUsers": 1500,
    "activeUsers": 1200,
    "suspendedUsers": 50,
    "totalInstitutions": 45,
    "activeInstitutions": 40,
    "pendingInstitutions": 5,
    "totalExams": 500,
    "totalReports": 2000,
    "recentRegistrations": 25,
    "recentLogins": 150
  }
}
```

---

#### [GET] `/api/admin/audit-logs`

Audit log kayıtlarını listele.

**Query Parameters:**

- `page`: Sayfa numarası (default: 1)
- `limit`: Sayfa başına kayıt (default: 20)
- `userId`: Kullanıcı ID filtresi (opsiyonel)
- `action`: Action filtresi (opsiyonel)
- `dateFrom`: Başlangıç tarihi (opsiyonel)
- `dateTo`: Bitiş tarihi (opsiyonel)
- `forceRefresh`: Cache'i bypass et (default: false)

---

#### [GET] `/api/admin/audit-logs/user/{userId}`

Belirli bir kullanıcının audit log kayıtlarını getir.

**Query Parameters:**

- `page`: Sayfa numarası (default: 1)
- `limit`: Sayfa başına kayıt (default: 20)
- `forceRefresh`: Cache'i bypass et (default: false)

---

### 3.7. Account Link Operations (Genişletilmiş)

#### [GET] `/api/account/link-requests`

Hesap bağlama taleplerini listele.

**Query Parameters:**

- `status`: LinkStatus filtresi (opsiyonel)
- `page`: Sayfa numarası (default: 1)
- `limit`: Sayfa başına kayıt (default: 20)
- `forceRefresh`: Cache'i bypass et (default: false)

**Response:**

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 10,
        "mainUserName": "ahmet123",
        "institutionName": "ABC Dershanesi",
        "studentNumber": "2024001",
        "status": "Pending",
        "requestedAt": "2026-01-04T10:00:00Z"
      }
    ],
    "totalCount": 5,
    "page": 1,
    "limit": 20
  }
}
```

---

#### [GET] `/api/account/links`

Bağlı hesapları listele.

**Query Parameters:**

- `forceRefresh`: Cache'i bypass et (default: false)

**Response:**

```json
{
  "success": true,
  "data": [
    {
      "id": 5,
      "institutionName": "ABC Dershanesi",
      "studentNumber": "2024001",
      "status": "Approved",
      "linkedAt": "2026-01-03T15:00:00Z"
    }
  ]
}
```

---

#### [DELETE] `/api/account/link/{id}`

Hesap bağlantısını silme.

**Operation Logic:**

```csharp
public async Task<BaseResponse<string>> DeleteAccountLinkAsync(int linkId, int currentUserId)
{
    var link = await _context.AccountLinks
        .Include(al => al.MainUser)
        .Include(al => al.InstitutionUser)
            .ThenInclude(iu => iu.Institution)
        .FirstOrDefaultAsync(al => al.Id == linkId);

    if (link == null)
        return BaseResponse<string>.ErrorResponse("Link not found", ErrorCodes.ValidationFailed);

    // Sadece ana hesap sahibi veya kurum yöneticisi silebilir
    var isMainUser = link.MainUserId == currentUserId;
    var isManager = await _context.InstitutionUsers
        .AnyAsync(iu => iu.UserId == currentUserId && 
                       iu.InstitutionId == link.InstitutionUser.InstitutionId && 
                       iu.Role == InstitutionRole.Manager);

    if (!isMainUser && !isManager)
        return BaseResponse<string>.ErrorResponse("You don't have permission", ErrorCodes.AuthAccessDenied);

    _context.AccountLinks.Remove(link);
    await _context.SaveChangesAsync();

    // Cache invalidation
    await _cacheService.InvalidateAccountLinkCacheAsync(currentUserId);

    // Audit log
    await _auditService.LogAsync(currentUserId, "AccountLinkDeleted", JsonSerializer.Serialize(new { LinkId = linkId }));

    return BaseResponse<string>.SuccessResponse("Account link deleted successfully");
}
```

---

### 3.8. Health Check

#### [GET] `/api/health`

Uygulama sağlık durumunu kontrol et.

**Response:**

```json
{
  "success": true,
  "data": {
    "status": "Healthy",
    "database": "Connected",
    "redis": "Connected",
    "timestamp": "2026-01-05T15:00:00Z"
  }
}
```

**Operation Logic:**

```csharp
[HttpGet]
public async Task<IActionResult> GetHealth()
{
    var health = new
    {
        Status = "Healthy",
        Database = await CheckDatabaseAsync() ? "Connected" : "Disconnected",
        Redis = await CheckRedisAsync() ? "Connected" : "Disconnected",
        Timestamp = DateTime.UtcNow
    };

    return Ok(BaseResponse<object>.SuccessResponse(health));
}
```

---

### 3.9. Middleware'ler

#### GlobalExceptionMiddleware

Tüm exception'ları yakalar ve `BaseResponse` formatında döner.

**Kullanım:**

```csharp
app.UseMiddleware<GlobalExceptionMiddleware>();
```

**Özellikler:**

- Tüm exception'ları yakalar
- `BaseResponse` formatında hata döner
- 6 haneli hata kodları kullanır
- Loglama yapar

---

#### RequestLoggingMiddleware

Tüm HTTP isteklerini loglar.

**Kullanım:**

```csharp
app.UseMiddleware<RequestLoggingMiddleware>();
```

**Özellikler:**

- Request method, path, query string
- Response status code
- İşlem süresi
- IP adresi

---

#### TokenBlacklistMiddleware

Blacklist'teki JWT token'ları reddeder.

**Kullanım:**

```csharp
app.UseMiddleware<TokenBlacklistMiddleware>();
```

**Özellikler:**

- Logout edilen token'ları kontrol eder
- Güvenlik ihlali durumunda token'ları blacklist'e ekler

---

### 3.10. Rate Limiting

**Kullanım:**

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 1000, // 1000 requests
                Window = TimeSpan.FromMinutes(1) // per minute (very broad)
            }));

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsJsonAsync(
            BaseResponse<string>.ErrorResponse("Too many requests. Please try again later.", "100429"),
            cancellationToken);
    };
});

app.UseRateLimiter();
```

**Özellikler:**

- Global rate limiting: 1000 request/dakika/IP
- 429 (Too Many Requests) hatası döner
- `BaseResponse` formatında hata mesajı

---

### 3.11. Cache Service (Geliştirilmiş)

#### Pattern-Based Cache Removal

Redis SCAN kullanarak pattern'e göre cache temizleme.

**Kullanım:**

```csharp
public async Task RemoveByPatternAsync(string pattern)
{
    var server = _redis.GetServer(_redis.GetEndPoints().First());
    var keys = server.Keys(pattern: $"*{pattern}*").ToList();
    foreach (var key in keys)
    {
        await _cache.RemoveAsync(key!);
    }
}
```

#### Specific Cache Invalidation Methods

```csharp
public async Task InvalidateUserCacheAsync(int userId)
{
    await RemoveByPatternAsync($"user_profile_{userId}");
    await RemoveByPatternAsync($"user_statistics_{userId}");
    await RemoveByPatternAsync($"user_preferences_{userId}");
    await RemoveByPatternAsync($"User:{userId}:Notifications");
    await RemoveByPatternAsync($"User:{userId}:Conversations");
    await RemoveByPatternAsync($"User:{userId}:LinkRequests");
    await RemoveByPatternAsync($"User:{userId}:LinkedAccounts");
    await RemoveByPatternAsync($"search_users");
}

public async Task InvalidateAdminCacheAsync()
{
    await RemoveByPatternAsync("admin_statistics");
    await RemoveByPatternAsync("admin_users");
    await RemoveByPatternAsync("admin_institutions");
    await RemoveByPatternAsync("admin_audit_logs");
    await RemoveByPatternAsync("search_");
}
```

---

### 3.12. Force Refresh Mekanizması

Tüm `GET` endpoint'lerinde cache'i bypass etmek için `forceRefresh` query parametresi eklendi.

**Kullanım:**

```
GET /api/user/profile?forceRefresh=true
GET /api/admin/users?forceRefresh=true&page=1&limit=20
GET /api/exam?forceRefresh=false
```

**Frontend Kullanımı:**

1. Kullanıcı "Yenile" butonuna tıklar
2. `forceRefresh=true` parametresi ile istek gönderilir
3. Cache bypass edilir, fresh data döner
4. Yeni data cache'e yazılır

---

## 🏫 4. FAZ 2: KURUM YÖNETİMİ (Institution Management)

Bu fazda dershanelerin tüm operasyonel ihtiyaçları karşılanır.

### 4.1. Sınıf Yönetimi (Classroom Management)

#### [Model] Classroom

```csharp
public class Classroom
{
    public int Id { get; set; }
    public int InstitutionId { get; set; }
    public Institution Institution { get; set; }

    public string Name { get; set; } // "12-A"
    public int Grade { get; set; } // 12
    public int? HeadTeacherId { get; set; } // Sınıf öğretmeni (opsiyonel)

    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<ClassroomStudent> Students { get; set; }
    public ICollection<ClassroomTeacher> Teachers { get; set; }
    public Conversation ClassConversation { get; set; } // Sınıf grup mesajı
}
```

#### [Model] ClassroomStudent

Çoktan-çoğa ilişki (Bir sınıfta birden fazla öğrenci, bir öğrenci birden fazla sınıfta olabilir - farklı zamanlar için).

```csharp
public class ClassroomStudent
{
    public int Id { get; set; }
    public int ClassroomId { get; set; }
    public Classroom Classroom { get; set; }

    public int InstitutionUserId { get; set; } // InstitutionUser tablosundan öğrenci
    public InstitutionUser Student { get; set; }

    public DateTime AssignedAt { get; set; }
}
```

#### [POST] `/api/institution/classroom/create`

Sınıf oluşturma.

**Auth:** Sadece `InstitutionRole.Manager`

**Request:**

```json
{
  "name": "12-A",
  "grade": 12,
  "headTeacherId": 105
}
```

**Operation Logic:**

```csharp
public static async Task<BaseResponse<int>> CreateClassroomAsync(
    CreateClassroomRequest request,
    int currentUserId,
    ApplicationContext context)
{
    // 1. Yetki kontrolü
    var institutionUser = await context.InstitutionUsers
        .FirstOrDefaultAsync(iu =>
            iu.UserId == currentUserId &&
            iu.Role == InstitutionRole.Manager);

    if (institutionUser == null)
        return BaseResponse<int>.Error("Yetkiniz yok");

    // 2. Sınıf oluştur
    var classroom = new Classroom
    {
        InstitutionId = institutionUser.InstitutionId,
        Name = request.Name,
        Grade = request.Grade,
        HeadTeacherId = request.HeadTeacherId,
        CreatedAt = DateTime.UtcNow
    };

    context.Classrooms.Add(classroom);
    await context.SaveChangesAsync();

    // 3. Sınıf için grup sohbeti oluştur
    var conversation = new Conversation
    {
        Type = ConversationType.ClassGroup,
        ClassroomId = classroom.Id,
        Name = $"{classroom.Name} Sınıf Grubu",
        CreatedAt = DateTime.UtcNow
    };

    context.Conversations.Add(conversation);
    await context.SaveChangesAsync();

    // 4. Cache invalidate
    await _cacheService.RemoveAsync($"Inst:{institutionUser.InstitutionId}:Classrooms");

    // 5. Audit log
    await AuditService.LogAsync(currentUserId, "ClassroomCreated",
        JsonSerializer.Serialize(new { ClassroomId = classroom.Id, Name = classroom.Name }));

    return BaseResponse<int>.Success(classroom.Id);
}
```

---

### 4.2. Mesajlaşma Sistemi (Messaging System) ⭐ KRİTİK

Sistemin sosyal boyutunu sağlayan temel özellik.

#### [Model] Conversation

```csharp
public class Conversation
{
    public int Id { get; set; }
    public ConversationType Type { get; set; } // Private, ClassGroup

    public int? ClassroomId { get; set; } // Eğer sınıf grubu ise
    public Classroom Classroom { get; set; }

    public string Name { get; set; } // "12-A Sınıf Grubu" veya null (private için)
    public DateTime CreatedAt { get; set; }

    // Navigation
    public ICollection<ConversationMember> Members { get; set; }
    public ICollection<Message> Messages { get; set; }
}
```

#### [Enum] ConversationType

```csharp
public enum ConversationType : byte
{
    Private = 1,      // Birebir mesajlaşma
    ClassGroup = 2    // Sınıf grup sohbeti
}
```

#### [Model] ConversationMember

```csharp
public class ConversationMember
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public Conversation Conversation { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public DateTime JoinedAt { get; set; }
    public DateTime? LastReadAt { get; set; } // Okundu bilgisi için
}
```

#### [Model] Message

```csharp
public class Message
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public Conversation Conversation { get; set; }

    public int SenderId { get; set; }
    public User Sender { get; set; }

    public string Text { get; set; } // Mesaj metni (nullable)
    public MessageType Type { get; set; } // Text, Exam, ReportCard, Question

    // Ekler
    public int? AttachedContentId { get; set; } // Content tablosundan (Soru/Sınav)
    public Content AttachedContent { get; set; }

    public int? AttachedReportCardId { get; set; } // ExamResult tablosundan (Karne)
    public ExamResult AttachedReportCard { get; set; }

    public DateTime SentAt { get; set; }

    // Soft delete
    public bool IsDeleted { get; set; } = false;
}
```

#### [Enum] MessageType

```csharp
public enum MessageType : byte
{
    Text = 1,
    Exam = 2,
    ReportCard = 3,
    Question = 4,
    File = 5
}
```

#### Endpoints

#### [POST] `/api/message/send`

Mesaj gönderme.

**Request:**

```json
{
  "conversationId": 25,
  "text": "Yarınki sınav saat kaçta?",
  "type": "Text",
  "attachedContentId": null
}
```

**Operation Logic:**

```csharp
public static async Task<BaseResponse<MessageDto>> SendMessageAsync(
    SendMessageRequest request,
    int currentUserId,
    ApplicationContext context,
    IHubContext<ChatHub> chatHub) // SignalR Hub
{
    // 1. Conversation üyeliği kontrolü
    var member = await context.ConversationMembers
        .FirstOrDefaultAsync(cm =>
            cm.ConversationId == request.ConversationId &&
            cm.UserId == currentUserId);

    if (member == null)
        return BaseResponse<MessageDto>.Error("Bu sohbete erişiminiz yok");

    // 2. Mesaj oluştur
    var message = new Message
    {
        ConversationId = request.ConversationId,
        SenderId = currentUserId,
        Text = request.Text,
        Type = request.Type,
        AttachedContentId = request.AttachedContentId,
        AttachedReportCardId = request.AttachedReportCardId,
        SentAt = DateTime.UtcNow
    };

    context.Messages.Add(message);
    await context.SaveChangesAsync();

    // 3. SignalR ile real-time gönderim
    var messageDto = MapToDto(message, currentUserId);
    await chatHub.Clients.Group($"Conversation_{request.ConversationId}")
        .SendAsync("ReceiveMessage", messageDto);

    // 4. Diğer üyelere bildirim gönder
    var otherMembers = await context.ConversationMembers
        .Where(cm => cm.ConversationId == request.ConversationId && cm.UserId != currentUserId)
        .Select(cm => cm.UserId)
        .ToListAsync();

    foreach (var userId in otherMembers)
    {
        await NotificationService.SendAsync(
            userId,
            "Yeni Mesaj",
            message.Text.Length > 50 ? message.Text.Substring(0, 50) + "..." : message.Text,
            $"/messages/{request.ConversationId}"
        );
    }

    return BaseResponse<MessageDto>.Success(messageDto);
}
```

**Frontend Kullanımı (SignalR):**

```javascript
// 1. Connection kurma
const connection = new signalR.HubConnectionBuilder()
  .withUrl("/hubs/chat", {
    accessTokenFactory: () => localStorage.getItem("token"),
  })
  .build();

// 2. Conversation'a katılma
connection.invoke("JoinConversation", conversationId);

// 3. Mesaj dinleme
connection.on("ReceiveMessage", (message) => {
  // UI'da mesajı göster
  appendMessageToChat(message);
});

// 4. Mesaj gönderme
async function sendMessage(text) {
  await fetch("/api/message/send", {
    method: "POST",
    headers: {
      Authorization: `Bearer ${token}`,
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ conversationId, text, type: "Text" }),
  });
}
```

---

#### [POST] `/api/message/send-to-class`

Öğretmenin sınıfa toplu karne/sınav göndermesi.

**Auth:** Sadece `InstitutionRole.Teacher`

**Request:**

```json
{
  "classroomId": 5,
  "type": "ReportCard",
  "reportCardIds": [101, 102, 103, 104] // Her öğrenci için ayrı karne
}
```

**Operation Logic:**

```csharp
public static async Task<BaseResponse<int>> SendToClassAsync(
    SendToClassRequest request,
    int currentUserId,
    ApplicationContext context,
    IHubContext<ChatHub> chatHub)
{
    // 1. Öğretmen yetkisi kontrolü
    var classroom = await context.Classrooms
        .Include(c => c.ClassConversation)
        .FirstOrDefaultAsync(c => c.Id == request.ClassroomId);

    if (classroom == null)
        return BaseResponse<int>.Error("Sınıf bulunamadı");

    // 2. Bulk message oluştur
    var messages = new List<Message>();

    foreach (var reportCardId in request.ReportCardIds)
    {
        messages.Add(new Message
        {
            ConversationId = classroom.ClassConversation.Id,
            SenderId = currentUserId,
            Type = MessageType.ReportCard,
            AttachedReportCardId = reportCardId,
            Text = "Yeni karne paylaşıldı",
            SentAt = DateTime.UtcNow
        });
    }

    // 3. Bulk insert
    context.Messages.AddRange(messages);
    await context.SaveChangesAsync();

    // 4. SignalR ile bildirim
    await chatHub.Clients.Group($"Conversation_{classroom.ClassConversation.Id}")
        .SendAsync("BulkReportCardsReceived", messages.Count);

    // 5. Her öğrenciye bildirim
    var students = await context.ClassroomStudents
        .Where(cs => cs.ClassroomId == request.ClassroomId)
        .Select(cs => cs.Student.UserId)
        .ToListAsync();

    foreach (var studentId in students)
    {
        await NotificationService.SendAsync(
            studentId,
            "Yeni Karne",
            "Sınav karneniz hazır!",
            $"/classroom/{request.ClassroomId}/reports"
        );
    }

    return BaseResponse<int>.Success(messages.Count);
}
```

---

### 4.3. Bildirim Sistemi (Notification System) ⭐ KRİTİK

#### [Model] Notification

```csharp
public class Notification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }

    public string Title { get; set; } // "Yeni Karne"
    public string Message { get; set; } // "Matematik sınavı karneniz hazır"
    public NotificationType Type { get; set; }
    public string ActionUrl { get; set; } // "/report/123"

    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; }
}
```

#### [Enum] NotificationType

```csharp
public enum NotificationType : byte
{
    Message = 1,
    ReportCard = 2,
    ExamAssigned = 3,
    AccountLinkRequest = 4,
    InstitutionApproved = 5,
    NewFollower = 6
}
```

#### NotificationService (Helper)

```csharp
public class NotificationService
{
    public static async Task SendAsync(
        int userId,
        string title,
        string message,
        string actionUrl,
        ApplicationContext context,
        IHubContext<NotificationHub> notificationHub)
    {
        // 1. DB'ye kaydet
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            ActionUrl = actionUrl,
            Type = DetermineType(title), // Helper metod
            CreatedAt = DateTime.UtcNow
        };

        context.Notifications.Add(notification);
        await context.SaveChangesAsync();

        // 2. Redis'e ekle (unread count)
        await RedisHelper.IncrementAsync($"User:{userId}:UnreadNotifications");

        // 3. SignalR ile real-time push
        await notificationHub.Clients.User(userId.ToString())
            .SendAsync("ReceiveNotification", new
            {
                notification.Id,
                notification.Title,
                notification.Message,
                notification.ActionUrl,
                notification.CreatedAt
            });
    }

    public static async Task SendToAdminsAsync(
        string title,
        string message,
        string actionUrl,
        ApplicationContext context,
        IHubContext<NotificationHub> notificationHub)
    {
        var adminIds = await context.Users
            .Where(u => u.GlobalRole == UserRole.Admin || u.GlobalRole == UserRole.AdminAdmin)
            .Select(u => u.Id)
            .ToListAsync();

        foreach (var adminId in adminIds)
        {
            await SendAsync(adminId, title, message, actionUrl, context, notificationHub);
        }
    }
}
```

#### [GET] `/api/notification/my`

Kullanıcının bildirimlerini getir.

**Response:**

```json
{
  "success": true,
  "data": {
    "unreadCount": 3,
    "notifications": [
      {
        "id": 501,
        "title": "Yeni Karne",
        "message": "Matematik sınavı karneniz hazır",
        "actionUrl": "/report/123",
        "isRead": false,
        "createdAt": "2026-01-04T14:30:00Z"
      }
    ]
  }
}
```

**Frontend Kullanımı (SignalR):**

```javascript
// Bildirim dinleme
notificationConnection.on("ReceiveNotification", (notification) => {
  // Badge güncelle
  updateNotificationBadge();

  // Toast göster
  showToast(notification.title, notification.message);

  // Listeye ekle
  addNotificationToList(notification);
});
```

---

### 4.4. Sınav ve Optik Okuma Sistemi ⭐⭐⭐ EN KRİTİK

Bu sistem projenin en karmaşık ve en değerli özelliğidir. Öğretmenler optik form okuyucudan aldıkları TXT dosyasını sisteme yükler, sistem otomatik olarak tüm öğrencilerin netlerini hesaplar ve detaylı karneler oluşturur.

#### [Model] Exam

```csharp
public class Exam
{
    public int Id { get; set; }
    public int InstitutionId { get; set; }
    public Institution Institution { get; set; }

    public string Name { get; set; } // "TYT Deneme-1"
    public DateTime Date { get; set; }
    public ExamType Type { get; set; } // TYT, AYT, LGS, Custom

    public int? ClassroomId { get; set; } // Hangi sınıf için
    public Classroom Classroom { get; set; }

    // Cevap Anahtarı (JSON formatında)
    // Format: { "Matematik": "ABCDEABCDE...", "Fizik": "BCDABCDA..." }
    public string AnswerKeyJson { get; set; }

    // Her ders için soru dağılımı ve konu mapping (JSON)
    // Format: { "Matematik": { "StartIndex": 0, "QuestionCount": 40, "Topics": { "0-9": "Fonksiyonlar", "10-19": "Türev" } } }
    public string LessonConfigJson { get; set; }

    public DateTime CreatedAt { get; set; }
    public int CreatedByUserId { get; set; }
    public bool IsPublished { get; set; } = false; // Öğrencilere açılması

    // Navigation
    public ICollection<ExamResult> Results { get; set; }
}
```

#### [Enum] ExamType

```csharp
public enum ExamType : byte
{
    TYT = 1,
    AYT_MAT = 2,
    AYT_FEN = 3,
    AYT_SOZ = 4,
    AYT_DIL = 5,
    LGS = 6,
    YDS = 7,
    Custom = 99
}
```

#### [Model] ExamResult

```csharp
public class ExamResult
{
    public int Id { get; set; }
    public int ExamId { get; set; }
    public Exam Exam { get; set; }

    public int StudentId { get; set; } // User ID
    public User Student { get; set; }

    // Optik formdan gelen bilgiler
    public string StudentNumber { get; set; }
    public string BookletType { get; set; } // "A", "B", "C", "D"

    // Detaylı sonuçlar (JSON)
    // Format: { "Matematik": { "Correct": 30, "Wrong": 5, "Empty": 5, "Net": 28.75, "TopicScores": {...} } }
    public string DetailedResultsJson { get; set; }

    // Toplam değerler
    public float TotalNet { get; set; } // 98.25
    public float TotalScore { get; set; } // 385.50

    // Sıralama bilgileri
    public int? ClassRank { get; set; } // Sınıf içinde 3.
    public int? InstitutionRank { get; set; } // Kurum içinde 15.
    public int? NationalRank { get; set; } // Türkiye geneli (gelecek faz)

    public bool IsConfirmed { get; set; } = false; // Öğretmen onayı
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
}
```

#### Optik Form TXT Dosya Formatı

Optik okuyucu cihazlar genelde şu formatta TXT dosyası üretir:

```
[Öğr.No(0-9)] [AdSoyad(10-35)] [Kitapçık(36)] [Cevaplar(37-160)]
0000012345   AHMET YILMAZ            A  ABCDEABCDABCDEABCDEABCDE...
0000012346   AYŞE KAYA               A  BCDAEBCDABCDABCDEBCDA...
```

- **Öğrenci No:** 10 karakter, sağdan hizalı, soldan sıfırlarla doldurulmuş
- **Ad Soyad:** 25 karakter
- **Kitapçık Tipi:** 1 karakter (A, B, C, D)
- **Cevaplar:** 120 karakter (A, B, C, D, E veya boşluk/0 için boş)

#### Endpoints

#### [POST] `/api/exam/upload-optical`

Optik form yükleme ve toplu işleme.

**Auth:** Sadece `InstitutionRole.Teacher`

**Request:**

- `IFormFile file`: TXT dosyası
- `int examId`: Hangi sınava ait

**Operation Logic:**

```csharp
public static async Task<BaseResponse<OpticalUploadSummary>> UploadOpticalAsync(
    IFormFile file,
    int examId,
    int currentUserId,
    ApplicationContext context)
{
    // 1. Exam bilgilerini çek
    var exam = await context.Exams
        .Include(e => e.Institution)
        .Include(e => e.Classroom)
        .FirstOrDefaultAsync(e => e.Id == examId);

    if (exam == null)
        return BaseResponse<OpticalUploadSummary>.Error("Sınav bulunamadı");

    // 2. Yetki kontrolü (Öğretmen bu kurumda mı?)
    var hasPermission = await context.InstitutionUsers
        .AnyAsync(iu =>
            iu.UserId == currentUserId &&
            iu.InstitutionId == exam.InstitutionId &&
            iu.Role == InstitutionRole.Teacher);

    if (!hasPermission)
        return BaseResponse<OpticalUploadSummary>.Error("Bu sınava yükleme yetkiniz yok");

    // 3. Dosya validasyonu
    if (!file.FileName.EndsWith(".txt"))
        return BaseResponse<OpticalUploadSummary>.Error("Sadece .txt dosyası yüklenebilir");

    if (file.Length > 5 * 1024 * 1024) // 5MB
        return BaseResponse<OpticalUploadSummary>.Error("Dosya boyutu 5MB'dan küçük olmalı");

    // 4. TXT dosyasını parse et
    var parsedLines = await ParseOpticalFileAsync(file);

    if (parsedLines.Count == 0)
        return BaseResponse<OpticalUploadSummary>.Error("Dosya boş veya hatalı format");

    // 5. Cevap anahtarını hazırla
    var answerKey = JsonSerializer.Deserialize<Dictionary<string, string>>(exam.AnswerKeyJson);
    var lessonConfig = JsonSerializer.Deserialize<Dictionary<string, LessonConfig>>(exam.LessonConfigJson);

    // 6. Her satır için işlem
    var results = new List<ExamResult>();
    var errors = new List<string>();

    foreach (var line in parsedLines)
    {
        try
        {
            // 6a. Öğrenci numarasını bul
            var student = await context.InstitutionUsers
                .FirstOrDefaultAsync(iu =>
                    iu.InstitutionId == exam.InstitutionId &&
                    iu.StudentNumber == line.StudentNumber);

            if (student == null)
            {
                errors.Add($"Satır {line.LineNumber}: Öğrenci bulunamadı (No: {line.StudentNumber})");
                continue;
            }

            // 6b. Net hesaplama
            var detailedResults = CalculateDetailedResults(
                line.Answers,
                answerKey,
                lessonConfig
            );

            // 6c. Toplam net ve puan
            float totalNet = detailedResults.Values.Sum(v => v.Net);
            float totalScore = CalculateTotalScore(detailedResults, exam.Type);

            // 6d. ExamResult oluştur
            var result = new ExamResult
            {
                ExamId = examId,
                StudentId = student.UserId,
                StudentNumber = line.StudentNumber,
                BookletType = line.BookletType,
                DetailedResultsJson = JsonSerializer.Serialize(detailedResults),
                TotalNet = totalNet,
                TotalScore = totalScore,
                IsConfirmed = false,
                CreatedAt = DateTime.UtcNow
            };

            results.Add(result);
        }
        catch (Exception ex)
        {
            errors.Add($"Satır {line.LineNumber}: {ex.Message}");
        }
    }

    // 7. Bulk insert (performans için)
    if (results.Any())
    {
        context.ExamResults.AddRange(results);
        await context.SaveChangesAsync();

        // 8. Sıralama hesaplama (background job)
        BackgroundJob.Enqueue(() => CalculateRankingsAsync(examId));
    }

    // 9. Audit log
    await AuditService.LogAsync(currentUserId, "OpticalUploaded",
        JsonSerializer.Serialize(new { ExamId = examId, SuccessCount = results.Count, ErrorCount = errors.Count }));

    // 10. Response
    return BaseResponse<OpticalUploadSummary>.Success(new OpticalUploadSummary
    {
        TotalLines = parsedLines.Count,
        SuccessCount = results.Count,
        ErrorCount = errors.Count,
        Errors = errors,
        Message = $"{results.Count} öğrenci başarıyla işlendi. {errors.Count} hata."
    });
}
```

#### Helper: TXT Dosyası Parse Etme

```csharp
private static async Task<List<ParsedOpticalLine>> ParseOpticalFileAsync(IFormFile file)
{
    var lines = new List<ParsedOpticalLine>();

    using (var reader = new StreamReader(file.OpenReadStream()))
    {
        int lineNumber = 0;
        while (!reader.EndOfStream)
        {
            lineNumber++;
            var line = await reader.ReadLineAsync();

            if (string.IsNullOrWhiteSpace(line) || line.Length < 37)
                continue; // Boş veya çok kısa satırları atla

            lines.Add(new ParsedOpticalLine
            {
                LineNumber = lineNumber,
                StudentNumber = line.Substring(0, 10).Trim(),
                StudentName = line.Substring(10, 25).Trim(),
                BookletType = line.Substring(36, 1),
                Answers = line.Substring(37).Trim()
            });
        }
    }

    return lines;
}
```

#### Helper: Net Hesaplama Algoritması (Detaylı)

```csharp
private static Dictionary<string, LessonScore> CalculateDetailedResults(
    string studentAnswers,
    Dictionary<string, string> answerKey,
    Dictionary<string, LessonConfig> lessonConfig)
{
    var results = new Dictionary<string, LessonScore>();

    foreach (var lesson in answerKey)
    {
        string lessonName = lesson.Key; // "Matematik"
        string correctAnswers = lesson.Value; // "ABCDEABCDE..."
        var config = lessonConfig[lessonName];

        // Öğrenci cevaplarını al (bu dersin başlangıç index'inden itibaren)
        string studentLessonAnswers = studentAnswers.Substring(
            config.StartIndex,
            Math.Min(config.QuestionCount, studentAnswers.Length - config.StartIndex)
        );

        int correct = 0, wrong = 0, empty = 0;
        var topicScores = new Dictionary<string, TopicScore>();

        // Her soruyu kontrol et
        for (int i = 0; i < correctAnswers.Length && i < studentLessonAnswers.Length; i++)
        {
            char studentAnswer = studentLessonAnswers[i];
            char correctAnswer = correctAnswers[i];

            // Konu tespiti (config'de hangi konu aralığında?)
            string topicName = GetTopicForQuestion(i, config.TopicMapping);

            if (!topicScores.ContainsKey(topicName))
            {
                topicScores[topicName] = new TopicScore { TopicName = topicName };
            }

            // Cevap kontrolü
            if (studentAnswer == ' ' || studentAnswer == '0')
            {
                empty++;
                topicScores[topicName].Empty++;
            }
            else if (studentAnswer == correctAnswer)
            {
                correct++;
                topicScores[topicName].Correct++;
            }
            else
            {
                wrong++;
                topicScores[topicName].Wrong++;
            }
        }

        // Net hesaplama
        float net = correct - (wrong / 4.0f);

        // Her konu için net hesapla
        foreach (var topic in topicScores.Values)
        {
            topic.Net = topic.Correct - (topic.Wrong / 4.0f);
        }

        results[lessonName] = new LessonScore
        {
            LessonName = lessonName,
            Correct = correct,
            Wrong = wrong,
            Empty = empty,
            Net = net,
            SuccessRate = (int)((correct / (float)correctAnswers.Length) * 100),
            TopicScores = topicScores.Values.ToList()
        };
    }

    return results;
}

private static string GetTopicForQuestion(int questionIndex, Dictionary<string, string> topicMapping)
{
    // TopicMapping format: { "0-9": "Fonksiyonlar", "10-19": "Türev", ... }
    foreach (var mapping in topicMapping)
    {
        var range = mapping.Key.Split('-');
        int start = int.Parse(range[0]);
        int end = int.Parse(range[1]);

        if (questionIndex >= start && questionIndex <= end)
            return mapping.Value;
    }

    return "Diğer";
}
```

#### [POST] `/api/exam/confirm-results/{examId}`

Öğretmen sonuçları önizledikten sonra onaylar.

**Operation Logic:**

```csharp
public static async Task<BaseResponse<int>> ConfirmResultsAsync(
    int examId,
    int currentUserId,
    ApplicationContext context,
    IHubContext<NotificationHub> notificationHub)
{
    // 1. Tüm sonuçları onayla
    var results = await context.ExamResults
        .Where(r => r.ExamId == examId && !r.IsConfirmed)
        .ToListAsync();

    foreach (var result in results)
    {
        result.IsConfirmed = true;
        result.ConfirmedAt = DateTime.UtcNow;
    }

    await context.SaveChangesAsync();

    // 2. Öğrencilere bildirim gönder
    foreach (var result in results)
    {
        await NotificationService.SendAsync(
            result.StudentId,
            "Yeni Karne Hazır!",
            $"{result.Exam.Name} sınav karneniz hazır",
            $"/student/report/{result.Id}",
            context,
            notificationHub
        );
    }

    return BaseResponse<int>.Success(results.Count);
}
```

#### [GET] `/api/report/student/{resultId}`

Öğrenci karnesi detayı.

**Response:**

```json
{
  "success": true,
  "data": {
    "examName": "TYT Deneme-1",
    "examDate": "2026-01-04",
    "studentName": "Ahmet Yılmaz",
    "studentNumber": "2024001",
    "totalScore": 385.5,
    "totalNet": 98.25,
    "classRank": 3,
    "classSize": 40,
    "successPercentage": 75,
    "lessons": [
      {
        "name": "Matematik",
        "correct": 30,
        "wrong": 5,
        "empty": 5,
        "net": 28.75,
        "successRate": 75,
        "topicAnalysis": [
          {
            "topicName": "Fonksiyonlar",
            "correct": 7,
            "wrong": 2,
            "empty": 1,
            "net": 6.5,
            "recommendation": "Bu konuda iyisiniz!"
          },
          {
            "topicName": "Türev",
            "correct": 3,
            "wrong": 5,
            "empty": 2,
            "net": 1.75,
            "recommendation": "Bu konuyu tekrar çalışmalısınız"
          }
        ]
      }
    ],
    "chartData": {
      "labels": ["Matematik", "Fizik", "Kimya", "Biyoloji"],
      "netValues": [28.75, 12.5, 18.0, 15.25],
      "maxValues": [40, 14, 20, 16]
    }
  },
  "error": null,
  "errorCode": null
}
```

**Frontend Kullanımı:**

```javascript
// 1. Karne verisini çek
const response = await fetch(`/api/report/student/${resultId}`, {
  headers: { Authorization: `Bearer ${token}` },
});
const { data } = await response.json();

// 2. Chart.js ile radar grafik oluştur
const ctx = document.getElementById("performanceChart");
new Chart(ctx, {
  type: "radar",
  data: {
    labels: data.chartData.labels,
    datasets: [
      {
        label: "Net",
        data: data.chartData.netValues,
        borderColor: "rgb(54, 162, 235)",
        backgroundColor: "rgba(54, 162, 235, 0.2)",
      },
      {
        label: "Maksimum",
        data: data.chartData.maxValues,
        borderColor: "rgb(255, 99, 132)",
        backgroundColor: "rgba(255, 99, 132, 0.2)",
      },
    ],
  },
});

// 3. Detay tablosu oluştur
data.lessons.forEach((lesson) => {
  // Tablo satırı ekle
  appendLessonRow(lesson);

  // Konu analizi accordion
  lesson.topicAnalysis.forEach((topic) => {
    appendTopicRow(topic);
  });
});

// 4. PDF export butonu
document.getElementById("exportPdf").onclick = async () => {
  const pdfBlob = await fetch(`/api/report/export-pdf/${resultId}`).then((r) =>
    r.blob()
  );
  saveAs(pdfBlob, `karne_${data.studentNumber}.pdf`);
};
```

---

### 4.5. Institution Management (Genişletilmiş)

#### [GET] `/api/institution/my`

Kullanıcının üye olduğu kurumları listele.

**Query Parameters:**

- `forceRefresh`: Cache'i bypass et (default: false)

**Response:**

```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "ABC Dershanesi",
      "role": "Manager",
      "status": "Active",
      "joinedAt": "2025-01-01T10:00:00Z"
    }
  ]
}
```

---

#### [GET] `/api/institution/{id}`

Kurum detaylarını getir.

**Query Parameters:**

- `forceRefresh`: Cache'i bypass et (default: false)

**Response:**

```json
{
  "success": true,
  "data": {
    "id": 1,
    "name": "ABC Dershanesi",
    "licenseNumber": "34-12345",
    "address": "Kadıköy, İstanbul",
    "phone": "0216 123 45 67",
    "status": "Active",
    "subscriptionStartDate": "2025-01-01T00:00:00Z",
    "subscriptionEndDate": "2026-01-01T00:00:00Z",
    "managerName": "Ahmet Yılmaz",
    "totalClassrooms": 12,
    "totalStudents": 350,
    "totalTeachers": 25
  }
}
```

---

#### [PUT] `/api/institution/{id}`

Kurum bilgilerini güncelle (Sadece Manager).

**Request:**

```json
{
  "name": "ABC Dershanesi",
  "address": "Yeni Adres",
  "phone": "0216 999 99 99"
}
```

**Operation Logic:**

```csharp
public async Task<BaseResponse<string>> UpdateInstitutionAsync(int institutionId, UpdateInstitutionRequest request, int currentUserId)
{
    // Yetki kontrolü
    var institutionUser = await _context.InstitutionUsers
        .FirstOrDefaultAsync(iu => iu.UserId == currentUserId && 
                                   iu.InstitutionId == institutionId && 
                                   iu.Role == InstitutionRole.Manager);

    if (institutionUser == null)
        return BaseResponse<string>.ErrorResponse("You don't have permission", ErrorCodes.AuthAccessDenied);

    var institution = await _context.Institutions.FindAsync(institutionId);
    if (institution == null)
        return BaseResponse<string>.ErrorResponse("Institution not found", ErrorCodes.ValidationFailed);

    if (!string.IsNullOrEmpty(request.Name))
        institution.Name = request.Name;

    if (!string.IsNullOrEmpty(request.Address))
        institution.Address = request.Address;

    if (!string.IsNullOrEmpty(request.Phone))
        institution.Phone = request.Phone;

    await _context.SaveChangesAsync();

    // Cache invalidation
    await _cacheService.InvalidateInstitutionCacheAsync(institutionId);

    // Audit log
    await _auditService.LogAsync(currentUserId, "InstitutionUpdated", JsonSerializer.Serialize(new { InstitutionId = institutionId }));

    return BaseResponse<string>.SuccessResponse("Institution updated successfully");
}
```

---

#### [GET] `/api/institution/{id}/members`

Kurum üyelerini listele.

**Query Parameters:**

- `role`: InstitutionRole filtresi (opsiyonel)
- `forceRefresh`: Cache'i bypass et (default: false)

**Response:**

```json
{
  "success": true,
  "data": [
    {
      "id": 10,
      "userId": 105,
      "userName": "ahmet123",
      "fullName": "Ahmet Yılmaz",
      "role": "Teacher",
      "studentNumber": null,
      "employeeNumber": "EMP001",
      "joinedAt": "2025-01-01T10:00:00Z"
    }
  ]
}
```

---

#### [POST] `/api/institution/{id}/add-member`

Kuruma üye ekleme (Sadece Manager).

**Request:**

```json
{
  "userId": 105,
  "role": "Teacher",
  "number": "EMP001"
}
```

**Operation Logic:**

```csharp
public async Task<BaseResponse<string>> AddMemberAsync(int institutionId, AddMemberRequest request, int currentUserId)
{
    // Yetki kontrolü
    var institutionUser = await _context.InstitutionUsers
        .FirstOrDefaultAsync(iu => iu.UserId == currentUserId && 
                                   iu.InstitutionId == institutionId && 
                                   iu.Role == InstitutionRole.Manager);

    if (institutionUser == null)
        return BaseResponse<string>.ErrorResponse("You don't have permission", ErrorCodes.AuthAccessDenied);

    // Zaten üye mi?
    var existingMember = await _context.InstitutionUsers
        .AnyAsync(iu => iu.UserId == request.UserId && iu.InstitutionId == institutionId);

    if (existingMember)
        return BaseResponse<string>.ErrorResponse("User is already a member", ErrorCodes.ValidationFailed);

    var newMember = new InstitutionUser
    {
        UserId = request.UserId,
        InstitutionId = institutionId,
        Role = request.Role,
        JoinedAt = DateTime.UtcNow
    };

    if (request.Role == InstitutionRole.Student)
        newMember.StudentNumber = request.Number;
    else if (request.Role == InstitutionRole.Teacher)
        newMember.EmployeeNumber = request.Number;

    _context.InstitutionUsers.Add(newMember);
    await _context.SaveChangesAsync();

    // Cache invalidation
    await _cacheService.InvalidateInstitutionCacheAsync(institutionId);

    // Audit log
    await _auditService.LogAsync(currentUserId, "MemberAdded", JsonSerializer.Serialize(new { InstitutionId = institutionId, UserId = request.UserId }));

    return BaseResponse<string>.SuccessResponse("Member added successfully");
}
```

---

#### [DELETE] `/api/institution/{id}/member/{memberId}`

Kurumdan üye çıkarma (Sadece Manager).

**Operation Logic:**

```csharp
public async Task<BaseResponse<string>> RemoveMemberAsync(int institutionId, int memberId, int currentUserId)
{
    // Yetki kontrolü
    var institutionUser = await _context.InstitutionUsers
        .FirstOrDefaultAsync(iu => iu.UserId == currentUserId && 
                                   iu.InstitutionId == institutionId && 
                                   iu.Role == InstitutionRole.Manager);

    if (institutionUser == null)
        return BaseResponse<string>.ErrorResponse("You don't have permission", ErrorCodes.AuthAccessDenied);

    var member = await _context.InstitutionUsers
        .FirstOrDefaultAsync(iu => iu.Id == memberId && iu.InstitutionId == institutionId);

    if (member == null)
        return BaseResponse<string>.ErrorResponse("Member not found", ErrorCodes.ValidationFailed);

    // Manager kendini çıkaramaz
    if (member.UserId == currentUserId)
        return BaseResponse<string>.ErrorResponse("You cannot remove yourself", ErrorCodes.ValidationFailed);

    _context.InstitutionUsers.Remove(member);
    await _context.SaveChangesAsync();

    // Cache invalidation
    await _cacheService.InvalidateInstitutionCacheAsync(institutionId);

    // Audit log
    await _auditService.LogAsync(currentUserId, "MemberRemoved", JsonSerializer.Serialize(new { InstitutionId = institutionId, MemberId = memberId }));

    return BaseResponse<string>.SuccessResponse("Member removed successfully");
}
```

---

#### [PUT] `/api/institution/{id}/member/{memberId}/role`

Üye rolünü güncelleme (Sadece Manager).

**Request:**

```json
{
  "role": "Teacher"
}
```

---

#### [GET] `/api/institution/{id}/statistics`

Kurum istatistiklerini getir.

**Query Parameters:**

- `forceRefresh`: Cache'i bypass et (default: false)

**Response:**

```json
{
  "success": true,
  "data": {
    "totalClassrooms": 12,
    "totalStudents": 350,
    "totalTeachers": 25,
    "totalExams": 50,
    "totalReports": 200,
    "averageExamScore": 78.5,
    "activeMembers": 375
  }
}
```

---

### 4.6. Classroom Management (Genişletilmiş)

#### [GET] `/api/classroom/{id}`

Sınıf detaylarını getir.

**Query Parameters:**

- `forceRefresh`: Cache'i bypass et (default: false)

**Response:**

```json
{
  "success": true,
  "data": {
    "id": 5,
    "name": "12-A",
    "grade": 12,
    "institutionName": "ABC Dershanesi",
    "headTeacherName": "Mehmet Öğretmen",
    "totalStudents": 30,
    "totalTeachers": 5,
    "createdAt": "2025-01-01T10:00:00Z"
  }
}
```

---

#### [GET] `/api/classroom/institution/{institutionId}`

Kurumun tüm sınıflarını listele.

**Query Parameters:**

- `forceRefresh`: Cache'i bypass et (default: false)

---

#### [PUT] `/api/classroom/{id}`

Sınıf bilgilerini güncelle (Sadece Manager).

**Request:**

```json
{
  "name": "12-B",
  "grade": 12
}
```

---

#### [DELETE] `/api/classroom/{id}`

Sınıfı silme (Sadece Manager).

**Operation Logic:**

```csharp
public async Task<BaseResponse<string>> DeleteClassroomAsync(int classroomId, int currentUserId)
{
    // Yetki kontrolü
    var classroom = await _context.Classrooms
        .Include(c => c.Institution)
        .FirstOrDefaultAsync(c => c.Id == classroomId);

    if (classroom == null)
        return BaseResponse<string>.ErrorResponse("Classroom not found", ErrorCodes.ValidationFailed);

    var institutionUser = await _context.InstitutionUsers
        .FirstOrDefaultAsync(iu => iu.UserId == currentUserId && 
                                   iu.InstitutionId == classroom.InstitutionId && 
                                   iu.Role == InstitutionRole.Manager);

    if (institutionUser == null)
        return BaseResponse<string>.ErrorResponse("You don't have permission", ErrorCodes.AuthAccessDenied);

    // Soft delete
    classroom.IsActive = false;
    await _context.SaveChangesAsync();

    // Cache invalidation
    await _cacheService.InvalidateClassroomCacheAsync(classroomId);

    // Audit log
    await _auditService.LogAsync(currentUserId, "ClassroomDeleted", JsonSerializer.Serialize(new { ClassroomId = classroomId }));

    return BaseResponse<string>.SuccessResponse("Classroom deleted successfully");
}
```

---

#### [DELETE] `/api/classroom/{classroomId}/student/{studentId}`

Sınıftan öğrenci çıkarma (Sadece Manager veya Teacher).

**Operation Logic:**

```csharp
public async Task<BaseResponse<string>> RemoveStudentFromClassroomAsync(int classroomId, int studentId, int currentUserId)
{
    // Yetki kontrolü
    var classroom = await _context.Classrooms
        .Include(c => c.Institution)
        .FirstOrDefaultAsync(c => c.Id == classroomId);

    if (classroom == null)
        return BaseResponse<string>.ErrorResponse("Classroom not found", ErrorCodes.ValidationFailed);

    var hasPermission = await _context.InstitutionUsers
        .AnyAsync(iu => iu.UserId == currentUserId && 
                       iu.InstitutionId == classroom.InstitutionId && 
                       (iu.Role == InstitutionRole.Manager || iu.Role == InstitutionRole.Teacher));

    if (!hasPermission)
        return BaseResponse<string>.ErrorResponse("You don't have permission", ErrorCodes.AuthAccessDenied);

    var classroomStudent = await _context.ClassroomStudents
        .Include(cs => cs.Student)
        .FirstOrDefaultAsync(cs => cs.ClassroomId == classroomId && cs.Student.UserId == studentId);

    if (classroomStudent == null)
        return BaseResponse<string>.ErrorResponse("Student not found in classroom", ErrorCodes.ValidationFailed);

    // Soft delete
    classroomStudent.RemovedAt = DateTime.UtcNow;
    await _context.SaveChangesAsync();

    // Cache invalidation
    await _cacheService.InvalidateClassroomCacheAsync(classroomId);

    // Audit log
    await _auditService.LogAsync(currentUserId, "StudentRemovedFromClassroom", 
        JsonSerializer.Serialize(new { ClassroomId = classroomId, StudentId = studentId }));

    return BaseResponse<string>.SuccessResponse("Student removed from classroom successfully");
}
```

---

#### [GET] `/api/classroom/{classroomId}/students`

Sınıf öğrencilerini listele.

**Query Parameters:**

- `forceRefresh`: Cache'i bypass et (default: false)

**Response:**

```json
{
  "success": true,
  "data": [
    {
      "id": 10,
      "userId": 105,
      "fullName": "Ahmet Yılmaz",
      "studentNumber": "2024001",
      "assignedAt": "2025-01-01T10:00:00Z"
    }
  ]
}
```

---

### 4.7. Exam Management (Genişletilmiş)

#### [GET] `/api/exam`

Sınavları listele (filtreleme ve pagination).

**Query Parameters:**

- `institutionId`: Kurum ID filtresi (opsiyonel)
- `classroomId`: Sınıf ID filtresi (opsiyonel)
- `type`: ExamType filtresi (opsiyonel)
- `dateFrom`: Başlangıç tarihi (opsiyonel)
- `dateTo`: Bitiş tarihi (opsiyonel)
- `page`: Sayfa numarası (default: 1)
- `limit`: Sayfa başına kayıt (default: 20)
- `forceRefresh`: Cache'i bypass et (default: false)

**Response:**

```json
{
  "success": true,
  "data": [
    {
      "id": 5,
      "name": "TYT Deneme-1",
      "date": "2026-01-10T09:00:00Z",
      "type": "TYT",
      "institutionName": "ABC Dershanesi",
      "classroomName": "12-A",
      "totalStudents": 30,
      "isPublished": true,
      "createdAt": "2026-01-05T10:00:00Z"
    }
  ]
}
```

---

#### [GET] `/api/exam/{id}`

Sınav detaylarını getir.

**Query Parameters:**

- `forceRefresh`: Cache'i bypass et (default: false)

**Response:**

```json
{
  "success": true,
  "data": {
    "id": 5,
    "name": "TYT Deneme-1",
    "date": "2026-01-10T09:00:00Z",
    "type": "TYT",
    "institutionName": "ABC Dershanesi",
    "classroomName": "12-A",
    "answerKey": {
      "Matematik": "ABCDEABCDE...",
      "Fizik": "BCDABCDA..."
    },
    "totalStudents": 30,
    "processedResults": 28,
    "isPublished": true,
    "createdAt": "2026-01-05T10:00:00Z"
  }
}
```

---

#### [GET] `/api/exam/{id}/results`

Sınav sonuçlarını listele (öğretmen için).

**Query Parameters:**

- `page`: Sayfa numarası (default: 1)
- `limit`: Sayfa başına kayıt (default: 20)
- `forceRefresh`: Cache'i bypass et (default: false)

**Response:**

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 101,
        "studentName": "Ahmet Yılmaz",
        "studentNumber": "2024001",
        "totalNet": 98.25,
        "totalScore": 385.50,
        "classRank": 1,
        "institutionRank": 5,
        "isConfirmed": true,
        "createdAt": "2026-01-10T10:30:00Z"
      }
    ],
    "totalCount": 30,
    "page": 1,
    "limit": 20
  }
}
```

---

### 4.8. Report Management (Genişletilmiş)

#### [GET] `/api/report/student/{resultId}`

Öğrenci karnesi detayı.

**Query Parameters:**

- `forceRefresh`: Cache'i bypass et (default: false)

**Response:**

```json
{
  "success": true,
  "data": {
    "id": 101,
    "examName": "TYT Deneme-1",
    "examDate": "2026-01-10T09:00:00Z",
    "studentName": "Ahmet Yılmaz",
    "studentNumber": "2024001",
    "totalNet": 98.25,
    "totalScore": 385.50,
    "classRank": 1,
    "institutionRank": 5,
    "lessons": [
      {
        "name": "Matematik",
        "correct": 30,
        "wrong": 5,
        "empty": 5,
        "net": 28.75,
        "topicScores": [
          {
            "topicName": "Fonksiyonlar",
            "correct": 8,
            "wrong": 1,
            "empty": 1,
            "net": 7.75
          }
        ]
      }
    ],
    "createdAt": "2026-01-10T10:30:00Z"
  }
}
```

---

#### [GET] `/api/report/student/{studentId}/all`

Öğrencinin tüm karnelerini listele.

**Query Parameters:**

- `page`: Sayfa numarası (default: 1)
- `limit`: Sayfa başına kayıt (default: 20)
- `forceRefresh`: Cache'i bypass et (default: false)

**Response:**

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 101,
        "examName": "TYT Deneme-1",
        "examDate": "2026-01-10T09:00:00Z",
        "totalNet": 98.25,
        "totalScore": 385.50,
        "classRank": 1,
        "createdAt": "2026-01-10T10:30:00Z"
      }
    ],
    "totalCount": 12,
    "page": 1,
    "limit": 20
  }
}
```

---

#### [GET] `/api/report/classroom/{classroomId}`

Sınıf karnelerini listele (öğretmen için).

**Query Parameters:**

- `examId`: Sınav ID filtresi (opsiyonel)
- `page`: Sayfa numarası (default: 1)
- `limit`: Sayfa başına kayıt (default: 20)
- `forceRefresh`: Cache'i bypass et (default: false)

**Response:**

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 101,
        "studentName": "Ahmet Yılmaz",
        "studentNumber": "2024001",
        "totalNet": 98.25,
        "totalScore": 385.50,
        "classRank": 1,
        "isConfirmed": true
      }
    ],
    "totalCount": 30,
    "page": 1,
    "limit": 20
  }
}
```

---

### 4.9. Message Management (Genişletilmiş)

#### [POST] `/api/message/start`

Yeni bir konuşma başlatma.

**Request:**

```json
{
  "recipientId": 106,
  "text": "Merhaba, nasılsın?"
}
```

**Operation Logic:**

```csharp
public async Task<BaseResponse<int>> StartConversationAsync(int currentUserId, StartConversationRequest request)
{
    // 1. Mevcut konuşma var mı?
    var existingConversation = await _context.Conversations
        .Where(c => c.Type == ConversationType.Private)
        .Where(c => c.Members.Any(m => m.UserId == currentUserId) && 
                   c.Members.Any(m => m.UserId == request.RecipientId))
        .FirstOrDefaultAsync();

    if (existingConversation != null)
    {
        // Mevcut konuşmaya mesaj gönder
        var message = new Message
        {
            ConversationId = existingConversation.Id,
            SenderId = currentUserId,
            Text = request.Text,
            Type = MessageType.Text,
            SentAt = DateTime.UtcNow
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        // SignalR ile gönder
        await _chatHub.Clients.Group($"Conversation_{existingConversation.Id}")
            .SendAsync("ReceiveMessage", MapToMessageDto(message));

        return BaseResponse<int>.SuccessResponse(existingConversation.Id);
    }

    // 2. Yeni konuşma oluştur
    var conversation = new Conversation
    {
        Type = ConversationType.Private,
        CreatedAt = DateTime.UtcNow
    };

    _context.Conversations.Add(conversation);
    await _context.SaveChangesAsync();

    // 3. Üyeleri ekle
    _context.ConversationMembers.AddRange(new[]
    {
        new ConversationMember { ConversationId = conversation.Id, UserId = currentUserId, JoinedAt = DateTime.UtcNow },
        new ConversationMember { ConversationId = conversation.Id, UserId = request.RecipientId, JoinedAt = DateTime.UtcNow }
    });

    // 4. İlk mesajı gönder
    var firstMessage = new Message
    {
        ConversationId = conversation.Id,
        SenderId = currentUserId,
        Text = request.Text,
        Type = MessageType.Text,
        SentAt = DateTime.UtcNow
    };

    _context.Messages.Add(firstMessage);
    await _context.SaveChangesAsync();

    // Cache invalidation
    await _cacheService.InvalidateConversationCacheAsync(currentUserId);
    await _cacheService.InvalidateConversationCacheAsync(request.RecipientId);

    return BaseResponse<int>.SuccessResponse(conversation.Id);
}
```

---

#### [GET] `/api/message/conversations`

Kullanıcının konuşmalarını listele.

**Query Parameters:**

- `page`: Sayfa numarası (default: 1)
- `limit`: Sayfa başına kayıt (default: 20)
- `forceRefresh`: Cache'i bypass et (default: false)

**Response:**

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 25,
        "type": "Private",
        "name": null,
        "lastMessage": {
          "text": "Yarınki sınav saat kaçta?",
          "senderName": "Ahmet Yılmaz",
          "sentAt": "2026-01-05T14:30:00Z"
        },
        "unreadCount": 2,
        "participants": [
          {
            "id": 105,
            "fullName": "Ahmet Yılmaz",
            "profileImageUrl": "https://..."
          }
        ]
      }
    ],
    "totalCount": 15,
    "page": 1,
    "limit": 20
  }
}
```

---

#### [GET] `/api/message/conversation/{id}`

Konuşma detaylarını getir.

**Query Parameters:**

- `forceRefresh`: Cache'i bypass et (default: false)

**Response:**

```json
{
  "success": true,
  "data": {
    "id": 25,
    "type": "Private",
    "name": null,
    "participants": [
      {
        "id": 105,
        "fullName": "Ahmet Yılmaz",
        "profileImageUrl": "https://..."
      }
    ],
    "messages": [
      {
        "id": 501,
        "text": "Yarınki sınav saat kaçta?",
        "senderId": 105,
        "senderName": "Ahmet Yılmaz",
        "type": "Text",
        "sentAt": "2026-01-05T14:30:00Z"
      }
    ]
  }
}
```

---

#### [GET] `/api/message/history/{conversationId}`

Konuşma mesaj geçmişini getir (pagination).

**Query Parameters:**

- `page`: Sayfa numarası (default: 1)
- `limit`: Sayfa başına kayıt (default: 50)
- `forceRefresh`: Cache'i bypass et (default: false)

---

#### [PUT] `/api/message/conversation/{id}`

Konuşma bilgilerini güncelle (örn: isim değiştirme).

**Request:**

```json
{
  "name": "Özel Grup"
}
```

---

#### [DELETE] `/api/message/conversation/{id}`

Konuşmayı silme (soft delete).

---

#### [POST] `/api/message/conversation/{id}/leave`

Konuşmadan ayrılma (grup konuşmaları için).

---

#### [DELETE] `/api/message/{id}`

Mesajı silme (soft delete).

**Operation Logic:**

```csharp
public async Task<BaseResponse<string>> DeleteMessageAsync(int messageId, int currentUserId)
{
    var message = await _context.Messages
        .Include(m => m.Conversation)
            .ThenInclude(c => c.Members)
        .FirstOrDefaultAsync(m => m.Id == messageId);

    if (message == null)
        return BaseResponse<string>.ErrorResponse("Message not found", ErrorCodes.ValidationFailed);

    // Sadece mesaj sahibi silebilir
    if (message.SenderId != currentUserId)
        return BaseResponse<string>.ErrorResponse("You don't have permission", ErrorCodes.AuthAccessDenied);

    // Soft delete
    message.IsDeleted = true;
    message.DeletedAt = DateTime.UtcNow;
    await _context.SaveChangesAsync();

    // Cache invalidation
    await _cacheService.InvalidateConversationCacheAsync(message.ConversationId);

    // SignalR ile bildirim
    await _chatHub.Clients.Group($"Conversation_{message.ConversationId}")
        .SendAsync("MessageDeleted", messageId);

    return BaseResponse<string>.SuccessResponse("Message deleted successfully");
}
```

---

#### [PUT] `/api/message/{id}`

Mesajı düzenleme.

**Request:**

```json
{
  "text": "Düzenlenmiş mesaj metni"
}
```

---

#### [POST] `/api/message/conversation/{id}/mark-read`

Konuşmayı okundu olarak işaretleme.

**Operation Logic:**

```csharp
public async Task<BaseResponse<string>> MarkConversationAsReadAsync(int conversationId, int currentUserId)
{
    var member = await _context.ConversationMembers
        .FirstOrDefaultAsync(cm => cm.ConversationId == conversationId && cm.UserId == currentUserId);

    if (member == null)
        return BaseResponse<string>.ErrorResponse("You are not a member of this conversation", ErrorCodes.ValidationFailed);

    member.LastReadAt = DateTime.UtcNow;
    await _context.SaveChangesAsync();

    // Cache invalidation
    await _cacheService.InvalidateConversationCacheAsync(currentUserId);

    return BaseResponse<string>.SuccessResponse("Conversation marked as read");
}
```

---

#### [GET] `/api/message/search`

Mesajlarda arama.

**Query Parameters:**

- `query`: Arama metni
- `conversationId`: Konuşma ID filtresi (opsiyonel)
- `page`: Sayfa numarası (default: 1)
- `limit`: Sayfa başına kayıt (default: 20)
- `forceRefresh`: Cache'i bypass et (default: false)

---

### 4.10. Notification Management (Genişletilmiş)

#### [POST] `/api/notification/mark-read/{id}`

Bildirimi okundu olarak işaretleme.

---

#### [POST] `/api/notification/mark-all-read`

Tüm bildirimleri okundu olarak işaretleme.

**Operation Logic:**

```csharp
public async Task<BaseResponse<string>> MarkAllAsReadAsync(int userId)
{
    var unreadNotifications = await _context.Notifications
        .Where(n => n.UserId == userId && !n.IsRead)
        .ToListAsync();

    foreach (var notification in unreadNotifications)
    {
        notification.IsRead = true;
    }

    await _context.SaveChangesAsync();

    // Cache invalidation
    await _cacheService.InvalidateUserCacheAsync(userId);

    return BaseResponse<string>.SuccessResponse($"{unreadNotifications.Count} notifications marked as read");
}
```

---

#### [DELETE] `/api/notification/{id}`

Bildirimi silme.

---

#### [DELETE] `/api/notification/clear-all`

Tüm bildirimleri silme.

---

#### [GET] `/api/notification/settings`

Bildirim ayarlarını getir.

**Response:**

```json
{
  "success": true,
  "data": {
    "emailNotifications": true,
    "pushNotifications": true,
    "messageNotifications": true,
    "reportCardNotifications": true,
    "examNotifications": true,
    "accountLinkNotifications": true
  }
}
```

---

#### [PUT] `/api/notification/settings`

Bildirim ayarlarını güncelle.

**Request:**

```json
{
  "emailNotifications": false,
  "pushNotifications": true,
  "messageNotifications": true
}
```

---

#### [GET] `/api/notification/my` (Genişletilmiş)

Bildirimleri listele (filtreleme ve pagination).

**Query Parameters:**

- `page`: Sayfa numarası (default: 1)
- `limit`: Sayfa başına kayıt (default: 20)
- `type`: NotificationType filtresi (opsiyonel)
- `isRead`: Okundu filtresi (opsiyonel)
- `dateFrom`: Başlangıç tarihi (opsiyonel)
- `dateTo`: Bitiş tarihi (opsiyonel)
- `forceRefresh`: Cache'i bypass et (default: false)

**Response:**

```json
{
  "success": true,
  "data": {
    "unreadCount": 3,
    "items": [
      {
        "id": 501,
        "title": "Yeni Karne",
        "message": "Matematik sınavı karneniz hazır",
        "type": "ReportCard",
        "actionUrl": "/report/123",
        "isRead": false,
        "createdAt": "2026-01-04T14:30:00Z"
      }
    ],
    "totalCount": 50,
    "page": 1,
    "limit": 20
  }
}
```

---

### 4.11. Search Controller (YENİ)

Genel arama endpoint'leri.

#### [GET] `/api/search/users`

Kullanıcı arama.

**Query Parameters:**

- `query`: Arama metni
- `role`: UserRole filtresi (opsiyonel)
- `page`: Sayfa numarası (default: 1)
- `limit`: Sayfa başına kayıt (default: 20)
- `forceRefresh`: Cache'i bypass et (default: false)

---

#### [GET] `/api/search/institutions`

Kurum arama.

**Query Parameters:**

- `query`: Arama metni
- `status`: InstitutionStatus filtresi (opsiyonel)
- `page`: Sayfa numarası (default: 1)
- `limit`: Sayfa başına kayıt (default: 20)
- `forceRefresh`: Cache'i bypass et (default: false)

---

#### [GET] `/api/search/classrooms`

Sınıf arama.

**Query Parameters:**

- `query`: Arama metni
- `institutionId`: Kurum ID filtresi (opsiyonel)
- `grade`: Sınıf seviyesi filtresi (opsiyonel)
- `page`: Sayfa numarası (default: 1)
- `limit`: Sayfa başına kayıt (default: 20)
- `forceRefresh`: Cache'i bypass et (default: false)

---

#### [GET] `/api/search/exams`

Sınav arama.

**Query Parameters:**

- `query`: Arama metni
- `institutionId`: Kurum ID filtresi (opsiyonel)
- `type`: ExamType filtresi (opsiyonel)
- `dateFrom`: Başlangıç tarihi (opsiyonel)
- `dateTo`: Bitiş tarihi (opsiyonel)
- `page`: Sayfa numarası (default: 1)
- `limit`: Sayfa başına kayıt (default: 20)
- `forceRefresh`: Cache'i bypass et (default: false)

---

### 4.12. Cache Stratejisi ve Force Refresh

Tüm `GET` endpoint'lerinde cache kullanımı ve `forceRefresh` mekanizması:

**Cache Süreleri:**

- User Profile: 15 dakika
- User Statistics: 10 dakika
- User Preferences: 30 dakika
- Admin Statistics: 5 dakika
- Institution Details: 5 dakika
- Classroom Details: 15 dakika
- Exam List: 2 dakika
- Conversations: 1 dakika
- Notifications: 5 dakika
- Search Results: 5 dakika

**Cache Invalidation:**

- Tüm `Create`, `Update`, `Delete` işlemlerinde ilgili cache'ler temizlenir
- Pattern-based cache removal kullanılır
- Specific invalidation method'ları kullanılır (`InvalidateUserCacheAsync`, `InvalidateAdminCacheAsync`, vb.)

**Force Refresh Kullanımı:**

Frontend'de "Yenile" butonu veya kullanıcı isteği ile `forceRefresh=true` parametresi gönderilir:

```javascript
// Normal istek (cache'den gelir)
const response = await fetch('/api/user/profile');

// Force refresh (cache bypass)
const response = await fetch('/api/user/profile?forceRefresh=true');
```

---

## 🌍 5. FAZ 3: SOSYAL AĞ VE KEŞFET (Social Network & Discovery)

Bu fazda bireysel kullanıcılar (bağımsız öğretmen ve öğrenciler) soru paylaşabilir, birbirlerini takip edebilir ve keşfedebilir.

### 5.1. İçerik Paylaşımı

#### [Model] Lesson (Ders)

Önceden tanımlı dersler (seed data).

```csharp
public class Lesson
{
    public int Id { get; set; }
    public string Name { get; set; } // "Matematik", "Fizik"
    public int DisplayOrder { get; set; }
}
```

#### [Model] Topic (Konu)

Her dersin konuları.

```csharp
public class Topic
{
    public int Id { get; set; }
    public int LessonId { get; set; }
    public Lesson Lesson { get; set; }

    public string Name { get; set; } // "Fonksiyonlar", "Türev"
}
```

#### [Model] Content (Paylaşım/Soru)

```csharp
public class Content
{
    public int Id { get; set; }
    public int AuthorId { get; set; }
    public User Author { get; set; }

    public ContentType Type { get; set; } // Question, Post, Announcement, Exam
    public string Title { get; set; } // "Zorlu Türev Sorusu"
    public string Description { get; set; } // Soru metni veya açıklama
    public string? ImageUrl { get; set; } // Soru görseli

    // Kategorileme
    public int? LessonId { get; set; }
    public Lesson Lesson { get; set; }

    public int? TopicId { get; set; }
    public Topic Topic { get; set; }

    public DifficultyLevel Difficulty { get; set; } // Easy, Medium, Hard

    // Tags (JSON array): ["#zor", "#tyt", "#2024"]
    public string TagsJson { get; set; }

    // Cevap (eğer varsa)
    public string? AnswerText { get; set; }
    public string? AnswerImageUrl { get; set; }

    // Denormalized counts (performans için - Redis'ten güncellenir)
    public int LikeCount { get; set; } = 0;
    public int CommentCount { get; set; } = 0;
    public int ViewCount { get; set; } = 0;
    public int ShareCount { get; set; } = 0;

    public bool IsSolved { get; set; } = false; // Soru çözüldü mü?
    public DateTime CreatedAt { get; set; }

    // Navigation
    public ICollection<Comment> Comments { get; set; }
    public ICollection<Interaction> Interactions { get; set; }
}
```

#### [Enum] ContentType

```csharp
public enum ContentType : byte
{
    Question = 1,      // Soru
    Post = 2,          // Genel paylaşım
    Announcement = 3,  // Duyuru
    Exam = 4           // Sınav paylaşımı
}
```

#### [Enum] DifficultyLevel

```csharp
public enum DifficultyLevel : byte
{
    Easy = 1,
    Medium = 2,
    Hard = 3
}
```

#### [Model] Comment (Yorum)

```csharp
public class Comment
{
    public int Id { get; set; }
    public int ContentId { get; set; }
    public Content Content { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public string Text { get; set; }
    public string? ImageUrl { get; set; } // Çözüm görseli

    public bool IsCorrectAnswer { get; set; } = false; // Soru sahibi işaretler
    public DateTime CreatedAt { get; set; }
}
```

#### [Model] Interaction (Etkileşim)

```csharp
public class Interaction
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }

    public int ContentId { get; set; }
    public Content Content { get; set; }

    public InteractionType Type { get; set; } // Like, Save, Report
    public DateTime CreatedAt { get; set; }
}
```

#### [Enum] InteractionType

```csharp
public enum InteractionType : byte
{
    Like = 1,
    Save = 2,
    Report = 3
}
```

#### [Model] Follow (Takip)

```csharp
public class Follow
{
    public int Id { get; set; }
    public int FollowerId { get; set; } // Takip eden
    public User Follower { get; set; }

    public int FollowingId { get; set; } // Takip edilen
    public User Following { get; set; }

    public DateTime CreatedAt { get; set; }
}
```

#### Endpoints

#### [POST] `/api/social/content/create`

Soru/içerik paylaşma.

**Request:**

```json
{
  "type": "Question",
  "title": "Fonksiyon Grafiği Sorusu",
  "description": "f(x) = x² - 4x + 3 fonksiyonunun grafiğini çiziniz",
  "imageUrl": "https://cdn.../question123.jpg",
  "lessonId": 1,
  "topicId": 5,
  "difficulty": "Medium",
  "tags": ["#fonksiyon", "#grafik", "#tyt"],
  "answerText": "Kökleri: x=1, x=3. Tepe noktası: (2, -1)"
}
```

**Operation Logic:**

```csharp
public static async Task<BaseResponse<int>> CreateContentAsync(
    CreateContentRequest request,
    int currentUserId,
    ApplicationContext context)
{
    // 1. Content oluştur
    var content = new Content
    {
        AuthorId = currentUserId,
        Type = request.Type,
        Title = request.Title,
        Description = request.Description,
        ImageUrl = request.ImageUrl,
        LessonId = request.LessonId,
        TopicId = request.TopicId,
        Difficulty = request.Difficulty,
        TagsJson = JsonSerializer.Serialize(request.Tags),
        AnswerText = request.AnswerText,
        AnswerImageUrl = request.AnswerImageUrl,
        CreatedAt = DateTime.UtcNow
    };

    context.Contents.Add(content);
    await context.SaveChangesAsync();

    // 2. Redis'te index oluştur (RediSearch için)
    await RedisSearchHelper.IndexContentAsync(content);

    // 3. Takipçilere bildirim gönder
    var followers = await context.Follows
        .Where(f => f.FollowingId == currentUserId)
        .Select(f => f.FollowerId)
        .ToListAsync();

    foreach (var followerId in followers)
    {
        await NotificationService.SendAsync(
            followerId,
            "Yeni İçerik",
            $"{content.Author.FullName} yeni bir soru paylaştı",
            $"/content/{content.Id}"
        );
    }

    // 4. Audit log
    await AuditService.LogAsync(currentUserId, "ContentCreated",
        JsonSerializer.Serialize(new { ContentId = content.Id, Type = content.Type }));

    return BaseResponse<int>.Success(content.Id);
}
```

---

### 5.2. Feed Algoritması (Kişisel Akış)

Kullanıcının ana sayfasında göreceği içerikler akıllı bir algoritma ile seçilir.

#### [GET] `/api/social/feed`

Kullanıcının kişisel feed'i.

**Query Parameters:**

- `page`: Sayfa numarası (default: 1)
- `limit`: Sayfa başına kayıt (default: 20, max: 50)

**Operation Logic:**

```csharp
public static async Task<BaseResponse<List<ContentDto>>> GetFeedAsync(
    int userId,
    int page,
    int limit,
    ApplicationContext context,
    ICacheService cache)
{
    // 1. Cache kontrolü
    var cacheKey = $"User:{userId}:Feed:Page{page}";
    var cachedFeed = await cache.GetAsync<List<ContentDto>>(cacheKey);

    if (cachedFeed != null)
        return BaseResponse<List<ContentDto>>.Success(cachedFeed);

    // 2. Kullanıcının takip ettiklerini al
    var followingIds = await context.Follows
        .Where(f => f.FollowerId == userId)
        .Select(f => f.FollowingId)
        .ToListAsync();

    // 3. Kullanıcının ilgi alanlarını tespit et (geçmişte like/save ettiği dersler)
    var interestedLessons = await context.Interactions
        .Where(i => i.UserId == userId && (i.Type == InteractionType.Like || i.Type == InteractionType.Save))
        .Select(i => i.Content.LessonId)
        .Distinct()
        .ToListAsync();

    // 4. Aday içerikleri çek (son 7 gün)
    var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
    var candidateContents = await context.Contents
        .Where(c => c.CreatedAt >= sevenDaysAgo)
        .Include(c => c.Author)
        .Include(c => c.Lesson)
        .Include(c => c.Topic)
        .ToListAsync();

    // 5. Her içerik için skor hesapla
    var scoredContents = candidateContents.Select(content => new
    {
        Content = content,
        Score = CalculateFeedScore(content, userId, followingIds, interestedLessons)
    }).ToList();

    // 6. Skora göre sırala ve sayfalama
    var feed = scoredContents
        .OrderByDescending(sc => sc.Score)
        .Skip((page - 1) * limit)
        .Take(limit)
        .Select(sc => MapToContentDto(sc.Content))
        .ToList();

    // 7. Cache'e yaz (10 dakika)
    await cache.SetAsync(cacheKey, feed, TimeSpan.FromMinutes(10));

    return BaseResponse<List<ContentDto>>.Success(feed);
}
```

#### Feed Scoring Algoritması

```csharp
private static double CalculateFeedScore(
    Content content,
    int currentUserId,
    List<int> followingIds,
    List<int?> interestedLessons)
{
    double score = 0;

    // 1. Takip Edilen Kullanıcı Bonusu (+100)
    if (followingIds.Contains(content.AuthorId))
        score += 100;

    // 2. İlgi Alanı Bonusu (+50)
    if (content.LessonId.HasValue && interestedLessons.Contains(content.LessonId))
        score += 50;

    // 3. Popülerlik Puanı
    score += content.LikeCount * 1.5;
    score += content.CommentCount * 2.0;
    score += content.ViewCount * 0.1;
    score += content.ShareCount * 3.0;

    // 4. Yenilik Bonusu (Recency)
    var hoursSinceCreation = (DateTime.UtcNow - content.CreatedAt).TotalHours;
    if (hoursSinceCreation <= 24)
        score += 50 - hoursSinceCreation; // 24 saat içinde azalan bonus
    else if (hoursSinceCreation <= 48)
        score += 20;

    // 5. Zorluk Dengeleme (Zor sorular biraz daha fazla öne çıksın)
    if (content.Type == ContentType.Question && content.Difficulty == DifficultyLevel.Hard)
        score += 10;

    // 6. Çözülmemiş soru bonusu
    if (content.Type == ContentType.Question && !content.IsSolved)
        score += 15;

    return score;
}
```

**Frontend Kullanımı (Infinite Scroll):**

```javascript
let currentPage = 1;
let isLoading = false;

async function loadFeed() {
  if (isLoading) return;
  isLoading = true;

  const response = await fetch(
    `/api/social/feed?page=${currentPage}&limit=20`,
    {
      headers: { Authorization: `Bearer ${token}` },
    }
  );
  const { data } = await response.json();

  data.forEach((content) => {
    appendContentCard(content);
  });

  currentPage++;
  isLoading = false;
}

// Infinite scroll
window.addEventListener("scroll", () => {
  if (window.innerHeight + window.scrollY >= document.body.offsetHeight - 500) {
    loadFeed();
  }
});

// İlk yükleme
loadFeed();
```

---

### 5.3. Keşfet ve Arama (Discovery & Search)

#### [GET] `/api/social/discover`

Global keşfet sayfası - filtrelerle arama.

**Query Parameters:**

- `lesson`: Ders ID (opsiyonel)
- `topic`: Konu ID (opsiyonel)
- `difficulty`: Zorluk seviyesi (opsiyonel)
- `search`: Arama metni (opsiyonel)
- `sortBy`: `popular`, `recent`, `trending` (default: popular)

**Redis RediSearch Kullanımı:**

**1. Index Oluşturma (Startup sırasında):**

```csharp
public static async Task CreateContentIndexAsync(IDatabase redis)
{
    await redis.ExecuteAsync("FT.CREATE", "contentIdx",
        "ON", "JSON",
        "PREFIX", "1", "content:",
        "SCHEMA",
        "$.title", "AS", "title", "TEXT",
        "$.description", "AS", "description", "TEXT",
        "$.tags", "AS", "tags", "TAG",
        "$.lessonId", "AS", "lessonId", "NUMERIC",
        "$.topicId", "AS", "topicId", "NUMERIC",
        "$.difficulty", "AS", "difficulty", "NUMERIC",
        "$.likeCount", "AS", "likeCount", "NUMERIC",
        "$.createdAt", "AS", "createdAt", "NUMERIC"
    );
}
```

**2. İçerik Arama:**

```csharp
public static async Task<List<Content>> SearchContentsAsync(
    string searchTerm,
    int? lessonId,
    int? topicId,
    DifficultyLevel? difficulty,
    string sortBy,
    IDatabase redis,
    ApplicationContext context)
{
    // RediSearch query oluştur
    var query = "*"; // Tüm içerikler

    if (!string.IsNullOrEmpty(searchTerm))
        query = $"@title|description:{searchTerm}";

    var filters = new List<string>();
    if (lessonId.HasValue)
        filters.Add($"@lessonId:[{lessonId} {lessonId}]");
    if (topicId.HasValue)
        filters.Add($"@topicId:[{topicId} {topicId}]");
    if (difficulty.HasValue)
        filters.Add($"@difficulty:[{(int)difficulty} {(int)difficulty}]");

    if (filters.Any())
        query += " " + string.Join(" ", filters);

    // Sıralama
    var sortField = sortBy == "recent" ? "createdAt" : "like Count";

    var result = await redis.ExecuteAsync("FT.SEARCH", "contentIdx", query,
        "SORTBY", sortField, "DESC",
        "LIMIT", "0", "20"
    );

    // Redis'ten gelen ID'leri parse et ve DB'den çek
    var contentIds = ParseRedisSearchResults(result);
    return await context.Contents
        .Where(c => contentIds.Contains(c.Id))
        .Include(c => c.Author)
        .ToListAsync();
}
```

---

## 💰 6. FAZ 4: MARKETPLACE VE ÖDEME SİSTEMİ

Öğretmenler özel ders ilanı verir, öğrenciler arama yapar.

### 6.1. İlan Modeli

#### [Model] PrivateLessonAd

```csharp
public class PrivateLessonAd
{
    public int Id { get; set; }
    public int TeacherId { get; set; }
    public User Teacher { get; set; }

    public string Title { get; set; } // "TYT Matematik Özel Ders"
    public string Description { get; set; }
    public decimal PricePerHour { get; set; } // decimal(18,2) - 500.00 TL

    // Lokasyon
    public int CityId { get; set; }
    public City City { get; set; }

    public int? DistrictId { get; set; }
    public District District { get; set; }

    public string? Address { get; set; } // Detaylı adres (opsiyonel)

    // Online ders veriyor mu?
    public bool IsOnlineAvailable { get; set; }

    // Hangi dersler (JSON array: [1, 2, 5])
    public string LessonIdsJson { get; set; }

    // Müsait günler/saatler (JSON)
    // Format: { "Monday": ["09:00-12:00", "14:00-18:00"], ... }
    public string AvailabilityJson { get; set; }

    // Premium
    public bool IsPremium { get; set; } = false;
    public DateTime? PremiumStartDate { get; set; }
    public DateTime? PremiumEndDate { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}
```

#### Endpoints

#### [POST] `/api/marketplace/ad/create`

Öğretmen ilan oluşturur.

**Request:**

```json
{
  "title": "TYT Matematik Özel Ders",
  "description": "5 yıllık tecrübe, Boğaziçi Üniversitesi mezunu...",
  "pricePerHour": 500,
  "cityId": 34,
  "districtId": 450,
  "isOnlineAvailable": true,
  "lessonIds": [1, 2],
  "availability": {
    "Monday": ["09:00-12:00", "14:00-18:00"],
    "Wednesday": ["14:00-18:00"]
  }
}
```

#### [GET] `/api/marketplace/search`

İlan arama.

**Query:**

- `city`: Şehir ID
- `lesson`: Ders ID
- `minPrice`, `maxPrice`: Fiyat aralığı
- `onlineOnly`: Sadece online (boolean)
- `sortBy`: `price_asc`, `price_desc`, `rating`, `premium`

**Operation Logic:**

```csharp
public static async Task<List<PrivateLessonAdDto>> SearchAdsAsync(
    int? cityId,
    int? lessonId,
    decimal? minPrice,
    decimal? maxPrice,
    bool onlineOnly,
    string sortBy,
    ApplicationContext context)
{
    var query = context.PrivateLessonAds
        .Where(ad => ad.IsActive)
        .Include(ad => ad.Teacher)
        .Include(ad => ad.City)
        .AsQueryable();

    // Filtreler
    if (cityId.HasValue)
        query = query.Where(ad => ad.CityId == cityId || ad.IsOnlineAvailable);

    if (lessonId.HasValue)
        query = query.Where(ad => ad.LessonIdsJson.Contains($"\"{lessonId}\""));

    if (minPrice.HasValue)
        query = query.Where(ad => ad.PricePerHour >= minPrice);

    if (maxPrice.HasValue)
        query = query.Where(ad => ad.PricePerHour <= maxPrice);

    if (onlineOnly)
        query = query.Where(ad => ad.IsOnlineAvailable);

    // Sıralama: Premium > Rating > Price
    query = query
        .OrderByDescending(ad => ad.IsPremium)
        .ThenByDescending(ad => ad.Teacher.Rating);

    if (sortBy == "price_asc")
        query = query.ThenBy(ad => ad.PricePerHour);
    else
        query = query.ThenByDescending(ad => ad.PricePerHour);

    return await query
        .Take(50)
        .Select(ad => MapToDto(ad))
        .ToListAsync();
}
```

---

## ⏱️ 7. FAZ 5: ARAÇLAR (Tools)

### 7.1. Ders Programı (Schedule/Timetable)

#### [Model] Schedule

```csharp
public class Schedule
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }

    public DayOfWeek Day { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    public int? LessonId { get; set; }
    public Lesson Lesson { get; set; }

    public string? Note { get; set; }
    public string? Location { get; set; } // "Dershane A Sınıfı"
}
```

#### [POST] `/api/tools/schedule/create`

Ders programına ekleme.

#### [GET] `/api/tools/schedule/my`

Kullanıcının haftalık programı.

**Response:**

```json
{
  "success": true,
  "data": {
    "Monday": [
      { "time": "09:00-10:00", "lesson": "Matematik", "location": "A-101" }
    ],
    "Tuesday": []
  },
  "error": null,
  "errorCode": null
}
```

### 7.2. Zamanlayıcı ve Çalışma Takibi (Study Timer)

#### [Model] StudySession

```csharp
public class StudySession
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }

    public int? LessonId { get; set; }
    public Lesson Lesson { get; set; }

    public int DurationMinutes { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public string? Note { get; set; }
}
```

#### [POST] `/api/tools/timer/save`

Çalışma seansı kaydetme.

**Validation:**

- `EndTime > StartTime`
- `Duration` mantıklı (max 12 saat)
- `StartTime` gelecekte olamaz

#### [GET] `/api/tools/stats/weekly`

Haftalık çalışma istatistikleri.

**Response:**

```json
{
  "success": true,
  "data": {
    "totalHours": 12.5,
    "topLesson": { "name": "Matematik", "hours": 6 },
    "dailyBreakdown": [
      { "day": "Monday", "hours": 2 },
      { "day": "Tuesday", "hours": 0 }
    ]
  },
  "error": null,
  "errorCode": null
}
```

---

## 📱 8. UI AKIŞ SENARYOLARI (Frontend Integration Guide)

### 8.1. Öğretmen: Optik Form Yükleme Süreci

**Sayfa:** `/teacher/exam/upload`

**Akış:**

1. Dropdown'dan sınav seçimi
2. Drag&drop veya file input ile TXT yükleme
3. "Yükle ve İşle" butonu
4. Loading ekranı (progress bar: "38/40 işlendi...")
5. Sonuç ekranı:
   - ✅ Başarılı: 38 öğrenci
   - ❌ Hata: 2 kayıt (liste göster)
6. "Karneleri Önizle" butonu
7. Önizleme: Grid view ile tüm karneler
8. "Sınıfa Gönder" butonu → Toplu bildirim

### 8.2. Öğrenci: Karne Görüntüleme

**Trigger:** Bildirim "Yeni karne hazır!"

**Akış:**

1. Tıklama → `/student/report/{id}`
2. Sayfa içeriği:
   - Hero section: Total skor, sınıf sıralaması
   - Chart.js radar grafik (dersler bazında)
   - Accordion: Her ders için detay tablosu
   - Konu analizi: İyi/Zayıf konular badge'leri
3. "PDF İndir" butonu
4. "Arkadaşlarla Paylaş" (opsiyonel)

### 8.3. Ana Sayfa: Feed ve Keşfet

**Feed Tab:**

- Infinite scroll
- Her card: Soru görseli, başlık, like/comment count
- "Beğen" animasyonu (kalp)
- Tıklama → Detay modal

**Keşfet Tab:**

- Filter sidebar: Ders, konu, zorluk
- Search bar (debounce 300ms)
- Grid layout
- "Trendler" badge'i

---

## 📁 9. NİHAİ PROJE YAPISI VE FAZ ATAMALARI

```
KarneProject/
├── Controllers/
│   ├── BaseController.cs              [Faz 1] ✅
│   ├── AuthController.cs              [Faz 1] ✅
│   ├── AdminController.cs             [Faz 1] ✅
│   ├── UserController.cs              [Faz 1] ✅
│   ├── AccountController.cs           [Faz 1] ✅
│   ├── HealthController.cs            [Faz 1] ✅
│   ├── InstitutionController.cs       [Faz 2] ✅
│   ├── ClassroomController.cs         [Faz 2] ✅
│   ├── ExamController.cs              [Faz 2] ✅
│   ├── MessageController.cs           [Faz 2] ✅
│   ├── NotificationController.cs      [Faz 2] ✅
│   ├── ReportController.cs            [Faz 2] ✅
│   ├── SearchController.cs            [Faz 2] ✅
│   ├── SocialController.cs            [Faz 3]
│   ├── MarketplaceController.cs       [Faz 4]
│   └── ToolsController.cs             [Faz 5]
│
├── Operations/
│   ├── AuthOperations.cs              [Faz 1] ✅
│   ├── UserOperations.cs              [Faz 1] ✅
│   ├── AdminOperations.cs              [Faz 1] ✅
│   ├── AccountOperations.cs           [Faz 1] ✅
│   ├── InstitutionOperations.cs       [Faz 2] ✅
│   ├── ClassroomOperations.cs         [Faz 2] ✅
│   ├── ExamOperations.cs              [Faz 2] ✅
│   ├── MessageOperations.cs           [Faz 2] ✅
│   ├── SocialOperations.cs            [Faz 3]
│   ├── FeedOperations.cs              [Faz 3]
│   ├── MarketplaceOperations.cs       [Faz 4]
│   └── ToolsOperations.cs             [Faz 5]
│
├── Models/
│   ├── DBs/
│   │   ├── User.cs                    [Faz 1] ✅
│   │   ├── Institution.cs             [Faz 1] ✅
│   │   ├── InstitutionUser.cs         [Faz 1] ✅
│   │   ├── AccountLink.cs             [Faz 1] ✅
│   │   ├── AuditLog.cs                [Faz 1] ✅
│   │   ├── RefreshToken.cs            [Faz 1] ✅
│   │   ├── EmailVerification.cs       [Faz 1] ✅
│   │   ├── PasswordResetToken.cs      [Faz 1] ✅
│   │   ├── UserPreferences.cs          [Faz 1] ✅
│   │   ├── Classroom.cs               [Faz 2] ✅
│   │   ├── ClassroomStudent.cs        [Faz 2] ✅
│   │   ├── Exam.cs                    [Faz 2] ✅
│   │   ├── ExamResult.cs              [Faz 2] ✅
│   │   ├── Conversation.cs            [Faz 2] ✅
│   │   ├── ConversationMember.cs      [Faz 2] ✅
│   │   ├── Message.cs                 [Faz 2] ✅
│   │   ├── Notification.cs            [Faz 2] ✅
│   │   ├── Lesson.cs                  [Faz 3]
│   │   ├── Topic.cs                   [Faz 3]
│   │   ├── Content.cs                 [Faz 3]
│   │   ├── Comment.cs                 [Faz 3]
│   │   ├── Interaction.cs             [Faz 3]
│   │   ├── Follow.cs                  [Faz 3]
│   │   ├── PrivateLessonAd.cs         [Faz 4]
│   │   ├── City.cs                    [Faz 4]
│   │   ├── District.cs                [Faz 4]
│   │   ├── Schedule.cs                [Faz 5]
│   │   └── StudySession.cs            [Faz 5]
│   │
│   ├── DTOs/
│   │   ├── Requests/
│   │   │   ├── LoginRequest.cs
│   │   │   ├── RegisterRequest.cs
│   │   │   ├── OpticalUploadRequest.cs
│   │   │   ├── CreateContentRequest.cs
│   │   │   └── ...
│   │   └── Responses/
│   │       ├── LoginResponse.cs
│   │       ├── FeedResponse.cs
│   │       ├── ReportCardDto.cs
│   │       └── ...
│   │
│   └── Enums/
│       ├── UserRole.cs
│       ├── InstitutionRole.cs
│       ├── MessageType.cs
│       ├── ContentType.cs
│       ├── DifficultyLevel.cs
│       └── ...
│
├── Services/
│   ├── SessionService.cs              [Faz 1] ✅
│   ├── AuditService.cs                [Faz 1] ✅
│   ├── CacheService.cs                [Faz 1] ✅
│   ├── FileService.cs                 [Faz 1] ✅
│   ├── NotificationService.cs         [Faz 2] ✅
│   ├── OpticalParserService.cs        [Faz 2] ✅
│   ├── FeedService.cs                 [Faz 3]
│   └── RedisSearchHelper.cs           [Faz 3]
│
├── Hubs/ (SignalR)
│   ├── ChatHub.cs                     [Faz 2]
│   └── NotificationHub.cs             [Faz 2]
│
├── Core/
│   ├── Middleware/
│   │   ├── GlobalExceptionMiddleware.cs ✅
│   │   ├── RequestLoggingMiddleware.cs ✅
│   │   └── TokenBlacklistMiddleware.cs ✅
│   ├── Helpers/
│   │   ├── PasswordHelper.cs
│   │   └── RedisHelper.cs
│   └── Constants/
│       └── AppConstants.cs
│
├── Data/
│   ├── ApplicationContext.cs
│   └── Migrations/
│
└── Program.cs
```

**Tahmini Proje Büyüklüğü:**

- **Dosya Sayısı:** ~90-100 dosya
- **Kod Satırı:** ~30,000-35,000 satır
- **Model Sayısı:** 30+ entity
- **Endpoint Sayısı:** 100+ endpoint

**✅ Tamamlanan Fazlar:**

- **Faz 1:** ✅ %100 Tamamlandı
  - Authentication (Register, Login, Refresh Token, Forgot/Reset Password)
  - User Management (Profile, Preferences, Statistics, Activity)
  - Admin Operations (User CRUD, Institution Management, Audit Logs)
  - Account Linking (Request, Approve, Reject, List)
  - Health Check
  - Middleware'ler (Exception, Logging, Token Blacklist)
  - Rate Limiting
  - Cache Service (Pattern-based invalidation, Force Refresh)

- **Faz 2:** ✅ %100 Tamamlandı
  - Institution Management (CRUD, Members, Statistics)
  - Classroom Management (CRUD, Students, Bulk Operations)
  - Exam Management (Create, Upload Optical, Results, Confirm)
  - Message System (Send, Conversations, History, Search)
  - Notification System (List, Mark Read, Settings, Clear)
  - Report Management (Student Reports, Classroom Reports)
  - Search Controller (Users, Institutions, Classrooms, Exams)
  - Cache Integration (Tüm endpoint'lerde)
  - Force Refresh (Tüm GET endpoint'lerde)

**📊 Mevcut Durum:**

- **Toplam Endpoint:** 100+ endpoint implement edildi
- **Toplam Model:** 30+ entity tanımlandı
- **Cache Stratejisi:** Pattern-based invalidation ve force refresh mekanizması aktif
- **Background Jobs:** Hangfire ile ranking ve bulk notification job'ları implement edildi
- **Rate Limiting:** Global 1000 request/dakika/IP limiti aktif
- **Middleware'ler:** Exception handling, request logging, token blacklist aktif
- **SignalR Hubs:** ChatHub ve NotificationHub implement edildi ve aktif
- **Redis Cache:** IDistributedCache ve IConnectionMultiplexer ile pattern-based operations aktif

**🔧 Teknoloji Kullanım Detayları:**

**Redis (Cache):**
- ✅ Cache-aside pattern kullanılıyor
- ✅ Pattern-based cache removal (SCAN ile)
- ✅ Specific invalidation methods (User, Admin, Institution, Classroom, Exam, vb.)
- ✅ Force refresh mekanizması (tüm GET endpoint'lerde)
- ✅ Cache süreleri optimize edildi (1 dakika - 30 dakika arası)

**Hangfire (Background Jobs):**
- ✅ CalculateRankingsJob (sınav sonuçları yüklendikten sonra)
- ✅ BulkNotificationJob (toplu bildirim gönderimi)
- ✅ Automatic retry mekanizması (3 deneme)
- ✅ Batch processing (50'şer batch)

**SignalR (Real-time):**
- ✅ ChatHub (mesajlaşma için)
- ✅ NotificationHub (bildirimler için)
- ✅ Group-based messaging (conversation ve user groups)
- ✅ Automatic reconnection desteği

**Eksik Teknolojiler (Faz 3-5 için):**
- ❌ RediSearch (Full-text search için - Faz 3)
- ❌ FeedService (Feed algoritması için - Faz 3)
- ❌ RedisSearchHelper (Content indexing için - Faz 3)
- ❌ PDF Generation (Karne export için - Faz 2'de planlandı ama henüz implement edilmedi)
- ❌ Serilog (Structured logging - opsiyonel)

---

## 🔧 9. KULLANILAN TEKNOLOJİLER, KÜTÜPHANELER VE KURULUM

Bu bölüm, projede kullanılacak tüm harici kütüphaneler, açık kaynak projeler ve bunların kurulum talimatlarını içerir.

### 9.1. Backend NuGet Paketleri

#### **Temel Framework Paketleri**

```bash
# EF Core (Database)
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools

# ASP.NET Core
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Microsoft.AspNetCore.SignalR
```

#### **Güvenlik ve Authentication**

```bash
# JWT Token
dotnet add package System.IdentityModel.Tokens.Jwt

# Password Hashing (built-in, ek paket gerekmez)
# System.Security.Cryptography zaten .NET'te var
```

#### **Validation**

```bash
# FluentValidation - İstek validasyonu için
dotnet add package FluentValidation.AspNetCore
```

**Neden:** Input validation'ı declarative ve okunabilir yapar. Model validation attribute'larına göre daha güçlü.

**Kullanım Örneği:**

```csharp
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MinimumLength(5);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}
```

#### **Caching (Redis)**

```bash
# StackExchange.Redis - Redis client
dotnet add package StackExchange.Redis
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
```

**Neden:** En hızlı ve güvenilir .NET Redis client'ı. RediSearch desteği için gerekli.

**Setup (Program.cs):**

```csharp
// Redis Cache (IDistributedCache)
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "KarneProject_";
});

// Redis Connection Multiplexer (Pattern-based operations için)
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(redisConnectionString));
```

**Mevcut Kullanım (Faz 1-2):**

✅ **CacheService** implement edildi:
- Pattern-based cache removal (SCAN kullanarak)
- Specific invalidation methods (`InvalidateUserCacheAsync`, `InvalidateAdminCacheAsync`, vb.)
- Force refresh mekanizması (tüm GET endpoint'lerde `forceRefresh` parametresi)
- Cache-aside pattern (önce cache'e bak, yoksa DB'den çek ve cache'e yaz)

**Cache Süreleri:**
- User Profile: 15 dakika
- User Statistics: 10 dakika
- Admin Statistics: 5 dakika
- Institution Details: 5 dakika
- Classroom Details: 15 dakika
- Exam List: 2 dakika
- Conversations: 1 dakika
- Notifications: 5 dakika
- Search Results: 5 dakika

**Kullanım Örneği:**

```csharp
// CacheService.cs
public async Task<T?> GetAsync<T>(string key) where T : class
{
    var cached = await _cache.GetStringAsync(key);
    if (cached == null) return null;
    return JsonSerializer.Deserialize<T>(cached);
}

public async Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class
{
    var serialized = JsonSerializer.Serialize(value);
    await _cache.SetStringAsync(key, serialized, new DistributedCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = expiration
    });
}

// Pattern-based removal (IConnectionMultiplexer kullanarak)
public async Task RemoveByPatternAsync(string pattern)
{
    var server = _redis.GetServer(_redis.GetEndPoints().First());
    var keys = server.Keys(pattern: $"*{pattern}*").ToList();
    foreach (var key in keys)
    {
        await _cache.RemoveAsync(key!);
    }
}
```

#### **Redis RediSearch Modülü**

**Özel Kurulum Gerekli:** Redis sunucusuna RediSearch modülü kurulmalı.

**Docker ile kurulum (en kolay):**

```bash
docker run -d --name redis-stack -p 6379:6379 redis/redis-stack-server:latest
```

**Manuel kurulum:**

```bash
# Ubuntu/Debian
wget https://redismodules.s3.amazonaws.com/redisearch/redisearch.Linux-ubuntu18.04-x86_64.2.8.4.zip
unzip redisearch.*.zip
redis-server --loadmodule ./redisearch.so
```

**Neden:** Full-text search ve filtering için SQL'den 50-100x daha hızlı. Keşfet sayfası için kritik.

**Mevcut Durum:**
- ❌ **Henüz implement edilmedi** (Faz 3 için planlandı)
- ✅ Dökümanlarda detaylı açıklama mevcut (Faz 3 bölümünde)

**Gelecek Kullanım (Faz 3):**

**RedisSearchHelper.cs** servisi oluşturulacak:
- Content indexing (soru, post, announcement)
- Full-text search (title, description, tags)
- Filtering (lesson, topic, difficulty)
- Sorting (popular, recent, trending)

**Kullanım Örneği (Planlanan):**

```csharp
// RedisSearchHelper.cs
public static async Task CreateContentIndexAsync(IDatabase redis)
{
    await redis.ExecuteAsync("FT.CREATE", "contentIdx",
        "ON", "JSON",
        "PREFIX", "1", "content:",
        "SCHEMA",
        "$.title", "AS", "title", "TEXT",
        "$.description", "AS", "description", "TEXT",
        "$.tags", "AS", "tags", "TAG",
        "$.lessonId", "AS", "lessonId", "NUMERIC",
        "$.topicId", "AS", "topicId", "NUMERIC",
        "$.difficulty", "AS", "difficulty", "NUMERIC"
    );
}

public static async Task<List<Content>> SearchContentsAsync(
    string searchTerm,
    int? lessonId,
    int? topicId,
    DifficultyLevel? difficulty,
    IDatabase redis,
    ApplicationContext context)
{
    // RediSearch query oluştur ve çalıştır
    // Redis'ten gelen ID'leri parse et
    // DB'den detaylı bilgileri çek
}
```

#### **Background Jobs (Hangfire)**

```bash
dotnet add package Hangfire
dotnet add package Hangfire.AspNetCore
dotnet add package Hangfire.SqlServer
```

**Neden:** Sıralama hesaplama, feed generation gibi uzun süren işleri arka planda çalıştırmak için.

**Setup (Program.cs):**

```csharp
// Hangfire Configuration
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();

// Dashboard (Production'da authorization eklenmeli)
app.UseHangfireDashboard("/hangfire");
```

**Mevcut Kullanım (Faz 1-2):**

✅ **CalculateRankingsJob** implement edildi:
- Sınav sonuçları yüklendikten sonra sıralama hesaplama
- Institution rank ve class rank hesaplama
- Automatic retry (3 deneme)

✅ **BulkNotificationJob** implement edildi:
- Toplu bildirim gönderme (batch processing ile)
- 50'şer batch halinde gönderim
- Batch'ler arası 100ms delay

**Kullanım Örneği:**

```csharp
// ExamOperations.cs - Optik form yüklendikten sonra
BackgroundJob.Enqueue<CalculateRankingsJob>(job => job.Execute(examId));

// ExamOperations.cs - Sonuçlar onaylandıktan sonra
BackgroundJob.Enqueue<BulkNotificationJob>(job => job.Execute(examId));
```

**Job Implementation:**

```csharp
// Jobs/CalculateRankingsJob.cs
public class CalculateRankingsJob
{
    [AutomaticRetry(Attempts = 3)]
    public async Task Execute(int examId)
    {
        // Sıralama hesaplama logic
        // Institution rank ve class rank güncelleme
    }
}
```

**Gelecek Kullanım (Faz 3):**
- Feed generation job (günlük)
- Content indexing job (RediSearch için)
- Cache invalidation job (günlük temizlik)

#### **JSON Serialization**

```bash
# System.Text.Json (built-in, ek paket gerekmez)
# Newtonsoft.Json alternatif (opsiyonel)
dotnet add package Newtonsoft.Json
```

#### **Logging**

```bash
# Serilog - Structured logging
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.File
dotnet add package Serilog.Sinks.Console
```

**Neden:** Default logger'dan daha güçlü, structured logging desteği.

**Setup:**

```csharp
builder.Host.UseSerilog((context, config) =>
{
    config.WriteTo.Console()
          .WriteTo.File("logs/karneproject-.txt", rollingInterval: RollingInterval.Day);
});
```

#### **PDF Generation**

```bash
# iTextSharp (Karne PDF'i için)
dotnet add package itext7

# Alternatif: PuppeteerSharp (HTML to PDF)
dotnet add package PuppeteerSharp
```

**Neden:** Karne export için PDF oluşturma gerekli.

**Hangisini seçmeli:**

- iTextSharp: Programatik PDF (kod ile layout)
- PuppeteerSharp: HTML/CSS template → PDF (daha kolay styling)

**Tavsiye:** PuppeteerSharp (HTML template kullanımı daha kolay)

---

### 9.2. Frontend Kütüphaneleri (Tavsiye Edilenler)

#### **React Projesi için:**

```bash
npm install @microsoft/signalr          # Real-time messaging
npm install chart.js react-chartjs-2    # Grafikler (karne)
npm install axios                        # API istekleri
npm install react-query                  # Server state management
npm install zustand                      # Client state management
```

#### **Chart.js - Karne Grafikleri**

**Neden:** En popüler ve kolay kullanımlı grafik kütüphanesi.

**Kullanım Örneği (Karne Radar Chart):**

```javascript
import { Radar } from "react-chartjs-2";

const data = {
  labels: ["Matematik", "Fizik", "Kimya", "Biyoloji"],
  datasets: [
    {
      label: "Net",
      data: [28.75, 12.5, 18, 15.25],
      backgroundColor: "rgba(54, 162, 235, 0.2)",
      borderColor: "rgb(54, 162, 235)",
    },
  ],
};

<Radar data={data} />;
```

#### **SignalR (Real-time Communication)**

**Backend Setup (Program.cs):**

```csharp
// SignalR Service Registration
builder.Services.AddSignalR();

// Hub Mapping
app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<NotificationHub>("/hubs/notification");
```

**Mevcut Kullanım (Faz 1-2):**

✅ **ChatHub** implement edildi:
- Conversation group management (`JoinConversation`, `LeaveConversation`)
- Real-time mesaj gönderimi (`ReceiveMessage` event)
- Group-based messaging (sınıf grupları için)

✅ **NotificationHub** implement edildi:
- User-specific groups (`User_{userId}`)
- Real-time bildirim gönderimi (`ReceiveNotification` event)
- Automatic group assignment on connection

**Backend Kullanım Örneği:**

```csharp
// MessageOperations.cs
public async Task<BaseResponse<MessageDto>> SendMessageAsync(...)
{
    // Mesaj DB'ye kaydedilir
    await _context.SaveChangesAsync();
    
    // SignalR ile real-time gönderim
    await _chatHub.Clients.Group($"Conversation_{conversationId}")
        .SendAsync("ReceiveMessage", messageDto);
}

// NotificationService.cs
public async Task SendNotificationAsync(...)
{
    // Bildirim DB'ye kaydedilir
    await _context.SaveChangesAsync();
    
    // SignalR ile real-time gönderim
    await _hubContext.Clients.Group($"User_{userId}")
        .SendAsync("ReceiveNotification", notification);
}
```

**Frontend Kullanımı:**

```javascript
import * as signalR from "@microsoft/signalr";

// Chat Hub Connection
const chatConnection = new signalR.HubConnectionBuilder()
  .withUrl("/hubs/chat", {
    accessTokenFactory: () => localStorage.getItem("token"),
  })
  .withAutomaticReconnect()
  .build();

await chatConnection.start();

// Conversation'a katılma
await chatConnection.invoke("JoinConversation", conversationId);

// Mesaj dinleme
chatConnection.on("ReceiveMessage", (message) => {
  appendMessageToChat(message);
});

// Notification Hub Connection
const notificationConnection = new signalR.HubConnectionBuilder()
  .withUrl("/hubs/notification", {
    accessTokenFactory: () => localStorage.getItem("token"),
  })
  .withAutomaticReconnect()
  .build();

await notificationConnection.start();

// Bildirim dinleme
notificationConnection.on("ReceiveNotification", (notification) => {
  updateNotificationBadge();
  showToast(notification.title, notification.message);
});
```

**Gelecek Kullanım (Faz 3):**
- Feed updates (yeni içerik paylaşıldığında)
- Like/Comment notifications (real-time)
- Follow notifications

---

### 9.3. Açık Kaynak Projeler ve Algoritma Kaynakları

#### **Feed Algoritması**

**Kaynak:** Sosyal medya feed algoritmaları için best practice'ler

**Referanslar:**

- Reddit feed algorithm (open source): https://github.com/reddit-archive/reddit
- Instagram Explore algorithm paper: https://ai.facebook.com/blog/powered-by-ai-instagrams-explore-recommender-system/

**Projemizde kullanım:**
Scoring sistemimiz bu best practice'lerden ilham alınmıştır:

- Recency decay (zaman bonusu)
- Social signals (like, comment weights)
- Personalization (user interests)

**Kod yazdık, indirmedik:** Algoritma mantığını öğrenip kendi kodumuzu yazdık.

#### **Redis RediSearch**

**Açık kaynak:** https://github.com/RediSearch/RediSearch

**Neden:** SQL full-text search'ten çok daha hızlı, real-time indexing.

**Kurulum:** Docker image veya module build (yukarıda belirtildi)

**Kodumuzda kullanım:** `RedisSearchHelper.cs` servisi ile entegrasyon

#### **OpticalParserService**

**Tamamen bizim kodumuzdur.** TXT parsing için hazır kütüphane yok, kendi algoritmamızı yazdık.

**Algoritma:**

1. Substring ile kolon parse
2. Character-by-character cevap karşılaştırma
3. Net hesaplama: `Doğru - (Yanlış / 4.0f)`

---

### 9.4. Geliştirme Araçları

#### **Database Tools**

```bash
# SQL Server Management Studio (SSMS)
# Veya
# Azure Data Studio (cross-platform)
```

#### **Redis Client**

```bash
# RedisInsight (GUI)
# https://redis.com/redis-enterprise/redis-insight/

# Veya command-line
redis-cli
```

#### **API Testing**

```bash
# Postman veya Thunder Client (VS Code extension)
# Swagger UI (built-in with ASP.NET Core)
```

---

### 9.5. Sunucu Gereksinimleri (Production)

#### **Minimum Gereksinimler:**

- **CPU:** 4 core
- **RAM:** 8 GB (4 GB .NET app + 2 GB Redis + 2 GB SQL Server)
- **Disk:** 50 GB SSD
- **Bandwidth:** 100 Mbps

#### **Önerilen (Dershaneler için):**

- **CPU:** 8 core
- **RAM:** 16 GB
- **Disk:** 100 GB SSD
- **Redis:** Ayrı sunucu (cache için)

#### **Cloud Seçenekleri:**

- **Azure:** App Service + Azure SQL + Azure Cache for Redis
- **AWS:** EC2 + RDS + ElastiCache
- **Google Cloud:** Compute Engine + Cloud SQL + Memorystore

---

### 9.6. Development Environment Setup (Adım Adım)

#### **Gerekli Yazılımlar:**

```bash
# 1. .NET 8 SDK
https://dotnet.microsoft.com/download/dotnet/8.0

# 2. SQL Server 2022 (veya LocalDB)
https://www.microsoft.com/sql-server/sql-server-downloads

# 3. Redis (Docker tavsiye edilir)
docker pull redis/redis-stack-server

# 4. Visual Studio 2022 veya VS Code
https://visualstudio.microsoft.com/
```

#### **Proje Kurulumu:**

```bash
# 1. Repo clone
git clone <repo-url>
cd KarneProject

# 2. NuGet paketlerini restore
dotnet restore

# 3. appsettings.json düzenle
# ConnectionStrings güncelle

# 4. Redis başlat
docker run -d -p 6379:6379 redis/redis-stack-server

# 5. Database migration
dotnet ef database update

# 6. Seed data
dotnet run --seed

# 7. Çalıştır
dotnet run
```

#### **İlk Kurulum Kontrol Listesi:**

- ✅ SQL Server bağlantısı test edildi
- ✅ Redis bağlantısı test edildi
- ✅ Migration başarılı
- ✅ Seed data yüklendi
- ✅ Swagger UI açılıyor (`/swagger`)
- ✅ Hangfire dashboard açılıyor (`/hangfire`)
- ✅ SignalR hub test edildi

---

### 9.7. Üçüncü Parti API'ler (Gelecek Fazlar)

#### **Ödeme (Faz 4 - Marketplace Premium):**

- **iyzico** (Türkiye): https://www.iyzico.com/
- **Stripe** (Global): https://stripe.com/

```bash
dotnet add package Iyzipay
```

#### **SMS Bildirimleri (Opsiyonel):**

- **Netgsm** veya **İleti Merkezi**

#### **Email (Şifre sıfırlama vb.):**

- **SendGrid** veya **Mailgun**

```bash
dotnet add package SendGrid
```

---

## 📋 9.8. Özet: Kullanılan Tüm Teknolojiler ve Durumları

| Teknoloji            | Amaç                 | Kurulum      | Faz | Durum | Kendi Kodumuz mu?      |
| -------------------- | -------------------- | ------------ | --- | ----- | ---------------------- |
| **ASP.NET Core 8**   | Backend framework    | SDK indir    | 1   | ✅    | -                      |
| **EF Core**          | ORM                  | NuGet        | 1   | ✅    | -                      |
| **SQL Server**       | Database             | İndir/Cloud  | 1   | ✅    | -                      |
| **Redis**            | Cache                | Docker       | 1-2 | ✅    | -                      |
| **RediSearch**       | Full-text search     | Redis module | 3   | ❌    | ❌ Açık kaynak module  |
| **SignalR**          | Real-time            | Built-in     | 2   | ✅    | -                      |
| **FluentValidation** | Input validation     | NuGet        | 1   | ✅    | ❌ Açık kaynak paket   |
| **Hangfire**         | Background jobs      | NuGet        | 2   | ✅    | ❌ Açık kaynak paket   |
| **Serilog**          | Logging              | NuGet        | -   | ❌    | ❌ Açık kaynak paket   |
| **PuppeteerSharp**   | PDF generation       | NuGet        | 2   | ❌    | ❌ Açık kaynak paket   |
| **Chart.js**         | Grafikler (frontend) | npm          | 2   | ❌    | ❌ Açık kaynak library |
| **Feed Algorithm**   | Sosyal feed          | -            | 3   | ❌    | ✅ Kendi algoritmamız  |
| **Optical Parser**   | TXT parse            | -            | 2   | ✅    | ✅ Kendi algoritmamız  |
| **Net Calculation**  | Sınav hesaplama      | -            | 2   | ✅    | ✅ Kendi algoritmamız  |
| **CacheService**     | Cache yönetimi       | -            | 1-2 | ✅    | ✅ Kendi servisimiz    |
| **Rate Limiting**    | API koruması         | Built-in     | 1   | ✅    | -                      |

**Açıklama:**
- ✅ **Tamamlandı:** Teknoloji implement edildi ve aktif kullanılıyor
- ❌ **Planlandı:** Teknoloji dökümanlarda belirtilmiş ama henüz implement edilmedi
- **Faz:** Hangi fazda kullanılacağı/kullanıldığı

**Toplam Kullanılan Paket:** ~15 NuGet paketi + 5 npm paketi

**Kendi Yazdığımız Algoritmalar ve Servisler:**

**Tamamlanan (Faz 1-2):**
1. ✅ **Optical TXT Parser** (Exam) - `OpticalParserService.cs`
2. ✅ **Net Calculation Algorithm** (Exam) - `ExamOperations.cs`
3. ✅ **Topic-based Analysis** (Exam) - `ExamOperations.cs`
4. ✅ **Class Ranking Algorithm** (Exam) - `CalculateRankingsJob.cs`
5. ✅ **CacheService** - Pattern-based invalidation, force refresh
6. ✅ **Cache Invalidation Strategy** - Specific methods per entity

**Planlanan (Faz 3):**
1. ❌ **Feed Scoring Algorithm** (Social) - `FeedService.cs`
2. ❌ **RedisSearchHelper** - Content indexing ve search
3. ❌ **Content Recommendation Algorithm** - Kişiselleştirilmiş öneriler

**İndirilen Açık Kaynak:**

1. Redis RediSearch Module (Apache 2.0 License)
2. FluentValidation (Apache 2.0 License)
3. Hangfire (LGPL License - ticari kullanım için lisans gerekebilir)
4. PuppeteerSharp (MIT License)
5. Chart.js (MIT License)

**Lisans Uyarısı:** Hangfire ticari projede kullanılacaksa Hangfire Pro lisansı satın alınmalı ($999/yıl). Alternatif: Quartz.NET (ücretsiz).

---

## ✅ 10. SONUÇ VE BAŞLANGIŞ TALİMATI

Bu döküman tamamlandı. **3000+ satır** detaylı backend blueprint.

### İlk Adımlar

```bash
# 1. Database oluştur
dotnet ef migrations add InitialCreate
dotnet ef database update

# 2. Seed data ekle (Lesson, Topic)
dotnet run --seed

# 3. İlk admin kullanıcı oluştur
curl -X POST /api/admin/create-superuser
```

### Geliştirme Tahmini

| Faz        | Süre            | Özellikler                               |
| ---------- | --------------- | ---------------------------------------- |
| Faz 1      | 2-3 hafta       | Auth, User, Institution, Admin           |
| Faz 2      | 5-6 hafta       | Classroom, Exam, Messaging, Notification |
| Faz 3      | 3-4 hafta       | Social, Feed, Search                     |
| Faz 4      | 2 hafta         | Marketplace                              |
| Faz 5      | 1 hafta         | Tools                                    |
| **Toplam** | **13-16 hafta** | **Tam platform**                         |

### Performans Hedefleri

- ✅ **Optik Yükleme:** <30 saniye (100 öğrenci)
- ✅ **Feed Yükleme:** <200ms (cache ile)
- ✅ **Real-time Mesaj:** <100ms latency
- ✅ **API Response:** <500ms (p95)
- ✅ **Database Query:** Index kullanımı %100

### Güvenlik Kontrol Listesi

- ✅ JWT token expiration (7 gün)
- ✅ Refresh Token sistemi (30 gün)
- ✅ Password hashing (Salt + SHA256)
- ✅ Email Verification sistemi
- ✅ Password Reset (Forgot/Reset) sistemi
- ✅ Token Blacklist (Logout ve güvenlik ihlali)
- ✅ SQL Injection koruması (EF Core parametrized)
- ✅ XSS koruması (input sanitization)
- ✅ CORS policy tanımlı
- ✅ Rate limiting (1000 req/min - çok geniş limit)
- ✅ Audit logging tüm CUD işlemlerde
- ✅ Global Exception Handler
- ✅ Request Logging

---

## 📊 11. PROJE DURUMU VE TAMAMLANAN ÖZELLİKLER

### ✅ Faz 1: Foundation - %100 Tamamlandı

**Authentication & Authorization:**
- ✅ User Registration
- ✅ User Login (JWT Token)
- ✅ Refresh Token Sistemi
- ✅ Email Verification
- ✅ Password Reset (Forgot/Reset)
- ✅ Token Blacklist (Logout)

**User Management:**
- ✅ Get Profile (kendi profili)
- ✅ Get User Profile (başka kullanıcı - privacy kontrolü ile)
- ✅ Update Profile
- ✅ Change Password
- ✅ Upload Profile Image
- ✅ Update Email
- ✅ Delete Account (soft delete)
- ✅ Get Statistics
- ✅ Get Activity
- ✅ Search Users

**User Preferences:**
- ✅ Get Preferences
- ✅ Update Preferences
- ✅ Update Profile Layout
- ✅ Update Dashboard Layout

**Admin Operations:**
- ✅ Approve Institution
- ✅ Get Pending Institutions
- ✅ Get All Users (pagination, filtreleme, arama)
- ✅ Get User Details
- ✅ Update User
- ✅ Update User Status
- ✅ Delete User
- ✅ Reset User Password
- ✅ Get All Institutions
- ✅ Get Institution Details
- ✅ Reject Institution
- ✅ Update Institution Status
- ✅ Extend Subscription
- ✅ Create Admin
- ✅ Get All Admins
- ✅ Get Statistics
- ✅ Get Audit Logs
- ✅ Get User Audit Logs

**Account Linking:**
- ✅ Link Request
- ✅ Link Approve
- ✅ Link Reject
- ✅ Get Link Requests
- ✅ Get Linked Accounts
- ✅ Delete Account Link

**Health Check:**
- ✅ Health Endpoint

**Middleware:**
- ✅ GlobalExceptionMiddleware
- ✅ RequestLoggingMiddleware
- ✅ TokenBlacklistMiddleware

**Rate Limiting:**
- ✅ Global Rate Limiter (1000 req/min/IP)

**Cache Service:**
- ✅ Pattern-based cache removal
- ✅ Specific invalidation methods
- ✅ Force refresh mekanizması

---

### ✅ Faz 2: Kurum Yönetimi - %100 Tamamlandı

**Institution Management:**
- ✅ Get My Institutions
- ✅ Get Institution Details
- ✅ Update Institution
- ✅ Get Institution Members
- ✅ Add Member
- ✅ Remove Member
- ✅ Update Member Role
- ✅ Get Institution Statistics

**Classroom Management:**
- ✅ Create Classroom
- ✅ Get Classroom Details
- ✅ Get Institution Classrooms
- ✅ Update Classroom
- ✅ Delete Classroom
- ✅ Add Student
- ✅ Add Students (Bulk)
- ✅ Remove Student
- ✅ Get Classroom Students

**Exam Management:**
- ✅ Create Exam
- ✅ Process Optical File
- ✅ Confirm Results
- ✅ Get Exams (filtreleme, pagination)
- ✅ Get Exam Details
- ✅ Get Exam Results

**Message System:**
- ✅ Start Conversation
- ✅ Send Message
- ✅ Get Conversations
- ✅ Get Conversation Details
- ✅ Get Message History
- ✅ Update Conversation
- ✅ Delete Conversation
- ✅ Leave Conversation
- ✅ Delete Message
- ✅ Update Message
- ✅ Mark Conversation as Read
- ✅ Send to Class
- ✅ Search Messages

**Notification System:**
- ✅ Get My Notifications (filtreleme, pagination)
- ✅ Mark as Read
- ✅ Mark All as Read
- ✅ Delete Notification
- ✅ Clear All Notifications
- ✅ Get Notification Settings
- ✅ Update Notification Settings

**Report Management:**
- ✅ Get Student Report
- ✅ Get All Student Reports
- ✅ Get Classroom Reports

**Search:**
- ✅ Search Users
- ✅ Search Institutions
- ✅ Search Classrooms
- ✅ Search Exams

**Cache Integration:**
- ✅ Tüm GET endpoint'lerde cache kullanımı
- ✅ Tüm CUD işlemlerde cache invalidation
- ✅ Force refresh mekanizması (tüm GET endpoint'lerde)

**Background Jobs:**
- ✅ Calculate Rankings Job (Hangfire)
- ✅ Bulk Notification Job (Hangfire)
- ✅ Cache Invalidation Job (Hangfire - günlük)

---

### 📋 Özet: Tamamlanan Endpoint Sayıları

| Controller | Endpoint Sayısı | Durum |
|------------|----------------|-------|
| AuthController | 7 | ✅ Tamamlandı |
| UserController | 17 | ✅ Tamamlandı |
| AdminController | 18 | ✅ Tamamlandı |
| AccountController | 6 | ✅ Tamamlandı |
| HealthController | 1 | ✅ Tamamlandı |
| InstitutionController | 8 | ✅ Tamamlandı |
| ClassroomController | 9 | ✅ Tamamlandı |
| ExamController | 6 | ✅ Tamamlandı |
| MessageController | 13 | ✅ Tamamlandı |
| NotificationController | 7 | ✅ Tamamlandı |
| ReportController | 3 | ✅ Tamamlandı |
| SearchController | 4 | ✅ Tamamlandı |
| **TOPLAM** | **99+** | **✅ Faz 1-2 Tamamlandı** |

---

### 🔄 Sonraki Adımlar

**Faz 3: Sosyal Ağ ve Keşfet** (Henüz başlanmadı)
- Content Paylaşımı (Soru, Post, Announcement)
- Feed Algoritması
- Keşfet ve Arama (RediSearch)
- Follow/Unfollow
- Like/Comment
- Social Interactions

**Faz 4: Marketplace ve Ödeme** (Henüz başlanmadı)
- Private Lesson Ads
- Marketplace Search
- Payment Integration

**Faz 5: Araçlar** (Henüz başlanmadı)
- Schedule/Timetable
- Study Timer
- Statistics

---

**📘 Döküman Sonu**

_Bu döküman, KarneProject'in backend altyapısının tam teknik şartnamesini içermektedir. Tüm modeller, endpointler, algoritmalar, UI akışları ve geliştirme adımları detaylandırılmıştır. Toplam 6600+ satır comprehensive blueprint._

---

## 📍 12. MEVCUT DURUM ÖZETİ

### ✅ Tamamlanan Fazlar

**Faz 1: Foundation - %100 Tamamlandı**
- Authentication & Authorization (JWT, Refresh Token, Email Verification, Password Reset)
- User Management (17 endpoint)
- Admin Operations (18 endpoint)
- Account Linking (6 endpoint)
- Health Check
- Middleware'ler (Exception, Logging, Token Blacklist)
- Rate Limiting (1000 req/min/IP)
- Cache Service (Pattern-based invalidation, Force Refresh)

**Faz 2: Kurum Yönetimi - %100 Tamamlandı**
- Institution Management (8 endpoint)
- Classroom Management (9 endpoint)
- Exam Management (6 endpoint)
- Message System (13 endpoint) - SignalR ile real-time
- Notification System (7 endpoint) - SignalR ile real-time
- Report Management (3 endpoint)
- Search Controller (4 endpoint)
- Background Jobs (Hangfire: Ranking, Bulk Notification)

### 🔧 Aktif Teknolojiler

**✅ Implement Edilmiş ve Aktif:**
- Redis Cache (IDistributedCache + IConnectionMultiplexer)
- Hangfire (Background Jobs)
- SignalR (ChatHub, NotificationHub)
- Rate Limiting (.NET 8 built-in)
- FluentValidation
- Cache-aside pattern
- Pattern-based cache removal
- Force refresh mekanizması

**❌ Planlanmış (Faz 3-5 için):**
- RediSearch (Full-text search)
- FeedService (Feed algoritması)
- RedisSearchHelper (Content indexing)
- PDF Generation (PuppeteerSharp)
- Serilog (Structured logging - opsiyonel)

### 📊 İstatistikler

- **Toplam Endpoint:** 99+ endpoint
- **Toplam Model:** 30+ entity
- **Toplam Controller:** 12 controller
- **Background Jobs:** 2 job (Hangfire)
- **SignalR Hubs:** 2 hub (ChatHub, NotificationHub)
- **Cache Süreleri:** 1 dakika - 30 dakika (optimize edilmiş)

### 🎯 Şu Anki Konum

**✅ Faz 1 ve Faz 2 tamamlandı!**

**Sonraki Adım:** Faz 3 (Sosyal Ağ ve Keşfet) implementasyonuna başlanabilir.

**Faz 3 için Gerekenler:**
1. RediSearch modülü kurulumu (Redis sunucusuna)
2. RedisSearchHelper servisi implementasyonu
3. FeedService servisi implementasyonu
4. Content/Comment/Interaction modelleri
5. SocialController ve SocialOperations
6. Feed algoritması implementasyonu

**✅ Faz 1 ve Faz 2 tamamlandı! Faz 3'e geçmeye hazırsınız!** 🚀
