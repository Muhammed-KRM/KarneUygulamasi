# Backend Mantığı ve Veritabanı Etkileşimleri Rehberi

Bu belge, `frontend_integration_guide.md` dosyasındaki endpointlerin backend tarafında tam olarak ne yaptığını, hangi veritabanı tablolarına dokunduğunu ve cache mekanizmasını nasıl etkilediğini açıklar.

**Dosya Konumu:** `D:\KarneProject\DOC\backend_logic_guide.md`

---

## 1. Kimlik Doğrulama (Authentication)

### Kayıt Ol (Register)
`POST /api/auth/register`
*   **Logic:** Kullanıcı veritabanına kaydedilir. Şifre `Salt` ve `Hash` olarak saklanır.
*   **Tablolar:**
    *   `Users` (INSERT): `FullName`, `Username`, `Email`, `PasswordHash`, `PasswordSalt`, `GlobalRole`, `Status` (Active), `ProfileVisibility` (PublicToAll).
*   **Cache:** Etkilemez.

### Giriş Yap (Login)
`POST /api/auth/login`
*   **Logic:** Kullanıcı adı ve şifre doğrulanır. JWT Token ve Refresh Token üretilir.
*   **Tablolar:**
    *   `Users` (UPDATE): `LastLoginAt` güncellenir.
    *   `RefreshTokens` (INSERT): Kullanıcı ID, Token ve Bitiş Süresi kaydedilir.
*   **Cache:** Etkilemez.

### Çıkış Yap (Logout)
`POST /api/user/logout`
*   **Logic:** Gönderilen token geçersiz (blacklist) olarak işaretlenir.
*   **Tablolar:** Veritabanına yazmaz.
*   **Cache:**
    *   `Blacklist:Token:{token}` anahtarına yazılır (Süre: 7 gün).

---

## 2. Kurum Yönetimi (Institution Management)

### Kurum Başvurusu (Apply)
`POST /api/auth/apply-institution`
*   **Logic:** Kurum "PendingApproval" (Onay Bekliyor) statüsüyle oluşturulur. Başvuran kullanıcı "Primary Owner" (Birincil Sahip) yapılır.
*   **Tablolar:**
    *   `Institutions` (INSERT): `Name`, `LicenseNumber`, `Address`, `Phone`, `Status` (PendingApproval).
    *   `InstitutionOwners` (INSERT): `UserId`, `InstitutionId`, `IsPrimaryOwner` (True).
*   **Cache:** Etkilemez.

### Başvuruları Listele (List Applications)
`GET /api/admin/institutions`
*   **Logic:** Onay bekleyen kurumları listeler.
*   **Tablolar:** `Institutions` (SELECT).
*   **Cache:** Yok.

### Kurumu Onayla (Approve - Admin)
`POST /api/admin/institution/{id}/approve`
*   **Logic:** Kurum statüsü "Active" yapılır ve abonelik başlatılır.
*   **Tablolar:**
    *   `Institutions` (UPDATE): `Status` (Active), `ApprovedAt`, `ApprovedByAdminId`, `SubscriptionStartDate`, `SubscriptionEndDate`.
*   **Cache:**
    *   Sahiplerin (Owners) yetki cache'i (`SessionService`) temizlenir.

### Kurumlarım (My Institutions)
`GET /api/institution/my`
*   **Logic:** Kullanıcının sahibi veya üyesi olduğu kurumları listeler.
*   **Tablolar:** `InstitutionUsers`, `InstitutionOwners` (SELECT).
*   **Cache:** Yok.

### Kurum Detayı ve İstatistikleri
`GET /api/institution/{id}/statistics`
*   **Logic:** Kurum bilgileri, öğrenci/öğretmen sayıları ve sınav ortalamaları hesaplanır.
*   **Tablolar:**
    *   `Institutions` (SELECT)
    *   `InstitutionUsers` (COUNT)
    *   `Classrooms` (COUNT)
    *   `Exams` (COUNT)
    *   `ExamResults` (AVERAGE: TotalScore, TotalNet)
*   **Cache:**
    *   `institution_detail_{id}` (Okuma/Yazma, 5 dk)
    *   `institution_statistics_{id}` (Okuma/Yazma, 10 dk)

### Personel Ekleme (Create Manager/Teacher/Student)
`POST /api/institution/{id}/create-manager` (veya `create-teacher`, `create-student`)
*   **Logic:** Önce `Users` tablosuna kullanıcı eklenir (yoksa), sonra `InstitutionUsers` tablosuna kurum bağı eklenir. Kurum sahibi veya yöneticisi yapabilir.
*   **Tablolar:**
    *   `Users` (INSERT): `Email`, `PasswordHash` vb.
    *   `InstitutionUsers` (INSERT): `InstitutionId`, `UserId`, `Role` (Manager/Teacher/Student), `StudentNumber` (Varsa), `EmployeeNumber` (Varsa).
*   **Cache:**
    *   `institution_detail_{id}` silinir (Üye sayıları değiştiği için).
    *   `search_users_*` cache'leri etkilenmez (süre dolunca yenilenir).

---

## 3. Akademik Yapı (Classroom)

### Sınıf Oluştur (Create Classroom)
`POST /api/classroom/create`
*   **Logic:** Yeni sınıf oluşturulur ve otomatik olarak bir sohbet grubu (Conversation) açılır.
*   **Tablolar:**
    *   `Classrooms` (INSERT): `Name`, `Grade`, `InstitutionId`.
    *   `Conversations` (INSERT): `Title`, `IsGroup` (True), `ClassroomId`.
*   **Cache:**
    *   `Inst:{institutionId}:Classrooms` silinir (Liste değişti).

### Sınıfa Öğrenci Ata (Add Student Bulk)
`POST /api/classroom/add-students-bulk`
*   **Logic:** Öğrenciler sınıfa bağlanır.
*   **Tablolar:**
    *   `ClassroomStudents` (INSERT): `ClassroomId`, `InstitutionUserId` (Öğrencinin InstitutionUsers tablosundaki ID'si).
*   **Cache:**
    *   `Classroom:{id}:Details` silinir.
    *   `Inst:{institutionId}:Classrooms` silinir (Öğrenci sayıları değişti).

### Sınıf Detayı ve Listesi
`GET /api/classroom/{id}`
*   **Logic:** Sınıfın öğrencileri listelenir.
*   **Tablolar:**
    *   `Classrooms` (SELECT)
    *   `ClassroomStudents` (SELECT) -> `InstitutionUsers` -> `Users`
*   **Cache:**
    *   `Classroom:{id}:Details` (15 dk)
    *   `Inst:{institutionId}:Classrooms` (30 dk)
    *   `Classroom:{id}:Students:{page}...` (15 dk)

### Sınıf Arama
`GET /api/search/classrooms`
*   **Logic:** İsim veya kuruma göre sınıf arar.
*   **Tablolar:** `Classrooms` (SELECT).
*   **Cache:** `search_classrooms_*` (3 dk).

---

## 4. Sınav ve Sonuçlar (Exam & Results)

### Sınav Oluştur (Create Exam)
`POST /api/exam/create`
*   **Logic:** Sınavın genel bilgileri ve cevap anahtarı kaydedilir.
*   **Tablolar:**
    *   `Exams` (INSERT): `Title`, `Type`, `ExamDate`, `AnswerKeyJson`, `LessonConfigJson`.
*   **Cache:**
    *   `exam_list_*` (Tüm sınav listeleri) silinir.

### Optik Form İşle (Process Optical)
`POST /api/exam/{id}/process-optical`
*   **Logic:** TXT dosyası pars edilir, öğrenci numarasına göre `InstitutionUsers` tablosundan öğrenci bulunur, netler hesaplanır ve sonuçlar yazılır. Sıralama hesaplama job'ı (BackgroundJob) tetiklenir.
*   **Tablolar:**
    *   `ExamResults` (INSERT): `StudentId` (User ID), `TotalCorrect`, `TotalNet`, `DetailedResultsJson`.
*   **Cache:**
    *   `exam_results_{examId}_*` silinir.
    *   `exam_detail_{examId}` silinir (Sonuç sayısı değişti).

### Sonuçları Onayla (Confirm Results)
`POST /api/exam/{id}/confirm`
*   **Logic:**
    1.  Kurum sıralaması (`InstitutionRank`) ve Sınıf sıralaması (`ClassRank`) hesaplanıp güncellenir.
    2.  `IsConfirmed` = True yapılır.
    3.  Bildirim gönderme job'ı tetiklenir.
*   **Tablolar:**
    *   `ExamResults` (UPDATE): `InstitutionRank`, `ClassRank`, `IsConfirmed`, `ConfirmedAt`.
*   **Cache:**
    *   `exam_results_{examId}_*` silinir.
    *   `Student:{studentId}:*` (Öğrenci raporları) silinir.

### Sınav Sonuçlarını Getir (Get Exam Results)
`GET /api/exam/{id}/results`
*   **Logic:** Sınavın sonuç listesi döner.
*   **Tablolar:** `ExamResults` (SELECT) -> `Users` (Student).
*   **Cache:**
    *   `exam_results_{examId}_{filters}...` (2 dk).

### Sınav Arama
`GET /api/search/exams`
*   **Logic:** Sınav adı, tarih veya türe göre arama yapar.
*   **Tablolar:** `Exams` (SELECT).
*   **Cache:** `search_exams_*` (2 dk).

---

## 5. Raporlar (Reports)

### Öğrenci Karnesi (Student Report)
`GET /api/report/student/{resultId}`
*   **Logic:** Tek bir sınav sonucunun detayları JSON'dan okunarak döndürülür.
*   **Tablolar:** `ExamResults` (SELECT).
*   **Cache:** Yok (Anlık çekilir).

### Öğrenci Tüm Raporları (Portfolio)
`GET /api/report/student/{studentId}/all`
*   **Logic:** Öğrencinin onaylanmış tüm sınav sonuçları tarih sırasına göre getirilir.
*   **Tablolar:** `ExamResults` (SELECT) -> `Exams`
*   **Cache:**
    *   `Student:{studentId}:AllReports` (30 dk).

### Sınıf Raporu
`GET /api/report/classroom/{classroomId}`
*   **Logic:** Sınıftaki öğrencilerin belirli bir sınavdaki veya genel başarı durumu.
*   **Tablolar:** `ExamResults` -> `ClassroomStudents` ile filtreleme.
*   **Cache:** Yok.

---

## 6. Kullanıcı Profil & Ayarlar

### Profil Getir
`GET /api/user/profile`
*   **Logic:** Kullanıcının kendi profil bilgilerini getirir.
*   **Tablolar:** `Users` (SELECT).
*   **Cache:** `User:{id}:Profile` (Değişiklik olana kadar, veya kısa süreli).

### Profil Güncelle
`PUT /api/user/profile`
*   **Tablolar:** `Users` (UPDATE).
*   **Cache:** `User:{id}:Profile` silinir.

### Ayarlar (Preferences)
`GET /api/notification/settings` (veya `GET /api/user/preferences`)
*   **Logic:** Kullanıcının tema, bildirim vb. tercihleri.
*   **Tablolar:** `UserPreferences` (SELECT/INSERT/UPDATE).
*   **Cache:** `user_preferences_{id}` (30 dk).

### İstatistikler (Dashboard)
`GET /api/user/statistics`
*   **Logic:** Kullanıcının girdiği sınav sayısı, ortalama neti vb. hesaplanır.
*   **Tablolar:** `ExamResults` (COUNT, AVG), `Messages` (COUNT).
*   **Cache:** `user_statistics_{id}` (10 dk).

### Kullanıcı Arama
`GET /api/search/users`
*   **Logic:** İsim veya role göre kullanıcı arar.
*   **Tablolar:** `Users`, `InstitutionUsers` (SELECT).
*   **Cache:** `search_users_*` (2 dk).
