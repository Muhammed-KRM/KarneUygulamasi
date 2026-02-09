# Frontend Entegrasyon Rehberi: Karne Projesi

Bu belge, **Karne Projesi** backend API'lerinin frontend tarafında nasıl kullanılacağını açıklar. Rehber iki ana bölüme ayrılmıştır:
1.  **Doğrulanmış Senaryo Akışı:** Test edilmiş ve çalıştığı onaylanmış ana iş akışı (Kurum Oluşturma -> Personel Atama -> Sınav -> Karne).
2.  **Ekstra Frontend Endpointleri:** Profil yönetimi, güncelleme işlemleri ve mesajlaşma gibi ek özellikler.

---

## Bölüm 1: Doğrulanmış Senaryo Akışı (Adım Adım)

Bu akış, sıfırdan bir dershane sisteminin kurulup öğrenciye karnenin ulaştığı süreci kapsar.

### Giriş (Authentication)
Tüm isteklerde (Register/Login hariç) `Authorization: Bearer <token>` header'ı kullanılmalıdır.

#### 1. Kayıt Ol (Register)
Kullanıcı (Dershane Sahibi) sisteme kayıt olur.
*   **Endpoint:** `POST /api/auth/register`
*   **Payload:**
    ```json
    {
      "fullName": "Ahmet Yılmaz",
      "username": "ahmet@dershane.com",
      "email": "ahmet@dershane.com",
      "password": "SecurePassword123!",
      "phone": "5551112233",
      "registerAsOwner": true  // Kurum sahibi ise true
    }
    ```
*   **Frontend Notu:** Başarılı kayıttan sonra direkt token dönmez, kullanıcıyı Login sayfasına yönlendirin.

#### 2. Giriş Yap (Login)
*   **Endpoint:** `POST /api/auth/login`
*   **Payload:**
    ```json
    { "username": "ahmet@dershane.com", "password": "SecurePassword123!" }
    ```
*   **Response:**
    ```json
    {
      "data": {
        "token": "eyJhbGcV...",
        "user": { "id": 15, "fullName": "Ahmet Yılmaz" }
      }
    }
    ```
*   **Frontend Notu:** `token`'ı `localStorage`'a kaydedin.

---

### Kurum Yönetimi (Sahip & Admin)

#### 3. Kurum Başvurusu Yap (Sahip)
*   **Endpoint:** `POST /api/auth/apply-institution`
*   **Payload:**
    ```json
    {
      "name": "Yıldız Dershanesi",
      "licenseNumber": "LIC-2024-001",
      "address": "İstanbul, Kadıköy",
      "phone": "2163334455",
      "managerName": "Ahmet Yılmaz",
      "managerEmail": "ahmet@dershane.com"
    }
    ```

#### 4. Başvuruları Listele (Sistem Admini)
*   **Endpoint:** `GET /api/admin/institutions?status=PendingApproval`

#### 5. Kurumu Onayla (Sistem Admini)
*   **Endpoint:** `POST /api/admin/institution/{id}/approve`

#### 6. Kurumlarımı Listele (Sahip)
*   **Endpoint:** `GET /api/institution/my`
*   **Response:** Kurum listesi döner. Onaylanan kurumun `id`sini buradan alıp diğer işlemlerde kullanın.

#### 7. Kuruma Ortak (Co-Owner) Ekle (Sahip)
Başka bir kullanıcıyı kuruma ortak olarak ekler.
*   **Endpoint:** `POST /api/institution/{id}/add-owner`
*   **Payload:** ` { "userId": 16 }`

#### 8. Yönetici (Müdür) Oluştur (Sahip)
Kurumu yönetecek personeli oluşturur.
*   **Endpoint:** `POST /api/institution/{id}/managers`
*   **Payload:**
    ```json
    {
      "fullName": "Mehmet Müdür",
      "email": "mudur@dershane.com",
      "password": "Mudur123!",
      "phone": "5559998877"
    }
    ```

---

### Akademik Kurulum (Yönetici)

#### 9. Öğretmen Hesabı Oluştur
*   **Endpoint:** `POST /api/institution/{id}/create-teacher`
*   **Payload:**
    ```json
    {
      "fullName": "Ayşe Hoca",
      "email": "ayse@dershane.com",
      "password": "Teacher123!",
      "employeeNumber": "TCH-001"
    }
    ```

#### 10. Sınıf Oluştur
*   **Endpoint:** `POST /api/classroom/create`
*   **Payload:**
    ```json
    {
      "institutionId": 101, // Kurum ID
      "name": "12-A Sınıfı",
      "grade": 12
    }
    ```

#### 11. Öğrenci Hesabı Oluştur
*   **Endpoint:** `POST /api/institution/{id}/create-student`
*   **Payload:**
    ```json
    {
      "fullName": "Ali Öğrenci",
      "email": "ali@gmail.com",
      "password": "Student123!",
      "studentNumber": "STU101" // Optik formdaki numara ile AYNI olmalı (boşluksuz)
    }
    ```

#### 12. Sınıfa Toplu Öğrenci Ata
*   **Endpoint:** `POST /api/classroom/add-students-bulk`
*   **Payload:**
    ```json
    {
      "classroomId": 20,
      "studentIds": [1001, 1002, 1003] // Oluşturulan öğrencilerin ID'leri
    }
    ```

---

### Sınav & Karne Süreci (Öğretmen)

#### 13. Sınav Oluştur
*   **Endpoint:** `POST /api/exam/create`
*   **Payload:**
    ```json
    {
      "institutionId": 101,
      "classroomId": 20,
      "title": "TYT Deneme 1",
      "type": 0, // 0: Deneme
      "examDate": "2024-03-01T10:00:00",
      "answerKeyJson": "{\"Matematik\": \"ABCDE\", \"Türkçe\": \"AAAAA\"}",
      "lessonConfigJson": "{\"Matematik\": {\"StartIndex\": 0, \"QuestionCount\": 5}}"
    }
    ```

#### 14. Optik Form Yükle & İşle
Optik okuyucudan çıkan `.txt` dosyasını sisteme yükler.
*   **Endpoint:** `POST /api/exam/{id}/process-optical`
*   **Form-Data:** `file`: (binary txt dosyası)
*   **ÖNEMLİ:** Optik dosyada öğrenci numarası "STU101" ise veritabanındaki öğrenci numarası da birebir "STU101" olmalıdır. Boşluk/padding hatasına dikkat edin.

#### 15. Sonuçları Onayla
Optik okuma sonuçlarını kesinleştirir ve sıralamaları hesaplar.
*   **Endpoint:** `POST /api/exam/{id}/confirm` (POST, body yok)

#### 16. Sonuçları Görüntüle (Öğretmen)
*   **Endpoint:** `GET /api/exam/{id}/results`

#### 17. Karneleri Sınıfa Gönder (Mesaj)
Sonuçları öğrencilere ve velilere mesaj olarak atar.
*   **Endpoint:** `POST /api/message/send-to-class`
*   **Payload:**
    ```json
    {
      "classroomId": 20,
      "reportCardIds": [5001, 5002, 5003] // ExamResult ID'leri
    }
    ```

---

### Öğrenci Paneli

#### 18. Karnelerimi Gör
*   **Endpoint:** `GET /api/exam/my-results` (Eğer endpoint varsa, yoksa genel result endpoint'i)
    *   *Not: Test senaryosunda bu adım `GET /api/exam/{id}/results` (yetkiye göre) veya öğrenciye özel endpoint ile yapılır.*

#### 19. Mesajları/Duyuruları Gör
*   **Endpoint:** `GET /api/message/conversations`

---

## Bölüm 2: Ekstra Frontend Endpointleri

Senaryo dışında, frontend uygulamasında ihtiyaç duyacağınız diğer yararlı endpointler.

### Kullanıcı & Profil (UserController)
*   **Profilimi Gör:** `GET /api/user/profile`
*   **Profili Güncelle:** `PUT /api/user/profile`
    ```json
    { "fullName": "Yeni İsim", "phone": "555..." }
    ```
*   **Şifre Değiştir:** `POST /api/user/change-password`
*   **Profil Resmi Yükle:** `POST /api/user/upload-profile-image` (Form-Data)
*   **Hesap Sil:** `DELETE /api/user/account`

### Kurum Detayları (InstitutionController)
*   **Kurum İstatistikleri:** `GET /api/institution/{id}/statistics` (Öğrenci sayısı, öğretmen sayısı vb.)
*   **Kurum Güncelle:** `PUT /api/institution/{id}`
*   **Personel Listeleme:** `GET /api/institution/{id}/members?role=2` (Role: 1=Owner, 2=Manager, 3=Teacher, 4=Student)
*   **Personel Sil:** `DELETE /api/institution/{id}/member/{memberId}`

### Mesajlaşma (MessageController)
*   **Konuşma Başlat:** `POST /api/message/start`
    ```json
    { "title": "Soru Çözüm Grubu", "isGroup": true, "classroomId": 20 }
    ```
*   **Mesaj Gönder:** `POST /api/message/send` (Sadece metin veya sınav/sonuç eklentili)
*   **Konuşma Geçmişi:** `GET /api/message/history/{conversationId}`
*   **Mesaj Ara:** `GET /api/message/search?query=deneme`

---

## 3. Ek: Eksik ve Dikkat Edilmesi Gerekenler

Aşağıdaki endpointler senaryo testinde kullanılmamıştır ancak tam bir frontend deneyimi için gereklidir.

### Çıkış Yap (Logout)
Sadece frontend'den token silmek yetmez, backend'i de haberdar etmelisiniz.

*   **Endpoint:** `POST /api/user/logout`
*   **Header:** `Authorization: Bearer <token>`
*   **Frontend İşlemi:**
    1.  Önce bu endpoint'e istek atın.
    2.  İstek başarılı olsa da olmasa da (hata alsa bile), `localStorage.removeItem('token')` işlemini yaparak client tarafındaki oturumu kapatın.
    3.  Kullanıcıyı `/login` sayfasına yönlendirin.

### Listeleme ve Arama (Search & Lists)
Frontend'de tabloları ve dropdown'ları doldurmak için bu endpointleri kullanın.

#### Kullanıcı Arama
Öğrenci, öğretmen veya yönetici aramak için.
*   **Endpoint:** `GET /api/search/users`
*   **Parametreler:** `query=ahmet`, `role=3` (3: Öğrenci, 2: Öğretmen...), `institutionId=1`

#### Sınıf Listesi
*   **Hızlı Arama:** `GET /api/search/classrooms?query=12-A`
*   **Kurumun Sınıfları:** `GET /api/classroom/institution/{id}`
*   **Sınıftaki Öğrenciler:** `GET /api/classroom/{id}/students`

#### Sınav Listesi
*   **Arama:** `GET /api/search/exams`
*   **Filtreler:** `institutionId`, `classroomId`, `dateFrom`, `dateTo`
*   **Kullanımı:** Öğretmenin "Sınavlarım" sayfası veya öğrencinin "Gireceğim Sınavlar" listesi için.

### Raporlar ve Karneler (Reports)

#### Öğrenci Karnesi
Belirli bir sınavın detaylı sonucu.
*   **Endpoint:** `GET /api/report/student/{resultId}`
*   **Dönen Veri:** Doğru/Yanlış sayıları, puan, ders bazlı netler, sınıf ortalaması.

#### Öğrenci Portfolyosu (Tüm Geçmiş)
Öğrencinin tüm sınav sonuçlarını grafik olarak göstermek için.
*   **Endpoint:** `GET /api/report/student/{studentId}/all`

### Bildirimler (Notifications)

Sağ üst köşedeki "Çan" ikonu için.

*   **Listele:** `GET /api/notification/my?isRead=false` (Okunmamışları getir)
*   **Hepsini Okundu Say:** `POST /api/notification/mark-all-read`

### Notlar
1.  **Ders ve Konu Listesi:** Sınav oluştururken ("create exam") seçilecek Dersler ve Konular için backend'de dinamik bir liste endpoint'i şu an yoktur. Frontend'de manuel tanımlanmalıdır.
2.  **Hata Yönetimi:** API `success: false` dönerse `message` ve `errorCode` alanlarını kullanıcıya gösterin.
