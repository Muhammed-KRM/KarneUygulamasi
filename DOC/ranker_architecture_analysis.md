# Proje Mimarisi: Ranker - Backend (.NET Core) ve Frontend (Angular)

Bu doküman, Muhammed-KRM/ranker projesinin dosya yapısını, mimari katmanlarını ve her katmandaki önemli kod örneklerini içerir. Bu yapı, yeni projemiz için temel alacağımız referans mimarisidir.

---

## 📂 Backend Mimarisi (.NET Core Web API)

Backend tarafında, klasik **3-Katmanlı Mimari (N-Layer Architecture)** kullanılmış ancak daha da basitleştirilerek **Operations** adında özel bir iş mantığı katmanı eklenmiş.

### Genel Dosya Yapısı

```text
📂 KeremProject1backend
├── 📂 Controllers          # HTTP İsteklerini Karşılama Katmanı
│   ├── AuthController.cs
│   └── AnimeListController.cs
│
├── 📂 Operations           # İş Mantığı (Business Logic) Katmanı
│   ├── AuthOperations.cs
│   └── DragDropOperations.cs
│
├── 📂 Models               # Veri Transfer Nesneleri (DTO) ve Entity'ler
│   ├── 📂 DBs              # Veritabanı Entity Sınıfları
│   │   └── AppModels.cs
│   ├── 📂 Requests         # İstemciden Gelen Veriler (Request DTO)
│   │   └── AuthRequests.cs
│   └── 📂 Responses        # İstemciye Gönderilen Veriler (Response DTO)
│       └── AuthResponses.cs
│
├── 📂 Services             # Yardımcı Servisler (Token, Email, vb.)
│   ├── SessionServices.cs
│   ├── EmailService.cs
│   └── ServiceRegistration.cs
│
├── 📂 Migrations           # Entity Framework Core Veritabanı Geçmişi
├── appsettings.json        # Uygulama Yapılandırması
└── Program.cs              # Uygulama Giriş Noktası
```

---

### 1️⃣ Controllers Katmanı

**Görev:** HTTP isteklerini karşılar, ancak iş mantığına girmez. Sadece `Operations` katmanını çağırır ve sonucu HTTP response olarak döner.

**Dosya:** `Controllers/AuthController.cs`

```csharp
[HttpPost("register")]
[Produces("application/json", Type = typeof(BaseResponse))]
public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
{
    var response = await AuthOperations.Register(registerDto, _context);
    if (response.Errored)
        return BadRequest(response);
    return Ok(response);
}
```

**Açıklama:**

- `[HttpPost("register")]` → Bu metot `/auth/register` endpoint'ine gelen POST isteklerini karşılar.
- `[FromBody]` → JSON formatındaki veriyi C# nesnesine (RegisterDto) dönüştürür.
- `AuthOperations.Register()` → Asıl işi yapan katman burası. Controller sadece "köprü" görevi görür.
- `BadRequest()` / `Ok()` → HTTP 400 veya 200 durum kodları ile yanıt döner.

---

### 2️⃣ Operations Katmanı (İş Mantığı)

**Görev:** Uygulamanın beyni burasıdır. Veritabanı sorgularını yönetir, iş mantığını gerçekleştirir ve güvenlik kontrollerini yapar.

**Dosya:** `Operations/AuthOperations.cs`

```csharp
public static async Task<BaseResponse> Register(RegisterDto dto, ApplicationContext context)
{
    BaseResponse response = new();
    try
    {
        // Kullanıcı adı daha önce kullanılmış mı?
        var existingUser = await context.AppUsers.FirstOrDefaultAsync(u => u.UserName.ToLower() == dto.Username.ToLower());
        if (existingUser != null)
        {
            return response.GenerateError(1001, "Kullanıcı adı zaten kullanımda.");
        }

        // Şifreyi güvenli bir şekilde hash'le
        CreatePasswordHash(dto.Password, out byte[] passwordHash, out byte[] passwordSalt);

        var user = new AppUser
        {
            UserName = dto.Username.ToLower(),
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            UserRole = UserRole.User,
            State = false,
            ModTime = DateTime.Now,
            ModUser = 0
        };

        await context.AppUsers.AddAsync(user);
        await context.SaveChangesAsync();

        return response.GenerateSuccess("Kullanıcı başarıyla oluşturuldu.");
    }
    catch (Exception ex)
    {
        return response.GenerateError(9999, $"Beklenmeyen hata: {ex.Message}");
    }
}
```

**Açıklama:**

- **Validasyon:** Kullanıcı adının benzersiz olup olmadığını kontrol eder.
- **Güvenlik:** Şifreleri düz metin olarak değil, `hash` ve `salt` ile şifreler.
- **Hata Yönetimi:** Try-catch kullanarak beklenmeyen hataları yakalar.
- **Response Standartlaştırma:** Başarı/hata durumunu `BaseResponse` ile standart bir formatta döner.

---

### 3️⃣ Models/DBs - Entity Sınıfları

**Görev:** Veritabanındaki tabloları C# tarafında temsil eder. Entity Framework Core bu sınıfları kullanarak tabloları otomatik oluşturur.

**Dosya:** `Models/DBs/AppModels.cs`

```csharp
public class AppUser
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public UserRole UserRole { get; set; } = UserRole.User;
    public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
    public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
    public bool State { get; set; }
    public string UserImageLink { get; set; } = string.Empty;
    public DateTime ModTime { get; set; }
    public int ModUser { get; set; }
}
```

**Açıklama:**

- `Id` → Primary Key (Otomatik artan)
- `PasswordHash` ve `PasswordSalt` → Güvenlik için şifreler asla düz metin olarak saklanmaz.
- `UserRole` → Enum kullanarak rol yönetimi (User, Admin, vb.)
- `State` → Kullanıcının aktif/pasif durumu

---

### 4️⃣ Models/Requests - İstek DTO'ları

**Görev:** Frontend'den gelen verilerin formatını ve validasyon kurallarını tanımlar.

**Dosya:** `Models/Requests/AuthRequests.cs`

```csharp
public class RegisterDto
{
    [Required(ErrorMessage = "Kullanıcı adı boş olamaz.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Kullanıcı adı 3-50 karakter arası olmalı.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Parola boş olamaz.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Parola en az 6 karakter olmalı.")]
    public string Password { get; set; } = string.Empty;
}
```

**Açıklama:**

- **Data Annotations:** `[Required]`, `[StringLength]` gibi attribute'lar ile otomatik validasyon sağlanır.
- **Güvenlik:** Backend'e gelen veriler bu kuralları geçmedikçe işlem yapılmaz.
- **Ayrıştırma:** Veritabanı modeli (`AppUser`) ile istek modelini ayırmak, güvenlik açıklarını önler.

---

### 5️⃣ Models/Responses - Yanıt DTO'ları

**Görev:** Backend'den frontend'e gönderilecek verileri tanımlar. Hassas bilgiler (şifreler vb.) burada filtrelenir.

**Dosya:** `Models/Responses/AuthResponses.cs`

```csharp
public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string? MalUsername { get; set; } // MyAnimeList hesabı bağlı mı?
}
```

**Açıklama:**

- **Veri Gizleme:** `PasswordHash` ve `PasswordSalt` gibi hassas veriler burada yer almaz.
- **Token Ekleme:** JWT token bu response ile frontend'e iletilir.
- **Optional Fields:** `?` işareti ile nullable alanlar tanımlanır.

---

### 6️⃣ Services - Yardımcı Servisler

**Görev:** Token yönetimi, email gönderimi gibi tekrar kullanılabilir işlemleri barındırır.

**Dosya:** `Services/SessionServices.cs`

```csharp
public static ClaimsPrincipal? TestToken(string token)
{
    if (string.IsNullOrEmpty(token)) return null;

    if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
    {
        token = token.Substring(7);
    }

    try
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

        if (jwtToken == null) return null;

        var claims = jwtToken.Claims;
        var identity = new ClaimsIdentity(claims, "jwt");
        return new ClaimsPrincipal(identity);
    }
    catch
    {
        return null;
    }
}
```

**Açıklama:**

- **Token Parsing:** JWT token'dan kullanıcı bilgilerini (Claims) çıkarır.
- **"Bearer" Prefix:** HTTP Authorization header'ından "Bearer " ön ekini temizler.
- **Güvenlik:** Token geçersizse `null` döner, sistem kullanıcıyı tanımaz.

---

### 7️⃣ Program.cs - Uygulama Başlangıcı

**Görev:** Dependency Injection (DI), veritabanı bağlantısı, CORS ve middleware yapılandırmaları yapılır.

**Dosya:** `Program.cs`

```csharp
var builder = WebApplication.CreateBuilder(args);

// Uygulama servislerini kaydet (Dependency Injection)
builder.Services.AddAppServices(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// HTTP Request Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngularApp"); // CORS Politikası
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

**Açıklama:**

- **Dependency Injection:** `AddAppServices()` ile tüm servisler (DB Context, Email Service vb.) tek bir yerden kaydedilir.
- **Swagger:** Geliştirme ortamında API'leri test etmek için otomatik doküman oluşturur.
- **CORS:** Angular uygulaması farklı bir port'ta çalışıyorsa (örn: localhost:4200), backend istekleri kabul etmesi için CORS gereklidir.
- **Authentication/Authorization:** JWT token kontrolü yapan middleware'ler.

---

## 📂 Frontend Mimarisi (Angular)

Angular tarafında **Core Module** desenine dayalı temiz bir yapı kullanılmış. Servisler, modeller ve guard'lar merkezi bir yerden yönetiliyor.

### Genel Dosya Yapısı

```text
📂 frontend/src/app
├── 📂 core                 # Projenin Omurgası (Merkezi Servisler)
│   ├── 📂 services
│   │   ├── 📂 api          # Backend API Servisleri
│   │   │   └── auth.service.ts
│   │   ├── 📂 public       # Genel Amaçlı Servisler
│   │   │   └── auth.service.ts (State Management)
│   │   └── 📂 utils        # Yardımcı Servisler
│   │       └── http-header.service.ts
│   │
│   ├── 📂 models           # TypeScript Interface'leri
│   │   ├── 📂 entities     # Veri Modelleri
│   │   │   └── user.model.ts
│   │   ├── 📂 enums        # Enum Tanımları
│   │   └── 📂 requests     # Request DTO'ları
│   │
│   ├── 📂 guards           # Route Koruma Mekanizmaları
│   │   └── auth.guard.ts
│   │
│   └── 📂 pipes            # Özel Veri Dönüştürücüler
│
├── 📂 components           # Sayfa ve UI Bileşenleri
│   ├── 📂 modules
│   │   ├── 📂 login
│   │   │   ├── login.component.ts
│   │   │   ├── login.component.html
│   │   │   └── login.component.scss
│   │   └── 📂 register
│   └── 📂 shared           # Ortak Kullanılan Bileşenler
│
├── 📂 styles               # Global SCSS Dosyaları
│   └── styles.scss
│
├── app.routes.ts           # Routing Yapılandırması
└── app.config.ts           # Uygulama Konfigürasyonu
```

---

### 1️⃣ Services/API - Backend İletişimi

**Görev:** `HttpClient` kullanarak backend API'leriyle iletişim kurar. Tüm HTTP istekleri buradan yönetilir.

**Dosya:** `core/services/api/auth.service.ts`

```typescript
register(request: RegisterRequest): Observable<BaseResponse<RegisterResponse>> {
  return this.httpClient.post<BaseResponse<RegisterResponse>>(
    `${this.basePath}/auth/register`,
    request,
    { headers: this.httpHeaderService.getHeaders() }
  );
}

login(request: LoginRequest): Observable<BaseResponse<LoginResponse>> {
  return this.httpClient.post<BaseResponse<LoginResponse>>(
    `${this.basePath}/auth/login`,
    request,
    { headers: this.httpHeaderService.getHeaders() }
  );
}
```

**Açıklama:**

- **HttpClient:** Angular'ın yerleşik HTTP kütüphanesi. Asenkron işlemler için `Observable` döner.
- **Generic Tipler:** `<BaseResponse<RegisterResponse>>` → Type safety sağlar, IDE otomatik tamamlama yapar.
- **Headers:** `httpHeaderService.getHeaders()` → Her isteğe `Content-Type: application/json` ve token eklenir.
- **basePath:** Environment dosyasından (örn: `http://localhost:5000/api`) alınan API URL'si.

---

### 2️⃣ Models - Veri Tipleri

**Görev:** TypeScript interface'leri ile veri yapılarını tanımlar. Backend'den gelen JSON'ları tip güvenliği ile kullanır.

**Dosya:** `core/models/entities/user.model.ts`

```typescript
import { UserRole } from "../enums/user-role.enum";

export interface User {
  id: number;
  username: string;
  role: UserRole;
  malUsername?: string;
  userImageLink?: string;
  modTime?: string;
  token?: string;
}
```

**Açıklama:**

- **Interface:** Class'tan farklı olarak sadece tip tanımıdır, runtime'da kod üretmez.
- **Optional Properties:** `?` işareti ile property'nin zorunlu olmadığını belirtir.
- **Enum Kullanımı:** `UserRole` enum'u ile rol yönetimi tip güvenli hale gelir.

---

### 3️⃣ Guards - Route Koruması

**Görev:** Belirli sayfaların sadece giriş yapmış kullanıcılar tarafından görülmesini sağlar.

**Dosya:** `core/guards/auth.guard.ts`

```typescript
export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }

  router.navigate(["/login"], { queryParams: { returnUrl: state.url } });
  return false;
};
```

**Açıklama:**

- **CanActivateFn:** Modern Angular (v17+) fonksiyonel guard yapısı.
- **inject():** Dependency Injection ile servisleri alır.
- **isAuthenticated():** Token'ın geçerliliğini kontrol eder.
- **returnUrl:** Kullanıcı giriş yaptıktan sonra geldiği sayfaya yönlendirilir.

**Kullanım Örneği (app.routes.ts):**

```typescript
{ path: 'dashboard', component: DashboardComponent, canActivate: [authGuard] }
```

---

### 4️⃣ Components - UI Mantığı

**Görev:** Kullanıcının etkileşime girdiği sayfaların mantığını yönetir.

**Dosya:** `components/modules/login/login.component.ts`

```typescript
onSubmit() {
  if (this.loginForm.valid) {
    const loginRequest: LoginRequest = this.loginForm.value;
    this.authApiService.login(loginRequest).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.authService.setSession(response.data.user);
          this.router.navigate(['/']);
        }
      },
      error: (err) => {
        this.errorMessage = 'Giriş başarısız. Lütfen bilgilerinizi kontrol edin.';
      }
    });
  }
}
```

**Açıklama:**

- **Form Validation:** `this.loginForm.valid` → Angular Reactive Forms ile otomatik validasyon.
- **Observable Subscribe:** `subscribe()` ile asenkron isteğin sonucunu yakalar.
- **State Management:** `setSession()` ile kullanıcı bilgileri localStorage'a kaydedilir.
- **Navigation:** `router.navigate(['/'])` ile ana sayfaya yönlendirilir.

---

### 5️⃣ Routing - Sayfa Yönlendirme

**Görev:** URL'lere karşılık gelen component'leri tanımlar.

**Dosya:** `app.routes.ts`

```typescript
export const routes: Routes = [
  { path: "", component: HomeComponent },
  { path: "login", component: LoginComponent },
  { path: "register", component: RegisterComponent },
  {
    path: "dashboard",
    component: DashboardComponent,
    canActivate: [authGuard],
  },
  { path: "**", redirectTo: "" },
];
```

**Açıklama:**

- **Path Matching:** `/login` → `LoginComponent` gösterilir.
- **Guard Protection:** `dashboard` sadece `authGuard`'ı geçenlere açık.
- **Wildcard Route:** `**` → Tanımsız URL'ler ana sayfaya yönlendirilir.

---

## 🔄 Uygulama Akışı (End-to-End)

### Örnek: Kullanıcı Kayıt İşlemi

1. **Frontend:** Kullanıcı formu doldurur → `RegisterComponent`
2. **Frontend:** Form valid mi? → Reactive Forms Validation
3. **Frontend:** API çağrısı → `AuthService.register()`
4. **Backend:** Request gelir → `AuthController.Register()`
5. **Backend:** İş mantığı → `AuthOperations.Register()`
   - Kullanıcı adı kontrol edilir
   - Şifre hash'lenir
   - Veritabanına kaydedilir
6. **Backend:** Response döner → `BaseResponse<RegisterResponse>`
7. **Frontend:** Başarı mesajı gösterilir ve login sayfasına yönlendirilir

---

## 📌 Anahtar Mimari Prensipler

### Backend

1. **Separation of Concerns:** Controller, Operations ve Services katmanları net ayrılmış.
2. **DTO Pattern:** Veritabanı modelleri doğrudan dışa açılmamış.
3. **Dependency Injection:** Tüm servisler merkezden yönetiliyor.
4. **Error Handling:** Try-catch ve BaseResponse ile standart hata yönetimi.

### Frontend

1. **Core Module Pattern:** Paylaşılan servisler ve modeller tek merkezde.
2. **Service Layer:** HTTP istekleri UI katmanından ayrılmış.
3. **Type Safety:** TypeScript interface'leri ile tip güvenliği.
4. **Reactive Forms:** Validasyon ve form yönetimi Angular tarafından otomatik.

---

## ✅ Bizim Projeye Uyarlama Stratejisi

Bu mimariyi projenizde uygularken:

1. **Backend:**

   - `Operations` klasörü oluşturun → İş mantıklarınız burada olsun.
   - `Models` klasörünü `DBs`, `Requests`, `Responses` olarak 3'e ayırın.
   - `Program.cs` içinde CORS ayarlarını Angular port'unuza göre yapın.

2. **Frontend:**

   - `core` klasörü oluşturun → Servisler ve modeller burada.
   - Her backend endpoint için bir API servisi yazın (örn: `user.service.ts`).
   - Guard'ları kullanarak sayfa koruması yapın.

3. **Genel:**
   - Backend ve Frontend arasında DTO isimlendirmelerini aynı tutun.
   - Token yönetimi için JWT kullanın.
   - Error handling için standart bir `BaseResponse` yapısı kurun.

---

**Hazırlayan:** AI Assistant  
**Tarih:** 03.01.2026  
**Amaç:** KarneProject için referans mimari dokümanı
