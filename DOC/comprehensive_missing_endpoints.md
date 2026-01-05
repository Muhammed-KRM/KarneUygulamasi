# Kapsamlı Eksik Endpoint'ler ve Özellikler Analizi

## 📋 1. ADMIN CONTROLLER - Tam CRUD ve Yönetim Özellikleri

### Mevcut Özellikler ✅
- Kurum onaylama
- Bekleyen kurumları listeleme

### Eksik Özellikler ❌

#### 1.1. Kullanıcı Yönetimi (Tam CRUD)
**Endpoint'ler:**
- `GET /api/admin/users?page={page}&limit={limit}&search={search}&status={status}&role={role}`
  - **Amaç:** Tüm kullanıcıları listeleme (Admin panel için)
  - **UI Kullanımı:** Admin panel > Kullanıcılar sayfası, tablo görünümü, filtreleme
  - **Özellikler:** Pagination, arama, filtreleme (Status, Role), sıralama

- `GET /api/admin/users/{id}`
  - **Amaç:** Kullanıcı detayını görüntüleme
  - **UI Kullanımı:** Kullanıcı detay modal/sayfası
  - **Özellikler:** Tüm kullanıcı bilgileri, kurum üyelikleri, aktivite özeti

- `PUT /api/admin/users/{id}`
  - **Amaç:** Kullanıcı bilgilerini güncelleme (Admin)
  - **UI Kullanımı:** Kullanıcı düzenleme formu
  - **Özellikler:** FullName, Email, Phone, GlobalRole, Status güncelleme

- `PUT /api/admin/users/{id}/status`
  - **Amaç:** Kullanıcı durumunu değiştirme (Active/Suspended/Deleted)
  - **UI Kullanımı:** Kullanıcı listesinde "Durum Değiştir" butonu
  - **Özellikler:** Status güncelleme, sebep kaydetme

- `DELETE /api/admin/users/{id}`
  - **Amaç:** Kullanıcıyı silme (hard delete)
  - **UI Kullanımı:** Kullanıcı detayında "Sil" butonu, onay dialog'u
  - **Özellikler:** Hard delete, ilişkili verileri temizleme

- `POST /api/admin/users/{id}/reset-password`
  - **Amaç:** Admin'in kullanıcı şifresini sıfırlama
  - **UI Kullanımı:** Kullanıcı detayında "Şifre Sıfırla" butonu
  - **Özellikler:** Yeni şifre oluşturma, email gönderme

#### 1.2. Kurum Yönetimi (Tam CRUD)
**Endpoint'ler:**
- `GET /api/admin/institutions?page={page}&status={status}&search={search}`
  - **Amaç:** Tüm kurumları listeleme
  - **UI Kullanımı:** Admin panel > Kurumlar sayfası
  - **Özellikler:** Filtreleme (Status), arama, pagination

- `GET /api/admin/institutions/{id}`
  - **Amaç:** Kurum detayını görüntüleme
  - **UI Kullanımı:** Kurum detay sayfası
  - **Özellikler:** Kurum bilgileri, üye sayıları, istatistikler

- `POST /api/admin/institutions/{id}/reject`
  - **Amaç:** Kurum başvurusunu reddetme
  - **UI Kullanımı:** Kurum detayında "Reddet" butonu, sebep girme
  - **Özellikler:** Red sebebi kaydetme, manager'a bildirim

- `PUT /api/admin/institutions/{id}/status`
  - **Amaç:** Kurum durumunu değiştirme (Active/Suspended/Expired)
  - **UI Kullanımı:** Kurum detayında durum değiştirme
  - **Özellikler:** Status güncelleme, sebep kaydetme

- `PUT /api/admin/institutions/{id}/subscription`
  - **Amaç:** Kurum abonelik süresini uzatma
  - **UI Kullanımı:** Kurum detayında "Abonelik Uzat" butonu
  - **Özellikler:** SubscriptionEndDate güncelleme

- `DELETE /api/admin/institutions/{id}`
  - **Amaç:** Kurumu silme
  - **UI Kullanımı:** Kurum detayında "Sil" butonu
  - **Özellikler:** Soft delete, ilişkili verileri koruma

#### 1.3. Admin Hesap Yönetimi
**Endpoint'ler:**
- `POST /api/admin/create-admin`
  - **Amaç:** Yeni admin hesabı oluşturma (Sadece AdminAdmin)
  - **UI Kullanımı:** Admin panel > Yöneticiler > Yeni Admin
  - **Özellikler:** Admin veya AdminAdmin rolü ile kullanıcı oluşturma

- `GET /api/admin/admins`
  - **Amaç:** Tüm admin hesaplarını listeleme
  - **UI Kullanımı:** Admin panel > Yöneticiler sayfası
  - **Özellikler:** Sadece Admin ve AdminAdmin rolleri

- `DELETE /api/admin/admins/{id}`
  - **Amaç:** Admin hesabını silme (Sadece AdminAdmin)
  - **UI Kullanımı:** Admin listesinde "Sil" butonu
  - **Özellikler:** AdminAdmin silinemez

#### 1.4. Sistem İstatistikleri ve Raporlar
**Endpoint'ler:**
- `GET /api/admin/statistics`
  - **Amaç:** Sistem genel istatistikleri
  - **UI Kullanımı:** Admin dashboard, istatistik kartları
  - **Özellikler:**
    - Toplam kullanıcı sayısı (Active, Suspended, Deleted)
    - Toplam kurum sayısı (Active, Pending, Suspended)
    - Toplam sınav sayısı
    - Toplam mesaj sayısı
    - Son 30 gün yeni kayıtlar
    - Son 30 gün aktif kullanıcılar

- `GET /api/admin/statistics/users-growth?days={days}`
  - **Amaç:** Kullanıcı büyüme grafiği verisi
  - **UI Kullanımı:** Dashboard'da line chart
  - **Özellikler:** Günlük/haftalık/aylık kullanıcı artışı

- `GET /api/admin/statistics/institutions-growth?days={days}`
  - **Amaç:** Kurum büyüme grafiği verisi
  - **UI Kullanımı:** Dashboard'da line chart

- `GET /api/admin/statistics/exam-statistics?days={days}`
  - **Amaç:** Sınav istatistikleri
  - **UI Kullanımı:** Dashboard'da istatistik kartları
  - **Özellikler:** Toplam sınav, ortalama öğrenci sayısı, en çok sınav yapan kurumlar

#### 1.5. Audit Log Yönetimi
**Endpoint'ler:**
- `GET /api/admin/audit-logs?userId={userId}&action={action}&dateFrom={dateFrom}&dateTo={dateTo}&page={page}`
  - **Amaç:** Sistem loglarını görüntüleme ve filtreleme
  - **UI Kullanımı:** Admin panel > Loglar sayfası, filtreleme paneli
  - **Özellikler:**
    - UserId, Action, Date range filtreleme
    - Pagination
    - Export (CSV/Excel)
    - Detaylı log görüntüleme

- `GET /api/admin/audit-logs/{id}`
  - **Amaç:** Log detayını görüntüleme
  - **UI Kullanımı:** Log listesinde detay modal
  - **Özellikler:** Tüm log bilgileri, JSON details parse

- `GET /api/admin/audit-logs/user/{userId}`
  - **Amaç:** Belirli kullanıcının loglarını görüntüleme
  - **UI Kullanımı:** Kullanıcı detayında "Aktivite Geçmişi" sekmesi
  - **Özellikler:** Kullanıcının tüm işlemleri

#### 1.6. Sistem Ayarları
**Endpoint'ler:**
- `GET /api/admin/settings`
  - **Amaç:** Sistem ayarlarını görüntüleme
  - **UI Kullanımı:** Admin panel > Ayarlar sayfası
  - **Özellikler:** Email ayarları, bildirim ayarları, genel ayarlar

- `PUT /api/admin/settings`
  - **Amaç:** Sistem ayarlarını güncelleme
  - **UI Kullanımı:** Ayarlar formu
  - **Özellikler:** Tüm sistem ayarlarını güncelleme

---

## 📋 2. USER CONTROLLER - Profil ve UI Özelleştirme

### Mevcut Özellikler ✅
- Kendi profilini görüntüleme
- Profil güncelleme
- Şifre değiştirme
- Profil resmi yükleme
- Logout
- Email doğrulama

### Eksik Özellikler ❌

#### 2.1. Başka Kullanıcı Profillerini Görüntüleme
**Endpoint'ler:**
- `GET /api/user/profile/{userId}`
  - **Amaç:** Başka kullanıcının profilini görüntüleme (ProfileVisibility kontrolü ile)
  - **UI Kullanımı:** 
    - Öğretmen öğrenci profilini görür
    - Kullanıcı arama sonuçlarında profil kartı
    - Mesajlaşmada kullanıcı bilgileri
  - **Özellikler:**
    - ProfileVisibility kontrolü (PublicToAll, TeachersOnly, Private)
    - Sadece görünür bilgileri döndürür
    - İstatistikler (sadece görünür olanlar)

#### 2.2. Email Güncelleme
**Endpoint:**
- `PUT /api/user/email`
  - **Amaç:** Email adresini güncelleme (yeni email doğrulaması ile)
  - **UI Kullanımı:** Ayarlar > Email değiştir formu
  - **Özellikler:**
    - Mevcut email doğrulaması
    - Yeni email'e verification token gönderilir
    - Email değişene kadar eski email geçerli

#### 2.3. Hesap Silme
**Endpoint:**
- `DELETE /api/user/account?password={password}&hardDelete={hardDelete}`
  - **Amaç:** Kullanıcının kendi hesabını silmesi
  - **UI Kullanımı:** Ayarlar > Hesap > Hesabı Sil, onay dialog'u
  - **Özellikler:**
    - Şifre doğrulaması
    - Soft delete (varsayılan) veya hard delete
    - 30 gün sonra hard delete (opsiyonel)

#### 2.4. Kullanıcı Arama
**Endpoint:**
- `GET /api/user/search?query={query}&role={role}&institutionId={institutionId}&page={page}&limit={limit}`
  - **Amaç:** Kullanıcı arama (öğretmen/öğrenci bulma)
  - **UI Kullanımı:** 
    - Ana sayfada arama çubuğu
    - "Öğretmen Bul" sayfası
    - Mesajlaşmada kullanıcı seçimi
  - **Özellikler:**
    - Username, FullName, Email ile arama
    - Filtreleme (Role, Institution)
    - Pagination
    - ProfileVisibility kontrolü

#### 2.5. İstatistikler ve Aktivite
**Endpoint'ler:**
- `GET /api/user/statistics`
  - **Amaç:** Kullanıcı istatistiklerini görüntüleme
  - **UI Kullanımı:** Profil sayfasında istatistik kartları
  - **Özellikler:**
    - Toplam sınav sayısı
    - Ortalama puan
    - En iyi ders
    - En çok gelişim gösterilen konu
    - Toplam mesaj sayısı
    - Son aktivite tarihi

- `GET /api/user/activity?page={page}&limit={limit}`
  - **Amaç:** Kullanıcının son aktivitelerini görüntüleme
  - **UI Kullanımı:** Profil sayfasında "Son Aktiviteler" sekmesi
  - **Özellikler:**
    - Son girişler
    - Son sınav sonuçları
    - Son mesajlaşmalar
    - Son paylaşımlar
    - Timeline görünümü

#### 2.6. UI Özelleştirme ve Ayarlar (YENİ MODEL GEREKLİ)
**Yeni Model:** `UserPreferences` veya `UserSettings`
```csharp
public class UserPreferences
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    
    // UI Ayarları
    public string Theme { get; set; } = "dark"; // dark, light, auto
    public string Language { get; set; } = "tr";
    public string DateFormat { get; set; } = "dd/MM/yyyy";
    public string TimeFormat { get; set; } = "24h"; // 24h, 12h
    
    // Bildirim Ayarları
    public bool EmailNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
    public bool ExamResultNotifications { get; set; } = true;
    public bool MessageNotifications { get; set; } = true;
    
    // Profil Düzenleme (UI Layout)
    public string ProfileLayout { get; set; } = "default"; // JSON string with widget positions
    public bool ShowStatistics { get; set; } = true;
    public bool ShowActivity { get; set; } = true;
    public bool ShowAchievements { get; set; } = true;
    
    // Dashboard Ayarları
    public string DashboardLayout { get; set; } = "default"; // JSON string
    public List<string> VisibleWidgets { get; set; } = new();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
```

**Endpoint'ler:**
- `GET /api/user/preferences`
  - **Amaç:** Kullanıcı tercihlerini görüntüleme
  - **UI Kullanımı:** Ayarlar sayfasında tüm tercihler
  - **Özellikler:** Tüm UI ve bildirim ayarları

- `PUT /api/user/preferences`
  - **Amaç:** Kullanıcı tercihlerini güncelleme
  - **UI Kullanımı:** 
    - Ayarlar > Görünüm sekmesi (Tema, Dil)
    - Ayarlar > Bildirimler sekmesi
    - Profil sayfasında widget sıralama (drag & drop)
    - Dashboard'ta widget göster/gizle
  - **Özellikler:** Tüm tercihleri güncelleme

- `PUT /api/user/preferences/profile-layout`
  - **Amaç:** Profil sayfası widget sıralamasını kaydetme
  - **UI Kullanımı:** Profil sayfasında "Düzenle" modu, drag & drop
  - **Özellikler:** JSON formatında widget pozisyonları

- `PUT /api/user/preferences/dashboard-layout`
  - **Amaç:** Dashboard widget sıralamasını kaydetme
  - **UI Kullanımı:** Dashboard'ta "Düzenle" modu, drag & drop
  - **Özellikler:** JSON formatında widget pozisyonları

#### 2.7. Kullanıcı Listeleme (Admin için)
**Endpoint:**
- `GET /api/user/all?page={page}&limit={limit}&search={search}&status={status}&role={role}`
  - **Amaç:** Tüm kullanıcıları listeleme (Admin için)
  - **UI Kullanımı:** Admin panel > Kullanıcılar sayfası
  - **Özellikler:** Pagination, arama, filtreleme

---

## 📋 3. EXAM CONTROLLER - Tam CRUD ve Raporlama

### Mevcut Özellikler ✅
- Sınav oluşturma
- Optik form işleme
- Sonuçları onaylama
- Öğrenci karne detayı

### Eksik Özellikler ❌

#### 3.1. Sınav Listeleme ve Görüntüleme
**Endpoint'ler:**
- `GET /api/exam/institution/{institutionId}?classroomId={classroomId}&page={page}&limit={limit}&sortBy={sortBy}`
  - **Amaç:** Kurum/sınıf sınavlarını listeleme
  - **UI Kullanımı:** 
    - Sınavlar sayfası
    - Sınıf detayında sınavlar sekmesi
  - **Özellikler:**
    - Filtreleme (Classroom, Date range)
    - Pagination
    - Sıralama (Date desc, Title asc)

- `GET /api/exam/{id}`
  - **Amaç:** Sınav detaylarını görüntüleme
  - **UI Kullanımı:** Sınav detay sayfası
  - **Özellikler:**
    - Sınav bilgileri
    - Toplam öğrenci sayısı
    - İşlenmiş sonuç sayısı
    - Onay durumu
    - Cevap anahtarı önizleme

- `GET /api/exam/student/{studentId}?page={page}`
  - **Amaç:** Öğrencinin tüm sınavlarını listeleme
  - **UI Kullanımı:** Öğrenci profilinde "Sınavlarım" sekmesi
  - **Özellikler:** Pagination, sıralama

#### 3.2. Sınav Güncelleme ve Silme
**Endpoint'ler:**
- `PUT /api/exam/{id}`
  - **Amaç:** Sınav bilgilerini güncelleme (sonuçlar onaylanmadan önce)
  - **UI Kullanımı:** Sınav detay sayfasında "Düzenle" butonu
  - **Özellikler:**
    - Sadece sonuçlar onaylanmadan önce
    - Title, Date, AnswerKey güncelleme
    - LessonConfig güncelleme

- `DELETE /api/exam/{id}`
  - **Amaç:** Sınavı silme (sonuçlar onaylanmadan önce)
  - **UI Kullanımı:** Sınav detay sayfasında "Sil" butonu
  - **Özellikler:**
    - Sadece sonuçlar onaylanmadan önce silinebilir
    - İlişkili sonuçlar da silinir
    - Onay dialog'u

#### 3.3. Sınav Sonuçları Yönetimi (Öğretmen)
**Endpoint'ler:**
- `GET /api/exam/{examId}/results?page={page}&limit={limit}&sortBy={sortBy}&classroomId={classroomId}`
  - **Amaç:** Öğretmenin sınav sonuçlarını görüntülemesi
  - **UI Kullanımı:** 
    - Sınav detay sayfasında "Sonuçlar" sekmesi
    - Tablo görünümü
  - **Özellikler:**
    - Tüm öğrenci sonuçları
    - Sıralama (Net, Rank, Name)
    - Filtreleme (Classroom)
    - Pagination
    - Export butonu (Excel/PDF)

- `GET /api/exam/{examId}/results/export?format={format}&classroomId={classroomId}`
  - **Amaç:** Sınav sonuçlarını export etme
  - **UI Kullanımı:** Sonuçlar tablosunda "Export" butonu
  - **Özellikler:**
    - Format: Excel, PDF, CSV
    - Filtreleme (Classroom)
    - Detaylı rapor

- `PUT /api/exam/{examId}/result/{resultId}`
  - **Amaç:** Öğretmenin sonuçları manuel düzeltmesi
  - **UI Kullanımı:** Sonuçlar tablosunda "Düzenle" butonu, modal form
  - **Özellikler:**
    - Net, Score, Rank güncelleme
    - DetailedResultsJson güncelleme
    - Audit log
    - Sıralamaları yeniden hesaplama

- `DELETE /api/exam/{examId}/result/{resultId}`
  - **Amaç:** Sonucu silme (yanlış işlenmişse)
  - **UI Kullanımı:** Sonuçlar tablosunda "Sil" butonu
  - **Özellikler:**
    - Sonuç silinir
    - Sıralamalar yeniden hesaplanır

#### 3.4. Sınav İstatistikleri
**Endpoint:**
- `GET /api/exam/{id}/statistics`
  - **Amaç:** Sınav istatistiklerini görüntüleme
  - **UI Kullanımı:** Sınav detay sayfasında "İstatistikler" sekmesi
  - **Özellikler:**
    - Ortalama net
    - En yüksek/en düşük net
    - Ders bazında ortalama
    - Başarı yüzdesi
    - Grafik verileri (histogram)

---

## 📋 4. REPORT CONTROLLER - Kapsamlı Raporlama

### Mevcut Özellikler ✅
- Öğrenci karne detayı (tek sınav)

### Eksik Özellikler ❌

#### 4.1. Öğrenci Karneleri Listeleme
**Endpoint'ler:**
- `GET /api/report/student/{studentId}/all?page={page}&limit={limit}&examType={type}&lesson={lesson}&dateFrom={dateFrom}&dateTo={dateTo}`
  - **Amaç:** Öğrencinin tüm karnelerini listeleme
  - **UI Kullanımı:** 
    - Öğrenci profilinde "Karnelerim" sekmesi
    - Liste görünümü, kart görünümü
  - **Özellikler:**
    - Tüm sınav sonuçları
    - Filtreleme (ExamType, Lesson, Date range)
    - Sıralama (Date desc)
    - Pagination

- `GET /api/report/student/{studentId}/summary`
  - **Amaç:** Öğrencinin genel performans özeti
  - **UI Kullanımı:** Profil sayfasında özet kart
  - **Özellikler:**
    - Toplam sınav sayısı
    - Ortalama net
    - En iyi ders
    - En çok gelişim gösterilen konu
    - Genel sıralama trendi

#### 4.2. Sınıf Karneleri (Öğretmen)
**Endpoint'ler:**
- `GET /api/report/classroom/{classroomId}/exam/{examId}`
  - **Amaç:** Öğretmenin sınıfın tüm karnelerini görüntülemesi
  - **UI Kullanımı:** 
    - Sınıf detayında "Sınav Sonuçları" sekmesi
    - Tablo görünümü
  - **Özellikler:**
    - Tüm öğrenci sonuçları
    - Sıralama ve filtreleme
    - Export (Excel/PDF)
    - İstatistikler

- `GET /api/report/classroom/{classroomId}/all?page={page}`
  - **Amaç:** Sınıfın tüm sınav sonuçlarını listeleme
  - **UI Kullanımı:** Sınıf detayında "Tüm Sınavlar" sekmesi
  - **Özellikler:** Pagination, sıralama

#### 4.3. İlerleme ve Analiz
**Endpoint'ler:**
- `GET /api/report/student/{studentId}/progress?lesson={lesson}&dateFrom={dateFrom}&dateTo={dateTo}`
  - **Amaç:** Öğrencinin ders bazında ilerleme grafiği
  - **UI Kullanımı:** 
    - Profil sayfasında "İlerleme" sekmesi
    - Line chart görünümü
  - **Özellikler:**
    - Zaman içinde net değişimi
    - Ders bazında ayrı grafikler
    - Trend analizi
    - Tahmin (opsiyonel)

- `GET /api/report/student/{studentId}/topic-analysis?lesson={lesson}`
  - **Amaç:** Konu bazında performans analizi
  - **UI Kullanımı:** Profil sayfasında "Konu Analizi" sekmesi
  - **Özellikler:**
    - Her konu için ortalama net
    - En iyi/en kötü konular
    - Gelişim önerileri

- `GET /api/report/student/{studentId}/comparison?compareWith={compareWith}`
  - **Amaç:** Öğrencinin diğer öğrencilerle karşılaştırması
  - **UI Kullanımı:** Profil sayfasında "Karşılaştırma" sekmesi
  - **Özellikler:**
    - Sınıf ortalaması ile karşılaştırma
    - Kurum ortalaması ile karşılaştırma
    - Grafik görünümü

#### 4.4. Kurum Raporları (Manager)
**Endpoint'ler:**
- `GET /api/report/institution/{institutionId}/summary?dateFrom={dateFrom}&dateTo={dateTo}`
  - **Amaç:** Kurum genel performans özeti
  - **UI Kullanımı:** Manager dashboard'u
  - **Özellikler:**
    - Toplam sınav sayısı
    - Ortalama başarı oranı
    - En başarılı sınıflar
    - En başarılı öğrenciler

- `GET /api/report/institution/{institutionId}/classroom-comparison`
  - **Amaç:** Sınıflar arası karşılaştırma
  - **UI Kullanımı:** Manager dashboard'unda grafik
  - **Özellikler:**
    - Sınıf bazında ortalama net
    - Bar chart görünümü

#### 4.5. Export ve Paylaşım
**Endpoint'ler:**
- `GET /api/report/student/{studentId}/export?format={format}&examIds={examIds}`
  - **Amaç:** Öğrenci karnelerini export etme
  - **UI Kullanımı:** Profil sayfasında "Export" butonu
  - **Özellikler:**
    - Format: PDF, Excel
    - Seçili sınavları export
    - Detaylı rapor

- `POST /api/report/share/{resultId}`
  - **Amaç:** Karneyi paylaşma (link oluşturma)
  - **UI Kullanımı:** Karne detayında "Paylaş" butonu
  - **Özellikler:**
    - Geçici paylaşım linki oluşturma
    - Expiry date
    - Password protection (opsiyonel)

---

## 📋 5. CLASSROOM CONTROLLER - Tam CRUD

### Mevcut Özellikler ✅
- Sınıf oluşturma
- Sınıf detayı görüntüleme
- Öğrenci ekleme (tekil ve toplu)
- Sınıf listesi

### Eksik Özellikler ❌

#### 5.1. Sınıf Güncelleme ve Silme
**Endpoint'ler:**
- `PUT /api/classroom/{id}`
  - **Amaç:** Sınıf bilgilerini güncelleme
  - **UI Kullanımı:** Sınıf ayarları sayfası
  - **Özellikler:**
    - Name, Grade, HeadTeacherId güncelleme
    - Cache invalidation

- `DELETE /api/classroom/{id}`
  - **Amaç:** Sınıfı silme (soft delete)
  - **UI Kullanımı:** Sınıf ayarları > Sil butonu
  - **Özellikler:**
    - IsActive = false
    - İlişkili veriler korunur
    - Onay dialog'u

#### 5.2. Öğrenci Yönetimi
**Endpoint'ler:**
- `DELETE /api/classroom/{classroomId}/student/{studentId}`
  - **Amaç:** Sınıftan öğrenci çıkarma
  - **UI Kullanımı:** Sınıf detay sayfasında öğrenci listesinde "Çıkar" butonu
  - **Özellikler:**
    - Sadece Manager yetkisi
    - Audit log
    - Cache invalidation

- `GET /api/classroom/{classroomId}/students?page={page}&search={search}`
  - **Amaç:** Sınıf öğrencilerini listeleme (filtreleme ile)
  - **UI Kullanımı:** Sınıf detayında öğrenci listesi
  - **Özellikler:**
    - Arama (Name, StudentNumber)
    - Pagination
    - Sıralama

#### 5.3. Öğretmen Yönetimi
**Endpoint'ler:**
- `POST /api/classroom/{classroomId}/teacher/{teacherId}`
  - **Amaç:** Sınıfa öğretmen atama
  - **UI Kullanımı:** Sınıf detay sayfasında "Öğretmenler" sekmesi
  - **Özellikler:**
    - ClassroomTeacher tablosu kullanılır
    - Sadece Manager yetkisi

- `DELETE /api/classroom/{classroomId}/teacher/{teacherId}`
  - **Amaç:** Sınıftan öğretmen kaldırma
  - **UI Kullanımı:** Öğretmen listesinde "Kaldır" butonu
  - **Özellikler:**
    - Sadece Manager yetkisi
    - Audit log

- `GET /api/classroom/{classroomId}/teachers`
  - **Amaç:** Sınıf öğretmenlerini listeleme
  - **UI Kullanımı:** Sınıf detayında öğretmen listesi
  - **Özellikler:** Tüm öğretmenler

#### 5.4. Sınıf İstatistikleri
**Endpoint:**
- `GET /api/classroom/{id}/statistics`
  - **Amaç:** Sınıf istatistiklerini görüntüleme
  - **UI Kullanımı:** Sınıf detay sayfasında istatistik kartları
  - **Özellikler:**
    - Öğrenci sayısı
    - Toplam sınav sayısı
    - Ortalama başarı oranı
    - En başarılı öğrenciler

#### 5.5. Sınıf Filtreleme ve Arama
**Endpoint:**
- `GET /api/classroom/institution/{institutionId}?grade={grade}&search={search}&page={page}`
  - **Amaç:** Sınıfları filtreleme ve arama
  - **UI Kullanımı:** Sınıf listesi sayfasında filtreler
  - **Özellikler:**
    - Grade'e göre filtreleme
    - İsme göre arama
    - Pagination

---

## 📋 6. MESSAGE CONTROLLER - Tam Özellikler

### Mevcut Özellikler ✅
- Konuşma başlatma
- Mesaj gönderme
- Mesaj geçmişi
- Sınıfa toplu gönderim

### Eksik Özellikler ❌

#### 6.1. Konuşma Yönetimi
**Endpoint'ler:**
- `GET /api/message/conversations?page={page}&limit={limit}`
  - **Amaç:** Kullanıcının tüm konuşmalarını listeleme
  - **UI Kullanımı:** Mesajlaşma sayfasında sol panel
  - **Özellikler:**
    - Son mesaj önizlemesi
    - Okunmamış mesaj sayısı
    - Son mesaj zamanı
    - Sıralama (en son mesaj üstte)
    - Pagination

- `GET /api/message/conversation/{id}`
  - **Amaç:** Konuşma bilgilerini görüntüleme
  - **UI Kullanımı:** Konuşma başlığı, üye listesi
  - **Özellikler:**
    - Konuşma bilgileri
    - Üye listesi
    - Konuşma ayarları

- `PUT /api/message/conversation/{id}`
  - **Amaç:** Konuşma bilgilerini güncelleme (grup konuşmaları için)
  - **UI Kullanımı:** Konuşma ayarları
  - **Özellikler:**
    - Title güncelleme
    - Sadece grup konuşmaları

- `DELETE /api/message/conversation/{id}`
  - **Amaç:** Konuşmayı arşivleme/silme
  - **UI Kullanımı:** Konuşma ayarları > "Sil" butonu
  - **Özellikler:**
    - Soft delete
    - Sadece özel mesajlar silinebilir

- `POST /api/message/conversation/{id}/leave`
  - **Amaç:** Grup konuşmasından çıkma
  - **UI Kullanımı:** Konuşma ayarları > "Konuşmadan Çık" butonu
  - **Özellikler:**
    - ConversationMember'dan kaldırılır
    - Özel mesajlardan çıkılamaz

#### 6.2. Mesaj Yönetimi
**Endpoint'ler:**
- `DELETE /api/message/{id}`
  - **Amaç:** Kendi mesajını silme
  - **UI Kullanımı:** Mesaj üzerinde "Sil" butonu
  - **Özellikler:**
    - Soft delete (IsDeleted = true)
    - Sadece kendi mesajı silinebilir

- `PUT /api/message/{id}`
  - **Amaç:** Mesajı düzenleme
  - **UI Kullanımı:** Mesaj üzerinde "Düzenle" butonu
  - **Özellikler:**
    - Sadece kendi mesajı
    - Sadece Text mesajları
    - Edit history (opsiyonel)

#### 6.3. Okundu İşaretleme
**Endpoint:**
- `POST /api/message/conversation/{id}/mark-read`
  - **Amaç:** Konuşmadaki tüm mesajları okundu işaretleme
  - **UI Kullanımı:** 
    - Otomatik (konuşma açıldığında)
    - Manuel "Tümünü Okundu İşaretle" butonu
  - **Özellikler:**
    - ConversationMember.LastReadAt güncellenir
    - Okunmamış sayısı sıfırlanır

#### 6.4. Mesaj Arama
**Endpoint:**
- `GET /api/message/search?query={query}&conversationId={conversationId}&page={page}`
  - **Amaç:** Mesajlarda arama
  - **UI Kullanımı:** Konuşma içinde arama çubuğu
  - **Özellikler:**
    - Text içinde arama
    - Belirli konuşmada veya tüm konuşmalarda
    - Pagination

---

## 📋 7. NOTIFICATION CONTROLLER - Tam Özellikler

### Mevcut Özellikler ✅
- Bildirimleri listeleme
- Bildirimi okundu işaretleme

### Eksik Özellikler ❌

#### 7.1. Bildirim Yönetimi
**Endpoint'ler:**
- `POST /api/notification/mark-all-read`
  - **Amaç:** Tüm bildirimleri okundu işaretleme
  - **UI Kullanımı:** Bildirimler sayfasında "Tümünü Okundu İşaretle" butonu
  - **Özellikler:**
    - Toplu güncelleme
    - Cache invalidation

- `DELETE /api/notification/{id}`
  - **Amaç:** Bildirimi silme
  - **UI Kullanımı:** Bildirim kartında "Sil" butonu
  - **Özellikler:**
    - Soft delete veya hard delete

- `DELETE /api/notification/clear-all`
  - **Amaç:** Tüm bildirimleri silme
  - **UI Kullanımı:** Bildirimler sayfasında "Tümünü Temizle" butonu
  - **Özellikler:**
    - Onay dialog'u
    - Toplu silme

#### 7.2. Bildirim Ayarları
**Endpoint'ler:**
- `GET /api/notification/settings`
  - **Amaç:** Bildirim tercihlerini görüntüleme
  - **UI Kullanımı:** Ayarlar > Bildirimler sekmesi
  - **Özellikler:**
    - Email bildirimleri aç/kapa
    - Push bildirimleri aç/kapa
    - Bildirim türlerine göre ayarlar

- `PUT /api/notification/settings`
  - **Amaç:** Bildirim tercihlerini güncelleme
  - **UI Kullanımı:** Ayarlar formu, toggle switch'ler
  - **Özellikler:**
    - Tüm bildirim ayarlarını güncelleme
    - UserPreferences'a kaydedilir

#### 7.3. Bildirim Filtreleme
**Endpoint:**
- `GET /api/notification/my?type={type}&isRead={isRead}&dateFrom={dateFrom}&dateTo={dateTo}&page={page}`
  - **Amaç:** Bildirimleri filtreleme
  - **UI Kullanımı:** 
    - Bildirimler sayfasında filtreler
    - "Okunmamışlar", "Mesajlar", "Sınavlar" sekmeleri
  - **Özellikler:**
    - Type'a göre filtreleme
    - Okunma durumuna göre filtreleme
    - Tarih aralığı
    - Pagination

---

## 📋 8. ACCOUNT CONTROLLER - Hesap Bağlama Tam Özellikler

### Mevcut Özellikler ✅
- Hesap bağlama talebi
- Hesap bağlama onayı
- Hesap bağlama reddi

### Eksik Özellikler ❌

#### 8.1. Bağlantı Yönetimi
**Endpoint'ler:**
- `GET /api/account/link-requests?status={status}&page={page}`
  - **Amaç:** Bekleyen/onaylanmış/reddedilmiş bağlantı taleplerini listeleme
  - **UI Kullanımı:** 
    - Manager dashboard'unda "Hesap Bağlama Talepleri" sekmesi
    - Liste görünümü
  - **Özellikler:**
    - Status'e göre filtreleme
    - Pagination
    - Detaylı bilgiler

- `GET /api/account/links`
  - **Amaç:** Kullanıcının bağlı hesaplarını listeleme
  - **UI Kullanımı:** 
    - Profil sayfasında "Bağlı Hesaplar" sekmesi
    - Liste görünümü
  - **Özellikler:**
    - Tüm bağlı kurumlar
    - Rol bilgisi
    - Bağlantı tarihi

- `DELETE /api/account/link/{id}`
  - **Amaç:** Onaylanmış bağlantıyı kaldırma
  - **UI Kullanımı:** 
    - Hesap ayarlarında "Bağlı Hesaplar" sekmesi
    - "Bağlantıyı Kaldır" butonu
  - **Özellikler:**
    - Sadece Manager veya MainUser kaldırabilir
    - Onay dialog'u
    - Audit log

---

## 📋 9. INSTITUTION CONTROLLER (YENİ)

### Tamamen Eksik ❌

#### 9.1. Kurum Yönetimi
**Endpoint'ler:**
- `GET /api/institution/my`
  - **Amaç:** Kullanıcının kurumlarını listeleme
  - **UI Kullanımı:** Dashboard'da kurum seçici
  - **Özellikler:** Tüm kurumlar ve rolleri

- `GET /api/institution/{id}`
  - **Amaç:** Kurum detayını görüntüleme
  - **UI Kullanımı:** Kurum detay sayfası
  - **Özellikler:**
    - Kurum bilgileri
    - Üye sayıları
    - İstatistikler

- `PUT /api/institution/{id}`
  - **Amaç:** Kurum bilgilerini güncelleme (Manager)
  - **UI Kullanımı:** Kurum ayarları sayfası
  - **Özellikler:**
    - Name, Address, Phone güncelleme
    - Sadece Manager yetkisi

#### 9.2. Kurum Üye Yönetimi
**Endpoint'ler:**
- `GET /api/institution/{id}/members?role={role}&page={page}&search={search}`
  - **Amaç:** Kurum üyelerini listeleme
  - **UI Kullanımı:** Kurum detayında "Üyeler" sekmesi
  - **Özellikler:**
    - Filtreleme (Role)
    - Arama
    - Pagination

- `POST /api/institution/{id}/add-member`
  - **Amaç:** Kuruma üye ekleme (Manager)
  - **UI Kullanımı:** Üyeler sekmesinde "Üye Ekle" butonu
  - **Özellikler:**
    - UserId, Role, Number (StudentNumber/EmployeeNumber)
    - Sadece Manager yetkisi

- `DELETE /api/institution/{id}/member/{memberId}`
  - **Amaç:** Üye çıkarma (Manager)
  - **UI Kullanımı:** Üye listesinde "Çıkar" butonu
  - **Özellikler:**
    - Sadece Manager yetkisi
    - Manager çıkarılamaz

- `PUT /api/institution/{id}/member/{memberId}/role`
  - **Amaç:** Üye rolünü değiştirme (Manager)
  - **UI Kullanımı:** Üye listesinde "Rol Değiştir" butonu
  - **Özellikler:**
    - Sadece Manager yetkisi
    - Manager rolü değiştirilemez

#### 9.3. Kurum İstatistikleri
**Endpoint:**
- `GET /api/institution/{id}/statistics`
  - **Amaç:** Kurum istatistiklerini görüntüleme
  - **UI Kullanımı:** Manager dashboard'u
  - **Özellikler:**
    - Toplam öğrenci sayısı
    - Toplam öğretmen sayısı
    - Toplam sınıf sayısı
    - Toplam sınav sayısı
    - Ortalama başarı oranı

---

## 📋 10. SEARCH CONTROLLER (YENİ)

### Tamamen Eksik ❌

#### 10.1. Genel Arama
**Endpoint'ler:**
- `GET /api/search/users?query={query}&role={role}&institutionId={institutionId}&page={page}`
  - **Amaç:** Kullanıcı arama
  - **UI Kullanımı:** 
    - Ana sayfada arama çubuğu
    - "Öğretmen Bul" sayfası
  - **Özellikler:**
    - Username, FullName, Email ile arama
    - Filtreleme (Role, Institution)
    - ProfileVisibility kontrolü
    - Pagination

- `GET /api/search/institutions?query={query}&status={status}&page={page}`
  - **Amaç:** Kurum arama
  - **UI Kullanımı:** Kurum arama sayfası
  - **Özellikler:**
    - Name, LicenseNumber ile arama
    - Filtreleme (Status)
    - Pagination

- `GET /api/search/classrooms?query={query}&institutionId={institutionId}&grade={grade}&page={page}`
  - **Amaç:** Sınıf arama
  - **UI Kullanımı:** Sınıf arama sayfası
  - **Özellikler:**
    - Name ile arama
    - Filtreleme (Institution, Grade)
    - Pagination

- `GET /api/search/exams?query={query}&institutionId={institutionId}&type={type}&page={page}`
  - **Amaç:** Sınav arama
  - **UI Kullanımı:** Sınav arama sayfası
  - **Özellikler:**
    - Title ile arama
    - Filtreleme (Institution, Type, Date)
    - Pagination

---

## 📋 11. YENİ MODELLER GEREKLİ

### 11.1. UserPreferences Modeli
```csharp
public class UserPreferences
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    
    // UI Ayarları
    public string Theme { get; set; } = "dark";
    public string Language { get; set; } = "tr";
    public string DateFormat { get; set; } = "dd/MM/yyyy";
    public string TimeFormat { get; set; } = "24h";
    
    // Bildirim Ayarları
    public bool EmailNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
    public bool ExamResultNotifications { get; set; } = true;
    public bool MessageNotifications { get; set; } = true;
    
    // Profil Düzenleme (UI Layout)
    public string ProfileLayout { get; set; } = "{}"; // JSON
    public bool ShowStatistics { get; set; } = true;
    public bool ShowActivity { get; set; } = true;
    
    // Dashboard Ayarları
    public string DashboardLayout { get; set; } = "{}"; // JSON
    public List<string> VisibleWidgets { get; set; } = new();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
```

### 11.2. ShareableLink Modeli (Karne Paylaşımı için)
```csharp
public class ShareableLink
{
    public int Id { get; set; }
    public int ExamResultId { get; set; }
    public ExamResult ExamResult { get; set; }
    
    public string Token { get; set; } = string.Empty;
    public string? Password { get; set; } // Optional password protection
    public DateTime ExpiresAt { get; set; }
    public int AccessCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

---

## 📊 Öncelik Matrisi

### 🔴 KRİTİK (Hemen Eklenmeli - Sistem Çalışması İçin)
1. **Admin:** GetAllUsers, GetUser, UpdateUser, DeleteUser
2. **User:** GetUserProfile (başka kullanıcı), SearchUsers
3. **Exam:** GetExams, GetExam, GetExamResults (öğretmen)
4. **Report:** GetStudentAllReports, GetClassroomReports
5. **Institution:** Tüm CRUD operasyonları
6. **UserPreferences:** Model ve endpoint'ler (UI özelleştirme)

### 🟡 YÜKSEK ÖNCELİK (Yakında - UX İçin)
7. **Message:** GetConversations, DeleteMessage, MarkRead
8. **Notification:** MarkAllRead, DeleteNotification, Settings
9. **Classroom:** UpdateClassroom, DeleteClassroom, RemoveStudent
10. **Account:** GetLinkRequests, GetLinks, DeleteLink
11. **Search:** Tüm arama endpoint'leri

### 🟢 ORTA ÖNCELİK (Gelecek Fazlarda)
12. **Report:** Progress, TopicAnalysis, Comparison
13. **Admin:** Statistics, AuditLogs detaylı
14. **User:** Activity, Statistics detaylı
15. **Exam:** Statistics, Export

---

## 📝 Özet

**Toplam Eksik Endpoint Sayısı: ~80+**

- **Admin Controller:** 15+ endpoint
- **User Controller:** 10+ endpoint
- **Exam Controller:** 8+ endpoint
- **Report Controller:** 10+ endpoint
- **Classroom Controller:** 8+ endpoint
- **Message Controller:** 6+ endpoint
- **Notification Controller:** 5+ endpoint
- **Account Controller:** 3+ endpoint
- **Institution Controller (YENİ):** 10+ endpoint
- **Search Controller (YENİ):** 4+ endpoint
- **Yeni Modeller:** UserPreferences, ShareableLink

Bu endpoint'lerin hepsi eklenmeli mi, yoksa öncelikli olanlardan başlayalım mı?

