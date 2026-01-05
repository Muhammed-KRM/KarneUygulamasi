# Yetki Matrisi (Permission Matrix)

## Kullanıcı Tipleri

1. **AdminAdmin** - Sistem kurucusu
2. **Admin** - Sistem yöneticisi
3. **Institution Manager** - Dershane yöneticisi
4. **Institution Teacher** - Dershane öğretmeni
5. **Institution Student** - Dershane öğrencisi
6. **Standalone Teacher** - Bağımsız öğretmen (dershaneye bağlı değil)
7. **Standalone Student** - Bağımsız öğrenci (dershaneye bağlı değil)

## İşlem Kategorileri ve Yetkiler

### 1. KURUM YÖNETİMİ (Institution Management)

| İşlem | AdminAdmin | Admin | Inst. Manager | Inst. Teacher | Inst. Student | Standalone Teacher | Standalone Student |
|-------|-----------|-------|---------------|--------------|---------------|-------------------|-------------------|
| Kurum oluşturma | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Kurum onaylama | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Kurum güncelleme | ✅ | ✅ | ✅ (Kendi) | ❌ | ❌ | ❌ | ❌ |
| Kurum silme | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Kurum görüntüleme | ✅ | ✅ | ✅ (Kendi) | ✅ (Kendi) | ✅ (Kendi) | ❌ | ❌ |
| Kurum istatistikleri | ✅ | ✅ | ✅ (Kendi) | ✅ (Kendi) | ❌ | ❌ | ❌ |

### 2. KULLANICI YÖNETİMİ (User Management)

| İşlem | AdminAdmin | Admin | Inst. Manager | Inst. Teacher | Inst. Student | Standalone Teacher | Standalone Student |
|-------|-----------|-------|---------------|--------------|---------------|-------------------|-------------------|
| Kullanıcı oluşturma | ✅ | ✅ | ✅ (Kurum içi) | ❌ | ❌ | ❌ | ❌ |
| Kullanıcı güncelleme | ✅ | ✅ | ✅ (Kurum içi) | ❌ | ❌ | ✅ (Kendi) | ✅ (Kendi) |
| Kullanıcı silme | ✅ | ✅ | ✅ (Kurum içi) | ❌ | ❌ | ❌ | ❌ |
| Kullanıcı listeleme | ✅ | ✅ | ✅ (Kurum içi) | ✅ (Kurum içi) | ✅ (Sınıf içi) | ❌ | ❌ |
| Profil görüntüleme | ✅ | ✅ | ✅ | ✅ | ✅ (Kurallara göre) | ✅ | ✅ |
| Admin tanımlama | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |

### 3. SINIF YÖNETİMİ (Classroom Management)

| İşlem | AdminAdmin | Admin | Inst. Manager | Inst. Teacher | Inst. Student | Standalone Teacher | Standalone Student |
|-------|-----------|-------|---------------|--------------|---------------|-------------------|-------------------|
| Sınıf oluşturma | ✅ | ✅ | ✅ (Kendi kurumu) | ❌ | ❌ | ✅ (Kendi) | ❌ |
| Sınıf güncelleme | ✅ | ✅ | ✅ (Kendi kurumu) | ✅ (Kendi sınıfı) | ❌ | ✅ (Kendi) | ❌ |
| Sınıf silme | ✅ | ✅ | ✅ (Kendi kurumu) | ❌ | ❌ | ✅ (Kendi) | ❌ |
| Sınıf görüntüleme | ✅ | ✅ | ✅ (Kendi kurumu) | ✅ (Kendi sınıfı) | ✅ (Kendi sınıfı) | ✅ (Kendi) | ❌ |
| Öğrenci ekleme | ✅ | ✅ | ✅ (Kendi kurumu) | ✅ (Kendi sınıfı) | ❌ | ✅ (Kendi) | ❌ |
| Öğrenci çıkarma | ✅ | ✅ | ✅ (Kendi kurumu) | ✅ (Kendi sınıfı) | ❌ | ✅ (Kendi) | ❌ |
| Sınıf karnesi görüntüleme | ✅ | ✅ | ✅ (Kendi kurumu) | ✅ (Kendi sınıfı) | ❌ | ✅ (Kendi) | ❌ |

### 4. SINAV YÖNETİMİ (Exam Management)

| İşlem | AdminAdmin | Admin | Inst. Manager | Inst. Teacher | Inst. Student | Standalone Teacher | Standalone Student |
|-------|-----------|-------|---------------|--------------|---------------|-------------------|-------------------|
| Sınav oluşturma | ✅ | ✅ | ✅ (Kendi kurumu) | ✅ (Kendi kurumu) | ❌ | ✅ | ❌ |
| Sınav güncelleme | ✅ | ✅ | ✅ (Kendi kurumu) | ✅ (Kendi sınavı) | ❌ | ✅ (Kendi) | ❌ |
| Sınav silme | ✅ | ✅ | ✅ (Kendi kurumu) | ✅ (Kendi sınavı) | ❌ | ✅ (Kendi) | ❌ |
| Sınav görüntüleme | ✅ | ✅ | ✅ (Kendi kurumu) | ✅ (Kendi kurumu) | ✅ (Kendi sınıfı) | ✅ | ✅ |
| Optik sonuç işleme | ✅ | ✅ | ✅ (Kendi kurumu) | ✅ (Kendi kurumu) | ❌ | ✅ (Kendi) | ❌ |
| Sınav sonuçlarını onaylama | ✅ | ✅ | ✅ (Kendi kurumu) | ✅ (Kendi kurumu) | ❌ | ✅ (Kendi) | ❌ |

### 5. SINAV SONUÇLARI (Exam Results)

| İşlem | AdminAdmin | Admin | Inst. Manager | Inst. Teacher | Inst. Student | Standalone Teacher | Standalone Student |
|-------|-----------|-------|---------------|--------------|---------------|-------------------|-------------------|
| Kendi sonucunu görüntüleme | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Başka öğrencinin sonucunu görüntüleme | ✅ | ✅ | ✅ (Kendi kurumu) | ✅ (Kendi sınıfı) | ❌ | ❌ | ❌ |
| Sınıf sonuçlarını görüntüleme | ✅ | ✅ | ✅ (Kendi kurumu) | ✅ (Kendi sınıfı) | ❌ | ✅ (Kendi) | ❌ |
| Kurum sonuçlarını görüntüleme | ✅ | ✅ | ✅ (Kendi kurumu) | ✅ (Kendi kurumu) | ❌ | ❌ | ❌ |
| Karne oluşturma | ✅ | ✅ | ✅ (Kendi kurumu) | ✅ (Kendi kurumu) | ❌ | ✅ (Kendi) | ❌ |
| Karne gönderme | ✅ | ✅ | ✅ (Kendi kurumu) | ✅ (Kendi kurumu) | ❌ | ✅ (Kendi) | ❌ |

### 6. SOSYAL MEDYA & İÇERİK (Social Media & Content)

| İşlem | AdminAdmin | Admin | Inst. Manager | Inst. Teacher | Inst. Student | Standalone Teacher | Standalone Student |
|-------|-----------|-------|---------------|--------------|---------------|-------------------|-------------------|
| İçerik oluşturma (Soru/Sınav) | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| İçerik güncelleme | ✅ | ✅ | ✅ | ✅ | ✅ (Kendi) | ✅ (Kendi) | ✅ (Kendi) |
| İçerik silme | ✅ | ✅ | ✅ | ✅ | ✅ (Kendi) | ✅ (Kendi) | ✅ (Kendi) |
| İçerik beğenme | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| İçerik yorumlama | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| İçerik kaydetme | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| İçerik paylaşma | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Kullanıcı takip etme | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Anket oluşturma | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Story oluşturma | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Taslak kaydetme | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| İçerik sabitleme | ✅ | ✅ | ✅ | ✅ | ✅ (Kendi) | ✅ (Kendi) | ✅ (Kendi) |

### 7. MESAJLAŞMA (Messaging)

| İşlem | AdminAdmin | Admin | Inst. Manager | Inst. Teacher | Inst. Student | Standalone Teacher | Standalone Student |
|-------|-----------|-------|---------------|--------------|---------------|-------------------|-------------------|
| Mesaj gönderme | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Mesaj görüntüleme | ✅ | ✅ | ✅ | ✅ | ✅ (Kendi) | ✅ (Kendi) | ✅ (Kendi) |
| Mesaj silme | ✅ | ✅ | ✅ | ✅ | ✅ (Kendi) | ✅ (Kendi) | ✅ (Kendi) |
| Sınıf grubuna mesaj | ✅ | ✅ | ✅ (Kendi kurumu) | ✅ (Kendi sınıfı) | ✅ (Kendi sınıfı) | ✅ (Kendi) | ❌ |
| Toplu mesaj gönderme | ✅ | ✅ | ✅ (Kendi kurumu) | ✅ (Kendi sınıfı) | ❌ | ✅ (Kendi) | ❌ |
| Konuşma oluşturma | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Konuşma güncelleme | ✅ | ✅ | ✅ (Kendi kurumu) | ✅ (Kendi sınıfı) | ❌ | ✅ (Kendi) | ❌ |
| Konuşma silme | ✅ | ✅ | ✅ | ✅ | ✅ (Kendi) | ✅ (Kendi) | ✅ (Kendi) |

### 8. HESAP BAĞLAMA (Account Linking)

| İşlem | AdminAdmin | Admin | Inst. Manager | Inst. Teacher | Inst. Student | Standalone Teacher | Standalone Student |
|-------|-----------|-------|---------------|--------------|---------------|-------------------|-------------------|
| Ana hesap oluşturma | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Dershane hesabı bağlama | ✅ | ✅ | ❌ | ✅ (Kendi) | ✅ (Kendi) | ✅ (Kendi) | ✅ (Kendi) |
| Hesap bağlantısını kaldırma | ✅ | ✅ | ❌ | ✅ (Kendi) | ✅ (Kendi) | ✅ (Kendi) | ✅ (Kendi) |
| Bağlantı isteği onaylama | ✅ | ✅ | ✅ (Kendi kurumu) | ❌ | ❌ | ❌ | ❌ |
| Bağlantı isteği reddetme | ✅ | ✅ | ✅ (Kendi kurumu) | ❌ | ❌ | ❌ | ❌ |

### 9. ÖZEL DERS (Private Tutoring)

| İşlem | AdminAdmin | Admin | Inst. Manager | Inst. Teacher | Inst. Student | Standalone Teacher | Standalone Student |
|-------|-----------|-------|---------------|--------------|---------------|-------------------|-------------------|
| Özel ders bilgisi oluşturma | ✅ | ✅ | ❌ | ✅ | ❌ | ✅ | ❌ |
| Özel ders bilgisi güncelleme | ✅ | ✅ | ❌ | ✅ (Kendi) | ❌ | ✅ (Kendi) | ❌ |
| Özel ders bilgisi silme | ✅ | ✅ | ❌ | ✅ (Kendi) | ❌ | ✅ (Kendi) | ❌ |
| Öğretmen arama | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Özel ders bilgilerini görüntüleme | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |

### 10. DERS PROGRAMI & ZAMANLAYICI (Schedule & Timer)

| İşlem | AdminAdmin | Admin | Inst. Manager | Inst. Teacher | Inst. Student | Standalone Teacher | Standalone Student |
|-------|-----------|-------|---------------|--------------|---------------|-------------------|-------------------|
| Ders programı oluşturma | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Ders programı güncelleme | ✅ | ✅ | ✅ | ✅ | ✅ (Kendi) | ✅ (Kendi) | ✅ (Kendi) |
| Ders programı silme | ✅ | ✅ | ✅ | ✅ | ✅ (Kendi) | ✅ (Kendi) | ✅ (Kendi) |
| Zamanlayıcı oluşturma | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Zamanlayıcı güncelleme | ✅ | ✅ | ✅ | ✅ | ✅ (Kendi) | ✅ (Kendi) | ✅ (Kendi) |
| Zamanlayıcı silme | ✅ | ✅ | ✅ | ✅ | ✅ (Kendi) | ✅ (Kendi) | ✅ (Kendi) |

### 11. RAPORLAMA (Reporting)

| İşlem | AdminAdmin | Admin | Inst. Manager | Inst. Teacher | Inst. Student | Standalone Teacher | Standalone Student |
|-------|-----------|-------|---------------|--------------|---------------|-------------------|-------------------|
| Öğrenci raporu görüntüleme | ✅ | ✅ | ✅ (Kendi kurumu) | ✅ (Kendi sınıfı) | ✅ (Kendi) | ❌ | ✅ (Kendi) |
| Sınıf raporu görüntüleme | ✅ | ✅ | ✅ (Kendi kurumu) | ✅ (Kendi sınıfı) | ❌ | ✅ (Kendi) | ❌ |
| Kurum raporu görüntüleme | ✅ | ✅ | ✅ (Kendi kurumu) | ✅ (Kendi kurumu) | ❌ | ❌ | ❌ |
| Rapor oluşturma | ✅ | ✅ | ✅ (Kendi kurumu) | ✅ (Kendi kurumu) | ❌ | ✅ (Kendi) | ❌ |

## Özel Kurallar

### Öğrenci Kısıtlamaları
- ❌ Sınav oluşturma
- ❌ Sınıf oluşturma/yönetme
- ❌ Öğrenci ekleme/çıkarma
- ❌ Kurum yönetimi
- ❌ Kullanıcı silme
- ❌ Admin işlemleri
- ❌ Rapor oluşturma (kendi raporları hariç)
- ❌ Toplu mesaj gönderme
- ❌ Sınıf grubunu güncelleme

### Öğretmen Kısıtlamaları
- ❌ Kurum oluşturma
- ❌ Kurum onaylama
- ❌ Admin tanımlama
- ❌ Başka kurumların sınıflarını yönetme
- ❌ Başka kurumların öğrencilerini görüntüleme

### Manager Kısıtlamaları
- ❌ Kurum oluşturma (sadece başvuru)
- ❌ Kurum onaylama
- ❌ Admin tanımlama
- ❌ Başka kurumları yönetme

### Standalone Kullanıcılar
- ✅ Tüm sosyal medya özelliklerini kullanabilir
- ✅ Soru/sınav paylaşabilir
- ✅ Özel ders bilgileri paylaşabilir (öğretmenler)
- ❌ Kurum yönetimi yapamaz
- ❌ Dershane öğrencilerini göremez

