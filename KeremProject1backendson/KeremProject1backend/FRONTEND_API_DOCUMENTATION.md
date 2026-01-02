# 🎯 AnimeRanker Frontend API Dokümantasyonu

## 📋 İçindekiler
1. [Frontend Genel Yapısı ve Sayfalar](#frontend-genel-yapısı-ve-sayfalar)
2. [Genel Bilgiler](#genel-bilgiler)
3. [Kimlik Doğrulama (Authentication)](#1-kimlik-doğrulama-authentication)
4. [MyAnimeList Entegrasyonu](#2-myanimelist-entegrasyonu)
5. [Anime Listesi Yönetimi](#3-anime-listesi-yönetimi)
6. [Otomatik Liste Oluşturma](#4-otomatik-liste-oluşturma)
7. [Arama ve Keşif](#5-arama-ve-keşif)
8. [Paylaşım Özellikleri](#6-paylaşım-özellikleri)
9. [Sosyal Özellikler](#7-sosyal-özellikler)
10. [Yorumlar](#8-yorumlar)
11. [Kullanıcı Yönetimi](#9-kullanıcı-yönetimi)
12. [Dosya Yönetimi](#10-dosya-yönetimi)
13. [İstatistikler](#11-istatistikler)
14. [Aktivite Akışı](#12-aktivite-akışı)
15. [Öneriler ve Trending](#13-öneriler-ve-trending)
16. [Liste Kopyalama](#14-liste-kopyalama)
17. [Export ve Embed](#15-export-ve-embed)
18. [MAL Senkronizasyonu](#16-mal-senkronizasyonu)
19. [Drag & Drop](#17-drag--drop)

---

## Frontend Genel Yapısı ve Sayfalar

### 🎨 Genel Görünüm ve Tasarım

AnimeRanker, modern ve kullanıcı dostu bir anime listeleme platformudur. Frontend, aşağıdaki temel bileşenlerden oluşur:

#### Ana Bileşenler:
- **Header/Navbar**: Logo, navigasyon menüsü, arama çubuğu, bildirimler, profil dropdown
- **Sidebar** (Opsiyonel): Hızlı erişim menüsü, kategoriler, filtreler
- **Main Content Area**: Sayfa içeriği
- **Footer**: İletişim, sosyal medya linkleri, yasal bilgiler

#### Renk Paleti Önerisi:
- **Primary**: Anime temalı renkler (koyu mavi, mor, pembe tonları)
- **Secondary**: Beyaz, açık gri
- **Accent**: Vurgu renkleri (tier renkleri için)
- **Dark Mode**: Koyu tema desteği önerilir

#### Responsive Tasarım:
- **Desktop**: Tam özellikli görünüm
- **Tablet**: Uyarlanmış layout
- **Mobile**: Mobil-first yaklaşım, touch-friendly

---

### 📄 Sayfa Yapısı ve Özellikler

#### 1. **Ana Sayfa / Dashboard** (`/` veya `/dashboard`)

**Amaç:** Kullanıcının listelerini, önerileri ve trending içerikleri gösterir.

**Özellikler:**
- ✅ Kullanıcının tüm listelerini grid/liste görünümünde göster
- ✅ "Yeni Liste Oluştur" butonu (modal veya sayfa)
- ✅ Hızlı erişim butonları:
  - "Puana Göre Liste Oluştur"
  - "Yıla Göre Liste Oluştur"
  - "Kategoriye Göre Liste Oluştur"
- ✅ Önerilen animeler bölümü (carousel veya grid)
- ✅ Trending listeler bölümü
- ✅ Son aktiviteler (kısa özet)
- ✅ İstatistik kartları (toplam liste, beğeni, takipçi sayısı)

**Kullanılan Endpoint'ler:**
- `GET /api/list/all` - Kullanıcının listelerini getir
- `GET /api/recommendation/anime` - Önerileri getir
- `GET /api/recommendation/trending` - Trending listeleri getir
- `GET /api/statistics/me` - İstatistikleri getir

**UI Bileşenleri:**
- Liste kartları (thumbnail, başlık, mod, item sayısı, beğeni sayısı)
- Filtreleme ve sıralama seçenekleri
- Arama çubuğu (listeler arasında arama)

---

#### 2. **Giriş Sayfası** (`/login`)

**Amaç:** Kullanıcı girişi ve kayıt işlemleri.

**Özellikler:**
- ✅ Login formu (username, password)
- ✅ Register formu (username, password, confirm password)
- ✅ "Beni Hatırla" checkbox
- ✅ "Şifremi Unuttum" linki (opsiyonel)
- ✅ MyAnimeList bağlantı butonu (giriş yapıldıktan sonra)
- ✅ Hata mesajları gösterimi
- ✅ Loading state

**Kullanılan Endpoint'ler:**
- `POST /api/auth/login` - Giriş yap
- `POST /api/auth/register` - Kayıt ol

**UI Bileşenleri:**
- Form validasyonu
- Password strength indicator (register için)
- Success/error toast notifications

---

#### 3. **Liste Oluşturma Sayfası** (`/list/create`)

**Amaç:** Yeni anime listesi oluşturma.

**Özellikler:**
- ✅ Liste başlığı input
- ✅ Liste modu seçimi (Radio buttons veya dropdown):
  - Ranked (1, 2, 3 sıralama)
  - Tiered (TierMaker tarzı)
  - Fusion (TierMaker + Ranked)
- ✅ Özel tier oluşturma (opsiyonel):
  - Tier ismi
  - Renk seçici (color picker)
  - Sıra numarası
  - Tier ekle/sil butonları
- ✅ "Varsayılan Tier'ları Kullan" checkbox
- ✅ Önizleme bölümü
- ✅ "Oluştur" butonu

**Kullanılan Endpoint'ler:**
- `POST /api/list/create` - Liste oluştur

**UI Bileşenleri:**
- Drag & drop tier sıralama (opsiyonel)
- Color picker component
- Tier preview cards

---

#### 4. **Otomatik Liste Oluşturma Sayfası** (`/list/generate`)

**Amaç:** MAL verilerine göre otomatik liste oluşturma.

**Özellikler:**
- ✅ Üç ana seçenek:
  1. **Puana Göre Liste** (`/list/generate/score`)
     - "Oluştur" butonu
     - Açıklama: "Tüm izlediğiniz animeler puana göre 1,2,3 şeklinde sıralanır"
   
  2. **Yıla Göre Liste** (`/list/generate/year`)
     - "Oluştur" butonu
     - Açıklama: "Animeler çıkış yıllarına göre kategorilere ayrılır"
   
  3. **Kategoriye Göre Liste** (`/list/generate/genre`)
     - Kategori dropdown/autocomplete
     - Kategori listesi (Jikan API'den)
     - "Oluştur" butonu
     - Açıklama: "Seçtiğiniz kategorideki animeler puana göre tier'lara ayrılır"
- ✅ Loading state (liste oluşturulurken)
- ✅ Başarı mesajı ve yeni listeye yönlendirme

**Kullanılan Endpoint'ler:**
- `GET /api/generate/genres` - Kategorileri getir
- `POST /api/generate/by-score` - Puana göre liste oluştur
- `POST /api/generate/by-year` - Yıla göre liste oluştur
- `POST /api/generate/by-genre` - Kategoriye göre liste oluştur

**UI Bileşenleri:**
- Card-based layout (her seçenek için bir kart)
- Progress indicator
- Success animation

---

#### 5. **Liste Düzenleme Sayfası** (`/list/{listId}/edit`)

**Amaç:** Mevcut listeyi düzenleme, anime ekleme/çıkarma, tier yönetimi.

**Özellikler:**
- ✅ Liste başlığı düzenleme
- ✅ Liste modu gösterimi
- ✅ Tier'ları görüntüleme ve düzenleme:
  - Tier başlığı
  - Tier rengi
  - Tier sırası (drag & drop)
  - Tier içindeki animeler
- ✅ Anime ekleme:
  - Arama modalı (Jikan API'den anime ara)
  - Duplicate kontrolü
  - Tier seçimi
  - Sıra belirleme
- ✅ Anime silme (her anime için sil butonu)
- ✅ Drag & drop ile sıralama:
  - Tier'lar arası taşıma
  - Tier içinde sıralama
- ✅ Mod dönüştürme butonları:
  - "Fusion Moduna Çevir" (Tiered için)
  - "Ranked Moduna Çevir" (Tiered/Fusion için)
- ✅ "Kaydet" butonu
- ✅ "İptal" butonu
- ✅ Liste istatistikleri (sidebar'da)

**Kullanılan Endpoint'ler:**
- `GET /api/list/{listId}` - Listeyi getir
- `PUT /api/list/save` - Listeyi kaydet
- `PATCH /api/list/{listId}/title` - Başlık güncelle
- `POST /api/list/item/add` - Item ekle
- `DELETE /api/list/item/remove` - Item sil
- `POST /api/list/tier/add` - Tier ekle
- `PUT /api/list/tier/update` - Tier güncelle
- `DELETE /api/list/tier/remove` - Tier sil
- `POST /api/list/check-duplicate` - Duplicate kontrol
- `POST /api/list/convert-to-fusion` - Fusion'a çevir
- `POST /api/list/convert-to-ranked` - Ranked'e çevir
- `POST /api/dragdrop/move-item` - Item taşı
- `POST /api/dragdrop/reorder-items` - Item sırala
- `GET /api/list/{listId}/statistics` - İstatistikleri getir
- `GET /api/search/anime` - Anime ara

**UI Bileşenleri:**
- Drag & drop library (react-beautiful-dnd, dnd-kit, vb.)
- Tier cards (renkli arka plan)
- Anime cards (thumbnail, başlık, rank numarası)
- Modal dialogs (anime arama, tier ekleme)
- Color picker
- Confirmation dialogs (silme işlemleri için)

---

#### 6. **Liste Görüntüleme Sayfası** (`/list/{listId}`)

**Amaç:** Listeyi görüntüleme (sadece okuma modu).

**Özellikler:**
- ✅ Liste başlığı ve bilgileri
- ✅ Liste sahibi bilgisi (profil linki)
- ✅ Tier'ları görüntüleme:
  - Ranked modu: Tek sütun, numaralı liste
  - Tiered modu: Yatay tier'lar (TierMaker tarzı)
  - Fusion modu: TierMaker + tier içinde numaralı
- ✅ Anime kartları (thumbnail, başlık, rank)
- ✅ Beğeni butonu ve sayısı
- ✅ Yorumlar bölümü:
  - Yorum listesi
  - Yorum ekleme formu (giriş yapılmışsa)
- ✅ Paylaşım butonları:
  - "Paylaş" butonu (link oluştur)
  - "Embed" butonu
  - "Export" butonu (görsel olarak)
- ✅ Liste istatistikleri
- ✅ "Düzenle" butonu (sadece liste sahibi için)

**Kullanılan Endpoint'ler:**
- `GET /api/list/{listId}` - Listeyi getir
- `POST /api/social/like/{listId}` - Beğen
- `GET /api/comment/list/{listId}` - Yorumları getir
- `POST /api/comment/add` - Yorum ekle
- `POST /api/share/generate-link` - Paylaşım linki oluştur
- `GET /api/export/embed/{listId}` - Embed kodu getir
- `POST /api/export/image/{listId}` - Export verisi getir
- `GET /api/list/{listId}/statistics` - İstatistikleri getir

**UI Bileşenleri:**
- TierMaker-style layout (Tiered/Fusion için)
- Ranked list layout (Ranked için)
- Social buttons (like, share, comment)
- Comment section
- Share modal (link kopyalama, embed kodu)

---

#### 7. **Public Liste Sayfası** (`/share/{shareToken}`)

**Amaç:** Paylaşılan listeyi herkese açık görüntüleme.

**Özellikler:**
- ✅ Liste görüntüleme (Liste Görüntüleme Sayfası ile aynı)
- ✅ Liste sahibi bilgisi
- ✅ Görüntülenme sayısı
- ✅ Beğeni sayısı
- ✅ "Kopyala" butonu (kendi listene kopyala)
- ✅ Yorumlar (herkese açık)
- ✅ Giriş yapmamış kullanıcılar için "Giriş Yap" çağrısı

**Kullanılan Endpoint'ler:**
- `GET /api/share/public/{shareToken}` - Public listeyi getir
- `POST /api/social/like/{listId}` - Beğen (giriş yapılmışsa)
- `GET /api/comment/list/{listId}` - Yorumları getir
- `POST /api/comment/add` - Yorum ekle (giriş yapılmışsa)
- `POST /api/copy/list` - Listeyi kopyala (giriş yapılmışsa)

**UI Bileşenleri:**
- Liste Görüntüleme Sayfası ile aynı
- "Copy to My Lists" butonu
- Login prompt (giriş yapılmamışsa)

---

#### 8. **Profil Sayfası** (`/profile/{userId}` veya `/profile/me`)

**Amaç:** Kullanıcı profil bilgileri, listeleri, istatistikleri.

**Özellikler:**
- ✅ Profil bilgileri:
  - Profil resmi
  - Kullanıcı adı
  - MAL kullanıcı adı (varsa)
  - Kayıt tarihi
- ✅ İstatistikler:
  - Toplam liste sayısı
  - Public liste sayısı
  - Toplam beğeni
  - Takipçi/Takip edilen sayıları
  - MAL istatistikleri (izlenen anime, ortalama puan, vb.)
- ✅ Kullanıcının listeleri (grid/liste görünümü)
- ✅ "Takip Et" / "Takipten Çık" butonu (kendi profili değilse)
- ✅ "Mesaj Gönder" butonu (opsiyonel)
- ✅ Aktivite akışı (son aktiviteler)

**Kullanılan Endpoint'ler:**
- `GET /api/user/{userId}` - Kullanıcı bilgilerini getir
- `GET /api/social/profile/{userId}` - Profil detaylarını getir
- `GET /api/statistics/user/{userId}` - İstatistikleri getir
- `GET /api/list/all` - Kullanıcının listelerini getir (filtrele)
- `GET /api/activity/user/{userId}` - Aktiviteleri getir
- `POST /api/social/follow` - Takip et/takipten çık

**UI Bileşenleri:**
- Profil header (büyük profil resmi, bilgiler)
- İstatistik kartları
- Tab navigation (Listeler, Aktiviteler, İstatistikler)
- Liste grid
- Follow button
- Charts/graphs (istatistikler için)

---

#### 9. **Profil Ayarları Sayfası** (`/profile/settings`)

**Amaç:** Kullanıcı profil ayarları, şifre değiştirme, hesap yönetimi.

**Özellikler:**
- ✅ Profil bilgileri düzenleme:
  - Kullanıcı adı
  - MAL kullanıcı adı
- ✅ Profil resmi yükleme:
  - Mevcut resim gösterimi
  - Upload butonu
  - Resim önizleme
- ✅ Şifre değiştirme:
  - Mevcut şifre
  - Yeni şifre
  - Yeni şifre tekrar
- ✅ MyAnimeList bağlantısı:
  - Bağlı durumu gösterimi
  - "Bağla" / "Bağlantıyı Kes" butonu
- ✅ Hesap silme:
  - "Hesabı Sil" butonu
  - Onay dialogu

**Kullanılan Endpoint'ler:**
- `GET /api/user/me` - Kullanıcı bilgilerini getir
- `PUT /api/user/profile` - Profil güncelle
- `POST /api/user/upload-image` - Profil resmi yükle
- `POST /api/user/change-password` - Şifre değiştir
- `GET /api/mal/get-auth-url` - MAL bağlantı URL'i
- `POST /api/mal/callback` - MAL callback
- `DELETE /api/user` - Kullanıcı sil

**UI Bileşenleri:**
- Form sections (accordion veya tabs)
- File upload component
- Image preview
- Confirmation dialogs
- Success/error messages

---

#### 10. **Arama Sayfası** (`/search`)

**Amaç:** Anime arama ve filtreleme.

**Özellikler:**
- ✅ Arama çubuğu (anime adı)
- ✅ Filtreler:
  - Kategori (dropdown)
  - Yıl (slider veya input)
  - Minimum puan (slider)
  - Maximum puan (slider)
- ✅ Arama sonuçları:
  - Grid/liste görünümü
  - Anime kartları (thumbnail, başlık, yıl, puan, kategoriler)
  - "Listeye Ekle" butonu (her anime için)
- ✅ Pagination
- ✅ Sıralama seçenekleri (puan, yıl, alfabetik)

**Kullanılan Endpoint'ler:**
- `GET /api/search/anime` - Anime ara
- `GET /api/generate/genres` - Kategorileri getir (filtre için)

**UI Bileşenleri:**
- Search bar (autocomplete önerilir)
- Filter sidebar
- Result grid
- Anime cards
- Pagination component
- Loading skeleton

---

#### 11. **Keşfet Sayfası** (`/discover`)

**Amaç:** Public listeleri keşfetme, trending içerikler.

**Özellikler:**
- ✅ Public listeler grid'i
- ✅ Filtreleme:
  - Kategori
  - Mod (Ranked, Tiered, Fusion)
  - Sıralama (beğeni, görüntülenme, tarih)
- ✅ Trending listeler bölümü
- ✅ "En Çok Beğenilen" bölümü
- ✅ "En Çok Görüntülenen" bölümü
- ✅ Pagination

**Kullanılan Endpoint'ler:**
- `GET /api/share/public` - Public listeleri getir
- `GET /api/recommendation/trending` - Trending listeleri getir

**UI Bileşenleri:**
- Filter bar
- List grid
- List cards (thumbnail, başlık, sahip, beğeni, görüntülenme)
- Pagination

---

#### 12. **Bildirimler Sayfası** (`/notifications`)

**Amaç:** Kullanıcı bildirimlerini görüntüleme ve yönetme.

**Özellikler:**
- ✅ Bildirim listesi:
  - Okunmamış bildirimler (vurgulu)
  - Okunmuş bildirimler
  - Bildirim tipi ikonu (like, comment, follow, vb.)
  - Bildirim mesajı
  - Zaman damgası
  - İlgili içerik linki
- ✅ "Tümünü Okundu İşaretle" butonu
- ✅ "Tümünü Sil" butonu
- ✅ Her bildirim için:
  - "Okundu İşaretle" butonu
  - "Sil" butonu
- ✅ Okunmamış bildirim sayısı (header'da badge)

**Kullanılan Endpoint'ler:**
- `GET /api/social/notifications` - Bildirimleri getir
- `PUT /api/social/notifications/{notificationId}/read` - Okundu işaretle
- `PUT /api/social/notifications/read-all` - Tümünü okundu işaretle
- `DELETE /api/social/notification/{notificationId}` - Bildirim sil
- `DELETE /api/social/notifications/all` - Tümünü sil

**UI Bileşenleri:**
- Notification list
- Notification cards
- Badge (okunmamış sayısı)
- Action buttons
- Empty state (bildirim yoksa)

---

#### 13. **Şablonlar Sayfası** (`/templates`)

**Amaç:** Liste şablonlarını görüntüleme ve kullanma.

**Özellikler:**
- ✅ Şablon listesi (grid)
- ✅ Şablon kartları:
  - Başlık
  - Mod
  - Oluşturan kullanıcı
  - Kullanım sayısı
  - Önizleme
- ✅ "Kullan" butonu (listeyi kopyala)
- ✅ "Şablon Oluştur" butonu (kendi listenden)
- ✅ Filtreleme (mod, kullanıcı)

**Kullanılan Endpoint'ler:**
- `GET /api/social/templates` - Şablonları getir
- `POST /api/social/template/create` - Şablon oluştur
- `POST /api/copy/list` - Listeyi kopyala (şablonu kullan)

**UI Bileşenleri:**
- Template grid
- Template cards
- Preview modal
- Create template modal

---

#### 14. **MAL Bağlantı Sayfası** (`/mal/connect`)

**Amaç:** MyAnimeList hesabını bağlama.

**Özellikler:**
- ✅ MAL bağlantı durumu gösterimi
- ✅ "MAL Hesabını Bağla" butonu
- ✅ Bağlantı adımları açıklaması
- ✅ Bağlı ise:
  - MAL kullanıcı adı
  - "Listemi Getir" butonu
  - "Bağlantıyı Kes" butonu

**Kullanılan Endpoint'ler:**
- `GET /api/mal/get-auth-url` - Auth URL al
- `POST /api/mal/callback` - Callback işle
- `GET /api/mal/get-my-list` - Listeyi getir

**UI Bileşenleri:**
- Connection status card
- Step-by-step guide
- MAL logo/branding

---

#### 15. **MAL Senkronizasyon Sayfası** (`/list/{listId}/sync`)

**Amaç:** Mevcut listeyi MAL listesi ile senkronize etme.

**Özellikler:**
- ✅ Liste bilgileri
- ✅ Senkronizasyon modu seçimi:
  - Ranked
  - Tiered
  - Fusion
- ✅ "Mevcut item'ları değiştir" checkbox
- ✅ "Senkronize Et" butonu
- ✅ Progress indicator
- ✅ Sonuç gösterimi (kaç anime eklendi)

**Kullanılan Endpoint'ler:**
- `POST /api/sync/mal` - MAL listesini senkronize et

**UI Bileşenleri:**
- Mode selection
- Progress bar
- Result summary

---

### 🧩 Ortak UI Bileşenleri

#### Header/Navbar
- Logo (anasayfaya link)
- Navigasyon menüsü:
  - Ana Sayfa
  - Keşfet
  - Arama
  - Şablonlar
- Arama çubuğu (global arama)
- Bildirim ikonu (badge ile okunmamış sayısı)
- Profil dropdown:
  - Profilim
  - Ayarlar
  - Çıkış Yap

#### Footer
- Hızlı linkler
- Sosyal medya
- Yasal bilgiler
- Copyright

#### Modals
- **Anime Arama Modal**: Liste düzenleme sayfasında anime eklerken
- **Tier Ekleme Modal**: Tier ekleme/düzenleme
- **Paylaşım Modal**: Link kopyalama, embed kodu
- **Onay Modal**: Silme işlemleri için
- **Bildirim Modal**: Bildirim detayları

#### Toast Notifications
- Başarı mesajları (yeşil)
- Hata mesajları (kırmızı)
- Bilgi mesajları (mavi)
- Uyarı mesajları (sarı)

#### Loading States
- Skeleton loaders
- Spinner components
- Progress bars

#### Empty States
- Liste yoksa
- Arama sonucu yoksa
- Bildirim yoksa

---

### 🎯 Kullanıcı Akışları

#### 1. Yeni Kullanıcı Akışı
1. Ana sayfa → Register
2. Kayıt ol → Dashboard
3. MAL bağla (opsiyonel)
4. İlk listeyi oluştur (manuel veya otomatik)

#### 2. Liste Oluşturma Akışı
1. Dashboard → "Yeni Liste Oluştur"
2. Liste bilgilerini gir
3. Tier'ları özelleştir (opsiyonel)
4. Listeyi oluştur
5. Liste düzenleme sayfasına yönlendir
6. Animeleri ekle (arama veya MAL'dan)
7. Kaydet

#### 3. Otomatik Liste Oluşturma Akışı
1. Dashboard → "Puana Göre Liste Oluştur" (veya diğer seçenekler)
2. Loading göster
3. Liste oluşturuldu → Liste görüntüleme sayfasına yönlendir

#### 4. Liste Paylaşma Akışı
1. Liste görüntüleme sayfası → "Paylaş" butonu
2. Paylaşım modalı açılır
3. Link oluştur
4. Linki kopyala veya sosyal medyada paylaş

#### 5. Liste Kopyalama Akışı
1. Public liste sayfası → "Kopyala" butonu
2. Yeni liste adı gir
3. Liste kopyalanır → Liste düzenleme sayfasına yönlendir

---

### 📱 Responsive Tasarım Detayları

#### Mobile (< 768px)
- Hamburger menü
- Bottom navigation (opsiyonel)
- Tek sütun layout
- Touch-friendly butonlar
- Swipe gestures (bildirimler, listeler)

#### Tablet (768px - 1024px)
- Sidebar collapse/expand
- İki sütun layout (mümkünse)
- Touch + mouse desteği

#### Desktop (> 1024px)
- Tam sidebar
- Çok sütunlu grid'ler
- Hover effects
- Keyboard shortcuts (opsiyonel)

---

### 🎨 Tasarım Önerileri

#### TierMaker Tarzı Görünüm
- Tiered ve Fusion modları için yatay tier'lar
- Her tier için renkli arka plan
- Drag & drop ile anime taşıma
- Animasyonlu geçişler

#### Ranked Modu Görünümü
- Dikey liste
- Numaralı sıralama
- Thumbnail + başlık
- Sıralama değiştirme (drag & drop)

#### Genel Tasarım Prensipleri
- Modern ve minimal
- Hızlı yükleme
- Smooth animasyonlar
- Accessibility (WCAG uyumlu)
- Dark mode desteği

---

## 📁 Frontend Dosya Yapısı ve Kullanım Kılavuzu

### 🏗️ Proje Yapısı (Angular 19 Standalone Components)

AnimeRanker frontend projesi, Angular 19 standalone component yapısı kullanılarak organize edilmiştir. Backend'deki modüler yapıya uygun olarak tasarlanmıştır.

```
anime-ranker-frontend/
├── frontend/
│   ├── src/
│   │   ├── app/
│   │   │   ├── app.component.ts              # Ana root component
│   │   │   ├── app.component.html
│   │   │   ├── app.component.scss
│   │   │   ├── app.config.ts                 # Uygulama yapılandırması
│   │   │   ├── app.routes.ts                 # Route tanımları
│   │   │   └── index.html
│   │   │
│   │   │   ├── components/                   # Component'ler
│   │   │   │   ├── modules/                  # Sayfa component'leri
│   │   │   │   │   ├── dashboard/
│   │   │   │   │   │   ├── dashboard.component.ts
│   │   │   │   │   │   ├── dashboard.component.html
│   │   │   │   │   │   └── dashboard.component.scss
│   │   │   │   │   │
│   │   │   │   │   ├── login/
│   │   │   │   │   │   ├── login.component.ts
│   │   │   │   │   │   ├── login.component.html
│   │   │   │   │   │   └── login.component.scss
│   │   │   │   │   │
│   │   │   │   │   ├── signin/
│   │   │   │   │   │   ├── signin.component.ts
│   │   │   │   │   │   ├── signin.component.html
│   │   │   │   │   │   └── signin.component.scss
│   │   │   │   │   │
│   │   │   │   │   ├── list-create/
│   │   │   │   │   │   ├── list-create.component.ts
│   │   │   │   │   │   ├── list-create.component.html
│   │   │   │   │   │   └── list-create.component.scss
│   │   │   │   │   │
│   │   │   │   │   ├── list-edit/
│   │   │   │   │   │   ├── list-edit.component.ts
│   │   │   │   │   │   ├── list-edit.component.html
│   │   │   │   │   │   └── list-edit.component.scss
│   │   │   │   │   │
│   │   │   │   │   ├── list-view/
│   │   │   │   │   │   ├── list-view.component.ts
│   │   │   │   │   │   ├── list-view.component.html
│   │   │   │   │   │   └── list-view.component.scss
│   │   │   │   │   │
│   │   │   │   │   ├── list-generate/
│   │   │   │   │   │   ├── list-generate.component.ts
│   │   │   │   │   │   ├── list-generate.component.html
│   │   │   │   │   │   └── list-generate.component.scss
│   │   │   │   │   │
│   │   │   │   │   ├── profile/
│   │   │   │   │   │   ├── profile.component.ts
│   │   │   │   │   │   ├── profile.component.html
│   │   │   │   │   │   └── profile.component.scss
│   │   │   │   │   │
│   │   │   │   │   ├── profile-settings/
│   │   │   │   │   │   ├── profile-settings.component.ts
│   │   │   │   │   │   ├── profile-settings.component.html
│   │   │   │   │   │   └── profile-settings.component.scss
│   │   │   │   │   │
│   │   │   │   │   ├── search/
│   │   │   │   │   │   ├── search.component.ts
│   │   │   │   │   │   ├── search.component.html
│   │   │   │   │   │   └── search.component.scss
│   │   │   │   │   │
│   │   │   │   │   ├── discover/
│   │   │   │   │   │   ├── discover.component.ts
│   │   │   │   │   │   ├── discover.component.html
│   │   │   │   │   │   └── discover.component.scss
│   │   │   │   │   │
│   │   │   │   │   ├── notifications/
│   │   │   │   │   │   ├── notifications.component.ts
│   │   │   │   │   │   ├── notifications.component.html
│   │   │   │   │   │   └── notifications.component.scss
│   │   │   │   │   │
│   │   │   │   │   ├── templates/
│   │   │   │   │   │   ├── templates.component.ts
│   │   │   │   │   │   ├── templates.component.html
│   │   │   │   │   │   └── templates.component.scss
│   │   │   │   │   │
│   │   │   │   │   ├── share-view/
│   │   │   │   │   │   ├── share-view.component.ts
│   │   │   │   │   │   ├── share-view.component.html
│   │   │   │   │   │   └── share-view.component.scss
│   │   │   │   │   │
│   │   │   │   │   └── mal-connect/
│   │   │   │   │       ├── mal-connect.component.ts
│   │   │   │   │       ├── mal-connect.component.html
│   │   │   │   │       └── mal-connect.component.scss
│   │   │   │   │
│   │   │   │   ├── shared/                 # Paylaşılan component'ler
│   │   │   │   │   ├── navbar/
│   │   │   │   │   │   ├── navbar.component.ts
│   │   │   │   │   │   ├── navbar.component.html
│   │   │   │   │   │   └── navbar.component.scss
│   │   │   │   │   │
│   │   │   │   │   ├── list-card/
│   │   │   │   │   │   ├── list-card.component.ts
│   │   │   │   │   │   ├── list-card.component.html
│   │   │   │   │   │   └── list-card.component.scss
│   │   │   │   │   │
│   │   │   │   │   ├── tier-card/
│   │   │   │   │   │   ├── tier-card.component.ts
│   │   │   │   │   │   ├── tier-card.component.html
│   │   │   │   │   │   └── tier-card.component.scss
│   │   │   │   │   │
│   │   │   │   │   ├── anime-card/
│   │   │   │   │   │   ├── anime-card.component.ts
│   │   │   │   │   │   ├── anime-card.component.html
│   │   │   │   │   │   └── anime-card.component.scss
│   │   │   │   │   │
│   │   │   │   │   ├── tier-maker/
│   │   │   │   │   │   ├── tier-maker.component.ts
│   │   │   │   │   │   ├── tier-maker.component.html
│   │   │   │   │   │   └── tier-maker.component.scss
│   │   │   │   │   │
│   │   │   │   │   ├── comment-section/
│   │   │   │   │   │   ├── comment-section.component.ts
│   │   │   │   │   │   ├── comment-section.component.html
│   │   │   │   │   │   └── comment-section.component.scss
│   │   │   │   │   │
│   │   │   │   │   ├── like-button/
│   │   │   │   │   │   ├── like-button.component.ts
│   │   │   │   │   │   ├── like-button.component.html
│   │   │   │   │   │   └── like-button.component.scss
│   │   │   │   │   │
│   │   │   │   │   └── share-modal/
│   │   │   │   │       ├── share-modal.component.ts
│   │   │   │   │       ├── share-modal.component.html
│   │   │   │   │       └── share-modal.component.scss
│   │   │   │   │
│   │   │   │   └── tools/                   # Admin araçları (opsiyonel)
│   │   │   │       └── admin-tools/
│   │   │   │           ├── admin-tools.component.ts
│   │   │   │           ├── admin-tools.component.html
│   │   │   │           └── admin-tools.component.scss
│   │   │   │
│   │   │   ├── core/                        # Core servisler ve yapılandırmalar
│   │   │   │   ├── services/
│   │   │   │   │   ├── public/              # Public servisler
│   │   │   │   │   │   ├── auth.service.ts
│   │   │   │   │   │   ├── http-header.service.ts
│   │   │   │   │   │   └── main-config.service.ts
│   │   │   │   │   │
│   │   │   │   │   └── api/                 # API servisleri
│   │   │   │   │       ├── auth.service.ts
│   │   │   │   │       ├── mal-integration.service.ts
│   │   │   │   │       ├── anime-list.service.ts
│   │   │   │   │       ├── list-generator.service.ts
│   │   │   │   │       ├── search.service.ts
│   │   │   │   │       ├── share.service.ts
│   │   │   │   │       ├── social.service.ts
│   │   │   │   │       ├── comment.service.ts
│   │   │   │   │       ├── user.service.ts
│   │   │   │   │       ├── file.service.ts
│   │   │   │   │       ├── statistics.service.ts
│   │   │   │   │       ├── activity.service.ts
│   │   │   │   │       ├── recommendation.service.ts
│   │   │   │   │       ├── copy.service.ts
│   │   │   │   │       ├── export.service.ts
│   │   │   │   │       ├── sync.service.ts
│   │   │   │   │       └── dragdrop.service.ts
│   │   │   │   │
│   │   │   │   ├── guards/
│   │   │   │   │   ├── auth.guard.ts
│   │   │   │   │   └── guest.guard.ts
│   │   │   │   │
│   │   │   │   ├── pipes/
│   │   │   │   │   ├── date-format.pipe.ts
│   │   │   │   │   ├── truncate.pipe.ts
│   │   │   │   │   └── safe-url.pipe.ts
│   │   │   │   │
│   │   │   │   └── modals/
│   │   │   │       ├── anime-search-modal/
│   │   │   │       │   ├── anime-search-modal.component.ts
│   │   │   │       │   ├── anime-search-modal.component.html
│   │   │   │       │   └── anime-search-modal.component.scss
│   │   │   │       │
│   │   │   │       ├── tier-add-modal/
│   │   │   │       │   ├── tier-add-modal.component.ts
│   │   │   │       │   ├── tier-add-modal.component.html
│   │   │   │       │   └── tier-add-modal.component.scss
│   │   │   │       │
│   │   │   │       └── share-modal/
│   │   │   │           ├── share-modal.component.ts
│   │   │   │           ├── share-modal.component.html
│   │   │   │           └── share-modal.component.scss
│   │   │   │
│   │   │   ├── models/                      # TypeScript modelleri
│   │   │   │   ├── requests/
│   │   │   │   │   ├── auth-requests.model.ts
│   │   │   │   │   ├── anime-list-requests.model.ts
│   │   │   │   │   ├── list-generator-requests.model.ts
│   │   │   │   │   ├── search-requests.model.ts
│   │   │   │   │   ├── share-requests.model.ts
│   │   │   │   │   ├── social-requests.model.ts
│   │   │   │   │   ├── comment-requests.model.ts
│   │   │   │   │   ├── user-requests.model.ts
│   │   │   │   │   ├── copy-requests.model.ts
│   │   │   │   │   ├── dragdrop-requests.model.ts
│   │   │   │   │   └── sync-requests.model.ts
│   │   │   │   │
│   │   │   │   ├── responses/
│   │   │   │   │   ├── base-response.model.ts
│   │   │   │   │   ├── auth-responses.model.ts
│   │   │   │   │   ├── anime-list-responses.model.ts
│   │   │   │   │   ├── list-generator-responses.model.ts
│   │   │   │   │   ├── search-responses.model.ts
│   │   │   │   │   ├── share-responses.model.ts
│   │   │   │   │   ├── social-responses.model.ts
│   │   │   │   │   ├── comment-responses.model.ts
│   │   │   │   │   ├── user-responses.model.ts
│   │   │   │   │   ├── statistics-responses.model.ts
│   │   │   │   │   ├── activity-responses.model.ts
│   │   │   │   │   └── recommendation-responses.model.ts
│   │   │   │   │
│   │   │   │   └── entities/
│   │   │   │       ├── user.model.ts
│   │   │   │       ├── anime-list.model.ts
│   │   │   │       ├── tier.model.ts
│   │   │   │       ├── ranked-item.model.ts
│   │   │   │       ├── comment.model.ts
│   │   │   │       ├── notification.model.ts
│   │   │   │       └── enums/
│   │   │   │           ├── list-mode.enum.ts
│   │   │   │           ├── user-role.enum.ts
│   │   │   │           └── file-type.enum.ts
│   │   │   │
│   │   │   └── utils/                       # Yardımcı fonksiyonlar
│   │   │       ├── constants.ts
│   │   │       ├── helpers.ts
│   │   │       ├── validators.ts
│   │   │       └── formatters.ts
│   │   │
│   │   ├── styles/
│   │   │   ├── _variables.scss
│   │   │   ├── _mixins.scss
│   │   │   ├── _reset.scss
│   │   │   ├── _typography.scss
│   │   │   ├── _colors.scss
│   │   │   ├── _animations.scss
│   │   │   └── main.scss
│   │   │
│   │   ├── assets/
│   │   │   ├── images/
│   │   │   └── fonts/
│   │   │
│   │   ├── environments/
│   │   │   ├── environment.ts
│   │   │   └── environment.prod.ts
│   │   │
│   │   ├── main.ts
│   │   └── index.html
│   │
│   ├── package.json
│   ├── angular.json
│   ├── tsconfig.json
│   └── README.md
│
├── .gitignore
├── .editorconfig
└── README.md
```

---

### 📝 Dosya Detayları ve Endpoint Kullanımları

#### 🔧 **Core Servisler**

##### **`core/services/public/auth.service.ts`**
**Amaç:** Kimlik doğrulama ve token yönetimi

**Kullanılan Endpoint'ler:**
- `POST /api/auth/register` - Kullanıcı kaydı
- `POST /api/auth/login` - Kullanıcı girişi

**Metodlar:**
```typescript
register(username: string, password: string): Observable<BaseResponse>
login(username: string, password: string): Observable<BaseResponse>
isAuthenticated(): boolean
getToken(): string | null
getUserId(): number | null
logout(): void
```

**LocalStorage Kullanımı:**
- `token`: JWT token
- `userId`: Kullanıcı ID
- `username`: Kullanıcı adı

---

##### **`core/services/public/http-header.service.ts`**
**Amaç:** HTTP istekleri için header yönetimi

**Özellikler:**
- Token header'ı ekleme
- Content-Type ayarlama
- Base URL yapılandırması: `https://localhost:7123/api`

**Kullanım:**
```typescript
getHeaders(): HttpHeaders {
  const token = localStorage.getItem('token');
  return new HttpHeaders({
    'Content-Type': 'application/json',
    'Token': token || ''
  });
}
```

---

##### **`core/services/public/main-config.service.ts`**
**Amaç:** Konfigürasyon verilerini yönetir (enum'lar, sabitler)

**Kullanım:**
- ListMode enum (Ranked, Tiered, Fusion)
- UserRole enum
- FileType enum
- Diğer sabit değerler

---

#### 🌐 **API Servisleri**

##### **`core/services/api/auth.service.ts`**
**Amaç:** Authentication işlemleri

**Kullanılan Endpoint'ler:**
- `POST /api/auth/register` - Kayıt
- `POST /api/auth/login` - Giriş

**Metodlar:**
```typescript
register(request: RegisterDto): Observable<BaseResponse>
login(request: LoginDto): Observable<BaseResponse>
```

---

##### **`core/services/api/mal-integration.service.ts`**
**Amaç:** MyAnimeList entegrasyonu

**Kullanılan Endpoint'ler:**
- `GET /api/mal/get-auth-url` - MAL auth URL al
- `POST /api/mal/callback` - MAL callback işle
- `GET /api/mal/get-my-list` - MAL listesini getir

**Metodlar:**
```typescript
getAuthUrl(): Observable<BaseResponse<MalAuthUrlResponse>>
handleCallback(code: string, codeVerifier: string): Observable<BaseResponse>
getMyList(): Observable<BaseResponse<MalAnimeListResponse>>
```

**Kullanıldığı Component'ler:**
- `mal-connect.component.ts`
- `list-generate.component.ts`
- `sync.service.ts`

---

##### **`core/services/api/anime-list.service.ts`**
**Amaç:** Anime listesi CRUD işlemleri

**Kullanılan Endpoint'ler:**
- `POST /api/list/create` - Liste oluştur
- `PUT /api/list/save` - Liste kaydet
- `GET /api/list/{listId}` - Liste getir
- `GET /api/list/all` - Tüm listeleri getir
- `POST /api/list/convert-to-fusion` - Fusion'a çevir
- `POST /api/list/convert-to-ranked` - Ranked'e çevir
- `DELETE /api/list/{listId}` - Liste sil
- `PATCH /api/list/{listId}/title` - Başlık güncelle
- `POST /api/list/item/add` - Item ekle
- `DELETE /api/list/item/remove` - Item sil
- `POST /api/list/tier/add` - Tier ekle
- `PUT /api/list/tier/update` - Tier güncelle
- `DELETE /api/list/tier/remove` - Tier sil
- `POST /api/list/check-duplicate` - Duplicate kontrol
- `GET /api/list/{listId}/statistics` - İstatistikleri getir
- `POST /api/list/swap-ranks` - Rank değiştir
- `POST /api/list/reset-ranks` - Rank sıfırla
- `POST /api/list/bulk-add-items` - Toplu item ekle
- `POST /api/list/bulk-remove-items` - Toplu item sil

**Metodlar:**
```typescript
createList(request: CreateListRequest): Observable<BaseResponse>
saveList(request: SaveListRequest): Observable<BaseResponse>
getList(listId: number): Observable<BaseResponse<AnimeListDto>>
getAllLists(): Observable<BaseResponse>
convertToFusion(listId: number): Observable<BaseResponse>
convertToRanked(listId: number): Observable<BaseResponse>
deleteList(listId: number): Observable<BaseResponse>
updateListTitle(listId: number, title: string): Observable<BaseResponse>
addItem(request: AddItemRequest): Observable<BaseResponse>
removeItem(itemId: number): Observable<BaseResponse>
addTier(request: AddTierRequest): Observable<BaseResponse>
updateTier(request: UpdateTierRequest): Observable<BaseResponse>
removeTier(tierId: number, moveItemsToTierId?: number): Observable<BaseResponse>
checkDuplicate(listId: number, animeMalId: number): Observable<BaseResponse>
getListStatistics(listId: number): Observable<BaseResponse>
swapRanks(listId: number, itemId1: number, itemId2: number): Observable<BaseResponse>
resetRanks(tierId: number): Observable<BaseResponse>
bulkAddItems(request: BulkAddItemsRequest): Observable<BaseResponse>
bulkRemoveItems(request: BulkRemoveItemsRequest): Observable<BaseResponse>
```

**Kullanıldığı Component'ler:**
- `list-create.component.ts`
- `list-edit.component.ts`
- `list-view.component.ts`
- `dashboard.component.ts`

---

##### **`core/services/api/list-generator.service.ts`**
**Amaç:** Otomatik liste oluşturma

**Kullanılan Endpoint'ler:**
- `POST /api/generate/by-score` - Puana göre liste oluştur
- `POST /api/generate/by-year` - Yıla göre liste oluştur
- `POST /api/generate/by-genre` - Kategoriye göre liste oluştur
- `GET /api/generate/genres` - Kategorileri getir

**Metodlar:**
```typescript
generateByScore(): Observable<BaseResponse>
generateByYear(): Observable<BaseResponse>
generateByGenre(genreTag: string): Observable<BaseResponse>
getGenres(): Observable<BaseResponse<string[]>>
```

**Kullanıldığı Component'ler:**
- `list-generate.component.ts`
- `dashboard.component.ts`

---

##### **`core/services/api/search.service.ts`**
**Amaç:** Anime arama

**Kullanılan Endpoint'ler:**
- `GET /api/search/anime` - Anime ara (query params ile)
- `POST /api/search/anime` - Anime ara (body ile)

**Metodlar:**
```typescript
searchAnime(request: SearchAnimeRequest): Observable<BaseResponse<AnimeSearchResultDto>>
```

**Kullanıldığı Component'ler:**
- `search.component.ts`
- `anime-search-modal.component.ts` (liste düzenleme sayfasında)

---

##### **`core/services/api/share.service.ts`**
**Amaç:** Liste paylaşım işlemleri

**Kullanılan Endpoint'ler:**
- `POST /api/share/set-visibility` - Görünürlük ayarla
- `POST /api/share/generate-link` - Paylaşım linki oluştur
- `GET /api/share/public/{shareToken}` - Public listeyi getir
- `GET /api/share/public` - Public listeleri getir
- `DELETE /api/share/link/{listId}` - Paylaşım linkini sil

**Metodlar:**
```typescript
setVisibility(listId: number, isPublic: boolean): Observable<BaseResponse>
generateShareLink(listId: number): Observable<BaseResponse<ShareLinkResponse>>
getPublicList(shareToken: string): Observable<BaseResponse>
getPublicLists(page?: number, limit?: number): Observable<BaseResponse>
deleteShareLink(listId: number): Observable<BaseResponse>
```

**Kullanıldığı Component'ler:**
- `share-view.component.ts`
- `list-view.component.ts`
- `discover.component.ts`
- `share-modal.component.ts`

---

##### **`core/services/api/social.service.ts`**
**Amaç:** Sosyal özellikler (beğeni, takip, bildirimler, şablonlar)

**Kullanılan Endpoint'ler:**
- `POST /api/social/like/{listId}` - Liste beğen
- `POST /api/social/follow` - Kullanıcı takip et
- `GET /api/social/profile/{userId}` - Kullanıcı profili getir
- `GET /api/social/notifications` - Bildirimleri getir
- `PUT /api/social/notifications/{notificationId}/read` - Bildirimi okundu işaretle
- `PUT /api/social/notifications/read-all` - Tümünü okundu işaretle
- `DELETE /api/social/notification/{notificationId}` - Bildirim sil
- `DELETE /api/social/notifications/all` - Tüm bildirimleri sil
- `POST /api/social/template/create` - Şablon oluştur
- `GET /api/social/templates` - Şablonları getir
- `DELETE /api/social/template/{templateId}` - Şablon sil

**Metodlar:**
```typescript
likeList(listId: number): Observable<BaseResponse>
followUser(userId: number): Observable<BaseResponse>
getUserProfile(userId: number): Observable<BaseResponse<UserProfileDto>>
getNotifications(page?: number, limit?: number): Observable<BaseResponse>
markNotificationAsRead(notificationId: number): Observable<BaseResponse>
markAllNotificationsAsRead(): Observable<BaseResponse>
deleteNotification(notificationId: number): Observable<BaseResponse>
deleteAllNotifications(): Observable<BaseResponse>
createTemplate(listId: number): Observable<BaseResponse>
getTemplates(page?: number, limit?: number): Observable<BaseResponse>
deleteTemplate(templateId: number): Observable<BaseResponse>
```

**Kullanıldığı Component'ler:**
- `like-button.component.ts`
- `profile.component.ts`
- `notifications.component.ts`
- `templates.component.ts`
- `list-view.component.ts`

---

##### **`core/services/api/comment.service.ts`**
**Amaç:** Yorum işlemleri

**Kullanılan Endpoint'ler:**
- `POST /api/comment/add` - Yorum ekle
- `GET /api/comment/list/{listId}` - Yorumları getir
- `PUT /api/comment/update` - Yorum güncelle
- `DELETE /api/comment/{commentId}` - Yorum sil

**Metodlar:**
```typescript
addComment(listId: number, content: string): Observable<BaseResponse<CommentDto>>
getComments(listId: number): Observable<BaseResponse<CommentDto[]>>
updateComment(commentId: number, content: string): Observable<BaseResponse>
deleteComment(commentId: number): Observable<BaseResponse>
```

**Kullanıldığı Component'ler:**
- `comment-section.component.ts`
- `list-view.component.ts`
- `share-view.component.ts`

---

##### **`core/services/api/user.service.ts`**
**Amaç:** Kullanıcı yönetimi

**Kullanılan Endpoint'ler:**
- `POST /api/user/upload-image` - Profil resmi yükle
- `GET /api/user/{userId}` - Kullanıcı bilgilerini getir
- `GET /api/user/me` - Kendi profilimi getir
- `GET /api/user/all` - Tüm kullanıcıları getir
- `POST /api/user/search` - Kullanıcı ara
- `PUT /api/user/update` - Kullanıcı bilgilerini güncelle
- `POST /api/user/change-password` - Şifre değiştir
- `PUT /api/user/profile` - Profil güncelle
- `DELETE /api/user` - Kullanıcı sil

**Metodlar:**
```typescript
uploadUserImage(file: File): Observable<BaseResponse>
getUser(userId: number): Observable<BaseResponse<GetUserResponse>>
getMyProfile(): Observable<BaseResponse<GetUserResponse>>
getAllUsers(request: GetAllUsersRequest): Observable<BaseResponse<UserListResponse>>
searchUsers(request: SearchUsersRequest): Observable<BaseResponse>
updateUser(request: UpdateUserRequest): Observable<BaseResponse>
changePassword(currentPassword: string, newPassword: string): Observable<BaseResponse>
updateProfile(request: UpdateProfileRequest): Observable<BaseResponse>
deleteUser(userId: number, hardDelete: boolean, password?: string): Observable<BaseResponse>
```

**Kullanıldığı Component'ler:**
- `profile.component.ts`
- `profile-settings.component.ts`
- `navbar.component.ts`

---

##### **`core/services/api/file.service.ts`**
**Amaç:** Dosya yönetimi

**Kullanılan Endpoint'ler:**
- `GET /api/file/download` - Dosya indir (güvenli link)
- `GET /api/file/info` - Dosya bilgilerini getir
- `POST /api/file/clean-temp` - Temp dosyaları temizle

**Metodlar:**
```typescript
downloadFile(filename: string, type: FileType, sessionno: string, signature: string): Observable<Blob>
getFileInfo(filename: string, type: FileType): Observable<BaseResponse>
generateFileLink(filename: string, type: FileType, userId: number): string
cleanTempFiles(): Observable<BaseResponse>
```

**Kullanıldığı Component'ler:**
- `profile-settings.component.ts` (profil resmi yükleme)
- `user-avatar.component.ts` (profil resmi gösterimi)

---

##### **`core/services/api/statistics.service.ts`**
**Amaç:** İstatistikler

**Kullanılan Endpoint'ler:**
- `GET /api/statistics/user/{userId}` - Kullanıcı istatistikleri
- `GET /api/statistics/me` - Kendi istatistiklerim

**Metodlar:**
```typescript
getUserStatistics(userId: number): Observable<BaseResponse<UserStatisticsDto>>
getMyStatistics(): Observable<BaseResponse<UserStatisticsDto>>
```

**Kullanıldığı Component'ler:**
- `profile.component.ts`
- `dashboard.component.ts`

---

##### **`core/services/api/activity.service.ts`**
**Amaç:** Aktivite akışı

**Kullanılan Endpoint'ler:**
- `GET /api/activity/user/{userId}` - Kullanıcı aktivitesi
- `GET /api/activity/me` - Kendi aktivitem

**Metodlar:**
```typescript
getUserActivity(userId: number, page?: number, limit?: number): Observable<BaseResponse>
getMyActivity(page?: number, limit?: number): Observable<BaseResponse>
```

**Kullanıldığı Component'ler:**
- `profile.component.ts`
- `dashboard.component.ts`

---

##### **`core/services/api/recommendation.service.ts`**
**Amaç:** Öneriler ve trending

**Kullanılan Endpoint'ler:**
- `GET /api/recommendation/anime` - Anime önerileri
- `GET /api/recommendation/trending` - Trending listeler

**Metodlar:**
```typescript
getRecommendations(limit?: number): Observable<BaseResponse<RecommendationDto[]>>
getTrendingLists(page?: number, limit?: number): Observable<BaseResponse>
```

**Kullanıldığı Component'ler:**
- `dashboard.component.ts`
- `discover.component.ts`

---

##### **`core/services/api/copy.service.ts`**
**Amaç:** Liste kopyalama

**Kullanılan Endpoint'ler:**
- `POST /api/copy/list` - Liste kopyala

**Metodlar:**
```typescript
copyList(sourceListId: number, newTitle: string): Observable<BaseResponse>
```

**Kullanıldığı Component'ler:**
- `share-view.component.ts`
- `templates.component.ts`

---

##### **`core/services/api/export.service.ts`**
**Amaç:** Export ve embed

**Kullanılan Endpoint'ler:**
- `POST /api/export/image/{listId}` - Liste export (görsel)
- `GET /api/export/embed/{listId}` - Embed kodu getir

**Metodlar:**
```typescript
exportListAsImage(listId: number): Observable<BaseResponse>
getEmbedCode(listId: number): Observable<BaseResponse<EmbedCodeResponse>>
```

**Kullanıldığı Component'ler:**
- `list-view.component.ts`
- `share-modal.component.ts`

---

##### **`core/services/api/sync.service.ts`**
**Amaç:** MAL senkronizasyonu

**Kullanılan Endpoint'ler:**
- `POST /api/sync/mal` - MAL listesini senkronize et

**Metodlar:**
```typescript
syncMalList(listId: number, mode: ListMode, replaceExisting: boolean): Observable<BaseResponse>
```

**Kullanıldığı Component'ler:**
- `list-edit.component.ts` (MAL senkronizasyon butonu)

---

##### **`core/services/api/dragdrop.service.ts`**
**Amaç:** Drag & drop işlemleri

**Kullanılan Endpoint'ler:**
- `POST /api/dragdrop/move-item` - Item taşı
- `POST /api/dragdrop/reorder-items` - Item sırala

**Metodlar:**
```typescript
moveItem(itemId: number, targetTierId: number, newRankInTier: number): Observable<BaseResponse>
reorderItems(tierId: number, items: Array<{itemId: number, rankInTier: number}>): Observable<BaseResponse>
```

**Kullanıldığı Component'ler:**
- `list-edit.component.ts`
- `tier-maker.component.ts`

---

#### 📄 **Component'ler ve Endpoint Kullanımları**

##### **`components/modules/login/login.component.ts`**
**Amaç:** Kullanıcı girişi

**Kullanılan Endpoint'ler:**
- `POST /api/auth/login`

**Özellikler:**
- SHA1 ile şifre hash'leme (eski projede olduğu gibi)
- Token localStorage'a kaydetme
- SweetAlert2 ile hata/success mesajları
- Giriş sonrası dashboard'a yönlendirme

**Kod Yapısı:**
```typescript
login() {
  const hashedPassword = sha1(this.password);
  this.authService.login(this.username, hashedPassword).subscribe({
    next: (response) => {
      if (response.success) {
        localStorage.setItem('token', response.response.token);
        localStorage.setItem('userId', response.response.userId);
        this.router.navigate(['/dashboard']);
      }
    }
  });
}
```

---

##### **`components/modules/signin/signin.component.ts`**
**Amaç:** Kullanıcı kaydı

**Kullanılan Endpoint'ler:**
- `POST /api/auth/register`

**Özellikler:**
- Şifre doğrulama (password === confirmPassword)
- SHA1 hash ile şifre işleme
- Kayıt sonrası login sayfasına yönlendirme

---

##### **`components/modules/dashboard/dashboard.component.ts`**
**Amaç:** Ana sayfa - kullanıcının listeleri, öneriler, trending

**Kullanılan Endpoint'ler:**
- `GET /api/list/all` - Kullanıcının listelerini getir
- `GET /api/recommendation/anime` - Önerileri getir
- `GET /api/recommendation/trending` - Trending listeleri getir
- `GET /api/statistics/me` - İstatistikleri getir

**Özellikler:**
- Liste grid'i
- Hızlı erişim butonları (Puana/Yıla/Kategoriye göre liste oluştur)
- Önerilen animeler carousel
- Trending listeler
- İstatistik kartları

---

##### **`components/modules/list-create/list-create.component.ts`**
**Amaç:** Yeni liste oluşturma

**Kullanılan Endpoint'ler:**
- `POST /api/list/create`

**Özellikler:**
- Liste başlığı input
- Liste modu seçimi (Ranked, Tiered, Fusion)
- Özel tier oluşturma (opsiyonel)
- Varsayılan tier'ları kullan checkbox
- Liste oluşturulduktan sonra list-edit sayfasına yönlendirme

---

##### **`components/modules/list-edit/list-edit.component.ts`**
**Amaç:** Liste düzenleme

**Kullanılan Endpoint'ler:**
- `GET /api/list/{listId}` - Listeyi getir
- `PUT /api/list/save` - Listeyi kaydet
- `PATCH /api/list/{listId}/title` - Başlık güncelle
- `POST /api/list/item/add` - Item ekle
- `DELETE /api/list/item/remove` - Item sil
- `POST /api/list/tier/add` - Tier ekle
- `PUT /api/list/tier/update` - Tier güncelle
- `DELETE /api/list/tier/remove` - Tier sil
- `POST /api/list/check-duplicate` - Duplicate kontrol
- `POST /api/list/convert-to-fusion` - Fusion'a çevir
- `POST /api/list/convert-to-ranked` - Ranked'e çevir
- `POST /api/dragdrop/move-item` - Item taşı
- `POST /api/dragdrop/reorder-items` - Item sırala
- `GET /api/list/{listId}/statistics` - İstatistikleri getir
- `GET /api/search/anime` - Anime ara (modal için)
- `POST /api/sync/mal` - MAL senkronizasyon (opsiyonel)

**Özellikler:**
- Drag & drop ile tier/item sıralama
- Anime arama modalı
- Tier ekleme/düzenleme modalı
- Duplicate kontrolü
- Mod dönüştürme butonları
- Kaydet/İptal butonları

---

##### **`components/modules/list-view/list-view.component.ts`**
**Amaç:** Liste görüntüleme (okuma modu)

**Kullanılan Endpoint'ler:**
- `GET /api/list/{listId}` - Listeyi getir
- `POST /api/social/like/{listId}` - Beğen
- `GET /api/comment/list/{listId}` - Yorumları getir
- `POST /api/comment/add` - Yorum ekle
- `POST /api/share/generate-link` - Paylaşım linki oluştur
- `GET /api/export/embed/{listId}` - Embed kodu getir
- `POST /api/export/image/{listId}` - Export verisi getir
- `GET /api/list/{listId}/statistics` - İstatistikleri getir

**Özellikler:**
- TierMaker tarzı görünüm (Tiered/Fusion için)
- Ranked list görünümü (Ranked için)
- Beğeni butonu
- Yorumlar bölümü
- Paylaşım butonları
- "Düzenle" butonu (sadece liste sahibi için)

---

##### **`components/modules/list-generate/list-generate.component.ts`**
**Amaç:** Otomatik liste oluşturma

**Kullanılan Endpoint'ler:**
- `GET /api/generate/genres` - Kategorileri getir
- `POST /api/generate/by-score` - Puana göre liste oluştur
- `POST /api/generate/by-year` - Yıla göre liste oluştur
- `POST /api/generate/by-genre` - Kategoriye göre liste oluştur

**Özellikler:**
- Üç seçenek kartı (Score, Year, Genre)
- Kategori dropdown (Genre için)
- Loading state
- Başarı mesajı ve yeni listeye yönlendirme

---

##### **`components/modules/profile/profile.component.ts`**
**Amaç:** Kullanıcı profili görüntüleme

**Kullanılan Endpoint'ler:**
- `GET /api/user/{userId}` - Kullanıcı bilgilerini getir
- `GET /api/social/profile/{userId}` - Profil detaylarını getir
- `GET /api/statistics/user/{userId}` - İstatistikleri getir
- `GET /api/list/all` - Kullanıcının listelerini getir (filtrele)
- `GET /api/activity/user/{userId}` - Aktiviteleri getir
- `POST /api/social/follow` - Takip et/takipten çık

**Özellikler:**
- Profil bilgileri
- İstatistikler (grafiklerle)
- Kullanıcının listeleri
- "Takip Et" butonu
- Tab navigation (Listeler, Aktiviteler, İstatistikler)

---

##### **`components/modules/profile-settings/profile-settings.component.ts`**
**Amaç:** Profil ayarları

**Kullanılan Endpoint'ler:**
- `GET /api/user/me` - Kullanıcı bilgilerini getir
- `PUT /api/user/profile` - Profil güncelle
- `POST /api/user/upload-image` - Profil resmi yükle
- `POST /api/user/change-password` - Şifre değiştir
- `GET /api/mal/get-auth-url` - MAL bağlantı URL'i
- `POST /api/mal/callback` - MAL callback
- `DELETE /api/user` - Kullanıcı sil

**Özellikler:**
- Profil bilgileri düzenleme
- Profil resmi yükleme (FormData)
- Şifre değiştirme
- MAL bağlantısı yönetimi
- Hesap silme

---

##### **`components/modules/search/search.component.ts`**
**Amaç:** Anime arama

**Kullanılan Endpoint'ler:**
- `GET /api/search/anime` - Anime ara
- `GET /api/generate/genres` - Kategorileri getir (filtre için)

**Özellikler:**
- Arama çubuğu
- Filtreler (kategori, yıl, puan)
- Arama sonuçları grid'i
- Pagination
- "Listeye Ekle" butonu (her anime için)

---

##### **`components/modules/discover/discover.component.ts`**
**Amaç:** Public listeleri keşfetme

**Kullanılan Endpoint'ler:**
- `GET /api/share/public` - Public listeleri getir
- `GET /api/recommendation/trending` - Trending listeleri getir

**Özellikler:**
- Public listeler grid'i
- Filtreleme (kategori, mod, sıralama)
- Trending listeler
- Pagination

---

##### **`components/modules/notifications/notifications.component.ts`**
**Amaç:** Bildirimler

**Kullanılan Endpoint'ler:**
- `GET /api/social/notifications` - Bildirimleri getir
- `PUT /api/social/notifications/{notificationId}/read` - Okundu işaretle
- `PUT /api/social/notifications/read-all` - Tümünü okundu işaretle
- `DELETE /api/social/notification/{notificationId}` - Bildirim sil
- `DELETE /api/social/notifications/all` - Tümünü sil

**Özellikler:**
- Bildirim listesi
- Okunmamış bildirimler (vurgulu)
- "Tümünü Okundu İşaretle" butonu
- "Tümünü Sil" butonu

---

##### **`components/modules/templates/templates.component.ts`**
**Amaç:** Liste şablonları

**Kullanılan Endpoint'ler:**
- `GET /api/social/templates` - Şablonları getir
- `POST /api/social/template/create` - Şablon oluştur
- `POST /api/copy/list` - Listeyi kopyala (şablonu kullan)
- `DELETE /api/social/template/{templateId}` - Şablon sil

**Özellikler:**
- Şablon listesi
- "Kullan" butonu
- "Şablon Oluştur" butonu

---

##### **`components/modules/share-view/share-view.component.ts`**
**Amaç:** Paylaşılan liste görüntüleme

**Kullanılan Endpoint'ler:**
- `GET /api/share/public/{shareToken}` - Public listeyi getir
- `POST /api/social/like/{listId}` - Beğen (giriş yapılmışsa)
- `GET /api/comment/list/{listId}` - Yorumları getir
- `POST /api/comment/add` - Yorum ekle (giriş yapılmışsa)
- `POST /api/copy/list` - Listeyi kopyala (giriş yapılmışsa)

**Özellikler:**
- Liste görüntüleme (list-view ile aynı)
- "Kopyala" butonu
- Giriş yapılmamış kullanıcılar için "Giriş Yap" çağrısı

---

##### **`components/modules/mal-connect/mal-connect.component.ts`**
**Amaç:** MyAnimeList bağlantısı

**Kullanılan Endpoint'ler:**
- `GET /api/mal/get-auth-url` - Auth URL al
- `POST /api/mal/callback` - Callback işle
- `GET /api/mal/get-my-list` - Listeyi getir (test için)

**Özellikler:**
- MAL bağlantı durumu gösterimi
- "MAL Hesabını Bağla" butonu
- Bağlantı adımları açıklaması
- Bağlı ise MAL kullanıcı adı gösterimi

---

##### **`components/shared/navbar/navbar.component.ts`**
**Amaç:** Navigasyon menüsü

**Kullanılan Endpoint'ler:**
- `GET /api/social/notifications` - Okunmamış bildirim sayısı (badge için)

**Özellikler:**
- Logo
- Navigasyon menüsü
- Arama çubuğu
- Bildirim ikonu (badge ile)
- Profil dropdown
- Giriş durumu kontrolü
- Çıkış işlevi

---

##### **`components/shared/list-card/list-card.component.ts`**
**Amaç:** Liste kartı (yeniden kullanılabilir)

**Kullanılan Endpoint'ler:**
- Yok (sadece görüntüleme)

**Input'lar:**
- `list: AnimeList`
- `showActions?: boolean`

**Output'lar:**
- `onClick: EventEmitter`
- `onDelete: EventEmitter`
- `onShare: EventEmitter`

---

##### **`components/shared/tier-card/tier-card.component.ts`**
**Amaç:** Tier kartı (yeniden kullanılabilir)

**Kullanılan Endpoint'ler:**
- Yok (sadece görüntüleme)

**Input'lar:**
- `tier: Tier`
- `mode: ListMode`
- `editable?: boolean`

**Output'lar:**
- `onItemClick: EventEmitter`
- `onItemDelete: EventEmitter`
- `onTierUpdate: EventEmitter`

---

##### **`components/shared/anime-card/anime-card.component.ts`**
**Amaç:** Anime kartı (yeniden kullanılabilir)

**Kullanılan Endpoint'ler:**
- Yok (sadece görüntüleme)

**Input'lar:**
- `anime: RankedItemDto`
- `showRank?: boolean`
- `editable?: boolean`

**Output'lar:**
- `onClick: EventEmitter`
- `onDelete: EventEmitter`

---

##### **`components/shared/tier-maker/tier-maker.component.ts`**
**Amaç:** TierMaker tarzı görünüm (drag & drop)

**Kullanılan Endpoint'ler:**
- `POST /api/dragdrop/move-item` - Item taşı
- `POST /api/dragdrop/reorder-items` - Item sırala

**Özellikler:**
- Drag & drop ile tier'lar arası taşıma
- Tier içinde sıralama
- Renkli tier arka planları
- Animasyonlu geçişler

---

##### **`components/shared/comment-section/comment-section.component.ts`**
**Amaç:** Yorumlar bölümü

**Kullanılan Endpoint'ler:**
- `GET /api/comment/list/{listId}` - Yorumları getir
- `POST /api/comment/add` - Yorum ekle
- `PUT /api/comment/update` - Yorum güncelle
- `DELETE /api/comment/{commentId}` - Yorum sil

**Özellikler:**
- Yorum listesi
- Yorum ekleme formu
- Yorum düzenleme (sadece kendi yorumları)
- Yorum silme (sadece kendi yorumları)

---

##### **`components/shared/like-button/like-button.component.ts`**
**Amaç:** Beğeni butonu

**Kullanılan Endpoint'ler:**
- `POST /api/social/like/{listId}` - Beğen/takipten çık

**Özellikler:**
- Toggle butonu (beğen/beğenme)
- Beğeni sayısı gösterimi
- Animasyonlu ikon

---

##### **`components/shared/share-modal/share-modal.component.ts`**
**Amaç:** Paylaşım modalı

**Kullanılan Endpoint'ler:**
- `POST /api/share/generate-link` - Paylaşım linki oluştur
- `GET /api/export/embed/{listId}` - Embed kodu getir

**Özellikler:**
- Link kopyalama
- Embed kodu gösterimi
- Sosyal medya paylaşım butonları

---

#### 🔐 **Guard'lar**

##### **`core/guards/auth.guard.ts`**
**Amaç:** Route koruma (giriş yapmamış kullanıcıları engelle)

**Kullanım:**
```typescript
canActivate(): boolean {
  if (this.authService.isAuthenticated()) {
    return true;
  }
  this.router.navigate(['/login'], { queryParams: { returnUrl: this.route.url } });
  return false;
}
```

**Korunan Route'lar:**
- `/dashboard`
- `/list/create`
- `/list/edit/:id`
- `/profile`
- `/profile/settings`
- `/notifications`
- `/templates`

---

##### **`core/guards/guest.guard.ts`**
**Amaç:** Giriş yapmış kullanıcıları login/signin sayfalarından engelle

**Kullanım:**
```typescript
canActivate(): boolean {
  if (!this.authService.isAuthenticated()) {
    return true;
  }
  this.router.navigate(['/dashboard']);
  return false;
}
```

**Korunan Route'lar:**
- `/login`
- `/signin`

---

#### 🔧 **Pipe'lar**

##### **`core/pipes/date-format.pipe.ts`**
**Amaç:** Tarih formatlama

**Kullanım:**
```html
{{ createdAt | dateFormat:'dd/MM/yyyy' }}
```

---

##### **`core/pipes/truncate.pipe.ts`**
**Amaç:** Metin kısaltma

**Kullanım:**
```html
{{ longText | truncate:50 }}
```

---

##### **`core/pipes/safe-url.pipe.ts`**
**Amaç:** Güvenli URL oluşturma (file download için)

**Kullanım:**
```html
<img [src]="fileLink | safeUrl" />
```

---

#### 🎭 **Modal'lar**

##### **`core/modals/anime-search-modal/anime-search-modal.component.ts`**
**Amaç:** Anime arama modalı (liste düzenleme sayfasında)

**Kullanılan Endpoint'ler:**
- `GET /api/search/anime` - Anime ara
- `POST /api/list/check-duplicate` - Duplicate kontrol
- `POST /api/list/item/add` - Item ekle

**Özellikler:**
- Arama çubuğu
- Arama sonuçları listesi
- "Ekle" butonu (her anime için)
- Duplicate kontrolü

---

##### **`core/modals/tier-add-modal/tier-add-modal.component.ts`**
**Amaç:** Tier ekleme/düzenleme modalı

**Kullanılan Endpoint'ler:**
- `POST /api/list/tier/add` - Tier ekle
- `PUT /api/list/tier/update` - Tier güncelle

**Özellikler:**
- Tier ismi input
- Renk seçici (color picker)
- Sıra numarası input

---

#### 🛣️ **Routing Yapılandırması**

##### **`app.routes.ts`**
```typescript
export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'login', component: LoginComponent, canActivate: [GuestGuard] },
  { path: 'signin', component: SigninComponent, canActivate: [GuestGuard] },
  { path: 'dashboard', component: DashboardComponent, canActivate: [AuthGuard] },
  { path: 'list/create', component: ListCreateComponent, canActivate: [AuthGuard] },
  { path: 'list/edit/:id', component: ListEditComponent, canActivate: [AuthGuard] },
  { path: 'list/view/:id', component: ListViewComponent },
  { path: 'list/generate', component: ListGenerateComponent, canActivate: [AuthGuard] },
  { path: 'profile/:id', component: ProfileComponent },
  { path: 'profile/me', component: ProfileComponent, canActivate: [AuthGuard] },
  { path: 'profile/settings', component: ProfileSettingsComponent, canActivate: [AuthGuard] },
  { path: 'search', component: SearchComponent },
  { path: 'discover', component: DiscoverComponent },
  { path: 'notifications', component: NotificationsComponent, canActivate: [AuthGuard] },
  { path: 'templates', component: TemplatesComponent },
  { path: 'share/:token', component: ShareViewComponent },
  { path: 'mal/connect', component: MalConnectComponent, canActivate: [AuthGuard] },
  { path: '**', redirectTo: '' }
];
```

---

### 📦 **Bağımlılıklar (package.json)**

```json
{
  "dependencies": {
    "@angular/core": "^19.0.0",
    "@angular/common": "^19.0.0",
    "@angular/router": "^19.0.0",
    "@angular/forms": "^19.0.0",
    "@angular/common/http": "^19.0.0",
    "rxjs": "^7.8.0",
    "bootstrap": "^5.3.0",
    "@ng-bootstrap/ng-bootstrap": "^16.0.0",
    "sweetalert2": "^11.10.0",
    "js-sha1": "^0.6.0",
    "react-beautiful-dnd": "^13.1.1",
    "@angular/cdk": "^19.0.0",
    "@angular/animations": "^19.0.0"
  }
}
```

---

### 🎨 **Stil Yapılandırması**

#### **`styles/_variables.scss`**
```scss
// Renk paleti
$primary-color: #6366f1;
$secondary-color: #8b5cf6;
$accent-color: #ec4899;
$background-color: #f9fafb;
$text-color: #1f2937;

// Tier renkleri
$tier-s-color: #ff7f7f;
$tier-a-color: #ffbf7f;
$tier-b-color: #ffff7f;
$tier-c-color: #bfff7f;
$tier-d-color: #7fffff;
$tier-f-color: #bf7fff;
```

---

### 🔄 **Özet: Dosya-Endpoint Eşleştirmesi**

| Dosya | Kullanılan Endpoint'ler |
|-------|-------------------------|
| `login.component.ts` | `POST /api/auth/login` |
| `signin.component.ts` | `POST /api/auth/register` |
| `dashboard.component.ts` | `GET /api/list/all`, `GET /api/recommendation/anime`, `GET /api/recommendation/trending`, `GET /api/statistics/me` |
| `list-create.component.ts` | `POST /api/list/create` |
| `list-edit.component.ts` | `GET /api/list/{id}`, `PUT /api/list/save`, `POST /api/list/item/add`, `DELETE /api/list/item/remove`, `POST /api/list/tier/add`, `PUT /api/list/tier/update`, `DELETE /api/list/tier/remove`, `POST /api/list/check-duplicate`, `POST /api/list/convert-to-fusion`, `POST /api/list/convert-to-ranked`, `POST /api/dragdrop/move-item`, `POST /api/dragdrop/reorder-items`, `GET /api/search/anime` |
| `list-view.component.ts` | `GET /api/list/{id}`, `POST /api/social/like/{id}`, `GET /api/comment/list/{id}`, `POST /api/comment/add`, `POST /api/share/generate-link`, `GET /api/export/embed/{id}`, `POST /api/export/image/{id}` |
| `list-generate.component.ts` | `GET /api/generate/genres`, `POST /api/generate/by-score`, `POST /api/generate/by-year`, `POST /api/generate/by-genre` |
| `profile.component.ts` | `GET /api/user/{id}`, `GET /api/social/profile/{id}`, `GET /api/statistics/user/{id}`, `GET /api/list/all`, `GET /api/activity/user/{id}`, `POST /api/social/follow` |
| `profile-settings.component.ts` | `GET /api/user/me`, `PUT /api/user/profile`, `POST /api/user/upload-image`, `POST /api/user/change-password`, `GET /api/mal/get-auth-url`, `POST /api/mal/callback`, `DELETE /api/user` |
| `search.component.ts` | `GET /api/search/anime`, `GET /api/generate/genres` |
| `discover.component.ts` | `GET /api/share/public`, `GET /api/recommendation/trending` |
| `notifications.component.ts` | `GET /api/social/notifications`, `PUT /api/social/notifications/{id}/read`, `PUT /api/social/notifications/read-all`, `DELETE /api/social/notification/{id}`, `DELETE /api/social/notifications/all` |
| `templates.component.ts` | `GET /api/social/templates`, `POST /api/social/template/create`, `POST /api/copy/list`, `DELETE /api/social/template/{id}` |
| `share-view.component.ts` | `GET /api/share/public/{token}`, `POST /api/social/like/{id}`, `GET /api/comment/list/{id}`, `POST /api/comment/add`, `POST /api/copy/list` |
| `mal-connect.component.ts` | `GET /api/mal/get-auth-url`, `POST /api/mal/callback`, `GET /api/mal/get-my-list` |
| `navbar.component.ts` | `GET /api/social/notifications` (badge için) |
| `comment-section.component.ts` | `GET /api/comment/list/{id}`, `POST /api/comment/add`, `PUT /api/comment/update`, `DELETE /api/comment/{id}` |
| `like-button.component.ts` | `POST /api/social/like/{id}` |
| `share-modal.component.ts` | `POST /api/share/generate-link`, `GET /api/export/embed/{id}` |
| `anime-search-modal.component.ts` | `GET /api/search/anime`, `POST /api/list/check-duplicate`, `POST /api/list/item/add` |
| `tier-maker.component.ts` | `POST /api/dragdrop/move-item`, `POST /api/dragdrop/reorder-items` |

---

## Genel Bilgiler

### Base URL
```
https://localhost:7123/api
```

### Authentication
Çoğu endpoint için `token` header'ında JWT token gönderilmesi gerekmektedir:
```javascript
headers: {
  'Token': 'your-jwt-token-here',
  'Content-Type': 'application/json'
}
```

### Response Format
Tüm endpoint'ler `BaseResponse` formatında döner:
```typescript
interface BaseResponse {
  success: boolean;
  message: string;
  errorCode?: number;
  response?: any;
  userId?: number;
}
```

---

## 1. Kimlik Doğrulama (Authentication)

### 1.1. Kullanıcı Kaydı
**Endpoint:** `POST /api/auth/register`

**Request Body:**
```typescript
{
  username: string;      // 3-50 karakter
  password: string;      // Minimum 6 karakter
}
```

**Response:**
```typescript
{
  success: true,
  message: "Kullanıcı başarıyla kaydedildi.",
  response: {
    token: string;      // JWT token - localStorage'a kaydet
    userId: number;
  }
}
```

**Frontend Kullanımı:**
```javascript
// Register sayfasında
const register = async (username, password) => {
  const response = await fetch('https://localhost:7123/api/auth/register', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ username, password })
  });
  
  const data = await response.json();
  if (data.success) {
    localStorage.setItem('token', data.response.token);
    localStorage.setItem('userId', data.response.userId);
    // Ana sayfaya yönlendir
  }
};
```

---

### 1.2. Kullanıcı Girişi
**Endpoint:** `POST /api/auth/login`

**Request Body:**
```typescript
{
  username: string;
  password: string;
}
```

**Response:**
```typescript
{
  success: true,
  message: "Giriş başarılı.",
  response: {
    token: string;      // JWT token
    userId: number;
  }
}
```

**Frontend Kullanımı:**
```javascript
// Login sayfasında
const login = async (username, password) => {
  const response = await fetch('https://localhost:7123/api/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ username, password })
  });
  
  const data = await response.json();
  if (data.success) {
    localStorage.setItem('token', data.response.token);
    // Dashboard'a yönlendir
  }
};
```

---

## 2. MyAnimeList Entegrasyonu

### 2.1. MAL Auth URL Al
**Endpoint:** `GET /api/mal/get-auth-url`

**Headers:**
```
Token: your-jwt-token
```

**Response:**
```typescript
{
  success: true,
  response: {
    authUrl: string;        // Kullanıcıyı bu URL'ye yönlendir
    codeVerifier: string;   // localStorage'a kaydet (callback için gerekli)
  }
}
```

**Frontend Kullanımı:**
```javascript
// MAL bağla butonuna tıklandığında
const connectMAL = async () => {
  const token = localStorage.getItem('token');
  const response = await fetch('https://localhost:7123/api/mal/get-auth-url', {
    headers: { 'Token': token }
  });
  
  const data = await response.json();
  if (data.success) {
    localStorage.setItem('mal_code_verifier', data.response.codeVerifier);
    // Kullanıcıyı data.response.authUrl'e yönlendir
    window.location.href = data.response.authUrl;
  }
};
```

---

### 2.2. MAL Callback İşle
**Endpoint:** `POST /api/mal/callback`

**Headers:**
```
Token: your-jwt-token
```

**Request Body:**
```typescript
{
  code: string;           // MAL'den dönen authorization code
  codeVerifier: string;   // localStorage'dan alınan codeVerifier
}
```

**Response:**
```typescript
{
  success: true,
  message: "MAL hesabı başarıyla bağlandı."
}
```

**Frontend Kullanımı:**
```javascript
// MAL callback sayfasında (URL'den code'u al)
const handleMALCallback = async () => {
  const urlParams = new URLSearchParams(window.location.search);
  const code = urlParams.get('code');
  const codeVerifier = localStorage.getItem('mal_code_verifier');
  
  const token = localStorage.getItem('token');
  const response = await fetch('https://localhost:7123/api/mal/callback', {
    method: 'POST',
    headers: {
      'Token': token,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ code, codeVerifier })
  });
  
  const data = await response.json();
  if (data.success) {
    // Başarı mesajı göster ve dashboard'a yönlendir
  }
};
```

---

### 2.3. MAL Listemi Getir
**Endpoint:** `GET /api/mal/get-my-list`

**Headers:**
```
Token: your-jwt-token
```

**Response:**
```typescript
{
  success: true,
  response: {
    data: Array<{
      node: {
        id: number;           // MAL anime ID
        title: string;
        main_picture: {
          medium: string;
          large: string;
        };
      };
      list_status: {
        status: string;       // "completed", "watching", "plan_to_watch", etc.
        score: number;        // 0-10
      };
    }>;
  }
}
```

**Frontend Kullanımı:**
```javascript
// MAL listesini göster
const getMALList = async () => {
  const token = localStorage.getItem('token');
  const response = await fetch('https://localhost:7123/api/mal/get-my-list', {
    headers: { 'Token': token }
  });
  
  const data = await response.json();
  if (data.success) {
    const completedAnimes = data.response.data.filter(
      item => item.list_status.status === 'completed'
    );
    // Listeyi göster
  }
};
```

---

## 3. Anime Listesi Yönetimi

### 3.1. Liste Oluştur
**Endpoint:** `POST /api/list/create`

**Headers:**
```
Token: your-jwt-token
```

**Request Body:**
```typescript
{
  title: string;           // Liste başlığı (max 100 karakter)
  mode: "Ranked" | "Tiered" | "Fusion";
  tiers?: Array<{          // Opsiyonel - verilmezse varsayılan tier'lar oluşturulur
    title: string;         // Tier ismi (max 50 karakter)
    color: string;        // Hex renk kodu (örn: "#FF0000")
    order: number;        // Tier sırası (0, 1, 2...)
  }>;
}
```

**Response:**
```typescript
{
  success: true,
  response: {
    listId: number;
  }
}
```

**Frontend Kullanımı:**
```javascript
// Yeni liste oluştur
const createList = async (title, mode, customTiers = null) => {
  const token = localStorage.getItem('token');
  const body = { title, mode };
  
  if (customTiers) {
    body.tiers = customTiers;
  }
  
  const response = await fetch('https://localhost:7123/api/list/create', {
    method: 'POST',
    headers: {
      'Token': token,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(body)
  });
  
  const data = await response.json();
  if (data.success) {
    // Liste düzenleme sayfasına yönlendir
    router.push(`/list/${data.response.listId}/edit`);
  }
};
```

---

### 3.2. Liste Kaydet
**Endpoint:** `PUT /api/list/save`

**Headers:**
```
Token: your-jwt-token
```

**Request Body:**
```typescript
{
  listId: number;
  tiers: Array<{
    title: string;
    color: string;
    order: number;
    items: Array<{
      animeMalId: number;   // MAL anime ID
      rankInTier: number;   // Tier içindeki sıra (1, 2, 3...)
    }>;
  }>;
}
```

**Frontend Kullanımı:**
```javascript
// Liste düzenleme sayfasında "Kaydet" butonuna tıklandığında
const saveList = async (listId, tiers) => {
  const token = localStorage.getItem('token');
  const response = await fetch('https://localhost:7123/api/list/save', {
    method: 'PUT',
    headers: {
      'Token': token,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ listId, tiers })
  });
  
  const data = await response.json();
  if (data.success) {
    // Başarı mesajı göster
  }
};
```

---

### 3.3. Liste Getir
**Endpoint:** `GET /api/list/{listId}`

**Headers:**
```
Token: your-jwt-token
```

**Response:**
```typescript
{
  success: true,
  response: {
    id: number;
    title: string;
    mode: string;
    tiers: Array<{
      id: number;
      title: string;
      color: string;
      order: number;
      items: Array<{
        id: number;
        animeMalId: number;
        rankInTier: number;
        title: string;        // Jikan'dan çekilen anime adı
        imageUrl: string;     // Jikan'dan çekilen resim URL'i
      }>;
    }>;
  }
}
```

**Frontend Kullanımı:**
```javascript
// Liste detay sayfasında
const getList = async (listId) => {
  const token = localStorage.getItem('token');
  const response = await fetch(`https://localhost:7123/api/list/${listId}`, {
    headers: { 'Token': token }
  });
  
  const data = await response.json();
  if (data.success) {
    // Listeyi render et
    renderList(data.response);
  }
};
```

---

### 3.4. Tüm Listelerimi Getir
**Endpoint:** `GET /api/list/all`

**Headers:**
```
Token: your-jwt-token
```

**Response:**
```typescript
{
  success: true,
  response: Array<{
    id: number;
    title: string;
    mode: string;
    createdAt: string;
    modTime: string;
    tierCount: number;
    itemCount: number;
  }>;
}
```

**Frontend Kullanımı:**
```javascript
// Dashboard'da listeleri göster
const getAllLists = async () => {
  const token = localStorage.getItem('token');
  const response = await fetch('https://localhost:7123/api/list/all', {
    headers: { 'Token': token }
  });
  
  const data = await response.json();
  if (data.success) {
    // Listeleri grid/liste olarak göster
    displayLists(data.response);
  }
};
```

---

### 3.5. Tiered → Fusion Dönüştür
**Endpoint:** `POST /api/list/convert-to-fusion`

**Headers:**
```
Token: your-jwt-token
```

**Request Body:**
```typescript
{
  listId: number;
}
```

**Frontend Kullanımı:**
```javascript
// Liste düzenleme sayfasında "Fusion Moduna Çevir" butonuna tıklandığında
const convertToFusion = async (listId) => {
  const token = localStorage.getItem('token');
  const response = await fetch('https://localhost:7123/api/list/convert-to-fusion', {
    method: 'POST',
    headers: {
      'Token': token,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ listId })
  });
  
  const data = await response.json();
  if (data.success) {
    // Sayfayı yenile veya listeyi tekrar getir
    window.location.reload();
  }
};
```

---

### 3.6. Tiered/Fusion → Ranked Dönüştür
**Endpoint:** `POST /api/list/convert-to-ranked`

**Headers:**
```
Token: your-jwt-token
```

**Request Body:**
```typescript
{
  listId: number;
}
```

**Frontend Kullanımı:**
```javascript
// Liste düzenleme sayfasında "Ranked Moduna Çevir" butonuna tıklandığında
const convertToRanked = async (listId) => {
  const token = localStorage.getItem('token');
  const response = await fetch('https://localhost:7123/api/list/convert-to-ranked', {
    method: 'POST',
    headers: {
      'Token': token,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ listId })
  });
  
  const data = await response.json();
  if (data.success) {
    // Sayfayı yenile
    window.location.reload();
  }
};
```

---

### 3.7. Liste Sil
**Endpoint:** `DELETE /api/list/{listId}`

**Headers:**
```
Token: your-jwt-token
```

**Frontend Kullanımı:**
```javascript
// Liste silme butonuna tıklandığında
const deleteList = async (listId) => {
  if (!confirm('Listeyi silmek istediğinize emin misiniz?')) return;
  
  const token = localStorage.getItem('token');
  const response = await fetch(`https://localhost:7123/api/list/${listId}`, {
    method: 'DELETE',
    headers: { 'Token': token }
  });
  
  const data = await response.json();
  if (data.success) {
    // Dashboard'a yönlendir
    router.push('/dashboard');
  }
};
```

---

### 3.8. Liste Başlığını Güncelle
**Endpoint:** `PATCH /api/list/{listId}/title`

**Headers:**
```
Token: your-jwt-token
```

**Request Body:**
```typescript
{
  listId: number;
  title: string;
}
```

---

### 3.9. Item Ekle
**Endpoint:** `POST /api/list/item/add`

**Headers:**
```
Token: your-jwt-token
```

**Request Body:**
```typescript
{
  listId: number;
  tierId: number;
  animeMalId: number;
  rankInTier?: number;    // Opsiyonel - belirtilmezse sona eklenir
}
```

**Frontend Kullanımı:**
```javascript
// Anime arama sonuçlarından bir animeyi listeye ekle
const addItem = async (listId, tierId, animeMalId, rankInTier = null) => {
  const token = localStorage.getItem('token');
  const body = { listId, tierId, animeMalId };
  if (rankInTier) body.rankInTier = rankInTier;
  
  const response = await fetch('https://localhost:7123/api/list/item/add', {
    method: 'POST',
    headers: {
      'Token': token,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(body)
  });
  
  const data = await response.json();
  if (data.success) {
    // Listeyi yeniden getir
    await getList(listId);
  }
};
```

---

### 3.10. Item Sil
**Endpoint:** `DELETE /api/list/item/remove`

**Headers:**
```
Token: your-jwt-token
```

**Request Body:**
```typescript
{
  itemId: number;
}
```

---

### 3.11. Tier Ekle
**Endpoint:** `POST /api/list/tier/add`

**Headers:**
```
Token: your-jwt-token
```

**Request Body:**
```typescript
{
  listId: number;
  title: string;          // Max 50 karakter
  color: string;          // Hex renk (varsayılan: "#FFFFFF")
  order?: number;         // Opsiyonel - belirtilmezse sona eklenir
}
```

---

### 3.12. Tier Güncelle
**Endpoint:** `PUT /api/list/tier/update`

**Headers:**
```
Token: your-jwt-token
```

**Request Body:**
```typescript
{
  tierId: number;
  title?: string;
  color?: string;
  order?: number;
}
```

---

### 3.13. Tier Sil
**Endpoint:** `DELETE /api/list/tier/remove`

**Headers:**
```
Token: your-jwt-token
```

**Request Body:**
```typescript
{
  tierId: number;
  moveItemsToTierId?: number;  // Opsiyonel - silinen tier'daki item'ları taşı
}
```

---

### 3.14. Duplicate Kontrol
**Endpoint:** `POST /api/list/check-duplicate`

**Headers:**
```
Token: your-jwt-token
```

**Request Body:**
```typescript
{
  listId: number;
  animeMalId: number;
}
```

**Response:**
```typescript
{
  success: true,
  response: {
    isDuplicate: boolean;
    existingItemId?: number;
    existingTierId?: number;
    existingTierTitle?: string;
  }
}
```

**Frontend Kullanımı:**
```javascript
// Anime eklemeden önce duplicate kontrol et
const checkDuplicate = async (listId, animeMalId) => {
  const token = localStorage.getItem('token');
  const response = await fetch('https://localhost:7123/api/list/check-duplicate', {
    method: 'POST',
    headers: {
      'Token': token,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ listId, animeMalId })
  });
  
  const data = await response.json();
  if (data.success && data.response.isDuplicate) {
    alert(`Bu anime zaten "${data.response.existingTierTitle}" tier'ında mevcut!`);
    return false;
  }
  return true;
};
```

---

### 3.15. Liste İstatistikleri
**Endpoint:** `GET /api/list/{listId}/statistics`

**Headers:**
```
Token: your-jwt-token
```

**Response:**
```typescript
{
  success: true,
  response: {
    totalItems: number;
    totalTiers: number;
    uniqueAnimeCount: number;
    createdAt: string;
    lastModified: string;
    itemsPerTier: { [tierTitle: string]: number };
  }
}
```

---

### 3.16. Rank Değiştir (Swap)
**Endpoint:** `POST /api/list/swap-ranks`

**Headers:**
```
Token: your-jwt-token
```

**Request Body:**
```typescript
{
  listId: number;
  itemId1: number;
  itemId2: number;
}
```

---

### 3.17. Rank Sıfırla
**Endpoint:** `POST /api/list/reset-ranks`

**Headers:**
```
Token: your-jwt-token
```

**Request Body:**
```typescript
{
  tierId: number;
}
```

---

### 3.18. Toplu Item Ekle
**Endpoint:** `POST /api/list/bulk-add-items`

**Headers:**
```
Token: your-jwt-token
```

**Request Body:**
```typescript
{
  listId: number;
  tierId: number;
  animeMalIds: number[];
  skipDuplicates: boolean;  // Varsayılan: true
}
```

---

### 3.19. Toplu Item Sil
**Endpoint:** `POST /api/list/bulk-remove-items`

**Headers:**
```
Token: your-jwt-token
```

**Request Body:**
```typescript
{
  listId: number;
  itemIds: number[];
}
```

---

## 4. Otomatik Liste Oluşturma

### 4.1. Puana Göre Liste Oluştur (Create Sort List)
**Endpoint:** `POST /api/generate/by-score`

**Headers:**
```
Token: your-jwt-token
```

**Response:**
```typescript
{
  success: true,
  response: {
    listId: number;
    title: string;
  }
}
```

**Frontend Kullanımı:**
```javascript
// "Create Sort List" butonuna tıklandığında
const generateByScore = async () => {
  const token = localStorage.getItem('token');
  const response = await fetch('https://localhost:7123/api/generate/by-score', {
    method: 'POST',
    headers: { 'Token': token }
  });
  
  const data = await response.json();
  if (data.success) {
    // Yeni oluşturulan listeyi göster
    router.push(`/list/${data.response.listId}`);
  }
};
```

**Not:** Sadece "completed" animeler kullanılır. Aynı puandaki animeler alfabetik sıralanır.

---

### 4.2. Yıla Göre Liste Oluştur (Create Year List)
**Endpoint:** `POST /api/generate/by-year`

**Headers:**
```
Token: your-jwt-token
```

**Response:**
```typescript
{
  success: true,
  response: {
    listId: number;
    title: string;
  }
}
```

**Frontend Kullanımı:**
```javascript
// "Create Year List" butonuna tıklandığında
const generateByYear = async () => {
  const token = localStorage.getItem('token');
  const response = await fetch('https://localhost:7123/api/generate/by-year', {
    method: 'POST',
    headers: { 'Token': token }
  });
  
  const data = await response.json();
  if (data.success) {
    router.push(`/list/${data.response.listId}`);
  }
};
```

**Not:** Yıllar TierMaker tarzı kategorilere çevrilir. Sadece "completed" animeler kullanılır.

---

### 4.3. Kategoriye Göre Liste Oluştur (Create Genre List)
**Endpoint:** `POST /api/generate/by-genre`

**Headers:**
```
Token: your-jwt-token
```

**Request Body:**
```typescript
{
  genreTag: string;  // Örn: "Comedy", "Action", "Romance"
}
```

**Response:**
```typescript
{
  success: true,
  response: {
    listId: number;
    title: string;
  }
}
```

**Frontend Kullanımı:**
```javascript
// Kategori seçildikten sonra "Create Genre List" butonuna tıklandığında
const generateByGenre = async (genreTag) => {
  const token = localStorage.getItem('token');
  const response = await fetch('https://localhost:7123/api/generate/by-genre', {
    method: 'POST',
    headers: {
      'Token': token,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ genreTag })
  });
  
  const data = await response.json();
  if (data.success) {
    router.push(`/list/${data.response.listId}`);
  }
};
```

**Not:** Puanlara göre tier'lar oluşturulur (10 Puan, 9 Puan, 8 Puan...). Sadece "completed" animeler kullanılır.

---

### 4.4. Kategorileri Getir
**Endpoint:** `GET /api/generate/genres`

**Response:**
```typescript
{
  success: true,
  response: string[];  // ["Action", "Adventure", "Comedy", ...]
}
```

**Frontend Kullanımı:**
```javascript
// Kategori seçim dropdown'ını doldur
const getGenres = async () => {
  const response = await fetch('https://localhost:7123/api/generate/genres');
  const data = await response.json();
  if (data.success) {
    // Dropdown'ı doldur
    populateGenreDropdown(data.response);
  }
};
```

---

## 5. Arama ve Keşif

### 5.1. Anime Ara
**Endpoint:** `GET /api/search/anime` veya `POST /api/search/anime`

**Query Parameters (GET):**
```
query?: string
genre?: string
year?: number
minScore?: number
maxScore?: number
page?: number (varsayılan: 1)
limit?: number (varsayılan: 25)
```

**Request Body (POST):**
```typescript
{
  query?: string;
  genre?: string;
  year?: number;
  minScore?: number;
  maxScore?: number;
  page?: number;
  limit?: number;
}
```

**Response:**
```typescript
{
  success: true,
  response: {
    results: Array<{
      malId: number;
      title: string;
      imageUrl: string;
      year?: number;
      genres: string[];
    }>;
    totalCount: number;
    page: number;
    limit: number;
  }
}
```

**Frontend Kullanımı:**
```javascript
// Arama sayfasında
const searchAnime = async (query, filters = {}) => {
  const params = new URLSearchParams({
    query: query || '',
    page: filters.page || 1,
    limit: filters.limit || 25,
    ...filters
  });
  
  const response = await fetch(`https://localhost:7123/api/search/anime?${params}`);
  const data = await response.json();
  if (data.success) {
    // Arama sonuçlarını göster
    displaySearchResults(data.response.results);
  }
};
```

---

## 6. Paylaşım Özellikleri

### 6.1. Liste Görünürlüğünü Ayarla
**Endpoint:** `POST /api/share/set-visibility`

**Headers:**
```
Token: your-jwt-token
```

**Request Body:**
```typescript
{
  listId: number;
  isPublic: boolean;
}
```

**Frontend Kullanımı:**
```javascript
// Liste ayarları sayfasında "Public/Private" toggle
const setVisibility = async (listId, isPublic) => {
  const token = localStorage.getItem('token');
  const response = await fetch('https://localhost:7123/api/share/set-visibility', {
    method: 'POST',
    headers: {
      'Token': token,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ listId, isPublic })
  });
  
  const data = await response.json();
  if (data.success) {
    // Başarı mesajı göster
  }
};
```

---

### 6.2. Paylaşım Linki Oluştur
**Endpoint:** `POST /api/share/generate-link`

**Headers:**
```
Token: your-jwt-token
```

**Request Body:**
```typescript
{
  listId: number;
}
```

**Response:**
```typescript
{
  success: true,
  response: {
    shareUrl: string;      // Paylaşım URL'i
    shareToken: string;    // Unique token
  }
}
```

**Frontend Kullanımı:**
```javascript
// "Paylaş" butonuna tıklandığında
const generateShareLink = async (listId) => {
  const token = localStorage.getItem('token');
  const response = await fetch('https://localhost:7123/api/share/generate-link', {
    method: 'POST',
    headers: {
      'Token': token,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ listId })
  });
  
  const data = await response.json();
  if (data.success) {
    // Linki kopyala butonu göster
    copyToClipboard(data.response.shareUrl);
  }
};
```

---

### 6.3. Public Liste Getir (Share Token ile)
**Endpoint:** `GET /api/share/public/{shareToken}`

**Headers (Opsiyonel):**
```
Token: your-jwt-token  // Giriş yapmış kullanıcı için (beğenme durumunu görmek için)
```

**Response:**
```typescript
{
  success: true,
  response: {
    list: {
      id: number;
      title: string;
      mode: string;
      tiers: Array<...>;
    };
    publicInfo: {
      id: number;
      title: string;
      authorUsername: string;
      authorId: number;
      viewCount: number;
      likeCount: number;
      createdAt: string;
      isLiked: boolean;  // Kullanıcı beğenmiş mi?
    };
  }
}
```

**Frontend Kullanımı:**
```javascript
// Paylaşım sayfasında (public view)
const getPublicList = async (shareToken) => {
  const token = localStorage.getItem('token'); // Opsiyonel
  const headers = token ? { 'Token': token } : {};
  
  const response = await fetch(`https://localhost:7123/api/share/public/${shareToken}`, {
    headers
  });
  
  const data = await response.json();
  if (data.success) {
    // Public listeyi göster
    renderPublicList(data.response);
  }
};
```

---

### 6.4. Public Listeleri Getir
**Endpoint:** `GET /api/share/public?page=1&limit=20`

**Response:**
```typescript
{
  success: true,
  response: {
    lists: Array<{
      id: number;
      title: string;
      mode: string;
      authorUsername: string;
      authorId: number;
      viewCount: number;
      likeCount: number;
      createdAt: string;
      isLiked: boolean;
    }>;
    totalCount: number;
    page: number;
    totalPages: number;
  }
}
```

**Frontend Kullanımı:**
```javascript
// Keşfet sayfasında public listeleri göster
const getPublicLists = async (page = 1) => {
  const response = await fetch(`https://localhost:7123/api/share/public?page=${page}&limit=20`);
  const data = await response.json();
  if (data.success) {
    // Public listeleri göster
    displayPublicLists(data.response.lists);
  }
};
```

---

### 6.5. Paylaşım Linkini Sil
**Endpoint:** `DELETE /api/share/link/{listId}`

**Headers:**
```
Token: your-jwt-token
```

**Frontend Kullanımı:**
```javascript
// "Paylaşımı Kaldır" butonuna tıklandığında
const deleteShareLink = async (listId) => {
  const token = localStorage.getItem('token');
  const response = await fetch(`https://localhost:7123/api/share/link/${listId}`, {
    method: 'DELETE',
    headers: { 'Token': token }
  });
  
  const data = await response.json();
  if (data.success) {
    // Liste artık private oldu
  }
};
```

---

## 7. Sosyal Özellikler

### 7.1. Liste Beğen
**Endpoint:** `POST /api/social/like/{listId}`

**Headers:**
```
Token: your-jwt-token
```

**Response:**
```typescript
{
  success: true,
  message: "Liste beğenildi." veya "Beğeni kaldırıldı."
}
```

**Frontend Kullanımı:**
```javascript
// Beğen butonuna tıklandığında (toggle)
const likeList = async (listId) => {
  const token = localStorage.getItem('token');
  const response = await fetch(`https://localhost:7123/api/social/like/${listId}`, {
    method: 'POST',
    headers: { 'Token': token }
  });
  
  const data = await response.json();
  if (data.success) {
    // Beğeni sayısını güncelle
    updateLikeCount(listId);
  }
};
```

---

### 7.2. Kullanıcı Takip Et
**Endpoint:** `POST /api/social/follow`

**Headers:**
```
Token: your-jwt-token
```

**Request Body:**
```typescript
{
  userId: number;
}
```

**Response:**
```typescript
{
  success: true,
  message: "Kullanıcı takip edildi." veya "Kullanıcı takipten çıkarıldı."
}
```

**Frontend Kullanımı:**
```javascript
// Profil sayfasında "Takip Et" butonuna tıklandığında
const followUser = async (userId) => {
  const token = localStorage.getItem('token');
  const response = await fetch('https://localhost:7123/api/social/follow', {
    method: 'POST',
    headers: {
      'Token': token,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ userId })
  });
  
  const data = await response.json();
  if (data.success) {
    // Takip durumunu güncelle
    updateFollowStatus(userId);
  }
};
```

---

### 7.3. Kullanıcı Profili Getir
**Endpoint:** `GET /api/social/profile/{userId}`

**Headers (Opsiyonel):**
```
Token: your-jwt-token
```

**Response:**
```typescript
{
  success: true,
  response: {
    id: number;
    username: string;
    userImageLink?: string;
    malUsername?: string;
    totalLists: number;
    totalFollowers: number;
    totalFollowing: number;
    isFollowing: boolean;  // Kullanıcı bu profili takip ediyor mu?
    isOwnProfile: boolean; // Kendi profili mi?
  }
}
```

---

### 7.4. Bildirimleri Getir
**Endpoint:** `GET /api/social/notifications?page=1&limit=20`

**Headers:**
```
Token: your-jwt-token
```

**Response:**
```typescript
{
  success: true,
  response: {
    notifications: Array<{
      id: number;
      type: string;  // "like", "comment", "follow", "mention"
      message: string;
      relatedListId?: number;
      relatedUserId?: number;
      relatedUsername?: string;
      isRead: boolean;
      createdAt: string;
    }>;
    unreadCount: number;
    page: number;
    totalPages: number;
  }
}
```

**Frontend Kullanımı:**
```javascript
// Bildirimler dropdown'ında
const getNotifications = async (page = 1) => {
  const token = localStorage.getItem('token');
  const response = await fetch(`https://localhost:7123/api/social/notifications?page=${page}&limit=20`, {
    headers: { 'Token': token }
  });
  
  const data = await response.json();
  if (data.success) {
    // Bildirimleri göster
    displayNotifications(data.response.notifications);
    // Okunmamış sayısını göster
    updateUnreadBadge(data.response.unreadCount);
  }
};
```

---

### 7.5. Bildirimi Okundu İşaretle
**Endpoint:** `PUT /api/social/notifications/{notificationId}/read`

**Headers:**
```
Token: your-jwt-token
```

---

### 7.6. Tüm Bildirimleri Okundu İşaretle
**Endpoint:** `PUT /api/social/notifications/read-all`

**Headers:**
```
Token: your-jwt-token
```

---

### 7.7. Bildirim Sil
**Endpoint:** `DELETE /api/social/notification/{notificationId}`

**Headers:**
```
Token: your-jwt-token
```

---

### 7.8. Tüm Bildirimleri Sil
**Endpoint:** `DELETE /api/social/notifications/all`

**Headers:**
```
Token: your-jwt-token
```

---

### 7.9. Şablon Oluştur
**Endpoint:** `POST /api/social/template/create`

**Headers:**
```
Token: your-jwt-token
```

**Request Body:**
```typescript
{
  listId: number;
}
```

**Frontend Kullanımı:**
```javascript
// Liste ayarları sayfasında "Şablon Oluştur" butonuna tıklandığında
const createTemplate = async (listId) => {
  const token = localStorage.getItem('token');
  const response = await fetch('https://localhost:7123/api/social/template/create', {
    method: 'POST',
    headers: {
      'Token': token,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ listId })
  });
  
  const data = await response.json();
  if (data.success) {
    alert('Şablon başarıyla oluşturuldu!');
  }
};
```

---

### 7.10. Şablonları Getir
**Endpoint:** `GET /api/social/templates?page=1&limit=20`

**Response:**
```typescript
{
  success: true,
  response: Array<{
    id: number;
    title: string;
    mode: string;
    authorUsername: string;
    useCount: number;
    createdAt: string;
  }>;
}
```

---

### 7.11. Şablon Sil
**Endpoint:** `DELETE /api/social/template/{templateId}`

**Headers:**
```
Token: your-jwt-token
```

---

## 8. Yorumlar

### 8.1. Yorum Ekle
**Endpoint:** `POST /api/comment/add`

**Headers:**
```
Token: your-jwt-token
```

**Request Body:**
```typescript
{
  listId: number;
  content: string;  // 1-500 karakter
}
```

**Response:**
```typescript
{
  success: true,
  response: {
    id: number;
    listId: number;
    userId: number;
    username: string;
    content: string;
    createdAt: string;
    modTime: string;
  }
}
```

**Frontend Kullanımı:**
```javascript
// Yorum formunda "Gönder" butonuna tıklandığında
const addComment = async (listId, content) => {
  const token = localStorage.getItem('token');
  const response = await fetch('https://localhost:7123/api/comment/add', {
    method: 'POST',
    headers: {
      'Token': token,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ listId, content })
  });
  
  const data = await response.json();
  if (data.success) {
    // Yorumları yeniden getir
    await getComments(listId);
  }
};
```

---

### 8.2. Yorumları Getir
**Endpoint:** `GET /api/comment/list/{listId}`

**Headers (Opsiyonel):**
```
Token: your-jwt-token
```

**Response:**
```typescript
{
  success: true,
  response: Array<{
    id: number;
    listId: number;
    userId: number;
    username: string;
    content: string;
    createdAt: string;
    modTime?: string;
  }>;
}
```

---

### 8.3. Yorum Güncelle
**Endpoint:** `PUT /api/comment/update`

**Headers:**
```
Token: your-jwt-token
```

**Request Body:**
```typescript
{
  commentId: number;
  content: string;
}
```

---

### 8.4. Yorum Sil
**Endpoint:** `DELETE /api/comment/{commentId}`

**Headers:**
```
Token: your-jwt-token
```

---

## 9. Kullanıcı Yönetimi

### 9.1. Profil Resmi Yükle
**Endpoint:** `POST /api/user/upload-image`

**Headers:**
```
Token: your-jwt-token
Content-Type: multipart/form-data
```

**Request Body (FormData):**
```javascript
const formData = new FormData();
formData.append('file', file); // File object
```

**Response:**
```typescript
{
  success: true,
  response: {
    imageLink: string;  // Güvenli download linki
    fileName: string;
  }
}
```

**Frontend Kullanımı:**
```javascript
// Profil ayarları sayfasında
const uploadProfileImage = async (file) => {
  const token = localStorage.getItem('token');
  const formData = new FormData();
  formData.append('file', file);
  
  const response = await fetch('https://localhost:7123/api/user/upload-image', {
    method: 'POST',
    headers: { 'Token': token },
    body: formData
  });
  
  const data = await response.json();
  if (data.success) {
    // Profil resmini güncelle
    updateProfileImage(data.response.imageLink);
  }
};
```

---

### 9.2. Kullanıcı Bilgilerini Getir
**Endpoint:** `GET /api/user/{userId}`

**Headers (Opsiyonel):**
```
Token: your-jwt-token
```

**Response:**
```typescript
{
  success: true,
  response: {
    id: number;
    username: string;
    role: string;
    malUsername?: string;
    userImageLink?: string;  // Güvenli download linki (token varsa)
    modTime: string;
  }
}
```

---

### 9.3. Kendi Profilimi Getir
**Endpoint:** `GET /api/user/me`

**Headers:**
```
Token: your-jwt-token
```

---

### 9.4. Tüm Kullanıcıları Getir
**Endpoint:** `GET /api/user/all?page=1&limit=20&searchQuery=&isActive=`

**Query Parameters:**
```
page?: number
limit?: number
searchQuery?: string
isActive?: boolean
```

---

### 9.5. Kullanıcı Ara
**Endpoint:** `POST /api/user/search`

**Headers (Opsiyonel):**
```
Token: your-jwt-token
```

**Request Body:**
```typescript
{
  query: string;
  page?: number;
  limit?: number;
}
```

---

### 9.6. Kullanıcı Bilgilerini Güncelle
**Endpoint:** `PUT /api/user/update`

**Headers:**
```
Token: your-jwt-token
```

**Request Body:**
```typescript
{
  username?: string;      // 3-50 karakter
  malUsername?: string;
}
```

---

### 9.7. Şifre Değiştir
**Endpoint:** `POST /api/user/change-password`

**Headers:**
```
Token: your-jwt-token
```

**Request Body:**
```typescript
{
  currentPassword: string;
  newPassword: string;    // Minimum 6 karakter
}
```

---

### 9.8. Profil Güncelle
**Endpoint:** `PUT /api/user/profile`

**Headers:**
```
Token: your-jwt-token
```

**Request Body:**
```typescript
{
  username?: string;
  malUsername?: string;
}
```

---

### 9.9. Kullanıcı Sil
**Endpoint:** `DELETE /api/user`

**Headers:**
```
Token: your-jwt-token
```

**Request Body:**
```typescript
{
  userId: number;
  hardDelete: boolean;   // Varsayılan: false (soft delete)
  password?: string;      // Hard delete için şifre doğrulaması
}
```

---

## 10. Dosya Yönetimi

### 10.1. Dosya İndir (Güvenli Link)
**Endpoint:** `GET /api/file/download?filename={filename}&type={type}&sessionno={sessionno}&signature={signature}`

**Query Parameters:**
```
filename: string
type: number        // FileType enum (0: User, 1: Export, 2: Temp)
sessionno: string   // User ID
signature: string   // Güvenlik imzası
```

**Not:** Bu endpoint genellikle backend tarafından oluşturulan güvenli linkler üzerinden kullanılır. Frontend'de direkt kullanmak yerine, `FileService.GenerateFileLink` ile oluşturulan linkleri kullanın.

---

### 10.2. Dosya Bilgilerini Getir
**Endpoint:** `GET /api/file/info?filename={filename}&type={type}`

**Headers (Opsiyonel):**
```
Token: your-jwt-token
```

---

## 11. İstatistikler

### 11.1. Kullanıcı İstatistikleri
**Endpoint:** `GET /api/statistics/user/{userId}`

**Headers (Opsiyonel):**
```
Token: your-jwt-token
```

**Response:**
```typescript
{
  success: true,
  response: {
    totalLists: number;
    publicLists: number;
    totalLikes: number;
    totalFollowers: number;
    totalFollowing: number;
    totalAnimeWatched: number;
    averageScore: number;
    scoreDistribution: { [score: number]: number };  // 1-10
    yearDistribution: { [year: number]: number };
    genreDistribution: { [genre: string]: number };
  }
}
```

**Frontend Kullanımı:**
```javascript
// Profil sayfasında istatistikleri göster
const getUserStatistics = async (userId) => {
  const token = localStorage.getItem('token');
  const headers = token ? { 'Token': token } : {};
  
  const response = await fetch(`https://localhost:7123/api/statistics/user/${userId}`, {
    headers
  });
  
  const data = await response.json();
  if (data.success) {
    // İstatistikleri grafiklerle göster
    renderStatistics(data.response);
  }
};
```

---

### 11.2. Kendi İstatistiklerimi Getir
**Endpoint:** `GET /api/statistics/me`

**Headers:**
```
Token: your-jwt-token
```

---

## 12. Aktivite Akışı

### 12.1. Kullanıcı Aktivitesi
**Endpoint:** `GET /api/activity/user/{userId}?page=1&limit=20`

**Headers (Opsiyonel):**
```
Token: your-jwt-token
```

**Response:**
```typescript
{
  success: true,
  response: Array<{
    id: number;
    type: string;  // "comment", "like", "new_list", "follow"
    userId: number;
    username: string;
    message: string;
    relatedListId?: number;
    relatedListTitle?: string;
    relatedUserId?: number;
    createdAt: string;
  }>;
}
```

---

### 12.2. Kendi Aktivitemi Getir
**Endpoint:** `GET /api/activity/me?page=1&limit=20`

**Headers:**
```
Token: your-jwt-token
```

---

## 13. Öneriler ve Trending

### 13.1. Anime Önerileri
**Endpoint:** `GET /api/recommendation/anime?limit=10`

**Headers:**
```
Token: your-jwt-token
```

**Response:**
```typescript
{
  success: true,
  response: Array<{
    malId: number;
    title: string;
    imageUrl: string;
    score?: number;
    reason: string;
    matchCount: number;
  }>;
}
```

**Frontend Kullanımı:**
```javascript
// Ana sayfada önerileri göster
const getRecommendations = async () => {
  const token = localStorage.getItem('token');
  const response = await fetch('https://localhost:7123/api/recommendation/anime?limit=10', {
    headers: { 'Token': token }
  });
  
  const data = await response.json();
  if (data.success) {
    // Önerileri göster
    displayRecommendations(data.response);
  }
};
```

---

### 13.2. Trending Listeler
**Endpoint:** `GET /api/recommendation/trending?page=1&limit=20`

**Response:**
```typescript
{
  success: true,
  response: {
    lists: Array<{
      id: number;
      title: string;
      authorUsername: string;
      viewCount: number;
      likeCount: number;
      commentCount: number;
      createdAt: string;
      trendingScore: number;
    }>;
    totalCount: number;
    page: number;
    totalPages: number;
  }
}
```

---

## 14. Liste Kopyalama

### 14.1. Liste Kopyala
**Endpoint:** `POST /api/copy/list`

**Headers:**
```
Token: your-jwt-token
```

**Request Body:**
```typescript
{
  sourceListId: number;
  newTitle: string;
}
```

**Response:**
```typescript
{
  success: true,
  response: {
    listId: number;
    title: string;
  }
}
```

**Frontend Kullanımı:**
```javascript
// Public liste sayfasında "Kopyala" butonuna tıklandığında
const copyList = async (sourceListId, newTitle) => {
  const token = localStorage.getItem('token');
  const response = await fetch('https://localhost:7123/api/copy/list', {
    method: 'POST',
    headers: {
      'Token': token,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ sourceListId, newTitle })
  });
  
  const data = await response.json();
  if (data.success) {
    // Yeni listeye yönlendir
    router.push(`/list/${data.response.listId}/edit`);
  }
};
```

---

## 15. Export ve Embed

### 15.1. Liste Export (Görsel)
**Endpoint:** `POST /api/export/image/{listId}`

**Headers:**
```
Token: your-jwt-token
```

**Response:**
```typescript
{
  success: true,
  response: {
    imageBase64: string;  // JSON verisi (frontend'de görsel oluşturulacak)
    imageUrl: string;
  }
}
```

**Not:** Backend sadece veriyi hazırlar. Frontend'de görsel oluşturulmalıdır.

---

### 15.2. Embed Kodu Getir
**Endpoint:** `GET /api/export/embed/{listId}`

**Response:**
```typescript
{
  success: true,
  response: {
    embedCode: string;  // HTML iframe kodu
    embedUrl: string;
  }
}
```

---

## 16. MAL Senkronizasyonu

### 16.1. MAL Listesini Senkronize Et
**Endpoint:** `POST /api/sync/mal`

**Headers:**
```
Token: your-jwt-token
```

**Request Body:**
```typescript
{
  listId: number;
  mode: "Ranked" | "Tiered" | "Fusion";
  replaceExisting: boolean;  // Mevcut item'ları değiştir mi?
}
```

**Response:**
```typescript
{
  success: true,
  response: {
    updatedItemCount: number;
  }
}
```

**Frontend Kullanımı:**
```javascript
// "MAL'dan Senkronize Et" butonuna tıklandığında
const syncMALList = async (listId, mode, replaceExisting = false) => {
  const token = localStorage.getItem('token');
  const response = await fetch('https://localhost:7123/api/sync/mal', {
    method: 'POST',
    headers: {
      'Token': token,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ listId, mode, replaceExisting })
  });
  
  const data = await response.json();
  if (data.success) {
    alert(`${data.response.updatedItemCount} anime eklendi!`);
    // Listeyi yeniden getir
    await getList(listId);
  }
};
```

---

## 17. Drag & Drop

### 17.1. Item Taşı
**Endpoint:** `POST /api/dragdrop/move-item`

**Headers:**
```
Token: your-jwt-token
```

**Request Body:**
```typescript
{
  itemId: number;
  targetTierId: number;
  newRankInTier: number;
}
```

**Frontend Kullanımı:**
```javascript
// Drag & drop işlemi tamamlandığında
const moveItem = async (itemId, targetTierId, newRankInTier) => {
  const token = localStorage.getItem('token');
  const response = await fetch('https://localhost:7123/api/dragdrop/move-item', {
    method: 'POST',
    headers: {
      'Token': token,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ itemId, targetTierId, newRankInTier })
  });
  
  const data = await response.json();
  if (data.success) {
    // UI'ı güncelle
  }
};
```

---

### 17.2. Item Sıralamasını Güncelle
**Endpoint:** `POST /api/dragdrop/reorder-items`

**Headers:**
```
Token: your-jwt-token
```

**Request Body:**
```typescript
{
  tierId: number;
  items: Array<{
    itemId: number;
    rankInTier: number;
  }>;
}
```

**Frontend Kullanımı:**
```javascript
// Tier içinde sıralama değiştiğinde
const reorderItems = async (tierId, items) => {
  const token = localStorage.getItem('token');
  const response = await fetch('https://localhost:7123/api/dragdrop/reorder-items', {
    method: 'POST',
    headers: {
      'Token': token,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ tierId, items })
  });
  
  const data = await response.json();
  if (data.success) {
    // UI'ı güncelle
  }
};
```

---

## 📝 Önemli Notlar

### Token Yönetimi
- Tüm authenticated endpoint'ler için `Token` header'ında JWT token gönderilmelidir
- Token'ı `localStorage` veya `sessionStorage`'da saklayın
- Token süresi dolduğunda kullanıcıyı login sayfasına yönlendirin

### Hata Yönetimi
```javascript
// Tüm API çağrılarında hata kontrolü yapın
const handleApiCall = async (apiCall) => {
  try {
    const response = await apiCall();
    const data = await response.json();
    
    if (!data.success) {
      // Hata mesajını göster
      showError(data.message);
      return null;
    }
    
    return data.response;
  } catch (error) {
    showError('Bir hata oluştu. Lütfen tekrar deneyin.');
    return null;
  }
};
```

### Loading States
- Tüm API çağrılarında loading state gösterin
- Optimistic updates kullanarak kullanıcı deneyimini iyileştirin

### Pagination
- Liste endpoint'lerinde pagination kullanın
- Infinite scroll veya "Daha Fazla" butonu ekleyin

---

## 🎨 Frontend Sayfaları ve Endpoint'ler

### Ana Sayfa (Dashboard)
- `GET /api/list/all` - Kullanıcının listelerini göster
- `GET /api/recommendation/anime` - Önerileri göster
- `GET /api/recommendation/trending` - Trending listeleri göster

### Liste Oluşturma Sayfası
- `GET /api/generate/genres` - Kategorileri göster
- `POST /api/list/create` - Yeni liste oluştur
- `POST /api/generate/by-score` - Puana göre liste oluştur
- `POST /api/generate/by-year` - Yıla göre liste oluştur
- `POST /api/generate/by-genre` - Kategoriye göre liste oluştur

### Liste Düzenleme Sayfası
- `GET /api/list/{listId}` - Listeyi getir
- `PUT /api/list/save` - Listeyi kaydet
- `POST /api/list/item/add` - Item ekle
- `DELETE /api/list/item/remove` - Item sil
- `POST /api/list/tier/add` - Tier ekle
- `PUT /api/list/tier/update` - Tier güncelle
- `DELETE /api/list/tier/remove` - Tier sil
- `POST /api/list/check-duplicate` - Duplicate kontrol
- `POST /api/list/convert-to-fusion` - Fusion'a çevir
- `POST /api/list/convert-to-ranked` - Ranked'e çevir
- `POST /api/dragdrop/move-item` - Item taşı
- `POST /api/dragdrop/reorder-items` - Item sırala

### Public Liste Sayfası
- `GET /api/share/public/{shareToken}` - Public listeyi getir
- `POST /api/social/like/{listId}` - Beğen
- `GET /api/comment/list/{listId}` - Yorumları getir
- `POST /api/comment/add` - Yorum ekle
- `POST /api/copy/list` - Listeyi kopyala

### Profil Sayfası
- `GET /api/user/{userId}` - Kullanıcı bilgilerini getir
- `GET /api/social/profile/{userId}` - Profil detaylarını getir
- `GET /api/statistics/user/{userId}` - İstatistikleri getir
- `GET /api/activity/user/{userId}` - Aktiviteleri getir
- `POST /api/social/follow` - Takip et/takipten çık

### Arama Sayfası
- `GET /api/search/anime` - Anime ara

### Bildirimler
- `GET /api/social/notifications` - Bildirimleri getir
- `PUT /api/social/notifications/{notificationId}/read` - Okundu işaretle
- `PUT /api/social/notifications/read-all` - Tümünü okundu işaretle
- `DELETE /api/social/notification/{notificationId}` - Bildirim sil

---

**Son Güncelleme:** 2024
**Versiyon:** 1.0

