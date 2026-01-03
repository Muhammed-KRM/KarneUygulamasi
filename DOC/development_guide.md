# KarneProject Geliştirme Rehberi

Bu belge, **KarneProject** backend mimarisine uygun olarak yeni özelliklerin nasıl ekleneceğini, kod standartlarını ve yardımcı servislerin (Auth, Cache, Log) nasıl kullanılacağını açıklar.

## 🏗️ Mimari Özet

Proje, **Controller -> Operations -> Data** akışını izler.

1.  **Controller:** Sadece HTTP isteklerini karşılar, parametreleri alır, servisleri (Context, Cache, Config) enjekte eder ve ilgili **Operation** metodu çağırır. İş mantığı içermez.
2.  **Operations:** Tüm iş mantığı (Business Logic) burada, **static** metodlar içinde bulunur. Veritabanı işlemleri, hesaplamalar ve kontroller burada yapılır.
3.  **Models:**
    - **DB Entities:** Veritabanı tabloları (`Models/DBs`).
    - **Requests:** API'ye gelen istek modelleri (`Models/Requests`).
    - **Responses:** API'den dönen cevap modelleri (`Models/Responses`).

---

## 🚀 Adım Adım Yeni Özellik Ekleme

Yeni bir özellik eklerken (örneğin: "Sınıf Yönetimi") aşağıdaki adımları takip edin.

### 1. Model Katmanı (DB ve DTO)

Önce veri yapısını ve API iletişim modellerini oluşturun.

#### A. Veritabanı Entity'si (DBs)

`Models/DBs` klasörüne gidin.

```csharp
namespace KeremProject1backend.Models.DBs
{
    public class Classroom
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Grade { get; set; }
        public DateTime ModTime { get; set; }
        public int ModUser { get; set; }

        // Navigation Properties (İlişkiler)
        public ICollection<Student> Students { get; set; } = new List<Student>();
    }
}
```

_Not: `ApplicationContext.cs` içine `DbSet<Classroom> Classrooms { get; set; }` eklemeyi unutmayın._

#### B. Request Modelleri

`Models/Requests` klasörüne (yoksa oluşturun) gidin.

```csharp
namespace KeremProject1backend.Models.Requests
{
    public class CreateClassroomRequest
    {
        public string Name { get; set; } = string.Empty;
        public int Grade { get; set; }
    }
}
```

#### C. Response Modelleri

`Models/Responses` klasörüne gidin.

```csharp
namespace KeremProject1backend.Models.Responses
{
    public class ClassroomDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Grade { get; set; }
    }
}
```

### 2. Operations Katmanı (İş Mantığı)

`Operations` klasöründe yeni bir dosya oluşturun: `ClassroomOperations.cs`.
Bu sınıf **static** olmalı ve metodlar `ApplicationContext`'i parametre olarak almalıdır.

```csharp
using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.Requests;
using KeremProject1backend.Models.Responses;
using KeremProject1backend.Services; // SessionService için
using Microsoft.EntityFrameworkCore; // Async DB işlemleri için
using System.Security.Claims; // Auth için

namespace KeremProject1backend.Operations
{
    public static class ClassroomOperations
    {
        public static async Task<BaseResponse> CreateClassroom(
            CreateClassroomRequest request,
            ClaimsPrincipal session,
            ApplicationContext context)
        {
            BaseResponse response = new();

            try
            {
                // 1. Yetki Kontrolü
                int userId = SessionService.GetUserId(session);
                // Örneğin: Sadece adminler sınıf açabilir
                // if (!SessionService.isAuthorized(session, UserRole.Admin)) ...

                // 2. Validasyon (Basit kontroller burada, karmaşıklar FluentValidation'da)
                if (await context.Classrooms.AnyAsync(c => c.Name == request.Name))
                    return response.GenerateError(1001, "Bu sınıf zaten var.");

                // 3. İşlem
                var classroom = new Classroom
                {
                    Name = request.Name,
                    Grade = request.Grade,
                    ModTime = DateTime.Now,
                    ModUser = userId
                };

                await context.Classrooms.AddAsync(classroom);
                await context.SaveChangesAsync();

                // 4. Cevap
                response.SetUserID(userId);
                return response.GenerateSuccess("Sınıf başarıyla oluşturuldu.");
            }
            catch (Exception ex)
            {
                // Hata yönetimi
                return response.GenerateError(9999, $"Hata: {ex.Message}");
            }
        }
    }
}
```

### 3. Controller Katmanı (API Endpoint)

`Controllers` klasörüne gidin: `ClassroomController.cs`.

```csharp
using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.Requests;
using KeremProject1backend.Models.Responses;
using KeremProject1backend.Operations;
using KeremProject1backend.Services;
using KeremProject1backend.Core.Interfaces; // Cache için
using Microsoft.AspNetCore.Mvc;

namespace KeremProject1backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassroomController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly IConfiguration _configuration;
        private readonly ICacheService _cacheService; // Cache kullanacaksanız

        // Dependency Injection
        public ClassroomController(
            ApplicationContext context,
            IConfiguration configuration,
            ICacheService cacheService)
        {
            _context = context;
            _configuration = configuration;
            _cacheService = cacheService;
        }

        [HttpPost("create")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> Create([FromBody] CreateClassroomRequest request, [FromHeader(Name = "Token")] string? token = null)
        {
            // 1. Token Kontrolü
            var session = SessionService.TestToken(token);
            if (session == null)
                return Unauthorized(new BaseResponse().GenerateError(401, "Oturum geçersiz."));

            // 2. Operation Çağrısı
            var response = await ClassroomOperations.CreateClassroom(request, session, _context);

            // 3. Cevap
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }
    }
}
```

---

## 🛠️ Yardımcı Servisler ve Özellikler

### 🔐 Authentication & Session (Token)

Token kontrolü her Controller metodunun başında yapılmalıdır.

- `SessionService.TestToken(token)`: Token'ı doğrular ve `ClaimsPrincipal` (session) döner. Geçersizse `null` döner.
- `SessionService.GetUserId(session)`: Session'dan User ID'yi çeker.
- `SessionService.isAuthorized(session, UserRole.Admin)`: Rol kontrolü yapar.

### 💾 Caching (Redis)

Önbellekleme için `ICacheService` arayüzünü kullanın. Bunu Controller'da inject edip Operation'a parametre olarak geçebilirsiniz.

```csharp
// Controller'da (Inject edip Operation'a gönderin)
await ClassroomOperations.GetClassrooms(..., _cacheService);

// Operation içinde
public static async Task<BaseResponse> GetClassrooms(..., ICacheService cache)
{
    string cacheKey = "all_classrooms";

    // 1. Cache'den dene
    var cachedData = await cache.GetAsync<List<ClassroomDto>>(cacheKey);
    if (cachedData != null)
    {
        // Cache varsa dön
        response.Response = cachedData;
        return response.GenerateSuccess("Cache'den geldi.");
    }

    // 2. DB'den çek
    var data = await context.Classrooms...ToListAsync();

    // 3. Cache'e yaz (ör: 30 dakika)
    await cache.SetAsync(cacheKey, data, TimeSpan.FromMinutes(30));
}
```

### 📝 Logging (Günlükleme)

Projede **otomatik loglama** vardır (`RequestLoggingMiddleware`).

- Tüm HTTP istekleri, süreleri, statü kodları ve User ID (token varsa) otomatik loglanır.
- Ekstra loglama yapmak isterseniz Operation içinde `Console.WriteLine` veya Controller'a `ILogger` inject edip kullanabilirsiniz. Ancak standart akışta Middleware yeterlidir.

### ⚙️ Configuration (Ayarlar)

`appsettings.json` dosyasındaki ayarlara (ör: Dosya yolu, API anahtarı) erişmek için `IConfiguration` kullanılır. Operation metoduna parametre olarak geçilir.

```csharp
string secretKey = configuration["AppSettings:FileSettings:FileSecretKey"];
```

### ✅ Validation (Doğrulama)

Karmaşık validasyonlar için **FluentValidation** kuruludur.

1.  `Core/Validators` klasörüne gidin.
2.  `CreateClassroomRequestValidator` gibi bir sınıf oluşturun.
3.  Controller'da `[ApiController]` attribute'u sayesinde validasyon otomatik çalışır ve `400 Bad Request` döner.

---

## 📦 Sık Kullanılan Importlar

Kod yazarken şu namespace'leri sıkça eklemeniz gerekecek:

```csharp
using KeremProject1backend.Models.DBs;       // Entityler
using KeremProject1backend.Models.Requests;  // Request DTOs
using KeremProject1backend.Models.Responses; // Response DTOs
using KeremProject1backend.Operations;       // İş mantığı static sınıfları
using KeremProject1backend.Services;         // SessionService, FileService
using KeremProject1backend.Core.Interfaces;  // ICacheService vb.
using Microsoft.EntityFrameworkCore;         // ToListAsync, FirstOrDefaultAsync vb.
using Microsoft.AspNetCore.Mvc;              // Controller özellikleri
using System.Security.Claims;                // Auth session
```
