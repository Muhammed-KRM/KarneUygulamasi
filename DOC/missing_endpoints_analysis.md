# Eksik Endpoint'ler ve Özellikler Analizi

## 📋 1. USER CONTROLLER - Profil ve Hesap Yönetimi

### Mevcut Özellikler ✅
- Profil görüntüleme (kendi)
- Profil güncelleme
- Şifre değiştirme
- Profil resmi yükleme
- Logout
- Email doğrulama

### Eksik Özellikler ❌

#### 1.1. Başka Kullanıcı Profillerini Görüntüleme
**Endpoint:** `GET /api/user/profile/{userId}`
- **Amaç:** Başka kullanıcıların profillerini görüntüleme (ProfileVisibility kontrolü ile)
- **UI Kullanımı:** 
  - Öğretmen öğrenci profilini görmek istediğinde
  - Kullanıcı arama sonuçlarında profil kartı gösterilirken
  - Mesajlaşma sırasında kullanıcı bilgilerini gösterirken
- **Özellikler:**
  - ProfileVisibility kontrolü (PublicToAll, TeachersOnly, Private)
  - Sadece görünür bilgileri döndürür

#### 1.2. Email Güncelleme
**Endpoint:** `PUT /api/user/email`
- **Amaç:** Email adresini güncelleme (yeni email doğrulaması ile)
- **UI Kullanımı:** 
  - Ayarlar sayfasında email değiştirme formu
  - Yeni email'e doğrulama kodu gönderilir
- **Özellikler:**
  - Mevcut email doğrulaması
  - Yeni email'e verification token gönderilir
  - Email değişene kadar eski email geçerli kalır

#### 1.3. Hesap Silme (Soft Delete)
**Endpoint:** `DELETE /api/user/account`
- **Amaç:** Kullanıcının kendi hesabını silmesi
- **UI Kullanımı:** 
  - Ayarlar > Hesap > Hesabı Sil butonu
  - Onay dialog'u ile şifre istenir
- **Özellikler:**
  - Soft delete (Status = Deleted)
  - Şifre doğrulaması gerekir
  - 30 gün sonra hard delete (opsiyonel)

#### 1.4. Aktivite Geçmişi
**Endpoint:** `GET /api/user/activity`
- **Amaç:** Kullanıcının son aktivitelerini görüntüleme
- **UI Kullanımı:** 
  - Profil sayfasında "Son Aktiviteler" sekmesi
  - Timeline görünümü
- **Özellikler:**
  - Son girişler
  - Son sınav sonuçları
  - Son mesajlaşmalar
  - Son paylaşımlar

#### 1.5. İstatistikler
**Endpoint:** `GET /api/user/statistics`
- **Amaç:** Kullanıcı istatistiklerini görüntüleme
- **UI Kullanımı:** 
  - Profil sayfasında istatistik kartları
  - Dashboard'da özet bilgiler
- **Özellikler:**
  - Toplam sınav sayısı
  - Ortalama puan
  - En iyi ders
  - En çok gelişim gösterilen konu
  - Toplam mesaj sayısı

#### 1.6. Kullanıcı Arama
**Endpoint:** `GET /api/user/search?query={query}&page={page}&limit={limit}`
- **Amaç:** Kullanıcı arama (öğretmen/öğrenci bulma)
- **UI Kullanımı:** 
  - Ana sayfada arama çubuğu
  - "Öğretmen Bul" sayfası
  - Mesajlaşmada kullanıcı seçimi
- **Özellikler:**
  - Username, FullName, Email ile arama
  - Filtreleme (Role, Institution)
  - Pagination

---

## 📋 2. CLASSROOM CONTROLLER - Sınıf Yönetimi

### Mevcut Özellikler ✅
- Sınıf oluşturma
- Sınıf detayı görüntüleme
- Öğrenci ekleme (tekil ve toplu)
- Sınıf listesi

### Eksik Özellikler ❌

#### 2.1. Öğrenci Çıkarma
**Endpoint:** `DELETE /api/classroom/{classroomId}/student/{studentId}`
- **Amaç:** Sınıftan öğrenci çıkarma
- **UI Kullanımı:** 
  - Sınıf detay sayfasında öğrenci listesinde "Çıkar" butonu
  - Onay dialog'u
- **Özellikler:**
  - Sadece Manager yetkisi
  - Audit log

#### 2.2. Sınıf Güncelleme
**Endpoint:** `PUT /api/classroom/{id}`
- **Amaç:** Sınıf bilgilerini güncelleme (isim, sınıf öğretmeni)
- **UI Kullanımı:** 
  - Sınıf ayarları sayfası
  - Düzenle butonu
- **Özellikler:**
  - Name, Grade, HeadTeacherId güncelleme

#### 2.3. Sınıf Silme
**Endpoint:** `DELETE /api/classroom/{id}`
- **Amaç:** Sınıfı silme (soft delete)
- **UI Kullanımı:** 
  - Sınıf ayarları > Sil butonu
  - Onay dialog'u
- **Özellikler:**
  - IsActive = false
  - İlişkili veriler korunur

#### 2.4. Öğretmen Ekleme/Çıkarma
**Endpoint:** 
- `POST /api/classroom/{classroomId}/teacher/{teacherId}`
- `DELETE /api/classroom/{classroomId}/teacher/{teacherId}`
- **Amaç:** Sınıfa öğretmen atama/kaldırma
- **UI Kullanımı:** 
  - Sınıf detay sayfasında "Öğretmenler" sekmesi
  - Öğretmen ekle/çıkar butonları
- **Özellikler:**
  - ClassroomTeacher tablosu kullanılır
  - Sadece Manager yetkisi

#### 2.5. Sınıf Filtreleme ve Arama
**Endpoint:** `GET /api/classroom/institution/{institutionId}?grade={grade}&search={search}`
- **Amaç:** Sınıfları filtreleme ve arama
- **UI Kullanımı:** 
  - Sınıf listesi sayfasında filtreler
  - Arama çubuğu
- **Özellikler:**
  - Grade'e göre filtreleme
  - İsme göre arama
  - Pagination

---

## 📋 3. EXAM CONTROLLER - Sınav Yönetimi

### Mevcut Özellikler ✅
- Sınav oluşturma
- Optik form işleme
- Sonuçları onaylama

### Eksik Özellikler ❌

#### 3.1. Sınav Listesi
**Endpoint:** `GET /api/exam/institution/{institutionId}?classroomId={classroomId}&page={page}`
- **Amaç:** Kurum/sınıf sınavlarını listeleme
- **UI Kullanımı:** 
  - Sınavlar sayfası
  - Sınıf detayında sınavlar sekmesi
- **Özellikler:**
  - Filtreleme (Classroom, Date range)
  - Pagination
  - Sıralama (Date desc)

#### 3.2. Sınav Detayı
**Endpoint:** `GET /api/exam/{id}`
- **Amaç:** Sınav detaylarını görüntüleme
- **UI Kullanımı:** 
  - Sınav detay sayfası
  - Sınav kartına tıklayınca
- **Özellikler:**
  - Sınav bilgileri
  - Toplam öğrenci sayısı
  - İşlenmiş sonuç sayısı
  - Onay durumu

#### 3.3. Sınav Güncelleme
**Endpoint:** `PUT /api/exam/{id}`
- **Amaç:** Sınav bilgilerini güncelleme (sonuçlar onaylanmadan önce)
- **UI Kullanımı:** 
  - Sınav detay sayfasında "Düzenle" butonu
- **Özellikler:**
  - Sadece sonuçlar onaylanmadan önce
  - Title, Date, AnswerKey güncelleme

#### 3.4. Sınav Silme
**Endpoint:** `DELETE /api/exam/{id}`
- **Amaç:** Sınavı silme (sonuçlar onaylanmadan önce)
- **UI Kullanımı:** 
  - Sınav detay sayfasında "Sil" butonu
  - Onay dialog'u
- **Özellikler:**
  - Sadece sonuçlar onaylanmadan önce silinebilir
  - İlişkili sonuçlar da silinir

#### 3.5. Sınav Sonuçlarını Görüntüleme (Öğretmen)
**Endpoint:** `GET /api/exam/{examId}/results?page={page}&sortBy={sortBy}`
- **Amaç:** Öğretmenin sınav sonuçlarını görüntülemesi
- **UI Kullanımı:** 
  - Sınav detay sayfasında "Sonuçlar" sekmesi
  - Tablo görünümü
- **Özellikler:**
  - Tüm öğrenci sonuçları
  - Sıralama (Net, Rank)
  - Filtreleme
  - Export (Excel/PDF)

#### 3.6. Sınav Sonuçlarını Düzeltme
**Endpoint:** `PUT /api/exam/{examId}/result/{resultId}`
- **Amaç:** Öğretmenin sonuçları manuel düzeltmesi
- **UI Kullanımı:** 
  - Sonuçlar tablosunda "Düzenle" butonu
  - Modal form
- **Özellikler:**
  - Net, Score, Rank güncelleme
  - Audit log

---

## 📋 4. MESSAGE CONTROLLER - Mesajlaşma

### Mevcut Özellikler ✅
- Konuşma başlatma
- Mesaj gönderme
- Mesaj geçmişi
- Sınıfa toplu gönderim

### Eksik Özellikler ❌

#### 4.1. Konuşma Listesi
**Endpoint:** `GET /api/message/conversations`
- **Amaç:** Kullanıcının tüm konuşmalarını listeleme
- **UI Kullanımı:** 
  - Mesajlaşma sayfasında sol panel
  - Konuşma listesi
- **Özellikler:**
  - Son mesaj önizlemesi
  - Okunmamış mesaj sayısı
  - Son mesaj zamanı
  - Sıralama (en son mesaj üstte)

#### 4.2. Konuşma Detayı
**Endpoint:** `GET /api/message/conversation/{id}`
- **Amaç:** Konuşma bilgilerini görüntüleme
- **UI Kullanımı:** 
  - Konuşma başlığı
  - Üye listesi
- **Özellikler:**
  - Konuşma bilgileri
  - Üye listesi
  - Konuşma ayarları

#### 4.3. Mesaj Silme
**Endpoint:** `DELETE /api/message/{id}`
- **Amaç:** Kendi mesajını silme
- **UI Kullanımı:** 
  - Mesaj üzerinde "Sil" butonu
  - Onay dialog'u
- **Özellikler:**
  - Soft delete (IsDeleted = true)
  - Sadece kendi mesajı silinebilir

#### 4.4. Konuşmadan Çıkma
**Endpoint:** `DELETE /api/message/conversation/{id}/leave`
- **Amaç:** Grup konuşmasından çıkma
- **UI Kullanımı:** 
  - Konuşma ayarları > "Konuşmadan Çık" butonu
- **Özellikler:**
  - ConversationMember'dan kaldırılır
  - Özel mesajlardan çıkılamaz

#### 4.5. Konuşma Silme
**Endpoint:** `DELETE /api/message/conversation/{id}`
- **Amaç:** Konuşmayı arşivleme/silme
- **UI Kullanımı:** 
  - Konuşma ayarları > "Sil" butonu
- **Özellikler:**
  - Soft delete
  - Sadece özel mesajlar silinebilir

#### 4.6. Okundu İşaretleme
**Endpoint:** `POST /api/message/conversation/{id}/mark-read`
- **Amaç:** Konuşmadaki tüm mesajları okundu işaretleme
- **UI Kullanımı:** 
  - Otomatik (konuşma açıldığında)
  - Manuel "Tümünü Okundu İşaretle" butonu
- **Özellikler:**
  - ConversationMember.LastReadAt güncellenir

---

## 📋 5. NOTIFICATION CONTROLLER - Bildirimler

### Mevcut Özellikler ✅
- Bildirimleri listeleme
- Bildirimi okundu işaretleme

### Eksik Özellikler ❌

#### 5.1. Tüm Bildirimleri Okundu İşaretleme
**Endpoint:** `POST /api/notification/mark-all-read`
- **Amaç:** Tüm bildirimleri okundu işaretleme
- **UI Kullanımı:** 
  - Bildirimler sayfasında "Tümünü Okundu İşaretle" butonu
- **Özellikler:**
  - Toplu güncelleme
  - Cache invalidation

#### 5.2. Bildirim Silme
**Endpoint:** `DELETE /api/notification/{id}`
- **Amaç:** Bildirimi silme
- **UI Kullanımı:** 
  - Bildirim kartında "Sil" butonu
  - Swipe to delete (mobil)
- **Özellikler:**
  - Soft delete veya hard delete

#### 5.3. Bildirim Ayarları
**Endpoint:** 
- `GET /api/notification/settings`
- `PUT /api/notification/settings`
- **Amaç:** Bildirim tercihlerini yönetme
- **UI Kullanımı:** 
  - Ayarlar > Bildirimler sekmesi
  - Toggle switch'ler
- **Özellikler:**
  - Email bildirimleri aç/kapa
  - Push bildirimleri aç/kapa
  - Bildirim türlerine göre ayarlar

#### 5.4. Bildirim Filtreleme
**Endpoint:** `GET /api/notification/my?type={type}&isRead={isRead}&page={page}`
- **Amaç:** Bildirimleri filtreleme
- **UI Kullanımı:** 
  - Bildirimler sayfasında filtreler
  - "Okunmamışlar", "Mesajlar", "Sınavlar" sekmeleri
- **Özellikler:**
  - Type'a göre filtreleme
  - Okunma durumuna göre filtreleme
  - Tarih aralığı

---

## 📋 6. ACCOUNT CONTROLLER - Hesap Bağlama

### Mevcut Özellikler ✅
- Hesap bağlama talebi
- Hesap bağlama onayı
- Hesap bağlama reddi

### Eksik Özellikler ❌

#### 6.1. Bağlantı Taleplerini Listeleme
**Endpoint:** `GET /api/account/link-requests?status={status}`
- **Amaç:** Bekleyen/onaylanmış/reddedilmiş bağlantı taleplerini listeleme
- **UI Kullanımı:** 
  - Manager dashboard'unda "Hesap Bağlama Talepleri" sekmesi
  - Liste görünümü
- **Özellikler:**
  - Status'e göre filtreleme
  - Pagination

#### 6.2. Bağlantıyı Kaldırma
**Endpoint:** `DELETE /api/account/link/{id}`
- **Amaç:** Onaylanmış bağlantıyı kaldırma
- **UI Kullanımı:** 
  - Hesap ayarlarında "Bağlı Hesaplar" sekmesi
  - "Bağlantıyı Kaldır" butonu
- **Özellikler:**
  - Sadece Manager veya MainUser kaldırabilir
  - Onay dialog'u

#### 6.3. Bağlı Hesapları Listeleme
**Endpoint:** `GET /api/account/links`
- **Amaç:** Kullanıcının bağlı hesaplarını listeleme
- **UI Kullanımı:** 
  - Profil sayfasında "Bağlı Hesaplar" sekmesi
  - Liste görünümü
- **Özellikler:**
  - Tüm bağlı kurumlar
  - Rol bilgisi
  - Bağlantı tarihi

---

## 📋 7. ADMIN CONTROLLER - Yönetim

### Mevcut Özellikler ✅
- Kurum onaylama
- Bekleyen kurumları listeleme

### Eksik Özellikler ❌

#### 7.1. Kurum Reddetme
**Endpoint:** `POST /api/admin/reject-institution/{id}`
- **Amaç:** Kurum başvurusunu reddetme
- **UI Kullanımı:** 
  - Admin panelinde kurum detayında "Reddet" butonu
  - Red sebebi girme
- **Özellikler:**
  - Red sebebi kaydedilir
  - Manager'a bildirim gönderilir

#### 7.2. Kullanıcı Yönetimi
**Endpoint:** 
- `GET /api/admin/users?page={page}&search={search}&status={status}`
- `PUT /api/admin/users/{id}/status` (Suspend/Activate)
- `DELETE /api/admin/users/{id}`
- **Amaç:** Kullanıcıları yönetme
- **UI Kullanımı:** 
  - Admin panelinde "Kullanıcılar" sayfası
  - Kullanıcı listesi ve filtreler
- **Özellikler:**
  - Arama ve filtreleme
  - Kullanıcı durumu değiştirme
  - Kullanıcı silme

#### 7.3. Sistem İstatistikleri
**Endpoint:** `GET /api/admin/statistics`
- **Amaç:** Sistem genel istatistikleri
- **UI Kullanımı:** 
  - Admin dashboard'u
  - İstatistik kartları
- **Özellikler:**
  - Toplam kullanıcı sayısı
  - Toplam kurum sayısı
  - Toplam sınav sayısı
  - Aktif kullanıcı sayısı
  - Son 30 gün aktivite grafiği

#### 7.4. Audit Log Görüntüleme
**Endpoint:** `GET /api/admin/audit-logs?userId={userId}&action={action}&page={page}`
- **Amaç:** Sistem loglarını görüntüleme
- **UI Kullanımı:** 
  - Admin panelinde "Loglar" sayfası
  - Filtreleme ve arama
- **Özellikler:**
  - UserId, Action, Date range filtreleme
  - Detaylı log görüntüleme

---

## 📋 8. REPORT CONTROLLER - Karne ve Raporlar

### Mevcut Özellikler ✅
- Öğrenci karne detayı

### Eksik Özellikler ❌

#### 8.1. Öğrenci Tüm Karnelerini Listeleme
**Endpoint:** `GET /api/report/student/{studentId}/all?page={page}&examType={type}`
- **Amaç:** Öğrencinin tüm karnelerini listeleme
- **UI Kullanımı:** 
  - Öğrenci profilinde "Karnelerim" sekmesi
  - Liste görünümü
- **Özellikler:**
  - Tüm sınav sonuçları
  - Filtreleme (ExamType, Date)
  - Sıralama (Date desc)

#### 8.2. Sınıf Karnelerini Görüntüleme (Öğretmen)
**Endpoint:** `GET /api/report/classroom/{classroomId}/exams/{examId}`
- **Amaç:** Öğretmenin sınıfın tüm karnelerini görüntülemesi
- **UI Kullanımı:** 
  - Sınıf detayında "Sınav Sonuçları" sekmesi
  - Tablo görünümü
- **Özellikler:**
  - Tüm öğrenci sonuçları
  - Sıralama ve filtreleme
  - Export (Excel/PDF)

#### 8.3. Karne Filtreleme ve Arama
**Endpoint:** `GET /api/report/student/{studentId}/all?lesson={lesson}&dateFrom={dateFrom}&dateTo={dateTo}`
- **Amaç:** Karneleri filtreleme
- **UI Kullanımı:** 
  - Karne listesi sayfasında filtreler
- **Özellikler:**
  - Derse göre filtreleme
  - Tarih aralığı
  - Sınav tipine göre filtreleme

#### 8.4. İlerleme Grafiği
**Endpoint:** `GET /api/report/student/{studentId}/progress?lesson={lesson}`
- **Amaç:** Öğrencinin ders bazında ilerleme grafiği
- **UI Kullanımı:** 
  - Profil sayfasında "İlerleme" sekmesi
  - Line chart görünümü
- **Özellikler:**
  - Zaman içinde net değişimi
  - Ders bazında ayrı grafikler

---

## 📋 9. YENİ CONTROLLER'LAR

### 9.1. INSTITUTION CONTROLLER
**Endpoint'ler:**
- `GET /api/institution/my` - Kullanıcının kurumlarını listeleme
- `GET /api/institution/{id}` - Kurum detayı
- `PUT /api/institution/{id}` - Kurum bilgilerini güncelleme (Manager)
- `GET /api/institution/{id}/members` - Kurum üyelerini listeleme
- `POST /api/institution/{id}/add-member` - Kuruma üye ekleme (Manager)
- `DELETE /api/institution/{id}/member/{memberId}` - Üye çıkarma (Manager)

### 9.2. SEARCH CONTROLLER
**Endpoint'ler:**
- `GET /api/search/users?query={query}` - Kullanıcı arama
- `GET /api/search/institutions?query={query}` - Kurum arama
- `GET /api/search/classrooms?query={query}` - Sınıf arama

---

## 📊 Öncelik Sıralaması

### 🔴 Yüksek Öncelik (Hemen Eklenmeli)
1. User: Başka kullanıcı profillerini görüntüleme
2. Exam: Sınav listesi ve detayı
3. Message: Konuşma listesi
4. Notification: Tümünü okundu işaretleme
5. Report: Öğrenci tüm karnelerini listeleme

### 🟡 Orta Öncelik (Yakında Eklenmeli)
6. Classroom: Öğrenci çıkarma, sınıf güncelleme
7. Exam: Sınav sonuçlarını görüntüleme (öğretmen)
8. Message: Mesaj silme, konuşmadan çıkma
9. Account: Bağlantı taleplerini listeleme
10. Admin: Kurum reddetme, kullanıcı yönetimi

### 🟢 Düşük Öncelik (Gelecek Fazlarda)
11. User: İstatistikler, aktivite geçmişi
12. Report: İlerleme grafiği
13. Admin: Sistem istatistikleri
14. Notification: Bildirim ayarları

