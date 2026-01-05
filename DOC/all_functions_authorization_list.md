# Tüm Fonksiyonlar İçin Yetki Kontrolü Listesi

## 📋 ÖRNEK KULLANIM (CreateClassroomAsync)

```csharp
public async Task<BaseResponse<int>> CreateClassroomAsync(int institutionId, string name, int grade)
{
    // 1. YETKİ KONTROLÜ (EN BAŞTA - ZORUNLU!)
    if (!await _authorizationService.CanCreateClassroomAsync(institutionId))
    {
        return BaseResponse<int>.ErrorResponse("Sınıf oluşturma yetkiniz yok", ErrorCodes.AccessDenied);
    }

    // 2. Validation
    if (string.IsNullOrWhiteSpace(name))
        return BaseResponse<int>.ErrorResponse("Sınıf adı gereklidir", ErrorCodes.ValidationFailed);

    // 3. İş mantığı...
}
```

---

## 🔐 TÜM FONKSİYONLAR İÇİN YETKİ KONTROLLERİ

### 1. CLASSROOM OPERATIONS

| Fonksiyon | Yetki Kontrolü Metodu | Açıklama |
|-----------|----------------------|----------|
| `CreateClassroomAsync` | `CanCreateClassroomAsync(institutionId)` | ✅ ÖRNEK YAPILDI |
| `AddStudentToClassroomAsync` | `CanAddStudentToClassroomAsync(classroomId)` | Sınıfa öğrenci ekleme |
| `AddStudentsToClassroomAsync` | `CanAddStudentToClassroomAsync(classroomId)` | Toplu öğrenci ekleme |
| `GetClassroomDetailsAsync` | `CanManageClassroomAsync(classroomId)` veya `IsClassroomStudentAsync(classroomId)` | Sınıf detayları görüntüleme |
| `GetClassroomsAsync` | `CanManageInstitutionAsync(institutionId)` veya `IsTeacherAsync(institutionId)` | Sınıf listesi görüntüleme |
| `UpdateClassroomAsync` | `CanManageClassroomAsync(classroomId)` | Sınıf güncelleme |
| `DeleteClassroomAsync` | `CanManageClassroomAsync(classroomId)` | Sınıf silme |
| `RemoveStudentAsync` | `CanAddStudentToClassroomAsync(classroomId)` | Öğrenci çıkarma |
| `GetStudentsAsync` | `CanManageClassroomAsync(classroomId)` veya `IsClassroomStudentAsync(classroomId)` | Öğrenci listesi |

### 2. EXAM OPERATIONS

| Fonksiyon | Yetki Kontrolü Metodu | Açıklama |
|-----------|----------------------|----------|
| `CreateExamAsync` | `CanCreateExamAsync(dto.InstitutionId)` | Sınav oluşturma |
| `ProcessOpticalResultsAsync` | `CanProcessOpticalResultsAsync(examId)` | Optik sonuç işleme |
| `ConfirmResultsAndNotifyAsync` | `CanConfirmExamResultsAsync(examId)` | Sonuç onaylama |
| `GetExamAsync` | `CanViewExamAsync(examId)` | Sınav görüntüleme |
| `GetExamsAsync` | `CanViewExamAsync` (her sınav için) veya kurum yetkisi | Sınav listesi |
| `GetExamDetailAsync` | `CanViewExamAsync(examId)` | Sınav detayı |
| `DeleteExamAsync` | `CanManageExamAsync(examId)` | Sınav silme |
| `GetStudentReportAsync` | `CanViewOtherStudentResultAsync` veya kendi sonucu | Öğrenci raporu |
| `GetClassroomReportAsync` | `CanViewClassroomReportAsync(classroomId)` | Sınıf raporu |
| `GetInstitutionReportAsync` | `CanViewInstitutionReportAsync(institutionId)` | Kurum raporu |
| `GetStudentReportsAsync` | `CanViewOtherStudentResultAsync` veya kendi sonuçları | Öğrenci raporları listesi |

### 3. INSTITUTION OPERATIONS

| Fonksiyon | Yetki Kontrolü Metodu | Açıklama |
|-----------|----------------------|----------|
| `AddUserToInstitutionAsync` | `CanAddUserToInstitutionAsync(institutionId)` | Kuruma kullanıcı ekleme |
| `RemoveUserFromInstitutionAsync` | `CanAddUserToInstitutionAsync(institutionId)` | Kurumdan kullanıcı çıkarma |
| `GetInstitutionDetailAsync` | `CanManageInstitutionAsync(institutionId)` veya kurum üyesi | Kurum detayı |
| `UpdateInstitutionAsync` | `CanManageInstitutionAsync(institutionId)` | Kurum güncelleme |
| `GetInstitutionUsersAsync` | `CanManageInstitutionAsync(institutionId)` veya kurum üyesi | Kurum kullanıcıları |
| `UpdateUserRoleAsync` | `CanManageInstitutionAsync(institutionId)` | Kullanıcı rolü güncelleme |
| `GetInstitutionStatisticsAsync` | `CanManageInstitutionAsync(institutionId)` | Kurum istatistikleri |

### 4. SOCIAL OPERATIONS

| Fonksiyon | Yetki Kontrolü Metodu | Açıklama |
|-----------|----------------------|----------|
| `CreateContentAsync` | `CanCreateContent()` (herkes yapabilir, ama öğrenci kısıtlamaları kontrol edilebilir) | İçerik oluşturma |
| `GetContentByIdAsync` | - (Herkes görebilir, gizlilik kontrolü içerik sahibine göre) | İçerik görüntüleme |
| `UpdateContentAsync` | `CanManageContentAsync(contentId)` | İçerik güncelleme |
| `DeleteContentAsync` | `CanManageContentAsync(contentId)` | İçerik silme |
| `LikeContentAsync` | - (Herkes yapabilir) | İçerik beğenme |
| `UnlikeContentAsync` | - (Herkes yapabilir) | İçerik beğenmeme |
| `CreateCommentAsync` | - (Herkes yapabilir) | Yorum oluşturma |
| `GetContentCommentsAsync` | - (Herkes görebilir) | Yorum listesi |
| `UpdateCommentAsync` | Kendi yorumu mu kontrolü | Yorum güncelleme |
| `DeleteCommentAsync` | Kendi yorumu mu kontrolü | Yorum silme |
| `FollowUserAsync` | - (Herkes yapabilir) | Kullanıcı takip etme |
| `UnfollowUserAsync` | - (Herkes yapabilir) | Takibi bırakma |
| `SaveContentAsync` | - (Herkes yapabilir) | İçerik kaydetme |
| `UnsaveContentAsync` | - (Herkes yapabilir) | Kaydı kaldırma |
| `ShareContentAsync` | - (Herkes yapabilir) | İçerik paylaşma |
| `BlockUserAsync` | - (Herkes yapabilir) | Kullanıcı engelleme |
| `UnblockUserAsync` | - (Herkes yapabilir) | Engeli kaldırma |
| `MuteUserAsync` | - (Herkes yapabilir) | Kullanıcı sessize alma |
| `UnmuteUserAsync` | - (Herkes yapabilir) | Sessizi kaldırma |
| `CreateStoryAsync` | - (Herkes yapabilir) | Story oluşturma |
| `GetStoriesAsync` | - (Herkes görebilir) | Story listesi |
| `GetStoryByIdAsync` | - (Herkes görebilir) | Story görüntüleme |
| `DeleteStoryAsync` | Kendi story'si mi kontrolü | Story silme |
| `ReactToStoryAsync` | - (Herkes yapabilir) | Story'ye tepki verme |
| `GetFeedAsync` | - (Herkes görebilir) | Feed görüntüleme |
| `GetFollowingFeedAsync` | - (Herkes görebilir) | Takip edilenler feed'i |
| `GetForYouFeedAsync` | - (Herkes görebilir) | Senin için feed'i |
| `GetTrendingContentsAsync` | - (Herkes görebilir) | Trend içerikler |
| `GetPopularContentsAsync` | - (Herkes görebilir) | Popüler içerikler |
| `GetRecommendedContentsAsync` | - (Herkes görebilir) | Önerilen içerikler |
| `GetUserContentsAsync` | `CanViewProfileAsync(targetUserId)` | Kullanıcı içerikleri |
| `GetSavedContentsAsync` | - (Kendi kayıtlıları) | Kayıtlı içerikler |
| `GetFollowersAsync` | `CanViewProfileAsync(targetUserId)` | Takipçiler |
| `GetFollowingAsync` | `CanViewProfileAsync(targetUserId)` | Takip edilenler |
| `GetUserProfileSocialAsync` | `CanViewProfileAsync(targetUserId)` | Sosyal profil |
| `GetCommentRepliesAsync` | - (Herkes görebilir) | Yorum yanıtları |
| `GetTrendingHashtagsAsync` | - (Herkes görebilir) | Trend hashtag'ler |
| `GetHashtagDetailAsync` | - (Herkes görebilir) | Hashtag detayı |
| `GetContentsByTagAsync` | - (Herkes görebilir) | Tag'e göre içerikler |
| `SearchHashtagsAsync` | - (Herkes görebilir) | Hashtag arama |
| `SearchContentsAsync` | - (Herkes görebilir) | İçerik arama |
| `GetContentAnalyticsAsync` | `CanManageContentAsync(contentId)` | İçerik analitiği |
| `ReportContentAsync` | - (Herkes yapabilir) | İçerik şikayet etme |
| `GetContentReportsAsync` | `IsAdmin()` | Şikayet listesi (sadece admin) |
| `ReviewContentReportAsync` | `IsAdmin()` | Şikayet inceleme (sadece admin) |
| `GetShareLinkAsync` | - (Herkes yapabilir) | Paylaşım linki |
| `GetSharedContentAsync` | - (Herkes görebilir) | Paylaşılan içerik |
| `GetMutedUsersAsync` | - (Kendi sessize aldıkları) | Sessize alınanlar |
| `GetUserStoriesAsync` | `CanViewProfileAsync(userId)` | Kullanıcı story'leri |
| `CreatePollAsync` | `CanManageContentAsync(request.ContentId)` | Anket oluşturma |
| `VotePollAsync` | - (Herkes yapabilir) | Ankete oy verme |
| `GetPollAsync` | - (Herkes görebilir) | Anket görüntüleme |
| `GetPollResultsAsync` | `CanManageContentAsync` (içerik sahibi) | Anket sonuçları |
| `SaveDraftAsync` | - (Herkes yapabilir, kendi taslakları) | Taslak kaydetme |
| `GetDraftsAsync` | - (Kendi taslakları) | Taslak listesi |
| `GetDraftAsync` | - (Kendi taslağı) | Taslak görüntüleme |
| `PublishDraftAsync` | - (Kendi taslağı) | Taslağı yayınlama |
| `DeleteDraftAsync` | - (Kendi taslağı) | Taslak silme |
| `PinContentAsync` | `CanManageContentAsync(contentId)` | İçerik sabitleme |
| `UnpinContentAsync` | `CanManageContentAsync(contentId)` | Sabitlemeyi kaldırma |
| `GetPinnedContentsAsync` | `CanViewProfileAsync(userId)` | Sabitlenmiş içerikler |

### 5. MESSAGE OPERATIONS

| Fonksiyon | Yetki Kontrolü Metodu | Açıklama |
|-----------|----------------------|----------|
| `StartConversationAsync` | - (Herkes yapabilir) | Konuşma başlatma |
| `SendMessageAsync` | Konuşma üyesi mi kontrolü | Mesaj gönderme |
| `GetMessagesAsync` | Konuşma üyesi mi kontrolü | Mesaj listesi |
| `SendToClassAsync` | `CanSendBulkMessageAsync(institutionId, classroomId)` | Sınıfa toplu gönderme |
| `GetConversationsAsync` | - (Kendi konuşmaları) | Konuşma listesi |
| `GetConversationAsync` | Konuşma üyesi mi kontrolü | Konuşma detayı |
| `UpdateConversationAsync` | `CanUpdateConversationAsync(conversationId)` | Konuşma güncelleme |
| `DeleteConversationAsync` | Konuşma üyesi mi kontrolü | Konuşma silme |
| `LeaveConversationAsync` | Konuşma üyesi mi kontrolü | Konuşmadan ayrılma |
| `DeleteMessageAsync` | Kendi mesajı mı kontrolü | Mesaj silme |
| `UpdateMessageAsync` | Kendi mesajı mı kontrolü | Mesaj güncelleme |
| `MarkReadAsync` | Konuşma üyesi mi kontrolü | Okundu işaretleme |
| `SearchMessagesAsync` | Konuşma üyesi mi kontrolü | Mesaj arama |

### 6. USER OPERATIONS

| Fonksiyon | Yetki Kontrolü Metodu | Açıklama |
|-----------|----------------------|----------|
| `GetProfileAsync` | `CanViewProfileAsync(userId)` | Profil görüntüleme |
| `UpdateProfileAsync` | Kendi profili mi kontrolü | Profil güncelleme |
| `ChangePasswordAsync` | Kendi hesabı mı kontrolü | Şifre değiştirme |
| `UploadProfileImageAsync` | Kendi profili mi kontrolü | Profil resmi yükleme |
| `ForgotPasswordAsync` | - (Herkes yapabilir) | Şifre unutma |
| `ResetPasswordAsync` | Token kontrolü | Şifre sıfırlama |
| `LogoutAsync` | Kendi oturumu mu kontrolü | Çıkış yapma |
| `SendVerificationEmailAsync` | Kendi hesabı mı kontrolü | Doğrulama e-postası gönderme |
| `VerifyEmailAsync` | Token kontrolü | E-posta doğrulama |
| `GetUserProfileAsync` | `CanViewProfileAsync(targetUserId)` | Kullanıcı profili görüntüleme |
| `UpdateEmailAsync` | Kendi hesabı mı kontrolü | E-posta güncelleme |
| `DeleteAccountAsync` | Kendi hesabı mı kontrolü | Hesap silme |
| `GetStatisticsAsync` | Kendi istatistikleri mi kontrolü | İstatistikler |
| `GetActivityAsync` | Kendi aktiviteleri mi kontrolü | Aktiviteler |
| `SearchUsersAsync` | - (Herkes görebilir) | Kullanıcı arama |
| `GetPreferencesAsync` | Kendi tercihleri mi kontrolü | Tercihleri görüntüleme |
| `UpdatePreferencesAsync` | Kendi tercihleri mi kontrolü | Tercihleri güncelleme |
| `UpdateProfileLayoutAsync` | Kendi profili mi kontrolü | Profil düzeni güncelleme |
| `UpdateDashboardLayoutAsync` | Kendi dashboard'u mu kontrolü | Dashboard düzeni güncelleme |

### 7. ACCOUNT OPERATIONS

| Fonksiyon | Yetki Kontrolü Metodu | Açıklama |
|-----------|----------------------|----------|
| `RequestAccountLinkAsync` | - (Herkes yapabilir, kendi hesabı) | Hesap bağlama isteği |
| `ApproveAccountLinkAsync` | `CanApproveAccountLinkAsync(institutionId)` | Hesap bağlama onaylama |
| `RejectAccountLinkAsync` | `CanApproveAccountLinkAsync(institutionId)` | Hesap bağlama reddetme |
| `GetAccountLinksAsync` | Kendi bağlantıları mı kontrolü | Hesap bağlantıları listesi |
| `RemoveAccountLinkAsync` | Kendi bağlantısı mı kontrolü | Hesap bağlantısını kaldırma |

### 8. ADMIN OPERATIONS

| Fonksiyon | Yetki Kontrolü Metodu | Açıklama |
|-----------|----------------------|----------|
| `GetAllUsersAsync` | `IsAdmin()` | Tüm kullanıcıları listeleme |
| `GetUserAsync` | `IsAdmin()` | Kullanıcı detayı |
| `UpdateUserAsync` | `IsAdmin()` | Kullanıcı güncelleme |
| `UpdateUserStatusAsync` | `IsAdmin()` | Kullanıcı durumu güncelleme |
| `DeleteUserAsync` | `IsAdmin()` | Kullanıcı silme |
| `ResetUserPasswordAsync` | `IsAdmin()` | Kullanıcı şifresi sıfırlama |
| `GetAllInstitutionsAsync` | `IsAdmin()` | Tüm kurumları listeleme |
| `GetInstitutionAsync` | `IsAdmin()` | Kurum detayı |
| `RejectInstitutionAsync` | `CanApproveInstitution()` | Kurum reddetme |
| `UpdateInstitutionStatusAsync` | `IsAdmin()` | Kurum durumu güncelleme |
| `ExtendSubscriptionAsync` | `IsAdmin()` | Abonelik uzatma |
| `CreateAdminAsync` | `IsInGlobalRole(UserRole.AdminAdmin)` | Admin oluşturma |
| `GetAdminsAsync` | `IsAdmin()` | Admin listesi |
| `GetStatisticsAsync` | `IsAdmin()` | Admin istatistikleri |
| `GetAuditLogsAsync` | `IsAdmin()` | Audit log listesi |

### 9. AUTH OPERATIONS

| Fonksiyon | Yetki Kontrolü Metodu | Açıklama |
|-----------|----------------------|----------|
| `RegisterAsync` | - (Herkes yapabilir) | Kayıt olma |
| `LoginAsync` | - (Herkes yapabilir) | Giriş yapma |
| `RefreshTokenAsync` | Token kontrolü | Token yenileme |
| `ApplyInstitutionAsync` | - (Herkes yapabilir) | Kurum başvurusu |

---

## 📌 ÖZEL DURUMLAR

### Herkes Yapabilir (Yetki Kontrolü Gerektirmez)
- İçerik görüntüleme (gizlilik kontrolü içerik sahibine göre)
- İçerik beğenme/yorumlama
- Kullanıcı takip etme
- Story görüntüleme
- Feed görüntüleme
- Arama yapma
- Kayıt olma/Giriş yapma

### Kendi İşlemleri (Sadece Kendi Verileri)
- Kendi profilini güncelleme
- Kendi içeriğini silme/güncelleme
- Kendi mesajını silme/güncelleme
- Kendi taslaklarını yönetme
- Kendi kayıtlı içeriklerini görüntüleme

### Admin Sadece
- Tüm kullanıcıları görüntüleme
- Kullanıcı silme/güncelleme
- Kurum onaylama/reddetme
- Admin oluşturma (sadece AdminAdmin)
- Audit log görüntüleme

### Manager/Teacher Sadece
- Sınıf oluşturma/yönetme
- Sınav oluşturma/yönetme
- Öğrenci ekleme/çıkarma
- Rapor oluşturma
- Toplu mesaj gönderme

### Öğrenci Kısıtlamaları
- ❌ Sınav oluşturma
- ❌ Sınıf oluşturma
- ❌ Öğrenci ekleme/çıkarma
- ❌ Kurum yönetimi
- ❌ Toplu mesaj gönderme
- ❌ Rapor oluşturma (kendi raporları hariç)

---

## 🔄 Uygulama Sırası

1. **Önce:** Tüm Operations sınıflarına `AuthorizationService` ekle
2. **Sonra:** Her fonksiyonun başına uygun yetki kontrolü ekle
3. **Son olarak:** Eski manuel kontrolleri kaldır

