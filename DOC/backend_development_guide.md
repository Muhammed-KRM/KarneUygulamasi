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

### 5.4. Content Management (İçerik Yönetimi)

#### [GET] `/api/social/content/{id}`

İçerik detayını getir.

**Query Parameters:**
- `forceRefresh`: Cache'i bypass et (default: false)

**Teknoloji Kullanımı:**
- **CacheService**: İçerik detayı cache'lenir (15 dakika)
- **AsNoTracking()**: Read-only query için performans optimizasyonu
- **Include()**: Author, Lesson, Topic, Comments eager loading

**Operation Logic (SocialOperations.cs):**

```csharp
public async Task<BaseResponse<ContentDetailDto>> GetContentByIdAsync(int contentId, bool forceRefresh = false)
{
    // 1. Cache kontrolü
    var cacheKey = $"Content:{contentId}:Detail";
    if (!forceRefresh)
    {
        var cached = await _cacheService.GetAsync<ContentDetailDto>(cacheKey);
        if (cached != null)
            return BaseResponse<ContentDetailDto>.SuccessResponse(cached);
    }

    // 2. DB'den çek (AsNoTracking ile performans)
    var content = await _context.Contents
        .AsNoTracking()
        .Include(c => c.Author)
        .Include(c => c.Lesson)
        .Include(c => c.Topic)
        .Include(c => c.Comments.OrderByDescending(com => com.CreatedAt).Take(10))
            .ThenInclude(com => com.User)
        .FirstOrDefaultAsync(c => c.Id == contentId);

    if (content == null)
        return BaseResponse<ContentDetailDto>.ErrorResponse("Content not found", ErrorCodes.NotFound);

    // 3. View count artır (background job ile)
    BackgroundJob.Enqueue(() => IncrementContentViewCountAsync(contentId));

    // 4. DTO mapping
    var dto = MapToContentDetailDto(content);

    // 5. Cache'e yaz
    await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(15));

    return BaseResponse<ContentDetailDto>.SuccessResponse(dto);
}
```

**Cache Stratejisi:**
- Cache Key: `Content:{contentId}:Detail`
- Cache Süresi: 15 dakika
- Invalidation: Content güncellendiğinde veya silindiğinde

---

#### [PUT] `/api/social/content/{id}`

İçerik düzenleme (sadece sahibi).

**Request:**

```json
{
  "title": "Güncellenmiş Başlık",
  "description": "Güncellenmiş açıklama",
  "imageUrl": "https://cdn.../new-image.jpg",
  "tags": ["#güncel", "#tyt"]
}
```

**Operation Logic:**

```csharp
public async Task<BaseResponse<bool>> UpdateContentAsync(int contentId, UpdateContentRequest request)
{
    var userId = _sessionService.GetUserId();

    // 1. İçerik sahibi kontrolü
    var content = await _context.Contents
        .FirstOrDefaultAsync(c => c.Id == contentId && c.AuthorId == userId);

    if (content == null)
        return BaseResponse<bool>.ErrorResponse("Content not found or unauthorized", ErrorCodes.NotFound);

    // 2. Güncelleme
    if (!string.IsNullOrEmpty(request.Title))
        content.Title = request.Title;
    if (!string.IsNullOrEmpty(request.Description))
        content.Description = request.Description;
    if (request.ImageUrl != null)
        content.ImageUrl = request.ImageUrl;
    if (request.Tags != null)
        content.TagsJson = JsonSerializer.Serialize(request.Tags);

    await _context.SaveChangesAsync();

    // 3. Cache invalidation
    await _cacheService.RemoveAsync($"Content:{contentId}:Detail");
    await _cacheService.RemoveByPatternAsync($"User:{userId}:Content:*");
    await _cacheService.RemoveByPatternAsync("Feed:*");
    await _cacheService.RemoveByPatternAsync("Discover:*");

    // 4. RediSearch index güncelle (background job)
    BackgroundJob.Enqueue(() => RedisSearchHelper.UpdateContentIndexAsync(contentId));

    // 5. Audit log
    await _auditService.LogAsync(userId, "ContentUpdated", 
        JsonSerializer.Serialize(new { ContentId = contentId }));

    return BaseResponse<bool>.SuccessResponse(true);
}
```

**Teknoloji Kullanımı:**
- **CacheService**: Pattern-based invalidation (Feed, Discover, User content caches)
- **Hangfire**: RediSearch index güncelleme background job olarak
- **AuditService**: Değişiklik loglama

---

#### [DELETE] `/api/social/content/{id}`

İçerik silme (soft delete - IsDeleted flag).

**Operation Logic:**

```csharp
public async Task<BaseResponse<bool>> DeleteContentAsync(int contentId)
{
    var userId = _sessionService.GetUserId();

    var content = await _context.Contents
        .FirstOrDefaultAsync(c => c.Id == contentId && c.AuthorId == userId);

    if (content == null)
        return BaseResponse<bool>.ErrorResponse("Content not found or unauthorized", ErrorCodes.NotFound);

    // Soft delete
    content.IsDeleted = true;
    content.DeletedAt = DateTime.UtcNow;

    await _context.SaveChangesAsync();

    // Cache invalidation
    await _cacheService.RemoveAsync($"Content:{contentId}:Detail");
    await _cacheService.RemoveByPatternAsync($"User:{userId}:Content:*");
    await _cacheService.RemoveByPatternAsync("Feed:*");
    await _cacheService.RemoveByPatternAsync("Discover:*");

    // RediSearch'ten kaldır
    BackgroundJob.Enqueue(() => RedisSearchHelper.DeleteContentIndexAsync(contentId));

    // Audit log
    await _auditService.LogAsync(userId, "ContentDeleted", 
        JsonSerializer.Serialize(new { ContentId = contentId }));

    return BaseResponse<bool>.SuccessResponse(true);
}
```

**Model Güncellemesi (Content.cs):**

```csharp
public class Content
{
    // ... mevcut property'ler ...
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
}
```

---

#### [GET] `/api/social/content/user/{userId}`

Kullanıcının içeriklerini listele.

**Query Parameters:**
- `page`: Sayfa numarası (default: 1)
- `limit`: Sayfa başına kayıt (default: 20, max: 50)
- `type`: ContentType filtresi (opsiyonel)
- `forceRefresh`: Cache bypass (default: false)

**Operation Logic:**

```csharp
public async Task<BaseResponse<PagedResponse<ContentDto>>> GetUserContentsAsync(
    int userId, 
    int page = 1, 
    int limit = 20, 
    ContentType? type = null,
    bool forceRefresh = false)
{
    // 1. Cache kontrolü
    var cacheKey = $"User:{userId}:Content:Page{page}:Type{type}";
    if (!forceRefresh)
    {
        var cached = await _cacheService.GetAsync<PagedResponse<ContentDto>>(cacheKey);
        if (cached != null)
            return BaseResponse<PagedResponse<ContentDto>>.SuccessResponse(cached);
    }

    // 2. Query oluştur
    var query = _context.Contents
        .AsNoTracking()
        .Where(c => c.AuthorId == userId && !c.IsDeleted)
        .Include(c => c.Author)
        .Include(c => c.Lesson)
        .Include(c => c.Topic)
        .AsQueryable();

    // 3. Type filtresi
    if (type.HasValue)
        query = query.Where(c => c.Type == type.Value);

    // 4. Pagination
    var totalCount = await query.CountAsync();
    var contents = await query
        .OrderByDescending(c => c.CreatedAt)
        .Skip((page - 1) * limit)
        .Take(limit)
        .ToListAsync();

    // 5. DTO mapping
    var dtos = contents.Select(MapToContentDto).ToList();

    var response = new PagedResponse<ContentDto>
    {
        Data = dtos,
        Page = page,
        Limit = limit,
        TotalCount = totalCount,
        TotalPages = (int)Math.Ceiling(totalCount / (double)limit)
    };

    // 6. Cache'e yaz (10 dakika)
    await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

    return BaseResponse<PagedResponse<ContentDto>>.SuccessResponse(response);
}
```

**Cache Stratejisi:**
- Cache Key: `User:{userId}:Content:Page{page}:Type{type}`
- Cache Süresi: 10 dakika
- Invalidation: Kullanıcı yeni içerik oluşturduğunda veya güncellediğinde

---

#### [GET] `/api/social/content/my`

Kendi içeriklerimi listele.

**Query Parameters:** Aynı `GetUserContentsAsync` ile

**Operation Logic:**

```csharp
public async Task<BaseResponse<PagedResponse<ContentDto>>> GetMyContentsAsync(
    int page = 1, 
    int limit = 20, 
    ContentType? type = null,
    bool forceRefresh = false)
{
    var userId = _sessionService.GetUserId();
    return await GetUserContentsAsync(userId, page, limit, type, forceRefresh);
}
```

---

#### [GET] `/api/social/content/trending`

Trend içerikleri getir (son 24 saatte en çok etkileşim alan).

**Query Parameters:**
- `page`: Sayfa numarası (default: 1)
- `limit`: Sayfa başına kayıt (default: 20)
- `forceRefresh`: Cache bypass (default: false)

**Operation Logic:**

```csharp
public async Task<BaseResponse<List<ContentDto>>> GetTrendingContentsAsync(
    int page = 1, 
    int limit = 20,
    bool forceRefresh = false)
{
    // 1. Cache kontrolü (5 dakika - trend hızlı değişir)
    var cacheKey = $"Trending:Content:Page{page}";
    if (!forceRefresh)
    {
        var cached = await _cacheService.GetAsync<List<ContentDto>>(cacheKey);
        if (cached != null)
            return BaseResponse<List<ContentDto>>.SuccessResponse(cached);
    }

    // 2. Son 24 saat
    var yesterday = DateTime.UtcNow.AddDays(-1);

    // 3. Trending score hesapla: (LikeCount * 1.5) + (CommentCount * 2) + (ShareCount * 3) + (ViewCount * 0.1)
    var trendingContents = await _context.Contents
        .AsNoTracking()
        .Where(c => c.CreatedAt >= yesterday && !c.IsDeleted)
        .Include(c => c.Author)
        .Include(c => c.Lesson)
        .Include(c => c.Topic)
        .ToListAsync();

    var scored = trendingContents
        .Select(c => new
        {
            Content = c,
            Score = (c.LikeCount * 1.5) + (c.CommentCount * 2.0) + (c.ShareCount * 3.0) + (c.ViewCount * 0.1)
        })
        .OrderByDescending(x => x.Score)
        .Skip((page - 1) * limit)
        .Take(limit)
        .Select(x => MapToContentDto(x.Content))
        .ToList();

    // 4. Cache'e yaz (5 dakika)
    await _cacheService.SetAsync(cacheKey, scored, TimeSpan.FromMinutes(5));

    return BaseResponse<List<ContentDto>>.SuccessResponse(scored);
}
```

**Teknoloji Kullanımı:**
- **CacheService**: 5 dakika cache (trend hızlı değişir)
- **AsNoTracking()**: Read-only performans
- **Scoring Algorithm**: Kendi algoritmamız (like, comment, share, view ağırlıkları)

---

#### [GET] `/api/social/content/popular`

Popüler içerikler (tüm zamanlar).

**Query Parameters:**
- `page`: Sayfa numarası (default: 1)
- `limit`: Sayfa başına kayıt (default: 20)
- `forceRefresh`: Cache bypass (default: false)

**Operation Logic:**

```csharp
public async Task<BaseResponse<List<ContentDto>>> GetPopularContentsAsync(
    int page = 1, 
    int limit = 20,
    bool forceRefresh = false)
{
    // Cache: 15 dakika (popüler içerikler daha yavaş değişir)
    var cacheKey = $"Popular:Content:Page{page}";
    if (!forceRefresh)
    {
        var cached = await _cacheService.GetAsync<List<ContentDto>>(cacheKey);
        if (cached != null)
            return BaseResponse<List<ContentDto>>.SuccessResponse(cached);
    }

    var popularContents = await _context.Contents
        .AsNoTracking()
        .Where(c => !c.IsDeleted)
        .Include(c => c.Author)
        .Include(c => c.Lesson)
        .Include(c => c.Topic)
        .OrderByDescending(c => c.LikeCount + c.CommentCount + c.ShareCount)
        .Skip((page - 1) * limit)
        .Take(limit)
        .ToListAsync();

    var dtos = popularContents.Select(MapToContentDto).ToList();

    await _cacheService.SetAsync(cacheKey, dtos, TimeSpan.FromMinutes(15));

    return BaseResponse<List<ContentDto>>.SuccessResponse(dtos);
}
```

---

#### [GET] `/api/social/content/recommended`

Kişiselleştirilmiş öneriler.

**Query Parameters:**
- `page`: Sayfa numarası (default: 1)
- `limit`: Sayfa başına kayıt (default: 20)
- `forceRefresh`: Cache bypass (default: false)

**Operation Logic:**

```csharp
public async Task<BaseResponse<List<ContentDto>>> GetRecommendedContentsAsync(
    int page = 1, 
    int limit = 20,
    bool forceRefresh = false)
{
    var userId = _sessionService.GetUserId();

    // Cache: 10 dakika
    var cacheKey = $"Recommended:User:{userId}:Page{page}";
    if (!forceRefresh)
    {
        var cached = await _cacheService.GetAsync<List<ContentDto>>(cacheKey);
        if (cached != null)
            return BaseResponse<List<ContentDto>>.SuccessResponse(cached);
    }

    // 1. Kullanıcının ilgi alanlarını tespit et
    var userInteractions = await _context.Interactions
        .AsNoTracking()
        .Where(i => i.UserId == userId && (i.Type == InteractionType.Like || i.Type == InteractionType.Save))
        .Select(i => i.Content.LessonId)
        .Distinct()
        .ToListAsync();

    // 2. Kullanıcının takip ettiklerini al
    var followingIds = await _context.Follows
        .AsNoTracking()
        .Where(f => f.FollowerId == userId)
        .Select(f => f.FollowingId)
        .ToListAsync();

    // 3. Öneri algoritması: İlgi alanı + Takip edilenler + Popülerlik
    var candidateContents = await _context.Contents
        .AsNoTracking()
        .Where(c => !c.IsDeleted && 
                    c.CreatedAt >= DateTime.UtcNow.AddDays(-30) &&
                    (userInteractions.Contains(c.LessonId) || followingIds.Contains(c.AuthorId)))
        .Include(c => c.Author)
        .Include(c => c.Lesson)
        .Include(c => c.Topic)
        .ToListAsync();

    // 4. Scoring (FeedService kullan)
    var scored = candidateContents
        .Select(c => new
        {
            Content = c,
            Score = _feedService.CalculateRecommendationScore(c, userId, userInteractions, followingIds)
        })
        .OrderByDescending(x => x.Score)
        .Skip((page - 1) * limit)
        .Take(limit)
        .Select(x => MapToContentDto(x.Content))
        .ToList();

    await _cacheService.SetAsync(cacheKey, scored, TimeSpan.FromMinutes(10));

    return BaseResponse<List<ContentDto>>.SuccessResponse(scored);
}
```

**FeedService Kullanımı:**
- **FeedService**: Recommendation scoring algoritması
- **CacheService**: User-specific cache (her kullanıcı için ayrı öneriler)

---

#### [GET] `/api/social/content/by-tag/{tag}`

Belirli bir tag'e göre içerikler.

**Query Parameters:**
- `page`: Sayfa numarası (default: 1)
- `limit`: Sayfa başına kayıt (default: 20)
- `forceRefresh`: Cache bypass (default: false)

**Operation Logic:**

```csharp
public async Task<BaseResponse<List<ContentDto>>> GetContentsByTagAsync(
    string tag, 
    int page = 1, 
    int limit = 20,
    bool forceRefresh = false)
{
    // Cache: 10 dakika
    var cacheKey = $"Content:Tag:{tag}:Page{page}";
    if (!forceRefresh)
    {
        var cached = await _cacheService.GetAsync<List<ContentDto>>(cacheKey);
        if (cached != null)
            return BaseResponse<List<ContentDto>>.SuccessResponse(cached);
    }

    // Tag arama (JSON içinde)
    var tagLower = tag.ToLower();
    var contents = await _context.Contents
        .AsNoTracking()
        .Where(c => !c.IsDeleted && c.TagsJson.ToLower().Contains(tagLower))
        .Include(c => c.Author)
        .Include(c => c.Lesson)
        .Include(c => c.Topic)
        .OrderByDescending(c => c.CreatedAt)
        .Skip((page - 1) * limit)
        .Take(limit)
        .ToListAsync();

    var dtos = contents.Select(MapToContentDto).ToList();

    await _cacheService.SetAsync(cacheKey, dtos, TimeSpan.FromMinutes(10));

    return BaseResponse<List<ContentDto>>.SuccessResponse(dtos);
}
```

**Not:** RediSearch kullanıldığında bu query çok daha hızlı olacak.

---

#### [GET] `/api/social/content/by-lesson/{lessonId}`

Belirli bir derse göre içerikler.

**Query Parameters:**
- `page`: Sayfa numarası (default: 1)
- `limit`: Sayfa başına kayıt (default: 20)
- `forceRefresh`: Cache bypass (default: false)

**Operation Logic:**

```csharp
public async Task<BaseResponse<List<ContentDto>>> GetContentsByLessonAsync(
    int lessonId, 
    int page = 1, 
    int limit = 20,
    bool forceRefresh = false)
{
    var cacheKey = $"Content:Lesson:{lessonId}:Page{page}";
    if (!forceRefresh)
    {
        var cached = await _cacheService.GetAsync<List<ContentDto>>(cacheKey);
        if (cached != null)
            return BaseResponse<List<ContentDto>>.SuccessResponse(cached);
    }

    var contents = await _context.Contents
        .AsNoTracking()
        .Where(c => !c.IsDeleted && c.LessonId == lessonId)
        .Include(c => c.Author)
        .Include(c => c.Lesson)
        .Include(c => c.Topic)
        .OrderByDescending(c => c.CreatedAt)
        .Skip((page - 1) * limit)
        .Take(limit)
        .ToListAsync();

    var dtos = contents.Select(MapToContentDto).ToList();

    await _cacheService.SetAsync(cacheKey, dtos, TimeSpan.FromMinutes(10));

    return BaseResponse<List<ContentDto>>.SuccessResponse(dtos);
}
```

---

### 5.5. Interactions (Etkileşimler)

#### [POST] `/api/social/content/{id}/like`

İçeriği beğen.

**Teknoloji Kullanımı:**
- **SignalR**: Real-time like notification
- **CacheService**: Like count cache invalidation
- **Hangfire**: Like count denormalization (background job)

**Operation Logic:**

```csharp
public async Task<BaseResponse<bool>> LikeContentAsync(int contentId)
{
    var userId = _sessionService.GetUserId();

    // 1. Daha önce beğenmiş mi kontrol et
    var existingLike = await _context.Interactions
        .FirstOrDefaultAsync(i => i.ContentId == contentId && 
                                   i.UserId == userId && 
                                   i.Type == InteractionType.Like);

    if (existingLike != null)
        return BaseResponse<bool>.ErrorResponse("Already liked", ErrorCodes.ValidationFailed);

    // 2. Like oluştur
    var like = new Interaction
    {
        UserId = userId,
        ContentId = contentId,
        Type = InteractionType.Like,
        CreatedAt = DateTime.UtcNow,
        User = null!,
        Content = null!
    };

    _context.Interactions.Add(like);

    // 3. Like count'u güncelle (optimistic update)
    var content = await _context.Contents.FindAsync(contentId);
    if (content != null)
    {
        content.LikeCount++;
        await _context.SaveChangesAsync();
    }

    // 4. Cache invalidation
    await _cacheService.RemoveAsync($"Content:{contentId}:Detail");
    await _cacheService.RemoveByPatternAsync($"Content:{contentId}:*");
    await _cacheService.RemoveByPatternAsync("Feed:*");
    await _cacheService.RemoveByPatternAsync("Trending:*");
    await _cacheService.RemoveByPatternAsync("Popular:*");

    // 5. SignalR notification (content author'a)
    if (content != null && content.AuthorId != userId)
    {
        await _notificationService.SendNotificationAsync(
            content.AuthorId,
            "Yeni Beğeni",
            $"{_sessionService.GetUserFullName()} içeriğinizi beğendi",
            NotificationType.Like,
            $"/content/{contentId}"
        );
    }

    // 6. Audit log
    await _auditService.LogAsync(userId, "ContentLiked", 
        JsonSerializer.Serialize(new { ContentId = contentId }));

    return BaseResponse<bool>.SuccessResponse(true);
}
```

---

#### [DELETE] `/api/social/content/{id}/like`

Beğeniyi kaldır.

**Operation Logic:**

```csharp
public async Task<BaseResponse<bool>> UnlikeContentAsync(int contentId)
{
    var userId = _sessionService.GetUserId();

    var like = await _context.Interactions
        .FirstOrDefaultAsync(i => i.ContentId == contentId && 
                                   i.UserId == userId && 
                                   i.Type == InteractionType.Like);

    if (like == null)
        return BaseResponse<bool>.ErrorResponse("Not liked", ErrorCodes.NotFound);

    _context.Interactions.Remove(like);

    // Like count'u azalt
    var content = await _context.Contents.FindAsync(contentId);
    if (content != null && content.LikeCount > 0)
    {
        content.LikeCount--;
        await _context.SaveChangesAsync();
    }

    // Cache invalidation
    await _cacheService.RemoveAsync($"Content:{contentId}:Detail");
    await _cacheService.RemoveByPatternAsync($"Content:{contentId}:*");
    await _cacheService.RemoveByPatternAsync("Feed:*");

    // Audit log
    await _auditService.LogAsync(userId, "ContentUnliked", 
        JsonSerializer.Serialize(new { ContentId = contentId }));

    return BaseResponse<bool>.SuccessResponse(true);
}
```

---

#### [POST] `/api/social/content/{id}/save`

İçeriği kaydet (bookmark).

**Operation Logic:**

```csharp
public async Task<BaseResponse<bool>> SaveContentAsync(int contentId)
{
    var userId = _sessionService.GetUserId();

    // Daha önce kaydedilmiş mi?
    var existingSave = await _context.Interactions
        .FirstOrDefaultAsync(i => i.ContentId == contentId && 
                                   i.UserId == userId && 
                                   i.Type == InteractionType.Save);

    if (existingSave != null)
        return BaseResponse<bool>.ErrorResponse("Already saved", ErrorCodes.ValidationFailed);

    var save = new Interaction
    {
        UserId = userId,
        ContentId = contentId,
        Type = InteractionType.Save,
        CreatedAt = DateTime.UtcNow,
        User = null!,
        Content = null!
    };

    _context.Interactions.Add(save);
    await _context.SaveChangesAsync();

    // Cache invalidation (saved contents list)
    await _cacheService.RemoveByPatternAsync($"User:{userId}:Saved:*");

    await _auditService.LogAsync(userId, "ContentSaved", 
        JsonSerializer.Serialize(new { ContentId = contentId }));

    return BaseResponse<bool>.SuccessResponse(true);
}
```

---

#### [DELETE] `/api/social/content/{id}/save`

Kaydı kaldır.

**Operation Logic:**

```csharp
public async Task<BaseResponse<bool>> UnsaveContentAsync(int contentId)
{
    var userId = _sessionService.GetUserId();

    var save = await _context.Interactions
        .FirstOrDefaultAsync(i => i.ContentId == contentId && 
                                   i.UserId == userId && 
                                   i.Type == InteractionType.Save);

    if (save == null)
        return BaseResponse<bool>.ErrorResponse("Not saved", ErrorCodes.NotFound);

    _context.Interactions.Remove(save);
    await _context.SaveChangesAsync();

    await _cacheService.RemoveByPatternAsync($"User:{userId}:Saved:*");

    return BaseResponse<bool>.SuccessResponse(true);
}
```

---

#### [POST] `/api/social/content/{id}/report`

İçeriği raporla.

**Request:**

```json
{
  "reason": "Spam",
  "description": "Uygunsuz içerik"
}
```

**Operation Logic:**

```csharp
public async Task<BaseResponse<bool>> ReportContentAsync(int contentId, ReportContentRequest request)
{
    var userId = _sessionService.GetUserId();

    // Daha önce raporlanmış mı?
    var existingReport = await _context.Interactions
        .FirstOrDefaultAsync(i => i.ContentId == contentId && 
                                   i.UserId == userId && 
                                   i.Type == InteractionType.Report);

    if (existingReport != null)
        return BaseResponse<bool>.ErrorResponse("Already reported", ErrorCodes.ValidationFailed);

    var report = new Interaction
    {
        UserId = userId,
        ContentId = contentId,
        Type = InteractionType.Report,
        CreatedAt = DateTime.UtcNow,
        User = null!,
        Content = null!
    };

    _context.Interactions.Add(report);

    // Report kaydı (ayrı tablo olabilir - detaylı bilgi için)
    var contentReport = new ContentReport
    {
        ContentId = contentId,
        ReporterId = userId,
        Reason = request.Reason,
        Description = request.Description,
        Status = ReportStatus.Pending,
        CreatedAt = DateTime.UtcNow
    };

    _context.ContentReports.Add(contentReport);
    await _context.SaveChangesAsync();

    // Admin'lere bildirim (background job)
    BackgroundJob.Enqueue(() => NotifyAdminsAboutReportAsync(contentId, contentReport.Id));

    await _auditService.LogAsync(userId, "ContentReported", 
        JsonSerializer.Serialize(new { ContentId = contentId, Reason = request.Reason }));

    return BaseResponse<bool>.SuccessResponse(true);
}
```

**Yeni Model (ContentReport.cs):**

```csharp
public class ContentReport
{
    public int Id { get; set; }
    public int ContentId { get; set; }
    public Content Content { get; set; }
    public int ReporterId { get; set; }
    public User Reporter { get; set; }
    public string Reason { get; set; }
    public string? Description { get; set; }
    public ReportStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum ReportStatus : byte
{
    Pending = 1,
    Reviewed = 2,
    Resolved = 3,
    Rejected = 4
}
```

---

#### [GET] `/api/social/content/{id}/likes`

İçeriği beğenenler listesi.

**Query Parameters:**
- `page`: Sayfa numarası (default: 1)
- `limit`: Sayfa başına kayıt (default: 50)

**Operation Logic:**

```csharp
public async Task<BaseResponse<List<UserDto>>> GetContentLikesAsync(int contentId, int page = 1, int limit = 50)
{
    var cacheKey = $"Content:{contentId}:Likes:Page{page}";
    var cached = await _cacheService.GetAsync<List<UserDto>>(cacheKey);
    if (cached != null)
        return BaseResponse<List<UserDto>>.SuccessResponse(cached);

    var likes = await _context.Interactions
        .AsNoTracking()
        .Where(i => i.ContentId == contentId && i.Type == InteractionType.Like)
        .Include(i => i.User)
        .OrderByDescending(i => i.CreatedAt)
        .Skip((page - 1) * limit)
        .Take(limit)
        .Select(i => MapToUserDto(i.User))
        .ToListAsync();

    await _cacheService.SetAsync(cacheKey, likes, TimeSpan.FromMinutes(5));

    return BaseResponse<List<UserDto>>.SuccessResponse(likes);
}
```

---

#### [GET] `/api/social/content/{id}/saves`

İçeriği kaydedenler listesi (sadece içerik sahibi görebilir).

**Operation Logic:**

```csharp
public async Task<BaseResponse<List<UserDto>>> GetContentSavesAsync(int contentId)
{
    var userId = _sessionService.GetUserId();

    // Sadece içerik sahibi görebilir
    var content = await _context.Contents
        .AsNoTracking()
        .FirstOrDefaultAsync(c => c.Id == contentId);

    if (content == null || content.AuthorId != userId)
        return BaseResponse<List<UserDto>>.ErrorResponse("Unauthorized", ErrorCodes.AccessDenied);

    var saves = await _context.Interactions
        .AsNoTracking()
        .Where(i => i.ContentId == contentId && i.Type == InteractionType.Save)
        .Include(i => i.User)
        .OrderByDescending(i => i.CreatedAt)
        .Select(i => MapToUserDto(i.User))
        .ToListAsync();

    return BaseResponse<List<UserDto>>.SuccessResponse(saves);
}
```

---

#### [POST] `/api/social/content/{id}/share`

İçeriği paylaş (share count artır).

**Operation Logic:**

```csharp
public async Task<BaseResponse<bool>> ShareContentAsync(int contentId)
{
    var userId = _sessionService.GetUserId();

    var content = await _context.Contents.FindAsync(contentId);
    if (content == null)
        return BaseResponse<bool>.ErrorResponse("Content not found", ErrorCodes.NotFound);

    // Share count artır
    content.ShareCount++;
    await _context.SaveChangesAsync();

    // Cache invalidation
    await _cacheService.RemoveAsync($"Content:{contentId}:Detail");
    await _cacheService.RemoveByPatternAsync("Feed:*");
    await _cacheService.RemoveByPatternAsync("Trending:*");

    // Notification (content author'a)
    if (content.AuthorId != userId)
    {
        await _notificationService.SendNotificationAsync(
            content.AuthorId,
            "İçerik Paylaşıldı",
            $"{_sessionService.GetUserFullName()} içeriğinizi paylaştı",
            NotificationType.Share,
            $"/content/{contentId}"
        );
    }

    await _auditService.LogAsync(userId, "ContentShared", 
        JsonSerializer.Serialize(new { ContentId = contentId }));

    return BaseResponse<bool>.SuccessResponse(true);
}
```

---

### 5.6. Comments (Yorumlar)

#### [POST] `/api/social/content/{id}/comment`

Yorum yap.

**Request:**

```json
{
  "text": "Çok güzel bir soru, teşekkürler!",
  "imageUrl": "https://cdn.../solution.jpg"
}
```

**Operation Logic:**

```csharp
public async Task<BaseResponse<CommentDto>> AddCommentAsync(int contentId, AddCommentRequest request)
{
    var userId = _sessionService.GetUserId();

    // Content var mı?
    var content = await _context.Contents
        .Include(c => c.Author)
        .FirstOrDefaultAsync(c => c.Id == contentId && !c.IsDeleted);

    if (content == null)
        return BaseResponse<CommentDto>.ErrorResponse("Content not found", ErrorCodes.NotFound);

    // Comment oluştur
    var comment = new Comment
    {
        ContentId = contentId,
        UserId = userId,
        Text = request.Text,
        ImageUrl = request.ImageUrl,
        IsCorrectAnswer = false,
        CreatedAt = DateTime.UtcNow,
        Content = null!,
        User = null!
    };

    _context.Comments.Add(comment);

    // Comment count artır
    content.CommentCount++;
    await _context.SaveChangesAsync();

    // DTO mapping (User bilgisi ile)
    await _context.Entry(comment).Reference(c => c.User).LoadAsync();
    var dto = MapToCommentDto(comment);

    // Cache invalidation
    await _cacheService.RemoveAsync($"Content:{contentId}:Detail");
    await _cacheService.RemoveByPatternAsync($"Content:{contentId}:Comments:*");
    await _cacheService.RemoveByPatternAsync("Feed:*");

    // SignalR notification (real-time comment)
    await _chatHub.Clients.Group($"Content_{contentId}")
        .SendAsync("NewComment", dto);

    // Notification (content author'a)
    if (content.AuthorId != userId)
    {
        await _notificationService.SendNotificationAsync(
            content.AuthorId,
            "Yeni Yorum",
            $"{_sessionService.GetUserFullName()} içeriğinize yorum yaptı",
            NotificationType.Comment,
            $"/content/{contentId}"
        );
    }

    await _auditService.LogAsync(userId, "CommentAdded", 
        JsonSerializer.Serialize(new { ContentId = contentId, CommentId = comment.Id }));

    return BaseResponse<CommentDto>.SuccessResponse(dto);
}
```

**SignalR Hub Güncellemesi (ChatHub.cs):**

```csharp
public async Task JoinContentGroup(int contentId)
{
    await Groups.AddToGroupAsync(Context.ConnectionId, $"Content_{contentId}");
}

public async Task LeaveContentGroup(int contentId)
{
    await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Content_{contentId}");
}
```

---

#### [GET] `/api/social/content/{id}/comments`

İçeriğin yorumlarını getir.

**Query Parameters:**
- `page`: Sayfa numarası (default: 1)
- `limit`: Sayfa başına kayıt (default: 20)
- `forceRefresh`: Cache bypass (default: false)

**Operation Logic:**

```csharp
public async Task<BaseResponse<PagedResponse<CommentDto>>> GetContentCommentsAsync(
    int contentId, 
    int page = 1, 
    int limit = 20,
    bool forceRefresh = false)
{
    var cacheKey = $"Content:{contentId}:Comments:Page{page}";
    if (!forceRefresh)
    {
        var cached = await _cacheService.GetAsync<PagedResponse<CommentDto>>(cacheKey);
        if (cached != null)
            return BaseResponse<PagedResponse<CommentDto>>.SuccessResponse(cached);
    }

    var query = _context.Comments
        .AsNoTracking()
        .Where(c => c.ContentId == contentId)
        .Include(c => c.User)
        .AsQueryable();

    var totalCount = await query.CountAsync();
    var comments = await query
        .OrderByDescending(c => c.IsCorrectAnswer) // Correct answer önce
        .ThenByDescending(c => c.CreatedAt)
        .Skip((page - 1) * limit)
        .Take(limit)
        .ToListAsync();

    var dtos = comments.Select(MapToCommentDto).ToList();

    var response = new PagedResponse<CommentDto>
    {
        Data = dtos,
        Page = page,
        Limit = limit,
        TotalCount = totalCount,
        TotalPages = (int)Math.Ceiling(totalCount / (double)limit)
    };

    await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5));

    return BaseResponse<PagedResponse<CommentDto>>.SuccessResponse(response);
}
```

---

#### [PUT] `/api/social/comment/{id}`

Yorum düzenle (sadece sahibi).

**Operation Logic:**

```csharp
public async Task<BaseResponse<bool>> UpdateCommentAsync(int commentId, UpdateCommentRequest request)
{
    var userId = _sessionService.GetUserId();

    var comment = await _context.Comments
        .FirstOrDefaultAsync(c => c.Id == commentId && c.UserId == userId);

    if (comment == null)
        return BaseResponse<bool>.ErrorResponse("Comment not found or unauthorized", ErrorCodes.NotFound);

    if (!string.IsNullOrEmpty(request.Text))
        comment.Text = request.Text;
    if (request.ImageUrl != null)
        comment.ImageUrl = request.ImageUrl;

    await _context.SaveChangesAsync();

    // Cache invalidation
    await _cacheService.RemoveByPatternAsync($"Content:{comment.ContentId}:Comments:*");
    await _cacheService.RemoveAsync($"Content:{comment.ContentId}:Detail");

    await _auditService.LogAsync(userId, "CommentUpdated", 
        JsonSerializer.Serialize(new { CommentId = commentId }));

    return BaseResponse<bool>.SuccessResponse(true);
}
```

---

#### [DELETE] `/api/social/comment/{id}`

Yorum sil (sadece sahibi veya içerik sahibi).

**Operation Logic:**

```csharp
public async Task<BaseResponse<bool>> DeleteCommentAsync(int commentId)
{
    var userId = _sessionService.GetUserId();

    var comment = await _context.Comments
        .Include(c => c.Content)
        .FirstOrDefaultAsync(c => c.Id == commentId);

    if (comment == null)
        return BaseResponse<bool>.ErrorResponse("Comment not found", ErrorCodes.NotFound);

    // Sadece yorum sahibi veya içerik sahibi silebilir
    if (comment.UserId != userId && comment.Content.AuthorId != userId)
        return BaseResponse<bool>.ErrorResponse("Unauthorized", ErrorCodes.AccessDenied);

    _context.Comments.Remove(comment);

    // Comment count azalt
    var content = await _context.Contents.FindAsync(comment.ContentId);
    if (content != null && content.CommentCount > 0)
    {
        content.CommentCount--;
        await _context.SaveChangesAsync();
    }

    // Cache invalidation
    await _cacheService.RemoveByPatternAsync($"Content:{comment.ContentId}:Comments:*");
    await _cacheService.RemoveAsync($"Content:{comment.ContentId}:Detail");
    await _cacheService.RemoveByPatternAsync("Feed:*");

    await _auditService.LogAsync(userId, "CommentDeleted", 
        JsonSerializer.Serialize(new { CommentId = commentId }));

    return BaseResponse<bool>.SuccessResponse(true);
}
```

---

#### [POST] `/api/social/comment/{id}/like`

Yorumu beğen.

**Operation Logic:**

```csharp
public async Task<BaseResponse<bool>> LikeCommentAsync(int commentId)
{
    var userId = _sessionService.GetUserId();

    // CommentLike model'i gerekli (Interaction'tan ayrı veya Interaction kullanılabilir)
    var existingLike = await _context.CommentLikes
        .FirstOrDefaultAsync(cl => cl.CommentId == commentId && cl.UserId == userId);

    if (existingLike != null)
        return BaseResponse<bool>.ErrorResponse("Already liked", ErrorCodes.ValidationFailed);

    var like = new CommentLike
    {
        CommentId = commentId,
        UserId = userId,
        CreatedAt = DateTime.UtcNow
    };

    _context.CommentLikes.Add(like);

    var comment = await _context.Comments.FindAsync(commentId);
    if (comment != null)
    {
        comment.LikeCount++;
        await _context.SaveChangesAsync();
    }

    // Notification (comment author'a)
    if (comment != null && comment.UserId != userId)
    {
        await _notificationService.SendNotificationAsync(
            comment.UserId,
            "Yorum Beğenildi",
            $"{_sessionService.GetUserFullName()} yorumunuzu beğendi",
            NotificationType.Like,
            $"/content/{comment.ContentId}"
        );
    }

    return BaseResponse<bool>.SuccessResponse(true);
}
```

**Yeni Model (CommentLike.cs):**

```csharp
public class CommentLike
{
    public int Id { get; set; }
    public int CommentId { get; set; }
    public Comment Comment { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

**Comment Model Güncellemesi:**

```csharp
public class Comment
{
    // ... mevcut property'ler ...
    public int LikeCount { get; set; } = 0;
    public ICollection<CommentLike> Likes { get; set; }
}
```

---

#### [POST] `/api/social/comment/{id}/reply`

Yorum yanıtla (nested comments).

**Request:**

```json
{
  "text": "Haklısınız, teşekkürler!",
  "imageUrl": null
}
```

**Operation Logic:**

```csharp
public async Task<BaseResponse<CommentDto>> ReplyToCommentAsync(int commentId, AddCommentRequest request)
{
    var userId = _sessionService.GetUserId();

    var parentComment = await _context.Comments
        .Include(c => c.Content)
        .FirstOrDefaultAsync(c => c.Id == commentId);

    if (parentComment == null)
        return BaseResponse<CommentDto>.ErrorResponse("Comment not found", ErrorCodes.NotFound);

    // Reply oluştur
    var reply = new Comment
    {
        ContentId = parentComment.ContentId,
        UserId = userId,
        Text = request.Text,
        ImageUrl = request.ImageUrl,
        ParentCommentId = commentId, // Nested comment için
        IsCorrectAnswer = false,
        CreatedAt = DateTime.UtcNow,
        Content = null!,
        User = null!
    };

    _context.Comments.Add(reply);

    // Comment count artır (content'e)
    var content = await _context.Contents.FindAsync(parentComment.ContentId);
    if (content != null)
    {
        content.CommentCount++;
        await _context.SaveChangesAsync();
    }

    await _context.Entry(reply).Reference(c => c.User).LoadAsync();
    var dto = MapToCommentDto(reply);

    // Cache invalidation
    await _cacheService.RemoveByPatternAsync($"Content:{parentComment.ContentId}:Comments:*");
    await _cacheService.RemoveAsync($"Content:{parentComment.ContentId}:Detail");

    // SignalR notification
    await _chatHub.Clients.Group($"Content_{parentComment.ContentId}")
        .SendAsync("NewCommentReply", dto);

    // Notification (parent comment author'a)
    if (parentComment.UserId != userId)
    {
        await _notificationService.SendNotificationAsync(
            parentComment.UserId,
            "Yorum Yanıtı",
            $"{_sessionService.GetUserFullName()} yorumunuza yanıt verdi",
            NotificationType.Comment,
            $"/content/{parentComment.ContentId}"
        );
    }

    await _auditService.LogAsync(userId, "CommentReplied", 
        JsonSerializer.Serialize(new { CommentId = commentId, ReplyId = reply.Id }));

    return BaseResponse<CommentDto>.SuccessResponse(dto);
}
```

**Comment Model Güncellemesi:**

```csharp
public class Comment
{
    // ... mevcut property'ler ...
    public int? ParentCommentId { get; set; }
    public Comment? ParentComment { get; set; }
    public ICollection<Comment> Replies { get; set; }
}
```

---

#### [GET] `/api/social/comment/{id}/replies`

Yorum yanıtlarını getir.

**Operation Logic:**

```csharp
public async Task<BaseResponse<List<CommentDto>>> GetCommentRepliesAsync(int commentId)
{
    var cacheKey = $"Comment:{commentId}:Replies";
    var cached = await _cacheService.GetAsync<List<CommentDto>>(cacheKey);
    if (cached != null)
        return BaseResponse<List<CommentDto>>.SuccessResponse(cached);

    var replies = await _context.Comments
        .AsNoTracking()
        .Where(c => c.ParentCommentId == commentId)
        .Include(c => c.User)
        .OrderBy(c => c.CreatedAt)
        .ToListAsync();

    var dtos = replies.Select(MapToCommentDto).ToList();

    await _cacheService.SetAsync(cacheKey, dtos, TimeSpan.FromMinutes(5));

    return BaseResponse<List<CommentDto>>.SuccessResponse(dtos);
}
```

---

#### [PUT] `/api/social/content/{id}/mark-solved`

Soruyu çözüldü olarak işaretle (sadece soru sahibi).

**Request:**

```json
{
  "commentId": 123  // Doğru cevap olarak işaretlenecek yorum ID
}
```

**Operation Logic:**

```csharp
public async Task<BaseResponse<bool>> MarkContentAsSolvedAsync(int contentId, MarkSolvedRequest request)
{
    var userId = _sessionService.GetUserId();

    var content = await _context.Contents
        .FirstOrDefaultAsync(c => c.Id == contentId && c.AuthorId == userId && c.Type == ContentType.Question);

    if (content == null)
        return BaseResponse<bool>.ErrorResponse("Content not found or not a question", ErrorCodes.NotFound);

    // Tüm yorumları "correct answer" olmaktan çıkar
    var previousCorrect = await _context.Comments
        .Where(c => c.ContentId == contentId && c.IsCorrectAnswer)
        .ToListAsync();

    foreach (var comment in previousCorrect)
    {
        comment.IsCorrectAnswer = false;
    }

    // Yeni doğru cevabı işaretle
    if (request.CommentId.HasValue)
    {
        var correctComment = await _context.Comments
            .FirstOrDefaultAsync(c => c.Id == request.CommentId.Value && c.ContentId == contentId);

        if (correctComment != null)
        {
            correctComment.IsCorrectAnswer = true;
        }
    }

    content.IsSolved = true;
    await _context.SaveChangesAsync();

    // Cache invalidation
    await _cacheService.RemoveAsync($"Content:{contentId}:Detail");
    await _cacheService.RemoveByPatternAsync($"Content:{contentId}:Comments:*");
    await _cacheService.RemoveByPatternAsync("Feed:*");

    // Notification (doğru cevabı veren kullanıcıya)
    if (request.CommentId.HasValue)
    {
        var correctComment = await _context.Comments
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == request.CommentId.Value);

        if (correctComment != null && correctComment.UserId != userId)
        {
            await _notificationService.SendNotificationAsync(
                correctComment.UserId,
                "Doğru Cevap",
                "Cevabınız doğru olarak işaretlendi",
                NotificationType.Achievement,
                $"/content/{contentId}"
            );
        }
    }

    await _auditService.LogAsync(userId, "ContentMarkedSolved", 
        JsonSerializer.Serialize(new { ContentId = contentId, CommentId = request.CommentId }));

    return BaseResponse<bool>.SuccessResponse(true);
}
```

---

### 5.7. Follow System (Takip Sistemi)

#### [POST] `/api/social/user/{userId}/follow`

Kullanıcıyı takip et.

**Operation Logic:**

```csharp
public async Task<BaseResponse<bool>> FollowUserAsync(int targetUserId)
{
    var userId = _sessionService.GetUserId();

    if (userId == targetUserId)
        return BaseResponse<bool>.ErrorResponse("Cannot follow yourself", ErrorCodes.ValidationFailed);

    // Zaten takip ediliyor mu?
    var existingFollow = await _context.Follows
        .FirstOrDefaultAsync(f => f.FollowerId == userId && f.FollowingId == targetUserId);

    if (existingFollow != null)
        return BaseResponse<bool>.ErrorResponse("Already following", ErrorCodes.ValidationFailed);

    var follow = new Follow
    {
        FollowerId = userId,
        FollowingId = targetUserId,
        CreatedAt = DateTime.UtcNow,
        Follower = null!,
        Following = null!
    };

    _context.Follows.Add(follow);
    await _context.SaveChangesAsync();

    // Cache invalidation
    await _cacheService.RemoveByPatternAsync($"User:{userId}:Following:*");
    await _cacheService.RemoveByPatternAsync($"User:{targetUserId}:Followers:*");
    await _cacheService.RemoveByPatternAsync($"User:{userId}:Feed:*");
    await _cacheService.RemoveByPatternAsync($"Recommended:*");

    // Notification (takip edilen kullanıcıya)
    await _notificationService.SendNotificationAsync(
        targetUserId,
        "Yeni Takipçi",
        $"{_sessionService.GetUserFullName()} sizi takip etmeye başladı",
        NotificationType.Follow,
        $"/user/{userId}"
    );

    // SignalR notification (real-time)
    await _chatHub.Clients.Group($"User_{targetUserId}")
        .SendAsync("NewFollower", new { FollowerId = userId, FollowerName = _sessionService.GetUserFullName() });

    await _auditService.LogAsync(userId, "UserFollowed", 
        JsonSerializer.Serialize(new { FollowingId = targetUserId }));

    return BaseResponse<bool>.SuccessResponse(true);
}
```

---

#### [DELETE] `/api/social/user/{userId}/follow`

Takibi bırak.

**Operation Logic:**

```csharp
public async Task<BaseResponse<bool>> UnfollowUserAsync(int targetUserId)
{
    var userId = _sessionService.GetUserId();

    var follow = await _context.Follows
        .FirstOrDefaultAsync(f => f.FollowerId == userId && f.FollowingId == targetUserId);

    if (follow == null)
        return BaseResponse<bool>.ErrorResponse("Not following", ErrorCodes.NotFound);

    _context.Follows.Remove(follow);
    await _context.SaveChangesAsync();

    // Cache invalidation
    await _cacheService.RemoveByPatternAsync($"User:{userId}:Following:*");
    await _cacheService.RemoveByPatternAsync($"User:{targetUserId}:Followers:*");
    await _cacheService.RemoveByPatternAsync($"User:{userId}:Feed:*");

    await _auditService.LogAsync(userId, "UserUnfollowed", 
        JsonSerializer.Serialize(new { FollowingId = targetUserId }));

    return BaseResponse<bool>.SuccessResponse(true);
}
```

---

#### [GET] `/api/social/user/{userId}/followers`

Kullanıcının takipçilerini listele.

**Query Parameters:**
- `page`: Sayfa numarası (default: 1)
- `limit`: Sayfa başına kayıt (default: 50)
- `forceRefresh`: Cache bypass (default: false)

**Operation Logic:**

```csharp
public async Task<BaseResponse<PagedResponse<UserDto>>> GetUserFollowersAsync(
    int userId, 
    int page = 1, 
    int limit = 50,
    bool forceRefresh = false)
{
    var cacheKey = $"User:{userId}:Followers:Page{page}";
    if (!forceRefresh)
    {
        var cached = await _cacheService.GetAsync<PagedResponse<UserDto>>(cacheKey);
        if (cached != null)
            return BaseResponse<PagedResponse<UserDto>>.SuccessResponse(cached);
    }

    var query = _context.Follows
        .AsNoTracking()
        .Where(f => f.FollowingId == userId)
        .Include(f => f.Follower)
        .AsQueryable();

    var totalCount = await query.CountAsync();
    var follows = await query
        .OrderByDescending(f => f.CreatedAt)
        .Skip((page - 1) * limit)
        .Take(limit)
        .ToListAsync();

    var dtos = follows.Select(f => MapToUserDto(f.Follower)).ToList();

    var response = new PagedResponse<UserDto>
    {
        Data = dtos,
        Page = page,
        Limit = limit,
        TotalCount = totalCount,
        TotalPages = (int)Math.Ceiling(totalCount / (double)limit)
    };

    await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

    return BaseResponse<PagedResponse<UserDto>>.SuccessResponse(response);
}
```

---

#### [GET] `/api/social/user/{userId}/following`

Kullanıcının takip ettiklerini listele.

**Query Parameters:** Aynı `GetUserFollowersAsync` ile

**Operation Logic:**

```csharp
public async Task<BaseResponse<PagedResponse<UserDto>>> GetUserFollowingAsync(
    int userId, 
    int page = 1, 
    int limit = 50,
    bool forceRefresh = false)
{
    var cacheKey = $"User:{userId}:Following:Page{page}";
    if (!forceRefresh)
    {
        var cached = await _cacheService.GetAsync<PagedResponse<UserDto>>(cacheKey);
        if (cached != null)
            return BaseResponse<PagedResponse<UserDto>>.SuccessResponse(cached);
    }

    var query = _context.Follows
        .AsNoTracking()
        .Where(f => f.FollowerId == userId)
        .Include(f => f.Following)
        .AsQueryable();

    var totalCount = await query.CountAsync();
    var follows = await query
        .OrderByDescending(f => f.CreatedAt)
        .Skip((page - 1) * limit)
        .Take(limit)
        .ToListAsync();

    var dtos = follows.Select(f => MapToUserDto(f.Following)).ToList();

    var response = new PagedResponse<UserDto>
    {
        Data = dtos,
        Page = page,
        Limit = limit,
        TotalCount = totalCount,
        TotalPages = (int)Math.Ceiling(totalCount / (double)limit)
    };

    await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

    return BaseResponse<PagedResponse<UserDto>>.SuccessResponse(response);
}
```

---

#### [GET] `/api/social/user/{userId}/mutual-follows`

Ortak takipler (karşılıklı takip edilenler).

**Operation Logic:**

```csharp
public async Task<BaseResponse<List<UserDto>>> GetMutualFollowsAsync(int userId)
{
    var currentUserId = _sessionService.GetUserId();

    // Her iki yönde de takip edilenler
    var mutualFollows = await _context.Follows
        .AsNoTracking()
        .Where(f1 => f1.FollowerId == currentUserId &&
                     _context.Follows.Any(f2 => f2.FollowerId == userId && f2.FollowingId == f1.FollowingId))
        .Include(f => f.Following)
        .Select(f => MapToUserDto(f.Following))
        .ToListAsync();

    return BaseResponse<List<UserDto>>.SuccessResponse(mutualFollows);
}
```

---

#### [GET] `/api/social/user/recommendations`

Önerilen kullanıcılar (takip edilmeyen, benzer ilgi alanlarına sahip).

**Query Parameters:**
- `limit`: Öneri sayısı (default: 10)

**Operation Logic:**

```csharp
public async Task<BaseResponse<List<UserDto>>> GetUserRecommendationsAsync(int limit = 10)
{
    var userId = _sessionService.GetUserId();

    var cacheKey = $"User:{userId}:Recommendations";
    var cached = await _cacheService.GetAsync<List<UserDto>>(cacheKey);
    if (cached != null)
        return BaseResponse<List<UserDto>>.SuccessResponse(cached);

    // 1. Zaten takip edilenleri al
    var followingIds = await _context.Follows
        .AsNoTracking()
        .Where(f => f.FollowerId == userId)
        .Select(f => f.FollowingId)
        .ToListAsync();

    // 2. Kullanıcının ilgi alanlarını tespit et
    var userInterestedLessons = await _context.Interactions
        .AsNoTracking()
        .Where(i => i.UserId == userId && (i.Type == InteractionType.Like || i.Type == InteractionType.Save))
        .Select(i => i.Content.LessonId)
        .Distinct()
        .ToListAsync();

    // 3. Benzer ilgi alanlarına sahip kullanıcıları bul
    var recommendedUsers = await _context.Users
        .AsNoTracking()
        .Where(u => u.Id != userId && 
                    !followingIds.Contains(u.Id) &&
                    _context.Interactions.Any(i => i.UserId == u.Id && 
                                                    (i.Type == InteractionType.Like || i.Type == InteractionType.Save) &&
                                                    userInterestedLessons.Contains(i.Content.LessonId)))
        .OrderByDescending(u => _context.Interactions.Count(i => i.UserId == u.Id && 
                                                                  userInterestedLessons.Contains(i.Content.LessonId)))
        .Take(limit)
        .Select(u => MapToUserDto(u))
        .ToListAsync();

    await _cacheService.SetAsync(cacheKey, recommendedUsers, TimeSpan.FromMinutes(30));

    return BaseResponse<List<UserDto>>.SuccessResponse(recommendedUsers);
}
```

---

### 5.8. User Profile Social (Sosyal Profil)

#### [GET] `/api/social/user/{userId}/profile`

Kullanıcının sosyal profil bilgileri.

**Query Parameters:**
- `forceRefresh`: Cache bypass (default: false)

**Response:**

```json
{
  "success": true,
  "data": {
    "userId": 123,
    "fullName": "Ahmet Yılmaz",
    "profileImageUrl": "https://cdn.../avatar.jpg",
    "bio": "Matematik öğretmeni",
    "contentCount": 45,
    "followerCount": 120,
    "followingCount": 85,
    "isFollowing": true,
    "isBlocked": false
  }
}
```

**Operation Logic:**

```csharp
public async Task<BaseResponse<SocialProfileDto>> GetSocialProfileAsync(int userId, bool forceRefresh = false)
{
    var currentUserId = _sessionService.GetUserId();

    var cacheKey = $"SocialProfile:User:{userId}:Viewer:{currentUserId}";
    if (!forceRefresh)
    {
        var cached = await _cacheService.GetAsync<SocialProfileDto>(cacheKey);
        if (cached != null)
            return BaseResponse<SocialProfileDto>.SuccessResponse(cached);
    }

    var user = await _context.Users
        .AsNoTracking()
        .FirstOrDefaultAsync(u => u.Id == userId);

    if (user == null)
        return BaseResponse<SocialProfileDto>.ErrorResponse("User not found", ErrorCodes.NotFound);

    // İstatistikler
    var contentCount = await _context.Contents
        .AsNoTracking()
        .CountAsync(c => c.AuthorId == userId && !c.IsDeleted);

    var followerCount = await _context.Follows
        .AsNoTracking()
        .CountAsync(f => f.FollowingId == userId);

    var followingCount = await _context.Follows
        .AsNoTracking()
        .CountAsync(f => f.FollowerId == userId);

    // Takip durumu
    var isFollowing = await _context.Follows
        .AsNoTracking()
        .AnyAsync(f => f.FollowerId == currentUserId && f.FollowingId == userId);

    // Engelleme durumu
    var isBlocked = await _context.Blocks
        .AsNoTracking()
        .AnyAsync(b => (b.BlockerId == currentUserId && b.BlockedId == userId) ||
                       (b.BlockerId == userId && b.BlockedId == currentUserId));

    var dto = new SocialProfileDto
    {
        UserId = userId,
        FullName = user.FullName,
        ProfileImageUrl = user.ProfileImageUrl,
        Bio = user.Bio,
        ContentCount = contentCount,
        FollowerCount = followerCount,
        FollowingCount = followingCount,
        IsFollowing = isFollowing,
        IsBlocked = isBlocked
    };

    await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(15));

    return BaseResponse<SocialProfileDto>.SuccessResponse(dto);
}
```

---

#### [GET] `/api/social/user/{userId}/statistics`

Kullanıcının sosyal istatistikleri.

**Response:**

```json
{
  "success": true,
  "data": {
    "totalLikes": 1250,
    "totalComments": 340,
    "totalShares": 89,
    "averageLikesPerContent": 27.8,
    "mostLikedContent": {
      "id": 456,
      "title": "Zorlu Matematik Sorusu",
      "likeCount": 156
    },
    "topLesson": {
      "id": 1,
      "name": "Matematik",
      "contentCount": 25
    }
  }
}
```

**Operation Logic:**

```csharp
public async Task<BaseResponse<SocialStatisticsDto>> GetUserSocialStatisticsAsync(int userId)
{
    var cacheKey = $"SocialStatistics:User:{userId}";
    var cached = await _cacheService.GetAsync<SocialStatisticsDto>(cacheKey);
    if (cached != null)
        return BaseResponse<SocialStatisticsDto>.SuccessResponse(cached);

    var contents = await _context.Contents
        .AsNoTracking()
        .Where(c => c.AuthorId == userId && !c.IsDeleted)
        .ToListAsync();

    var totalLikes = contents.Sum(c => c.LikeCount);
    var totalComments = contents.Sum(c => c.CommentCount);
    var totalShares = contents.Sum(c => c.ShareCount);
    var contentCount = contents.Count;

    var averageLikesPerContent = contentCount > 0 ? (double)totalLikes / contentCount : 0;

    var mostLikedContent = contents
        .OrderByDescending(c => c.LikeCount)
        .FirstOrDefault();

    // En çok içerik paylaşılan ders
    var topLesson = await _context.Contents
        .AsNoTracking()
        .Where(c => c.AuthorId == userId && !c.IsDeleted && c.LessonId.HasValue)
        .GroupBy(c => c.LessonId)
        .Select(g => new { LessonId = g.Key, Count = g.Count() })
        .OrderByDescending(x => x.Count)
        .FirstOrDefaultAsync();

    var dto = new SocialStatisticsDto
    {
        TotalLikes = totalLikes,
        TotalComments = totalComments,
        TotalShares = totalShares,
        AverageLikesPerContent = averageLikesPerContent,
        MostLikedContent = mostLikedContent != null ? new ContentSummaryDto
        {
            Id = mostLikedContent.Id,
            Title = mostLikedContent.Title,
            LikeCount = mostLikedContent.LikeCount
        } : null,
        TopLesson = topLesson != null ? new LessonSummaryDto
        {
            Id = topLesson.LessonId!.Value,
            Name = await _context.Lessons.Where(l => l.Id == topLesson.LessonId).Select(l => l.Name).FirstOrDefaultAsync(),
            ContentCount = topLesson.Count
        } : null
    };

    await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(30));

    return BaseResponse<SocialStatisticsDto>.SuccessResponse(dto);
}
```

---

#### [POST] `/api/social/user/{userId}/block`

Kullanıcıyı engelle.

**Operation Logic:**

```csharp
public async Task<BaseResponse<bool>> BlockUserAsync(int targetUserId)
{
    var userId = _sessionService.GetUserId();

    if (userId == targetUserId)
        return BaseResponse<bool>.ErrorResponse("Cannot block yourself", ErrorCodes.ValidationFailed);

    // Zaten engellenmiş mi?
    var existingBlock = await _context.Blocks
        .FirstOrDefaultAsync(b => b.BlockerId == userId && b.BlockedId == targetUserId);

    if (existingBlock != null)
        return BaseResponse<bool>.ErrorResponse("Already blocked", ErrorCodes.ValidationFailed);

    var block = new Block
    {
        BlockerId = userId,
        BlockedId = targetUserId,
        CreatedAt = DateTime.UtcNow
    };

    _context.Blocks.Add(block);

    // Takip varsa kaldır (her iki yönde)
    var follows = await _context.Follows
        .Where(f => (f.FollowerId == userId && f.FollowingId == targetUserId) ||
                    (f.FollowerId == targetUserId && f.FollowingId == userId))
        .ToListAsync();

    _context.Follows.RemoveRange(follows);

    await _context.SaveChangesAsync();

    // Cache invalidation
    await _cacheService.RemoveByPatternAsync($"User:{userId}:*");
    await _cacheService.RemoveByPatternAsync($"User:{targetUserId}:*");
    await _cacheService.RemoveByPatternAsync($"Feed:*");

    await _auditService.LogAsync(userId, "UserBlocked", 
        JsonSerializer.Serialize(new { BlockedId = targetUserId }));

    return BaseResponse<bool>.SuccessResponse(true);
}
```

**Yeni Model (Block.cs):**

```csharp
public class Block
{
    public int Id { get; set; }
    public int BlockerId { get; set; }
    public User Blocker { get; set; }
    public int BlockedId { get; set; }
    public User Blocked { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

---

#### [DELETE] `/api/social/user/{userId}/block`

Engeli kaldır.

**Operation Logic:**

```csharp
public async Task<BaseResponse<bool>> UnblockUserAsync(int targetUserId)
{
    var userId = _sessionService.GetUserId();

    var block = await _context.Blocks
        .FirstOrDefaultAsync(b => b.BlockerId == userId && b.BlockedId == targetUserId);

    if (block == null)
        return BaseResponse<bool>.ErrorResponse("Not blocked", ErrorCodes.NotFound);

    _context.Blocks.Remove(block);
    await _context.SaveChangesAsync();

    await _cacheService.RemoveByPatternAsync($"User:{userId}:*");
    await _cacheService.RemoveByPatternAsync($"User:{targetUserId}:*");

    await _auditService.LogAsync(userId, "UserUnblocked", 
        JsonSerializer.Serialize(new { UnblockedId = targetUserId }));

    return BaseResponse<bool>.SuccessResponse(true);
}
```

---

#### [POST] `/api/social/user/{userId}/mute`

Kullanıcıyı sessizleştir (bildirimleri kapat ama takip etmeye devam et).

**Operation Logic:**

```csharp
public async Task<BaseResponse<bool>> MuteUserAsync(int targetUserId)
{
    var userId = _sessionService.GetUserId();

    if (userId == targetUserId)
        return BaseResponse<bool>.ErrorResponse("Cannot mute yourself", ErrorCodes.ValidationFailed);

    // Zaten sessizleştirilmiş mi?
    var existingMute = await _context.Mutes
        .FirstOrDefaultAsync(m => m.UserId == userId && m.MutedUserId == targetUserId);

    if (existingMute != null)
        return BaseResponse<bool>.ErrorResponse("Already muted", ErrorCodes.ValidationFailed);

    var mute = new Mute
    {
        UserId = userId,
        MutedUserId = targetUserId,
        CreatedAt = DateTime.UtcNow
    };

    _context.Mutes.Add(mute);
    await _context.SaveChangesAsync();

    // Cache invalidation
    await _cacheService.RemoveByPatternAsync($"User:{userId}:Feed:*");
    await _cacheService.RemoveByPatternAsync($"User:{userId}:Muted:*");

    await _auditService.LogAsync(userId, "UserMuted", 
        JsonSerializer.Serialize(new { MutedUserId = targetUserId }));

    return BaseResponse<bool>.SuccessResponse(true);
}
```

**Yeni Model (Mute.cs):**

```csharp
public class Mute
{
    public int Id { get; set; }
    public int UserId { get; set; } // Sessizleştiren
    public User User { get; set; }
    public int MutedUserId { get; set; } // Sessizleştirilen
    public User MutedUser { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

**Not:** Block'tan farklı olarak, Mute edilen kullanıcıların içerikleri feed'de gösterilmez ama takip ilişkisi devam eder.

---

#### [DELETE] `/api/social/user/{userId}/mute`

Sessizleştirmeyi kaldır.

**Operation Logic:**

```csharp
public async Task<BaseResponse<bool>> UnmuteUserAsync(int targetUserId)
{
    var userId = _sessionService.GetUserId();

    var mute = await _context.Mutes
        .FirstOrDefaultAsync(m => m.UserId == userId && m.MutedUserId == targetUserId);

    if (mute == null)
        return BaseResponse<bool>.ErrorResponse("Not muted", ErrorCodes.NotFound);

    _context.Mutes.Remove(mute);
    await _context.SaveChangesAsync();

    // Cache invalidation
    await _cacheService.RemoveByPatternAsync($"User:{userId}:Feed:*");
    await _cacheService.RemoveByPatternAsync($"User:{userId}:Muted:*");

    await _auditService.LogAsync(userId, "UserUnmuted", 
        JsonSerializer.Serialize(new { UnmutedUserId = targetUserId }));

    return BaseResponse<bool>.SuccessResponse(true);
}
```

---

#### [GET] `/api/social/user/muted`

Sessizleştirilen kullanıcılar listesi.

**Query Parameters:**
- `page`: Sayfa numarası (default: 1)
- `limit`: Sayfa başına kayıt (default: 50)
- `forceRefresh`: Cache bypass (default: false)

**Operation Logic:**

```csharp
public async Task<BaseResponse<PagedResponse<UserDto>>> GetMutedUsersAsync(
    int page = 1,
    int limit = 50,
    bool forceRefresh = false)
{
    var userId = _sessionService.GetUserId();

    var cacheKey = $"User:{userId}:Muted:Page{page}";
    if (!forceRefresh)
    {
        var cached = await _cacheService.GetAsync<PagedResponse<UserDto>>(cacheKey);
        if (cached != null)
            return BaseResponse<PagedResponse<UserDto>>.SuccessResponse(cached);
    }

    var query = _context.Mutes
        .AsNoTracking()
        .Where(m => m.UserId == userId)
        .Include(m => m.MutedUser)
        .AsQueryable();

    var totalCount = await query.CountAsync();
    var mutes = await query
        .OrderByDescending(m => m.CreatedAt)
        .Skip((page - 1) * limit)
        .Take(limit)
        .ToListAsync();

    var dtos = mutes.Select(m => MapToUserDto(m.MutedUser)).ToList();

    var response = new PagedResponse<UserDto>
    {
        Data = dtos,
        Page = page,
        Limit = limit,
        TotalCount = totalCount,
        TotalPages = (int)Math.Ceiling(totalCount / (double)limit)
    };

    await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

    return BaseResponse<PagedResponse<UserDto>>.SuccessResponse(response);
}
```

**Teknoloji Kullanımı:**
- **CacheService**: Muted users listesi 10 dakika cache'lenir
- **AsNoTracking()**: Read-only query'ler için performans
- **AuditService**: Mute/Unmute işlemleri loglanır
- **Feed Filtreleme**: Feed oluşturulurken mute edilen kullanıcıların içerikleri filtrelenir

**Feed'de Kullanımı:**

Feed oluşturulurken mute edilen kullanıcıların içerikleri gösterilmez:

```csharp
// FeedOperations.cs - GetPersonalizedFeedAsync içinde
var mutedUserIds = await _context.Mutes
    .AsNoTracking()
    .Where(m => m.UserId == userId)
    .Select(m => m.MutedUserId)
    .ToListAsync();

var contents = await _context.Contents
    .AsNoTracking()
    .Where(c => !c.IsDeleted && 
                !mutedUserIds.Contains(c.AuthorId) && // Mute edilenler filtrelenir
                followingIds.Contains(c.AuthorId))
    .ToListAsync();
```

---

#### [GET] `/api/social/user/{userId}/saved`

Kullanıcının kaydettiği içerikler.

**Query Parameters:**
- `page`: Sayfa numarası (default: 1)
- `limit`: Sayfa başına kayıt (default: 20)
- `forceRefresh`: Cache bypass (default: false)

**Operation Logic:**

```csharp
public async Task<BaseResponse<PagedResponse<ContentDto>>> GetSavedContentsAsync(
    int userId,
    int page = 1,
    int limit = 20,
    bool forceRefresh = false)
{
    var currentUserId = _sessionService.GetUserId();

    // Sadece kendi kaydettiklerini görebilir
    if (userId != currentUserId)
        return BaseResponse<PagedResponse<ContentDto>>.ErrorResponse("Unauthorized", ErrorCodes.AccessDenied);

    var cacheKey = $"User:{userId}:Saved:Page{page}";
    if (!forceRefresh)
    {
        var cached = await _cacheService.GetAsync<PagedResponse<ContentDto>>(cacheKey);
        if (cached != null)
            return BaseResponse<PagedResponse<ContentDto>>.SuccessResponse(cached);
    }

    var query = _context.Interactions
        .AsNoTracking()
        .Where(i => i.UserId == userId && i.Type == InteractionType.Save)
        .Include(i => i.Content)
            .ThenInclude(c => c.Author)
        .Include(i => i.Content)
            .ThenInclude(c => c.Lesson)
        .Include(i => i.Content)
            .ThenInclude(c => c.Topic)
        .Where(i => !i.Content.IsDeleted)
        .AsQueryable();

    var totalCount = await query.CountAsync();
    var saves = await query
        .OrderByDescending(i => i.CreatedAt)
        .Skip((page - 1) * limit)
        .Take(limit)
        .ToListAsync();

    var dtos = saves.Select(s => MapToContentDto(s.Content)).ToList();

    var response = new PagedResponse<ContentDto>
    {
        Data = dtos,
        Page = page,
        Limit = limit,
        TotalCount = totalCount,
        TotalPages = (int)Math.Ceiling(totalCount / (double)limit)
    };

    await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

    return BaseResponse<PagedResponse<ContentDto>>.SuccessResponse(response);
}
```

---

### 5.9. Hashtags & Tags

#### [GET] `/api/social/hashtags/trending`

Trend hashtag'ler (son 7 günde en çok kullanılan).

**Query Parameters:**
- `limit`: Hashtag sayısı (default: 20)

**Operation Logic:**

```csharp
public async Task<BaseResponse<List<HashtagDto>>> GetTrendingHashtagsAsync(int limit = 20)
{
    var cacheKey = $"Trending:Hashtags";
    var cached = await _cacheService.GetAsync<List<HashtagDto>>(cacheKey);
    if (cached != null)
        return BaseResponse<List<HashtagDto>>.SuccessResponse(cached);

    // Son 7 gün
    var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

    // Tüm içeriklerden hashtag'leri çıkar ve say
    var allContents = await _context.Contents
        .AsNoTracking()
        .Where(c => !c.IsDeleted && c.CreatedAt >= sevenDaysAgo && !string.IsNullOrEmpty(c.TagsJson))
        .Select(c => c.TagsJson)
        .ToListAsync();

    // Hashtag'leri parse et ve say
    var hashtagCounts = new Dictionary<string, int>();
    foreach (var tagsJson in allContents)
    {
        var tags = JsonSerializer.Deserialize<List<string>>(tagsJson) ?? new List<string>();
        foreach (var tag in tags)
        {
            var normalizedTag = tag.ToLower().TrimStart('#');
            if (!string.IsNullOrEmpty(normalizedTag))
            {
                hashtagCounts.TryGetValue(normalizedTag, out var count);
                hashtagCounts[normalizedTag] = count + 1;
            }
        }
    }

    var trending = hashtagCounts
        .OrderByDescending(kvp => kvp.Value)
        .Take(limit)
        .Select(kvp => new HashtagDto
        {
            Tag = kvp.Key,
            UsageCount = kvp.Value
        })
        .ToList();

    await _cacheService.SetAsync(cacheKey, trending, TimeSpan.FromMinutes(30));

    return BaseResponse<List<HashtagDto>>.SuccessResponse(trending);
}
```

**Not:** RediSearch kullanıldığında bu çok daha hızlı olacak (FT.AGGREGATE ile).

---

#### [GET] `/api/social/hashtags/{tag}`

Hashtag detayı.

**Response:**

```json
{
  "success": true,
  "data": {
    "tag": "matematik",
    "usageCount": 1250,
    "contentCount": 450,
    "trending": true
  }
}
```

**Operation Logic:**

```csharp
public async Task<BaseResponse<HashtagDetailDto>> GetHashtagDetailAsync(string tag)
{
    var normalizedTag = tag.ToLower().TrimStart('#');

    var cacheKey = $"Hashtag:{normalizedTag}:Detail";
    var cached = await _cacheService.GetAsync<HashtagDetailDto>(cacheKey);
    if (cached != null)
        return BaseResponse<HashtagDetailDto>.SuccessResponse(cached);

    // Son 7 günde kullanım sayısı
    var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
    var recentUsageCount = await _context.Contents
        .AsNoTracking()
        .Where(c => !c.IsDeleted && 
                    c.CreatedAt >= sevenDaysAgo && 
                    c.TagsJson.ToLower().Contains(normalizedTag))
        .CountAsync();

    // Toplam içerik sayısı
    var totalContentCount = await _context.Contents
        .AsNoTracking()
        .Where(c => !c.IsDeleted && c.TagsJson.ToLower().Contains(normalizedTag))
        .CountAsync();

    // Trending mi? (son 7 günde 50+ kullanım)
    var isTrending = recentUsageCount >= 50;

    var dto = new HashtagDetailDto
    {
        Tag = normalizedTag,
        UsageCount = recentUsageCount,
        ContentCount = totalContentCount,
        Trending = isTrending
    };

    await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(15));

    return BaseResponse<HashtagDetailDto>.SuccessResponse(dto);
}
```

---

#### [GET] `/api/social/hashtags/{tag}/contents`

Hashtag'e göre içerikler (zaten `GetContentsByTagAsync` ile aynı).

---

#### [GET] `/api/social/hashtags/search`

Hashtag arama.

**Query Parameters:**
- `query`: Arama metni
- `limit`: Sonuç sayısı (default: 10)

**Operation Logic:**

```csharp
public async Task<BaseResponse<List<HashtagDto>>> SearchHashtagsAsync(string query, int limit = 10)
{
    if (string.IsNullOrWhiteSpace(query))
        return BaseResponse<List<HashtagDto>>.ErrorResponse("Query required", ErrorCodes.ValidationFailed);

    var queryLower = query.ToLower().Trim();

    // Tüm unique hashtag'leri bul ve filtrele
    var allTags = await _context.Contents
        .AsNoTracking()
        .Where(c => !c.IsDeleted && !string.IsNullOrEmpty(c.TagsJson))
        .Select(c => c.TagsJson)
        .ToListAsync();

    var uniqueTags = new HashSet<string>();
    foreach (var tagsJson in allTags)
    {
        var tags = JsonSerializer.Deserialize<List<string>>(tagsJson) ?? new List<string>();
        foreach (var tag in tags)
        {
            var normalizedTag = tag.ToLower().TrimStart('#');
            if (normalizedTag.Contains(queryLower))
            {
                uniqueTags.Add(normalizedTag);
            }
        }
    }

    var results = uniqueTags
        .Take(limit)
        .Select(tag => new HashtagDto
        {
            Tag = tag,
            UsageCount = 0 // Detaylı sayım için GetHashtagDetailAsync kullanılabilir
        })
        .ToList();

    return BaseResponse<List<HashtagDto>>.SuccessResponse(results);
}
```

**Not:** RediSearch kullanıldığında bu çok daha hızlı olacak.

---

### 5.10. Advanced Feed (Gelişmiş Feed)

#### [GET] `/api/social/feed/following`

Sadece takip edilenlerin içerikleri.

**Query Parameters:**
- `page`: Sayfa numarası (default: 1)
- `limit`: Sayfa başına kayıt (default: 20)
- `forceRefresh`: Cache bypass (default: false)

**Operation Logic:**

```csharp
public async Task<BaseResponse<List<ContentDto>>> GetFollowingFeedAsync(
    int page = 1,
    int limit = 20,
    bool forceRefresh = false)
{
    var userId = _sessionService.GetUserId();

    var cacheKey = $"User:{userId}:Feed:Following:Page{page}";
    if (!forceRefresh)
    {
        var cached = await _cacheService.GetAsync<List<ContentDto>>(cacheKey);
        if (cached != null)
            return BaseResponse<List<ContentDto>>.SuccessResponse(cached);
    }

    // Takip edilenler
    var followingIds = await _context.Follows
        .AsNoTracking()
        .Where(f => f.FollowerId == userId)
        .Select(f => f.FollowingId)
        .ToListAsync();

    if (!followingIds.Any())
        return BaseResponse<List<ContentDto>>.SuccessResponse(new List<ContentDto>());

    var contents = await _context.Contents
        .AsNoTracking()
        .Where(c => !c.IsDeleted && followingIds.Contains(c.AuthorId))
        .Include(c => c.Author)
        .Include(c => c.Lesson)
        .Include(c => c.Topic)
        .OrderByDescending(c => c.CreatedAt)
        .Skip((page - 1) * limit)
        .Take(limit)
        .ToListAsync();

    var dtos = contents.Select(MapToContentDto).ToList();

    await _cacheService.SetAsync(cacheKey, dtos, TimeSpan.FromMinutes(5));

    return BaseResponse<List<ContentDto>>.SuccessResponse(dtos);
}
```

---

#### [GET] `/api/social/feed/for-you`

"Senin için" feed (önerilen içerikler - zaten `GetRecommendedContentsAsync` ile aynı).

---

#### [GET] `/api/social/feed/trending`

Trend feed (zaten `GetTrendingContentsAsync` ile aynı).

---

#### [GET] `/api/social/feed/saved`

Kaydedilenler feed (zaten `GetSavedContentsAsync` ile aynı).

---

### 5.11. Search & Discovery (Gelişmiş Arama)

#### [GET] `/api/social/search/contents`

Gelişmiş içerik arama.

**Query Parameters:**
- `query`: Arama metni
- `lessonId`: Ders filtresi (opsiyonel)
- `topicId`: Konu filtresi (opsiyonel)
- `difficulty`: Zorluk filtresi (opsiyonel)
- `type`: ContentType filtresi (opsiyonel)
- `sortBy`: `popular`, `recent`, `trending` (default: popular)
- `page`: Sayfa numarası (default: 1)
- `limit`: Sayfa başına kayıt (default: 20)
- `forceRefresh`: Cache bypass (default: false)

**Teknoloji Kullanımı:**
- **RediSearch**: Full-text search için (eğer aktifse)
- **EF Core**: RediSearch yoksa fallback
- **CacheService**: Arama sonuçları cache'lenir

**Operation Logic:**

```csharp
public async Task<BaseResponse<PagedResponse<ContentDto>>> SearchContentsAsync(
    string? query,
    int? lessonId,
    int? topicId,
    DifficultyLevel? difficulty,
    ContentType? type,
    string sortBy = "popular",
    int page = 1,
    int limit = 20,
    bool forceRefresh = false)
{
    // Cache key oluştur
    var cacheKey = $"Search:Contents:Q{query}:L{lessonId}:T{topicId}:D{difficulty}:Type{type}:Sort{sortBy}:Page{page}";
    if (!forceRefresh)
    {
        var cached = await _cacheService.GetAsync<PagedResponse<ContentDto>>(cacheKey);
        if (cached != null)
            return BaseResponse<PagedResponse<ContentDto>>.SuccessResponse(cached);
    }

    // RediSearch kullan (eğer aktifse)
    if (_redisSearchHelper != null && !string.IsNullOrEmpty(query))
    {
        var contentIds = await _redisSearchHelper.SearchContentIdsAsync(
            query, lessonId, topicId, difficulty, type, sortBy, page, limit);
        
        if (contentIds != null && contentIds.Any())
        {
            // Redis'ten gelen ID'lerle DB'den detaylı bilgileri çek
            var contents = await _context.Contents
                .AsNoTracking()
                .Where(c => contentIds.Contains(c.Id) && !c.IsDeleted)
                .Include(c => c.Author)
                .Include(c => c.Lesson)
                .Include(c => c.Topic)
                .ToListAsync();
            
            // RediSearch'ün döndürdüğü sırayı koru
            var orderedContents = contentIds
                .Select(id => contents.FirstOrDefault(c => c.Id == id))
                .Where(c => c != null)
                .Select(MapToContentDto)
                .ToList();
            
            var pagedResponse = new PagedResponse<ContentDto>
            {
                Items = orderedContents,
                TotalCount = await _context.Contents
                    .AsNoTracking()
                    .Where(c => !c.IsDeleted && 
                        (string.IsNullOrEmpty(query) || c.Title.Contains(query) || c.Description.Contains(query)) &&
                        (!lessonId.HasValue || c.LessonId == lessonId) &&
                        (!topicId.HasValue || c.TopicId == topicId) &&
                        (!difficulty.HasValue || c.Difficulty == difficulty) &&
                        (!type.HasValue || c.Type == type))
                    .CountAsync(),
                Page = page,
                PageSize = limit,
                TotalPages = (int)Math.Ceiling((double)totalCount / limit)
            };
            
            await _cacheService.SetAsync(cacheKey, pagedResponse, TimeSpan.FromMinutes(5));
            return BaseResponse<PagedResponse<ContentDto>>.SuccessResponse(pagedResponse);
        }
    }
    
    // Fallback: EF Core ile arama (RediSearch yoksa veya query boşsa)
    var queryable = _context.Contents
        .AsNoTracking()
        .Where(c => !c.IsDeleted)
        .Include(c => c.Author)
        .Include(c => c.Lesson)
        .Include(c => c.Topic)
        .AsQueryable();
    
    // Filtreler
    if (!string.IsNullOrEmpty(query))
    {
        queryable = queryable.Where(c => 
            c.Title.Contains(query) || 
            c.Description.Contains(query) ||
            c.TagsJson.Contains(query));
    }
    
    if (lessonId.HasValue)
        queryable = queryable.Where(c => c.LessonId == lessonId);
    
    if (topicId.HasValue)
        queryable = queryable.Where(c => c.TopicId == topicId);
    
    if (difficulty.HasValue)
        queryable = queryable.Where(c => c.Difficulty == difficulty);
    
    if (type.HasValue)
        queryable = queryable.Where(c => c.Type == type);
    
    // Sıralama
    queryable = sortBy switch
    {
        "recent" => queryable.OrderByDescending(c => c.CreatedAt),
        "trending" => queryable.OrderByDescending(c => 
            c.LikeCount * 2 + c.CommentCount * 3 + 
            (DateTime.UtcNow - c.CreatedAt).TotalHours < 24 ? 10 : 0),
        _ => queryable.OrderByDescending(c => c.LikeCount + c.CommentCount * 2)
    };
    
    var totalCount = await queryable.CountAsync();
    
    var contentsList = await queryable
        .Skip((page - 1) * limit)
        .Take(limit)
        .ToListAsync();
    
    var dtos = contentsList.Select(MapToContentDto).ToList();
    
    var response = new PagedResponse<ContentDto>
    {
        Items = dtos,
        TotalCount = totalCount,
        Page = page,
        PageSize = limit,
        TotalPages = (int)Math.Ceiling((double)totalCount / limit)
    };
    
    await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5));
    return BaseResponse<PagedResponse<ContentDto>>.SuccessResponse(response);
}
```

**Cache Invalidation:**
- Content oluşturulduğunda/güncellendiğinde/silindiğinde: `InvalidateContentCacheAsync` çağrılır
- Arama sonuçları cache'i: `Search:Contents:*` pattern'i ile temizlenir

**Teknoloji Kullanımı:**
- **RediSearch**: Full-text search için (eğer aktifse) - 50-100x daha hızlı
- **EF Core**: Fallback olarak kullanılır (RediSearch yoksa veya query boşsa)
- **CacheService**: Arama sonuçları 5 dakika cache'lenir
- **AsNoTracking()**: Read-only query'ler için performans optimizasyonu

**Hangfire Job (Opsiyonel):**
- Content indexing job: Yeni içerikler RediSearch'e index'lenir (arka planda)

---

### 5.12. Content Analytics & Insights (İçerik Analitiği)

#### [GET] `/api/social/content/{id}/analytics`

İçerik analitiği (sadece içerik sahibi veya admin).

**Query Parameters:**
- `period`: `day`, `week`, `month`, `all` (default: week)

**Response:**

```json
{
  "success": true,
  "data": {
    "contentId": 123,
    "views": 1250,
    "likes": 45,
    "comments": 12,
    "saves": 8,
    "shares": 3,
    "engagementRate": 5.44,
    "viewsByDay": [
      { "date": "2024-01-15", "views": 120 },
      { "date": "2024-01-16", "views": 150 }
    ],
    "topEngagers": [
      { "userId": 5, "username": "user5", "interactions": 8 }
    ]
  },
  "error": null,
  "errorCode": null
}
```

**Operation Logic:**

```csharp
public async Task<BaseResponse<ContentAnalyticsDto>> GetContentAnalyticsAsync(
    int contentId,
    int userId,
    string period = "week")
{
    var content = await _context.Contents
        .AsNoTracking()
        .FirstOrDefaultAsync(c => c.Id == contentId);
    
    if (content == null)
        return BaseResponse<ContentAnalyticsDto>.ErrorResponse(
            "Content not found", ErrorCodes.NotFound);
    
    // Yetki kontrolü
    if (content.AuthorId != userId && !await IsAdminAsync(userId))
        return BaseResponse<ContentAnalyticsDto>.ErrorResponse(
            "Unauthorized", ErrorCodes.Unauthorized);
    
    var cacheKey = $"Content:Analytics:{contentId}:{period}";
    var cached = await _cacheService.GetAsync<ContentAnalyticsDto>(cacheKey);
    if (cached != null)
        return BaseResponse<ContentAnalyticsDto>.SuccessResponse(cached);
    
    var startDate = period switch
    {
        "day" => DateTime.UtcNow.AddDays(-1),
        "week" => DateTime.UtcNow.AddDays(-7),
        "month" => DateTime.UtcNow.AddDays(-30),
        _ => DateTime.MinValue
    };
    
    var views = await _context.Interactions
        .AsNoTracking()
        .Where(i => i.ContentId == contentId && 
                   i.Type == InteractionType.View &&
                   i.CreatedAt >= startDate)
        .CountAsync();
    
    var likes = await _context.Interactions
        .AsNoTracking()
        .Where(i => i.ContentId == contentId && 
                   i.Type == InteractionType.Like &&
                   i.CreatedAt >= startDate)
        .CountAsync();
    
    var comments = await _context.Comments
        .AsNoTracking()
        .Where(c => c.ContentId == contentId && 
                   !c.IsDeleted &&
                   c.CreatedAt >= startDate)
        .CountAsync();
    
    var saves = await _context.Interactions
        .AsNoTracking()
        .Where(i => i.ContentId == contentId && 
                   i.Type == InteractionType.Save &&
                   i.CreatedAt >= startDate)
        .CountAsync();
    
    var shares = await _context.Interactions
        .AsNoTracking()
        .Where(i => i.ContentId == contentId && 
                   i.Type == InteractionType.Share &&
                   i.CreatedAt >= startDate)
        .CountAsync();
    
    var engagementRate = views > 0 
        ? ((likes + comments + saves + shares) / (double)views) * 100 
        : 0;
    
    // Günlük görüntülenme istatistikleri
    var viewsByDay = await _context.Interactions
        .AsNoTracking()
        .Where(i => i.ContentId == contentId && 
                   i.Type == InteractionType.View &&
                   i.CreatedAt >= startDate)
        .GroupBy(i => i.CreatedAt.Date)
        .Select(g => new { Date = g.Key, Views = g.Count() })
        .OrderBy(x => x.Date)
        .ToListAsync();
    
    // En çok etkileşimde bulunan kullanıcılar
    var topEngagers = await _context.Interactions
        .AsNoTracking()
        .Where(i => i.ContentId == contentId && 
                   i.CreatedAt >= startDate)
        .GroupBy(i => i.UserId)
        .Select(g => new { 
            UserId = g.Key, 
            Interactions = g.Count() 
        })
        .OrderByDescending(x => x.Interactions)
        .Take(10)
        .Join(_context.Users,
            e => e.UserId,
            u => u.Id,
            (e, u) => new { 
                UserId = u.Id, 
                Username = u.Username, 
                Interactions = e.Interactions 
            })
        .ToListAsync();
    
    var analytics = new ContentAnalyticsDto
    {
        ContentId = contentId,
        Views = views,
        Likes = likes,
        Comments = comments,
        Saves = saves,
        Shares = shares,
        EngagementRate = Math.Round(engagementRate, 2),
        ViewsByDay = viewsByDay.Select(v => new DailyViewDto
        {
            Date = v.Date.ToString("yyyy-MM-dd"),
            Views = v.Views
        }).ToList(),
        TopEngagers = topEngagers.Select(e => new TopEngagerDto
        {
            UserId = e.UserId,
            Username = e.Username,
            Interactions = e.Interactions
        }).ToList()
    };
    
    await _cacheService.SetAsync(cacheKey, analytics, TimeSpan.FromMinutes(10));
    return BaseResponse<ContentAnalyticsDto>.SuccessResponse(analytics);
}
```

**Cache Invalidation:**
- Yeni interaction oluşturulduğunda: `InvalidateContentAnalyticsCacheAsync(contentId)` çağrılır

**Teknoloji Kullanımı:**
- **EF Core**: Aggregation queries (Count, GroupBy)
- **CacheService**: Analytics verileri 10 dakika cache'lenir
- **AsNoTracking()**: Read-only query'ler için performans optimizasyonu

---

### 5.13. Content Moderation (İçerik Moderasyonu)

#### [POST] `/api/social/content/{id}/report`

İçeriği şikayet et.

**Request:**

```json
{
  "reason": "spam",
  "description": "Spam içerik"
}
```

**Report Reasons:**
- `spam`: Spam içerik
- `inappropriate`: Uygunsuz içerik
- `harassment`: Taciz
- `copyright`: Telif hakkı ihlali
- `other`: Diğer

**Operation Logic:**

```csharp
public async Task<BaseResponse<string>> ReportContentAsync(
    int contentId,
    int userId,
    ReportContentRequest request)
{
    var content = await _context.Contents
        .FirstOrDefaultAsync(c => c.Id == contentId);
    
    if (content == null)
        return BaseResponse<string>.ErrorResponse(
            "Content not found", ErrorCodes.NotFound);
    
    // Aynı kullanıcı aynı içeriği birden fazla kez şikayet edemez
    var existingReport = await _context.ContentReports
        .FirstOrDefaultAsync(r => r.ContentId == contentId && r.UserId == userId);
    
    if (existingReport != null)
        return BaseResponse<string>.ErrorResponse(
            "You have already reported this content", ErrorCodes.ValidationFailed);
    
    var report = new ContentReport
    {
        ContentId = contentId,
        UserId = userId,
        Reason = request.Reason,
        Description = request.Description,
        Status = ReportStatus.Pending,
        CreatedAt = DateTime.UtcNow
    };
    
    _context.ContentReports.Add(report);
    await _context.SaveChangesAsync();
    
    // Audit log
    await _auditService.LogAsync(userId, "ContentReported", 
        $"Content {contentId} reported: {request.Reason}");
    
    // Admin'lere bildirim gönder (Hangfire job ile)
    BackgroundJob.Enqueue<NotificationJob>(job => 
        job.NotifyAdminsAboutReportAsync(contentId, userId, request.Reason));
    
    return BaseResponse<string>.SuccessResponse("Content reported successfully");
}
```

**Model:**

```csharp
public class ContentReport
{
    public int Id { get; set; }
    public int ContentId { get; set; }
    public Content Content { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public string Reason { get; set; }
    public string? Description { get; set; }
    public ReportStatus Status { get; set; } // Pending, Reviewed, Resolved, Rejected
    public int? ReviewedBy { get; set; }
    public User? Reviewer { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

#### [GET] `/api/admin/content/reports`

Admin: Tüm şikayetleri listele (pagination, filtreleme).

**Query Parameters:**
- `status`: `pending`, `reviewed`, `resolved`, `rejected`
- `page`: Sayfa numarası (default: 1)
- `limit`: Sayfa başına kayıt (default: 20)
- `forceRefresh`: Cache bypass (default: false)

**Operation Logic:**

```csharp
public async Task<BaseResponse<PagedResponse<ContentReportDto>>> GetContentReportsAsync(
    int adminId,
    string? status,
    int page = 1,
    int limit = 20,
    bool forceRefresh = false)
{
    // Admin yetkisi kontrolü
    if (!await IsAdminAsync(adminId))
        return BaseResponse<PagedResponse<ContentReportDto>>.ErrorResponse(
            "Unauthorized", ErrorCodes.Unauthorized);
    
    var cacheKey = $"Admin:ContentReports:Status{status}:Page{page}";
    if (!forceRefresh)
    {
        var cached = await _cacheService.GetAsync<PagedResponse<ContentReportDto>>(cacheKey);
        if (cached != null)
            return BaseResponse<PagedResponse<ContentReportDto>>.SuccessResponse(cached);
    }
    
    var queryable = _context.ContentReports
        .AsNoTracking()
        .Include(r => r.Content)
        .Include(r => r.User)
        .Include(r => r.Reviewer)
        .AsQueryable();
    
    if (!string.IsNullOrEmpty(status))
    {
        var statusEnum = Enum.Parse<ReportStatus>(status, true);
        queryable = queryable.Where(r => r.Status == statusEnum);
    }
    
    var totalCount = await queryable.CountAsync();
    
    var reports = await queryable
        .OrderByDescending(r => r.CreatedAt)
        .Skip((page - 1) * limit)
        .Take(limit)
        .ToListAsync();
    
    var dtos = reports.Select(MapToContentReportDto).ToList();
    
    var response = new PagedResponse<ContentReportDto>
    {
        Items = dtos,
        TotalCount = totalCount,
        Page = page,
        PageSize = limit,
        TotalPages = (int)Math.Ceiling((double)totalCount / limit)
    };
    
    await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5));
    return BaseResponse<PagedResponse<ContentReportDto>>.SuccessResponse(response);
}
```

#### [POST] `/api/admin/content/report/{id}/review`

Admin: Şikayeti incele ve karar ver.

**Request:**

```json
{
  "action": "resolve",
  "notes": "İçerik uygunsuz, silindi"
}
```

**Actions:**
- `resolve`: Şikayet haklı, içerik silindi/gizlendi
- `reject`: Şikayet haksız, içerik korundu

**Operation Logic:**

```csharp
public async Task<BaseResponse<string>> ReviewContentReportAsync(
    int reportId,
    int adminId,
    ReviewReportRequest request)
{
    // Admin yetkisi kontrolü
    if (!await IsAdminAsync(adminId))
        return BaseResponse<string>.ErrorResponse(
            "Unauthorized", ErrorCodes.Unauthorized);
    
    var report = await _context.ContentReports
        .Include(r => r.Content)
        .FirstOrDefaultAsync(r => r.Id == reportId);
    
    if (report == null)
        return BaseResponse<string>.ErrorResponse(
            "Report not found", ErrorCodes.NotFound);
    
    if (report.Status != ReportStatus.Pending)
        return BaseResponse<string>.ErrorResponse(
            "Report already reviewed", ErrorCodes.ValidationFailed);
    
    report.Status = request.Action == "resolve" 
        ? ReportStatus.Resolved 
        : ReportStatus.Rejected;
    report.ReviewedBy = adminId;
    report.ReviewedAt = DateTime.UtcNow;
    report.ReviewNotes = request.Notes;
    
    if (request.Action == "resolve")
    {
        // İçeriği sil veya gizle
        report.Content.IsDeleted = true;
        report.Content.DeletedAt = DateTime.UtcNow;
        
        // İçerik sahibine bildirim gönder
        await _notificationService.SendAsync(
            report.Content.AuthorId,
            "ContentRemoved",
            "Your content was removed due to a report",
            $"/content/{report.ContentId}");
    }
    
    await _context.SaveChangesAsync();
    
    // Cache invalidation
    await _cacheService.InvalidateContentCacheAsync(report.ContentId);
    await _cacheService.InvalidateAdminCacheAsync();
    
    // Audit log
    await _auditService.LogAsync(adminId, "ContentReportReviewed", 
        $"Report {reportId} reviewed: {request.Action}");
    
    return BaseResponse<string>.SuccessResponse("Report reviewed successfully");
}
```

**Cache Invalidation:**
- Report oluşturulduğunda: Admin cache'i temizlenir
- Report review edildiğinde: Content cache'i ve admin cache'i temizlenir

**Teknoloji Kullanımı:**
- **EF Core**: Include ile ilişkili veriler çekilir
- **CacheService**: Report listesi 5 dakika cache'lenir
- **Hangfire**: Admin bildirimleri arka planda gönderilir
- **AuditService**: Tüm moderasyon işlemleri loglanır

---

### 5.14. Content Recommendations (İçerik Önerileri)

#### [GET] `/api/social/recommendations`

Kişiselleştirilmiş içerik önerileri.

**Query Parameters:**
- `limit`: Öneri sayısı (default: 20)
- `forceRefresh`: Cache bypass (default: false)

**Teknoloji Kullanımı:**
- **FeedService**: Recommendation algoritması kullanılır
- **CacheService**: Öneriler 15 dakika cache'lenir
- **Hangfire**: Günlük recommendation job'ı çalışır

**Operation Logic:**

```csharp
public async Task<BaseResponse<List<ContentDto>>> GetRecommendationsAsync(
    int userId,
    int limit = 20,
    bool forceRefresh = false)
{
    var cacheKey = $"User:Recommendations:{userId}:Limit{limit}";
    if (!forceRefresh)
    {
        var cached = await _cacheService.GetAsync<List<ContentDto>>(cacheKey);
        if (cached != null)
            return BaseResponse<List<ContentDto>>.SuccessResponse(cached);
    }
    
    // FeedService kullanarak önerileri al
    var recommendations = await _feedService.GetRecommendationsAsync(userId, limit);
    
    await _cacheService.SetAsync(cacheKey, recommendations, TimeSpan.FromMinutes(15));
    return BaseResponse<List<ContentDto>>.SuccessResponse(recommendations);
}
```

**Recommendation Algoritması (FeedService):**
- Kullanıcının beğendiği içeriklerin ders/konu analizi
- Takip ettiği kullanıcıların paylaştığı içerikler
- Trend içerikler (son 24 saatte popüler olanlar)
- Benzer kullanıcıların beğendiği içerikler (collaborative filtering)

---

### 5.15. Content Export & Sharing (İçerik Dışa Aktarma ve Paylaşma)

#### [GET] `/api/social/content/{id}/share-link`

İçerik paylaşım linki oluştur.

**Response:**

```json
{
  "success": true,
  "data": {
    "shareLink": "https://karneapp.com/share/content/123?token=abc123",
    "expiresAt": "2024-01-20T12:00:00Z"
  },
  "error": null,
  "errorCode": null
}
```

**Operation Logic:**

```csharp
public async Task<BaseResponse<ShareLinkDto>> GetShareLinkAsync(
    int contentId,
    int userId)
{
    var content = await _context.Contents
        .AsNoTracking()
        .FirstOrDefaultAsync(c => c.Id == contentId);
    
    if (content == null)
        return BaseResponse<ShareLinkDto>.ErrorResponse(
            "Content not found", ErrorCodes.NotFound);
    
    // Paylaşım token'ı oluştur (JWT benzeri)
    var token = GenerateShareToken(contentId, userId);
    
    var shareLink = new ShareLinkDto
    {
        ShareLink = $"https://karneapp.com/share/content/{contentId}?token={token}",
        ExpiresAt = DateTime.UtcNow.AddDays(30) // 30 gün geçerli
    };
    
    return BaseResponse<ShareLinkDto>.SuccessResponse(shareLink);
}
```

#### [GET] `/api/social/share/content/{id}`

Paylaşım linki ile içerik görüntüleme (public endpoint, token gerekli).

**Query Parameters:**
- `token`: Paylaşım token'ı

**Operation Logic:**

```csharp
public async Task<BaseResponse<ContentDto>> GetSharedContentAsync(
    int contentId,
    string token)
{
    // Token doğrulama
    if (!ValidateShareToken(token, contentId))
        return BaseResponse<ContentDto>.ErrorResponse(
            "Invalid or expired share token", ErrorCodes.Unauthorized);
    
    var content = await _context.Contents
        .AsNoTracking()
        .Where(c => c.Id == contentId && !c.IsDeleted)
        .Include(c => c.Author)
        .Include(c => c.Lesson)
        .Include(c => c.Topic)
        .FirstOrDefaultAsync();
    
    if (content == null)
        return BaseResponse<ContentDto>.ErrorResponse(
            "Content not found", ErrorCodes.NotFound);
    
    var dto = MapToContentDto(content);
    return BaseResponse<ContentDto>.SuccessResponse(dto);
}
```

**Teknoloji Kullanımı:**
- **JWT Token**: Paylaşım token'ı oluşturma ve doğrulama
- **CacheService**: Paylaşım linkleri cache'lenir (30 dakika)

---

### 5.16. Stories (24 Saatlik İçerik)

Stories, 24 saat sonra otomatik olarak silinen geçici içeriklerdir.

#### [Model] Story

```csharp
public class Story
{
    public int Id { get; set; }
    public int AuthorId { get; set; }
    public User Author { get; set; }
    
    public string? ImageUrl { get; set; } // Görsel story
    public string? VideoUrl { get; set; } // Video story
    public string? Text { get; set; } // Metin story
    
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; } // CreatedAt + 24 saat
    
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    
    // İstatistikler
    public int ViewsCount { get; set; } = 0;
    public int ReactionsCount { get; set; } = 0;
}
```

#### [POST] `/api/social/story/create`

Story oluştur.

**Request:**

```json
{
  "imageUrl": "https://cdn.../story.jpg",
  "text": "Bugün çok güzel bir gün! 📚"
}
```

**Validation:**
- `imageUrl` veya `videoUrl` veya `text` en az biri olmalı
- `text` max 200 karakter

**Operation Logic:**

```csharp
public async Task<BaseResponse<StoryDto>> CreateStoryAsync(CreateStoryRequest request)
{
    var userId = _sessionService.GetUserId();

    // Validation
    if (string.IsNullOrEmpty(request.ImageUrl) && 
        string.IsNullOrEmpty(request.VideoUrl) && 
        string.IsNullOrEmpty(request.Text))
    {
        return BaseResponse<StoryDto>.ErrorResponse(
            "At least one of imageUrl, videoUrl, or text is required", 
            ErrorCodes.ValidationFailed);
    }

    if (!string.IsNullOrEmpty(request.Text) && request.Text.Length > 200)
    {
        return BaseResponse<StoryDto>.ErrorResponse(
            "Text cannot exceed 200 characters", 
            ErrorCodes.ValidationFailed);
    }

    var story = new Story
    {
        AuthorId = userId,
        ImageUrl = request.ImageUrl,
        VideoUrl = request.VideoUrl,
        Text = request.Text,
        CreatedAt = DateTime.UtcNow,
        ExpiresAt = DateTime.UtcNow.AddHours(24) // 24 saat sonra expire
    };

    _context.Stories.Add(story);
    await _context.SaveChangesAsync();

    // Cache invalidation
    await _cacheService.RemoveByPatternAsync($"User:{userId}:Stories:*");
    await _cacheService.RemoveByPatternAsync($"Stories:Following:*");
    await _cacheService.RemoveByPatternAsync($"Stories:Active:*");

    // SignalR: Takipçilere bildirim
    var followers = await _context.Follows
        .AsNoTracking()
        .Where(f => f.FollowingId == userId)
        .Select(f => f.FollowerId)
        .ToListAsync();

    foreach (var followerId in followers)
    {
        await _notificationHub.Clients.Group($"User_{followerId}")
            .SendAsync("NewStory", new { AuthorId = userId, StoryId = story.Id });
    }

    await _auditService.LogAsync(userId, "StoryCreated", 
        JsonSerializer.Serialize(new { StoryId = story.Id }));

    var dto = MapToStoryDto(story);
    return BaseResponse<StoryDto>.SuccessResponse(dto);
}
```

---

#### [GET] `/api/social/stories`

Aktif story'leri listele (takip edilenler ve kendi story'lerim).

**Query Parameters:**
- `forceRefresh`: Cache bypass (default: false)

**Response:**

```json
{
  "success": true,
  "data": [
    {
      "authorId": 123,
      "authorName": "Ahmet Yılmaz",
      "authorImageUrl": "https://cdn.../avatar.jpg",
      "stories": [
        {
          "id": 456,
          "imageUrl": "https://cdn.../story.jpg",
          "text": "Bugün çok güzel bir gün!",
          "createdAt": "2024-01-15T10:00:00Z",
          "expiresAt": "2024-01-16T10:00:00Z",
          "viewsCount": 45,
          "isViewed": false
        }
      ]
    }
  ]
}
```

**Operation Logic:**

```csharp
public async Task<BaseResponse<List<StoryGroupDto>>> GetStoriesAsync(bool forceRefresh = false)
{
    var userId = _sessionService.GetUserId();

    var cacheKey = $"User:{userId}:Stories:Following";
    if (!forceRefresh)
    {
        var cached = await _cacheService.GetAsync<List<StoryGroupDto>>(cacheKey);
        if (cached != null)
            return BaseResponse<List<StoryGroupDto>>.SuccessResponse(cached);
    }

    // Takip edilenler + kendi story'lerim
    var followingIds = await _context.Follows
        .AsNoTracking()
        .Where(f => f.FollowerId == userId)
        .Select(f => f.FollowingId)
        .ToListAsync();

    followingIds.Add(userId); // Kendi story'lerimizi de ekle

    // Aktif story'ler (expire olmamış)
    var now = DateTime.UtcNow;
    var activeStories = await _context.Stories
        .AsNoTracking()
        .Where(s => followingIds.Contains(s.AuthorId) && 
                   !s.IsDeleted &&
                   s.ExpiresAt > now)
        .Include(s => s.Author)
        .OrderByDescending(s => s.CreatedAt)
        .ToListAsync();

    // Author'a göre grupla
    var storyGroups = activeStories
        .GroupBy(s => s.AuthorId)
        .Select(g => new StoryGroupDto
        {
            AuthorId = g.Key,
            AuthorName = g.First().Author.FullName,
            AuthorImageUrl = g.First().Author.ProfileImageUrl,
            Stories = g.Select(s => MapToStoryDto(s)).ToList()
        })
        .OrderByDescending(g => g.Stories.Max(s => s.CreatedAt))
        .ToList();

    await _cacheService.SetAsync(cacheKey, storyGroups, TimeSpan.FromMinutes(1)); // Çok dinamik, 1 dakika cache

    return BaseResponse<List<StoryGroupDto>>.SuccessResponse(storyGroups);
}
```

---

#### [GET] `/api/social/user/{userId}/stories`

Kullanıcının story'lerini listele.

**Query Parameters:**
- `forceRefresh`: Cache bypass (default: false)

**Operation Logic:**

```csharp
public async Task<BaseResponse<List<StoryDto>>> GetUserStoriesAsync(
    int userId,
    bool forceRefresh = false)
{
    var currentUserId = _sessionService.GetUserId();

    // Privacy kontrolü: Sadece kendi story'lerimizi veya takip ettiğimiz kullanıcıların story'lerini görebiliriz
    if (userId != currentUserId)
    {
        var isFollowing = await _context.Follows
            .AsNoTracking()
            .AnyAsync(f => f.FollowerId == currentUserId && f.FollowingId == userId);

        if (!isFollowing)
        {
            return BaseResponse<List<StoryDto>>.ErrorResponse(
                "You must follow this user to view their stories", 
                ErrorCodes.AccessDenied);
        }
    }

    var cacheKey = $"User:{userId}:Stories";
    if (!forceRefresh)
    {
        var cached = await _cacheService.GetAsync<List<StoryDto>>(cacheKey);
        if (cached != null)
            return BaseResponse<List<StoryDto>>.SuccessResponse(cached);
    }

    var now = DateTime.UtcNow;
    var stories = await _context.Stories
        .AsNoTracking()
        .Where(s => s.AuthorId == userId && 
                   !s.IsDeleted &&
                   s.ExpiresAt > now)
        .OrderByDescending(s => s.CreatedAt)
        .ToListAsync();

    var dtos = stories.Select(MapToStoryDto).ToList();

    await _cacheService.SetAsync(cacheKey, dtos, TimeSpan.FromMinutes(1));

    return BaseResponse<List<StoryDto>>.SuccessResponse(dtos);
}
```

---

#### [GET] `/api/social/story/{id}`

Story detayı.

**Query Parameters:**
- `markAsViewed`: Story'yi görüntülendi olarak işaretle (default: true)

**Operation Logic:**

```csharp
public async Task<BaseResponse<StoryDto>> GetStoryByIdAsync(
    int storyId,
    bool markAsViewed = true)
{
    var userId = _sessionService.GetUserId();

    var story = await _context.Stories
        .AsNoTracking()
        .Include(s => s.Author)
        .FirstOrDefaultAsync(s => s.Id == storyId && !s.IsDeleted);

    if (story == null)
        return BaseResponse<StoryDto>.ErrorResponse("Story not found", ErrorCodes.NotFound);

    // Expire kontrolü
    if (story.ExpiresAt <= DateTime.UtcNow)
        return BaseResponse<StoryDto>.ErrorResponse("Story has expired", ErrorCodes.NotFound);

    // Privacy kontrolü
    if (story.AuthorId != userId)
    {
        var isFollowing = await _context.Follows
            .AsNoTracking()
            .AnyAsync(f => f.FollowerId == userId && f.FollowingId == story.AuthorId);

        if (!isFollowing)
        {
            return BaseResponse<StoryDto>.ErrorResponse(
                "You must follow this user to view their stories", 
                ErrorCodes.AccessDenied);
        }
    }

    // View tracking (background job ile yapılabilir)
    if (markAsViewed)
    {
        var existingView = await _context.StoryViews
            .FirstOrDefaultAsync(v => v.StoryId == storyId && v.UserId == userId);

        if (existingView == null)
        {
            var view = new StoryView
            {
                StoryId = storyId,
                UserId = userId,
                ViewedAt = DateTime.UtcNow
            };
            _context.StoryViews.Add(view);

            // Views count'u güncelle (optimistic update)
            story.ViewsCount++;
            await _context.SaveChangesAsync();

            // Cache invalidation
            await _cacheService.RemoveByPatternAsync($"Story:{storyId}:*");
        }
    }

    var dto = MapToStoryDto(story);
    return BaseResponse<StoryDto>.SuccessResponse(dto);
}
```

**Yeni Model (StoryView.cs):**

```csharp
public class StoryView
{
    public int Id { get; set; }
    public int StoryId { get; set; }
    public Story Story { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public DateTime ViewedAt { get; set; }
}
```

---

#### [DELETE] `/api/social/story/{id}`

Story sil (24 saat dolmadan önce manuel silme).

**Operation Logic:**

```csharp
public async Task<BaseResponse<bool>> DeleteStoryAsync(int storyId)
{
    var userId = _sessionService.GetUserId();

    var story = await _context.Stories
        .FirstOrDefaultAsync(s => s.Id == storyId);

    if (story == null)
        return BaseResponse<bool>.ErrorResponse("Story not found", ErrorCodes.NotFound);

    // Yetki kontrolü
    if (story.AuthorId != userId)
        return BaseResponse<bool>.ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);

    // Soft delete
    story.IsDeleted = true;
    story.DeletedAt = DateTime.UtcNow;
    await _context.SaveChangesAsync();

    // Cache invalidation
    await _cacheService.RemoveByPatternAsync($"User:{userId}:Stories:*");
    await _cacheService.RemoveByPatternAsync($"Story:{storyId}:*");
    await _cacheService.RemoveByPatternAsync($"Stories:*");

    await _auditService.LogAsync(userId, "StoryDeleted", 
        JsonSerializer.Serialize(new { StoryId = storyId }));

    return BaseResponse<bool>.SuccessResponse(true);
}
```

---

#### [POST] `/api/social/story/{id}/reaction`

Story'ye tepki ver (emoji).

**Request:**

```json
{
  "reaction": "👍"
}
```

**Operation Logic:**

```csharp
public async Task<BaseResponse<bool>> ReactToStoryAsync(
    int storyId,
    ReactToStoryRequest request)
{
    var userId = _sessionService.GetUserId();

    var story = await _context.Stories
        .FirstOrDefaultAsync(s => s.Id == storyId && !s.IsDeleted);

    if (story == null)
        return BaseResponse<bool>.ErrorResponse("Story not found", ErrorCodes.NotFound);

    // Expire kontrolü
    if (story.ExpiresAt <= DateTime.UtcNow)
        return BaseResponse<bool>.ErrorResponse("Story has expired", ErrorCodes.NotFound);

    // Zaten tepki vermiş mi?
    var existingReaction = await _context.StoryReactions
        .FirstOrDefaultAsync(r => r.StoryId == storyId && r.UserId == userId);

    if (existingReaction != null)
    {
        // Tepkiyi güncelle
        existingReaction.Reaction = request.Reaction;
        existingReaction.CreatedAt = DateTime.UtcNow;
    }
    else
    {
        // Yeni tepki
        var reaction = new StoryReaction
        {
            StoryId = storyId,
            UserId = userId,
            Reaction = request.Reaction,
            CreatedAt = DateTime.UtcNow
        };
        _context.StoryReactions.Add(reaction);

        // Reactions count'u güncelle
        story.ReactionsCount++;
    }

    await _context.SaveChangesAsync();

    // Cache invalidation
    await _cacheService.RemoveByPatternAsync($"Story:{storyId}:*");

    // SignalR: Story sahibine bildirim
    await _notificationHub.Clients.Group($"User_{story.AuthorId}")
        .SendAsync("StoryReaction", new { StoryId = storyId, UserId = userId, Reaction = request.Reaction });

    await _auditService.LogAsync(userId, "StoryReacted", 
        JsonSerializer.Serialize(new { StoryId = storyId, Reaction = request.Reaction }));

    return BaseResponse<bool>.SuccessResponse(true);
}
```

**Yeni Model (StoryReaction.cs):**

```csharp
public class StoryReaction
{
    public int Id { get; set; }
    public int StoryId { get; set; }
    public Story Story { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public string Reaction { get; set; } // "👍", "❤️", "😊", vb.
    public DateTime CreatedAt { get; set; }
}
```

---

#### Hangfire Job: CleanupExpiredStoriesJob

24 saat geçen story'leri otomatik olarak silen background job.

**Job Implementation:**

```csharp
// Jobs/CleanupExpiredStoriesJob.cs
public class CleanupExpiredStoriesJob
{
    private readonly ApplicationContext _context;
    private readonly ICacheService _cacheService;

    public CleanupExpiredStoriesJob(ApplicationContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task Execute()
    {
        var now = DateTime.UtcNow;

        // 24 saat geçen story'leri bul
        var expiredStories = await _context.Stories
            .Where(s => !s.IsDeleted && s.ExpiresAt <= now)
            .ToListAsync();

        if (!expiredStories.Any())
            return;

        // Soft delete
        foreach (var story in expiredStories)
        {
            story.IsDeleted = true;
            story.DeletedAt = now;
        }

        await _context.SaveChangesAsync();

        // Cache invalidation
        foreach (var story in expiredStories)
        {
            await _cacheService.RemoveByPatternAsync($"User:{story.AuthorId}:Stories:*");
            await _cacheService.RemoveByPatternAsync($"Story:{story.Id}:*");
        }

        await _cacheService.RemoveByPatternAsync($"Stories:*");
    }
}
```

**Program.cs'de Schedule:**

```csharp
// Her saat başı çalışır
RecurringJob.AddOrUpdate<CleanupExpiredStoriesJob>(
    "cleanup-expired-stories",
    job => job.Execute(),
    Cron.Hourly);
```

**Teknoloji Kullanımı:**
- **CacheService**: Story listesi 1 dakika cache'lenir (çok dinamik)
- **SignalR**: Yeni story paylaşıldığında takipçilere bildirim
- **Hangfire**: Günlük story temizleme job'ı (her saat başı)
- **AuditService**: Story işlemleri loglanır
- **AsNoTracking()**: Read-only query'ler için performans
- **Soft Delete**: Story'ler silinmez, IsDeleted flag'i ile işaretlenir

---

## 📊 Faz 3 Özet: Tamamlanan Özellikler

**Toplam Endpoint Sayısı:** 60+ endpoint

**Kategoriler:**
1. ✅ Content Management (10 endpoint)
2. ✅ Feed System (8 endpoint)
3. ✅ Interactions (6 endpoint)
4. ✅ Comments (8 endpoint)
5. ✅ Follow System (6 endpoint)
6. ✅ User Profile Social (5 endpoint)
7. ✅ Hashtags & Tags (4 endpoint)
8. ✅ Advanced Feed (5 endpoint)
9. ✅ Search & Discovery (3 endpoint)
10. ✅ Content Analytics (1 endpoint)
11. ✅ Content Moderation (3 endpoint)
12. ✅ Content Recommendations (1 endpoint)
13. ✅ Content Export & Sharing (2 endpoint)
14. ✅ Mute System (3 endpoint)
15. ✅ Stories (6 endpoint)

**Teknoloji Kullanımı:**
- ✅ **Redis Cache**: Tüm GET endpoint'lerde cache kullanımı
- ✅ **RediSearch**: Full-text search için (opsiyonel, fallback EF Core)
- ✅ **SignalR**: Real-time like/comment/follow notifications
- ✅ **Hangfire**: Content indexing, feed generation, recommendation jobs
- ✅ **CacheService**: Pattern-based invalidation, force refresh
- ✅ **AuditService**: Tüm CUD işlemlerde loglama
- ✅ **AsNoTracking()**: Read-only query'ler için performans optimizasyonu

---

## 🔍 5.17. Faz 3 Eksik Özellikler Analizi (Profesyonel Platform Karşılaştırması)

Bu bölüm, mevcut Faz 3 implementasyonunu profesyonel sosyal medya platformları (Twitter, Instagram, Facebook, Reddit, LinkedIn, Pinterest, Discord, Stack Overflow) ile karşılaştırarak belirlenen eksik özellikleri içerir.

**Detaylı analiz için:** `DOC/missing_features_analysis.md` dosyasına bakınız.

### 📊 Özet Tablo

| Özellik | Öncelik | Durum | Endpoint Sayısı | Model Sayısı | Teknoloji |
|---------|---------|-------|-----------------|--------------|-----------|
| **Polls** | 🔴 Yüksek | ❌ Eksik | 4 | 2 | Cache, SignalR, Hangfire |
| **Drafts** | 🔴 Yüksek | ❌ Eksik | 5 | 1 | Cache, AuditService |
| **Content Pinning** | 🔴 Yüksek | ❌ Eksik | 3 | 0 (Content'e ekleme) | Cache, AuditService |
| **Multiple Reactions** | 🟡 Orta | ⚠️ Kısmen | 3 | 0 (Interaction'a ekleme) | Cache, SignalR |
| **Collections** | 🟡 Orta | ❌ Eksik | 6 | 2 | Cache, SignalR, AuditService |
| **Content Scheduling** | 🟡 Orta | ❌ Eksik | 4 | 1 | Hangfire, Cache, SignalR |
| **User Verification** | 🟡 Orta | ❌ Eksik | 4 | 1 (+ User'a ekleme) | Cache, AuditService |
| **Groups/Communities** | 🟢 Düşük | ❌ Eksik | 6 | 2 | Cache, SignalR, AuditService |
| **Badges/Achievements** | 🟢 Düşük | ❌ Eksik | 5 | 2 | Hangfire, Cache, SignalR |
| **Content Archiving** | 🟢 Düşük | ❌ Eksik | 3 | 0 (Content'e ekleme) | Cache, AuditService |
| **User Reputation** | 🟢 Düşük | ❌ Eksik | 2 | 1 (+ User'a ekleme) | Hangfire, Cache |
| **Content Templates** | 🟢 Düşük | ❌ Eksik | 4 | 1 | Cache, AuditService |

**Toplam:**
- **Yeni Endpoint:** ~49 endpoint
- **Yeni Model:** ~15 model
- **Model Değişikliği:** 3 model (Content, User, Interaction)

### 🎯 Önerilen Uygulama Sırası

#### Faz 3.1 (Hemen Yapılmalı):
1. ✅ **Polls (Anketler)** - Eğitim platformu için kritik
   - Model: `Poll`, `PollVote`
   - Endpoint'ler: Create, Vote, Get Results, Get Stats
   - Teknoloji: CacheService, SignalR (real-time updates), Hangfire (expired poll cleanup)
   - Nereye: `SocialOperations.cs`, `SocialController.cs`

2. ✅ **Drafts (Taslaklar)** - Kullanıcı deneyimi için önemli
   - Model: `ContentDraft`
   - Endpoint'ler: Create/Update, List, Get, Publish, Delete
   - Teknoloji: CacheService (10 dakika), AuditService
   - Nereye: `SocialOperations.cs`, `SocialController.cs`

3. ✅ **Content Pinning (İçerik Sabitleme)** - Standart özellik
   - Model Değişikliği: `Content.IsPinned`, `Content.PinnedAt`
   - Endpoint'ler: Pin, Unpin, Get Pinned
   - Teknoloji: CacheService, AuditService
   - Nereye: `Content.cs` (yeni property'ler), `SocialOperations.cs`, `SocialController.cs`

#### Faz 3.2 (Orta Vadede):
4. ✅ **Multiple Reactions (Çoklu Tepkiler)** - Mevcut Like sistemini genişletme
   - Model Değişikliği: `Interaction.ReactionEmoji`
   - Endpoint'ler: React, Unreact, Get Reactions
   - Teknoloji: CacheService, SignalR, AuditService
   - Nereye: `Interaction.cs` (yeni property), `SocialOperations.cs` (mevcut Like metodları güncellenecek)

5. ✅ **Collections (İçerik Koleksiyonları)** - İçerik organizasyonu
   - Model: `Collection`, `CollectionContent`
   - Endpoint'ler: Create, Get, Add Content, Remove Content, List User Collections
   - Teknoloji: CacheService (15 dakika), SignalR, AuditService
   - Nereye: Yeni modeller, `SocialOperations.cs`, `SocialController.cs`

6. ✅ **Content Scheduling (Zamanlanmış Paylaşım)** - Öğretmenler için önemli
   - Model: `ScheduledContent`
   - Endpoint'ler: Schedule, List Scheduled, Update, Cancel, Publish
   - Teknoloji: Hangfire (publish job - her dakika), CacheService, SignalR
   - Nereye: Yeni model, `SocialOperations.cs`, `SocialController.cs`, `Jobs/PublishScheduledContentJob.cs`

7. ✅ **User Verification (Kullanıcı Doğrulama)** - Güvenilirlik
   - Model: `VerificationRequest`, `User.IsVerified`
   - Endpoint'ler: Request Verification, Admin: Approve/Reject, List Requests
   - Teknoloji: CacheService, AuditService, NotificationService
   - Nereye: Yeni model, `User.cs` (yeni property), `AdminOperations.cs`, `UserOperations.cs`

#### Faz 3.3 (İleride):
8. ✅ **Groups/Communities (Gruplar/Topluluklar)** - Topluluk özelliği
   - Model: `Group`, `GroupMember`
   - Endpoint'ler: Create, Get, Join, Leave, Get Members, Get Contents
   - Teknoloji: CacheService (15 dakika), SignalR, AuditService
   - Nereye: Yeni modeller, `SocialOperations.cs` veya yeni `GroupOperations.cs`, `SocialController.cs` veya yeni `GroupController.cs`

9. ✅ **Badges/Achievements (Rozetler/Başarımlar)** - Gamification
   - Model: `Badge`, `UserBadge`
   - Endpoint'ler: Get User Badges, List All Badges, Admin: Create Badge, Award Badge
   - Teknoloji: Hangfire (auto-award job), CacheService (30 dakika), SignalR
   - Nereye: Yeni modeller, `UserOperations.cs`, `AdminOperations.cs`, `Jobs/AwardBadgesJob.cs`

10. ✅ **Content Archiving (İçerik Arşivleme)** - Kullanıcı deneyimi
    - Model Değişikliği: `Content.IsArchived`, `Content.ArchivedAt`
    - Endpoint'ler: Archive, Unarchive, Get Archived
    - Teknoloji: CacheService, AuditService
    - Nereye: `Content.cs` (yeni property'ler), `SocialOperations.cs`, `SocialController.cs`

11. ✅ **User Reputation (İtibar Sistemi)** - Topluluk kalitesi
    - Model: `ReputationHistory`, `User.Reputation`
    - Endpoint'ler: Get Reputation, Get Reputation History
    - Teknoloji: Hangfire (calculation job - günlük), CacheService
    - Nereye: Yeni model, `User.cs` (yeni property), `UserOperations.cs`, `Jobs/CalculateReputationJob.cs`

12. ✅ **Content Templates (İçerik Şablonları)** - Hızlı içerik oluşturma
    - Model: `ContentTemplate`
    - Endpoint'ler: Create Template, Get Template, List User Templates, Create Content from Template
    - Teknoloji: CacheService (30 dakika), AuditService
    - Nereye: Yeni model, `SocialOperations.cs`, `SocialController.cs`

### 📝 Teknoloji Kullanım Notları

**Tüm yeni özellikler için:**
- ✅ **CacheService**: Tüm GET endpoint'lerde cache kullanımı (1-30 dakika arası)
- ✅ **forceRefresh**: Tüm GET endpoint'lerde cache bypass parametresi
- ✅ **AsNoTracking()**: Tüm read-only query'lerde performans optimizasyonu
- ✅ **AuditService**: Tüm CUD işlemlerde loglama
- ✅ **SignalR**: Real-time updates (yeni içerik, yeni üye, vb.)
- ✅ **Hangfire**: Background jobs (cleanup, calculation, publish)
- ✅ **BaseResponse<T>**: Standart response formatı
- ✅ **Pattern-based Cache Invalidation**: Tüm CUD işlemlerde

**Detaylı implementasyon planı için:** `DOC/missing_features_analysis.md` dosyasına bakınız.

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
│   │   ├── Block.cs                   [Faz 3]
│   │   ├── Mute.cs                    [Faz 3]
│   │   ├── Story.cs                   [Faz 3]
│   │   ├── StoryView.cs               [Faz 3]
│   │   ├── StoryReaction.cs           [Faz 3]
│   │   ├── ContentReport.cs           [Faz 3]
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
- ✅ CleanupExpiredStoriesJob (her saat başı - story temizleme)

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
