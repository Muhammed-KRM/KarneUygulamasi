# KarneProject - Sistem Dönüşüm ve Entegrasyon Planı

Bu doküman, mevcut `ranker` projesinin bir anime sıralama uygulamasından, `KarneProject` eğitim platformuna dönüştürülmesi için gereken adımları, teknik değişiklikleri ve entegrasyon stratejilerini içerir.

---

## 1. Mevcut Durum Analizi ve Temizlik (Phase 1 Update)

İlk temizlik aşamasında sadece ismi "Anime" içeren dosyalar silindi. Ancak yapılan analizde sistemin derinliklerinde hala anime/manga odaklı, eğitim platformunda gereksiz olan birçok yapı tespit edildi.

### 🗑️ Silinecek/Değiştirilecek Backend Dosyaları

Aşağıdaki dosyalar projenin iş mantığına (Business Logic) gömülüdür ve tamamen kaldırılmalıdır:

- **Controllers:**
  - `SearchController.cs` (Anime araması yapıyor)
  - `MalIntegrationController.cs` (MyAnimeList entegrasyonu)
  - `JikanController.cs` (Eğer varsa, Anime API proxy)
- **Operations:**
  - `SearchOperations.cs`
  - `RecommendationOperations.cs` (Anime önerisi)
  - `ListGeneratorOperations.cs` (Anime listesi oluşturma)
  - `MalIntegrationOperations.cs`
  - `SyncOperations.cs` (MAL senkronizasyonu)
  - `ImportOperations.cs` / `ExportOperations.cs` (Anime listesi dışa/içe aktarma)
- **Services:**
  - `MalOauthService.cs`
  - `JikanService.cs`
- **ServiceRegistration.cs:**
  - Yukarıdaki servislerin DI konteynerinden kaldırılması gerekiyor.

### 🗑️ Silinecek/Değiştirilecek Frontend Dosyaları

Frontend tarafında da yönlendirmeler (routing) ve modüller temizlenmelidir:

- **app.routes.ts:** `mal/connect`, `list/generate`, `discover`, `search` rotaları kaldırılacak.
- **Components:**
  - `mal-connect`, `list-generate`, `discover`, `search`, `templates` modülleri silinecek.
  - `HomeComponent` tamamen yeniden tasarlanacak (Landing page olacak).
- **Services:**
  - `recommendation.service.ts`, `search.service.ts`, `jikan.service.ts` silinecek.

---

## 2. Mimari İyileştirmeler ve Entegrasyon Planı

Kullanıcının talep ettiği "Middleware", "Logging" ve "Repository Pattern" gibi yapılar, kod temizlendikten hemen sonra, yeni özellikler eklenmeden ÖNCE entegre edilecektir.

### 🏗️ Backend Entegrasyon Adımları

#### Adım 1: Middleware Entegrasyonu

Mevcut `Program.cs` içerisine eklenecek yapılar:

1.  **GlobalExceptionMiddleware:**

    - Tüm controller'lardaki `try-catch` bloklarını gereksiz kılacak.
    - `Middlewares/ExceptionMiddleware.cs` olarak oluşturulacak.
    - `Program.cs`'de `app.UseMiddleware<ExceptionMiddleware>();` olarak eklenecek.

2.  **RequestLoggingMiddleware:**
    - Gelen her isteği (Kullanıcı ID, IP, Endpoint, Süre) veritabanında `SystemLogs` tablosuna veya Serilog ile dosyaya yazacak.

#### Adım 2: Veritabanı ve Repository Pattern

Mevcut yapıdaki `Operations` statik sınıf kullanımı yerine, test edilebilir ve SOLID prensiplerine uygun Repository Pattern'e geçilecek.

1.  `Core/Interfaces/IRepository.cs` (Generic Repository Interface)
2.  `Core/Interfaces/IUnitOfWork.cs` (Transaction Yönetimi)
3.  `Infrastructure/Data/Repository.cs` (Implementation)
4.  `Operations` sınıfları, `Services` klasörü altına inject edilebilir servisler (`IAuthService`, `ISchoolService`) olarak taşınacak.

#### Adım 3: Entity Güncellemeleri

Mevcut `AppUser` sınıfı korunacak ancak eğitim platformu için genişletilecek:

- `UserRole` enum'ı güncellenecek (Admin, SchoolManager, Teacher, Student).
- İlişkisel tablolar (`School`, `Classroom`) eklenecek.

### 🏗️ Frontend Entegrasyon Adımları

#### Adım 1: Core Module Revizyonu

- **Auth Interceptor:** Token yönetimi için `auth.interceptor.ts` (Functional Interceptor) eklenecek.
- **Error Interceptor:** 401/403 hatalarını yakalayıp login'e yönlendiren yapı kurulacak.

#### Adım 2: State Management

- Kullanıcı oturum bilgileri için Angular Signals tabanlı basit bir store (`user.store.ts`) oluşturulacak.

---

## 3. Risk Analizi ve Önlemler

| Risk                    | Etki                                                                                          | Önlem                                                                                           |
| ----------------------- | --------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------- |
| **Bağımlılık Hataları** | `SearchOperations` silindiğinde onu kullanan diğer sınıflar (örn: Dashboard) derlenmeyebilir. | Silme işleminden sonra proje derlenip (Build) tüm referans hataları temizlenecek.               |
| **Veri Kaybı**          | Mevcut `AppUser` tablosunda yapısal değişiklikler (Migration) veri kaybına yol açabilir.      | Geliştirme ortamında olduğumuz için `Drop-Database` stratejisi ile temiz başlangıç yapılabilir. |
| **Frontend Routing**    | Silinen komponentlere giden rotalar uygulama açılışını bozar.                                 | `app.routes.ts` içerisindeki tüm ölü importlar temizlenecek.                                    |

---

## 4. Yol Haritası (Immediate Action Plan)

1.  **Derin Temizlik:** Belirtilen tüm anime dosyalarını sil.
2.  **Bağımlılık Temizliği:** `Program.cs` ve `ServiceRegistration.cs` dosyalarından silinen servisleri kaldır.
3.  **Altyapı Kurulumu:** Middleware ve Logging yapılarını kur.
4.  **Temel Veritabanı:** Yeni Entity'leri (`School`, `Classroom`) oluştur ve Migration al.
5.  **Kodlama:** Yeni özellikleri geliştirmeye başla (önce Backend, sonra Frontend).
