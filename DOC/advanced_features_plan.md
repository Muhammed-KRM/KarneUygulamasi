# KarneProject - İleri Seviye Özellikler ve Entegrasyon Planı

Sistemin temel temizliği ve mimarisi kuruldu. Ancak bir **Eğitim Platformu** (Karne ve Sınav Yönetimi) doğası gereği yüksek işlem gücü ve anlık etkileşim gerektirir. Performans ve ölçeklenebilirlik için aşağıdaki teknolojilerin sisteme entegre edilmesi kritiktir.

---

## 🚀 1. Performans ve Ölçeklenebilirlik

### 🧠 Caching (Redis)

- **Neden Gerekli?**
  - Öğrenciler sınav sonuçlarına bakarken veya "Keşfet" sayfasında soru ararken veritabanına sürekli yük biner.
  - Örneğin: _Bir sınıftaki 40 öğrencinin karnesi her görüntülendiğinde tekrar hesaplanmamalı._
- **Entegrasyon Planı:**
  - `IDistributedCache` arayüzü ile Redis entegrasyonu.
  - **Cache Strategy:** "Cache-Aside" (Önce Cache'e bak, yoksa DB'den çek ve Cache'e yaz).
  - **Kullanılacağı Yerler:** Sınav Sonuçları, Soru Akışı (Feed), Sınıf Listeleri.

### ⏱️ Asenkron İşlem (Background Jobs - Hangfire)

- **Neden Gerekli?**
  - **Karne Oluşturma:** Bir deneme sınavı sonrası yüzlerce öğrencinin netlerini hesaplamak, sıralama yapmak ve PDF üretmek uzun sürer. Kullanıcıyı "Lütfen bekleyiniz" ekranında tutamayız.
  - **Toplu Bildirim:** "Yarın sınav var" bildirimi aynı anda 1000 öğrenciye giderken sistem kilitlenmemeli.
- **Entegrasyon Planı:**
  - Hangfire kurulumu (SQL Server depolama alanı ile).
  - **Senaryo:** Öğretmen "Sınavı Bitir" dediğinde, arka planda bir `CalculateExamResultsJob` çalışacak. İşlem bitince öğretmene bildirim gidecek.

---

## ⚡ 2. Gerçek Zamanlı İletişim (SignalR)

### 💬 Chat ve Bildirimler

- **Neden Gerekli?**
  - Gereksinimlerde belirtilen **Mesajlaşma** ve **Soru/Sınav Paylaşımı** için anlık etkileşim şart.
  - Öğretmen sınavı paylaştığı anda öğrencinin ekranına düşmeli (Sayfa yenilemeden).
- **Entegrasyon Planı:**
  - `Hub` yapısı kurulacak: `NotificationHub`, `ChatHub`.
  - **Özellik:** Öğrenci soruyu beğendiğinde sahibine anında bildirim, sınıfa mesaj atıldığında anında iletim.

---

## 🛡️ 3. Güvenlik ve Veri Doğrulama

### ✅ FluentValidation

- **Neden Gerekli?**
  - DataAnnotations (basit attribute'lar) karmaşık iş kuralları için yetersizdir.
  - Örn: _"Bir sınav tarihi bugünden eski olamaz"_ veya _"Sınıf mevcudu dershane kotasını aşamaz"_.
- **Entegrasyon Planı:**
  - Controller katmanından iş kurallarını ayırarak temiz kod sağlar.

### 🚦 Rate Limiting

- **Neden Gerekli?**
  - Kötü niyetli kişilerin sisteme saniyede 1000 istek atıp çökertmesini (DDoS) engellemek için.
- **Entegrasyon Planı:**
  - ASP.NET Core Rate Limiting middleware (önceki adımda bahsetmiştik, şimdi konfigüre edeceğiz).

---

## 🗄️ 4. Dosya Yönetimi (Cloud Storage)

### ☁️ Dosya Depolama (MinIO / Azure Blob)

- **Neden Gerekli?**
  - Sınav soruları (resimler), profil fotoğrafları, optik okuyucu dosyaları sunucu diskinde (Local) saklanmamalı. Sunucu çökerse dosyalar gider.
- **Entegrasyon Planı:**
  - `IFileService` arayüzü yazılacak.
  - Local (Geliştirme) ve Cloud (Canlı) için iki ayrı implementasyon yapılacak.

---

## 🗺️ Entegrasyon Yol Haritası (Güncellenmiş)

Mevcut plana ek olarak bu teknolojileri şu sırayla eklemeyi öneriyorum:

1.  **FluentValidation:** Entity'leri oluştururken kuralları baştan yazalım. (Hemen Şimdi)
2.  **File Service:** Profil ve Soru resimleri için altyapı. (Entity'lerden önce lazım)
3.  **SignalR:** Temel yapıya hub'ları ekleyelim.
4.  **Redis & Hangfire:** Veritabanı işlemleri yoğunlaşınca (Faz 2-3 gibi) eklenebilir.

### Önerilen Paketler

- `FluentValidation.AspNetCore`
- `Microsoft.AspNetCore.SignalR`
- `Hangfire`
- `StackExchange.Redis`

Bu özellikleri "Implementation Plan"a dahil ediyorum. Onaylıyor musunuz?
