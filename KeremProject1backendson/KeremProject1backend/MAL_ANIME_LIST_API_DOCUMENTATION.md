# MyAnimeList ve Anime Listesi API Dokümantasyonu

Bu dokümantasyon, MyAnimeList (MAL) entegrasyonu ve anime listesi yönetimi için kullanılan tüm API endpoint'lerini detaylı bir şekilde açıklar.

## İçindekiler

1. [Genel Bilgiler](#genel-bilgiler)
2. [MAL Entegrasyonu Endpoint'leri](#mal-entegrasyonu-endpointleri)
3. [Anime Arama Endpoint'leri](#anime-arama-endpointleri)
4. [Anime Listesi Yönetimi Endpoint'leri](#anime-listesi-yönetimi-endpointleri)
5. [Hata Kodları](#hata-kodlari)

---

## Genel Bilgiler

### Base URL
```
https://localhost:7132/api
```

### Authentication
Çoğu endpoint için `token` header'ı gereklidir. Token, kullanıcı girişi sonrası alınır ve her istekte header olarak gönderilmelidir.

```http
token: <your-jwt-token>
```

### Response Formatı
Tüm endpoint'ler `BaseResponse` formatında yanıt döner:

```json
{
  "response": <data>,
  "returnValue": 0,
  "errorMessage": "İşlem başarılı.",
  "errored": false
}
```

**Hata durumunda:**
```json
{
  "response": null,
  "returnValue": <error-code>,
  "errorMessage": "Hata mesajı",
  "errored": true
}
```

---

## MAL Entegrasyonu Endpoint'leri

### 1. MAL OAuth URL Al

MAL hesabını bağlamak için OAuth URL'i alır.

**Endpoint:** `GET /api/mal/get-auth-url`

**Headers:**
```http
token: <your-jwt-token>
```

**Response:**
```json
{
  "response": {
    "authUrl": "https://myanimelist.net/v1/oauth2/authorize?...",
    "codeVerifier": "random-generated-string"
  },
  "returnValue": 0,
  "errorMessage": "MAL Auth URL oluşturuldu.",
  "errored": false
}
```

**Kullanım:**
1. Bu endpoint'ten `authUrl` ve `codeVerifier` alınır
2. Kullanıcı `authUrl`'e yönlendirilir
3. Kullanıcı MAL'da izin verir
4. Callback URL'ine `code` ve `state` (codeVerifier) döner
5. `callback` endpoint'i ile token'lar kaydedilir

---

### 2. MAL OAuth Callback

MAL OAuth callback'ini işler ve token'ları kaydeder.

**Endpoint:** `POST /api/mal/callback`

**Headers:**
```http
token: <your-jwt-token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "code": "oauth-authorization-code",
  "state": "code-verifier-from-step-1"
}
```

**Response:**
```json
{
  "response": null,
  "returnValue": 0,
  "errorMessage": "MAL hesabı başarıyla bağlandı.",
  "errored": false
}
```

**Hata Kodları:**
- `2002`: Token alınamadı
- `2003`: Callback hatası

---

### 3. Kendi MAL Listemi Çek (OAuth)

Bağlı MAL hesabından anime listesini çeker. Önce OAuth API'yi dener, başarısız olursa load.json'a geçer.

**Endpoint:** `GET /api/mal/get-my-list`

**Headers:**
```http
token: <your-jwt-token>
```

**Response:**
```json
{
  "response": {
    "data": [
      {
        "node": {
          "id": 48569,
          "title": "86 Part 2",
          "main_picture": {
            "medium": "https://cdn.myanimelist.net/...",
            "large": "https://cdn.myanimelist.net/..."
          }
        },
        "list_status": {
          "status": "completed",
          "score": 9
        }
      }
    ]
  },
  "returnValue": 0,
  "errorMessage": "Liste çekildi (OAuth API).",
  "errored": false
}
```

**Özellikler:**
- OAuth token varsa önce OAuth API kullanılır
- OAuth başarısızsa veya token yoksa load.json'a geçilir
- Token süresi dolmuşsa otomatik yenilenir

**Hata Kodları:**
- `2004`: Kullanıcı bulunamadı
- `2005`: Liste çekilemedi
- `2006`: Liste çekme hatası
- `2007`: Token yenilenemedi

---

### 4. Username ile Basit Liste Çek

Username ile direkt MAL listesini çeker (OAuth gerektirmez). Sadece temel bilgileri döner: id, title, image, score.

**Endpoint:** `POST /api/mal/get-list-by-username`

**Headers:**
```http
Content-Type: application/json
```

**Request Body:**
```json
{
  "username": "Mamito_Aga",
  "status": 2
}
```

**Status Değerleri:**
- `1`: Watching (İzleniyor)
- `2`: Completed (Tamamlandı) - Varsayılan
- `3`: On Hold (Beklemede)
- `4`: Dropped (Bırakıldı)
- `6`: Plan to Watch (İzlenecek)

**Response:**
```json
{
  "response": {
    "data": [
      {
        "node": {
          "id": 48569,
          "title": "86 Part 2",
          "main_picture": {
            "medium": "https://cdn.myanimelist.net/...",
            "large": "https://cdn.myanimelist.net/..."
          }
        },
        "list_status": {
          "status": "completed",
          "score": 9
        }
      }
    ]
  },
  "returnValue": 0,
  "errorMessage": "Liste başarıyla çekildi (load.json).",
  "errored": false
}
```

**Özellikler:**
- OAuth gerektirmez
- Proxy desteği (fallback mekanizması)
- Sayfalama desteği (tüm sayfalar otomatik çekilir)
- Relevance sıralaması (query'ye göre)

**Hata Kodları:**
- `2008`: Kullanıcı adı boş
- `2009`: Liste çekilemedi veya boş
- `2010`: Liste çekme hatası

---

### 5. Username ile Detaylı Liste Çek

Username ile detaylı MAL listesini çeker. Tüm alanları içerir: genres, demographics, dates, scores, popularity, vs.

**Endpoint:** `POST /api/mal/get-advanced-list-by-username`

**Headers:**
```http
Content-Type: application/json
```

**Request Body:**
```json
{
  "username": "Mamito_Aga",
  "status": 2
}
```

**Response:**
```json
{
  "response": {
    "data": [
      {
        "anime_id": 48569,
        "anime_title": "86 Part 2",
        "anime_title_eng": "86 Eighty-Six Part 2",
        "anime_image_path": "https://cdn.myanimelist.net/...",
        "anime_airing_status": 2,
        "anime_end_date_string": "03-19-22",
        "anime_start_date_string": "10-03-21",
        "anime_media_type_string": "TV",
        "anime_mpaa_rating_string": "R",
        "anime_num_episodes": 12,
        "anime_popularity": 399,
        "anime_score_val": 8.72,
        "anime_total_members": 610406,
        "anime_total_scores": 354105,
        "anime_url": "/anime/48569/86_Part_2",
        "score": 9,
        "status": 2,
        "num_watched_episodes": 12,
        "is_rewatching": 0,
        "priority_string": "Low",
        "genres": [
          {
            "id": 8,
            "name": "Drama"
          },
          {
            "id": 24,
            "name": "Sci-Fi"
          }
        ],
        "demographics": [],
        "created_at": 1759263975,
        "updated_at": 1759263975,
        "has_video": true,
        "has_episode_video": false,
        "has_promotion_video": true,
        "video_url": "/anime/48569/86_Part_2/video",
        "notes": "",
        "tags": "",
        "editable_notes": "",
        "storage_string": "",
        "title_localized": null,
        "days_string": null,
        "finish_date_string": null,
        "start_date_string": null,
        "is_added_to_list": false
      }
    ]
  },
  "returnValue": 0,
  "errorMessage": "Detaylı liste başarıyla çekildi (load.json).",
  "errored": false
}
```

**Özellikler:**
- Tüm load.json alanları dahil
- Genres ve Demographics array formatında
- Tarih bilgileri (created_at, updated_at, dates)
- Video bilgileri
- Kullanıcı notları ve tag'leri

**Hata Kodları:**
- `2011`: Kullanıcı adı boş
- `2012`: Detaylı liste çekilemedi veya boş
- `2013`: Detaylı liste çekme hatası

---

## Anime Arama Endpoint'leri

### 1. Anime Ara (POST)

Jikan API üzerinden anime araması yapar. Relevance sıralaması yapılır (query'ye göre).

**Endpoint:** `POST /api/search/anime`

**Headers:**
```http
Content-Type: application/json
```

**Request Body:**
```json
{
  "query": "naruto",
  "genre": "",
  "year": 0,
  "minScore": 0,
  "maxScore": 0,
  "page": 1,
  "limit": 25
}
```

**Parametreler:**
- `query` (string, optional): Arama terimi
- `genre` (string, optional): Genre filtresi
- `year` (int, optional): Yıl filtresi
- `minScore` (double, optional): Minimum score
- `maxScore` (double, optional): Maximum score
- `page` (int, default: 1): Sayfa numarası
- `limit` (int, default: 25): Sayfa başına sonuç sayısı

**Response:**
```json
{
  "response": {
    "results": [
      {
        "malId": 20,
        "title": "Naruto",
        "imageUrl": "https://cdn.myanimelist.net/...",
        "score": 8.01,
        "year": 2002,
        "genres": ["Action", "Adventure", "Fantasy"],
        "synopsis": "Twelve years ago, a colossal demon fox..."
      }
    ],
    "totalResults": 29372,
    "page": 1,
    "totalPages": 1175
  },
  "returnValue": 0,
  "errorMessage": "Arama başarıyla tamamlandı.",
  "errored": false
}
```

**Relevance Sıralaması:**
1. Tam eşleşme (title == query) - En üstte
2. Başlıkta query ile başlayanlar
3. Başlıkta query içerenler
4. Kelime bazında eşleşme
5. Aynı relevance'da score'a göre sıralama

**Hata Kodları:**
- `6001`: Anime arama yapılamadı
- `6002`: Arama hatası

---

### 2. Anime Ara (GET)

Aynı işlevi GET metodu ile yapar. Query parametreleri kullanılır.

**Endpoint:** `GET /api/search/anime`

**Query Parameters:**
```
?query=naruto&genre=&year=0&minScore=0&maxScore=0&page=1&limit=25
```

**Response:** POST ile aynı

---

## Anime Listesi Yönetimi Endpoint'leri

### 1. Liste Oluştur

Yeni bir anime listesi oluşturur.

**Endpoint:** `POST /api/list/create`

**Headers:**
```http
token: <your-jwt-token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "title": "My Top 100 Anime",
  "mode": 1,
  "tiers": [
    {
      "title": "S",
      "color": "#FF7F7F",
      "order": 0
    },
    {
      "title": "A",
      "color": "#FFBF7F",
      "order": 1
    }
  ]
}
```

**ListMode Değerleri:**
- `0`: Ranked - Sadece 1 tier (1, 2, 3 sıralama)
- `1`: Tiered - Birden fazla tier (S, A, B, C...)
- `2`: Fusion - TierMaker + Ranked (Tier içinde sıralama)

**Tiers (Opsiyonel):**
- Verilmezse varsayılan tier'lar oluşturulur
- Tiered/Fusion modunda: S, A, B, C, D, F
- Ranked modunda: "Ranked" (tek tier)

**Response:**
```json
{
  "response": {
    "listId": 123
  },
  "returnValue": 0,
  "errorMessage": "Liste başarıyla oluşturuldu.",
  "errored": false
}
```

**Hata Kodları:**
- `3001`: Ranked modunda sadece bir tier olabilir
- `3002`: En az bir tier belirtilmeli
- `3003`: Liste oluşturma hatası

---

### 2. Liste Kaydet

Liste içeriğini kaydeder (tier'lar ve item'lar).

**Endpoint:** `PUT /api/list/save`

**Headers:**
```http
token: <your-jwt-token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "listId": 123,
  "tiers": [
    {
      "title": "S",
      "color": "#FF7F7F",
      "order": 0,
      "items": [
        {
          "animeMalId": 48569,
          "rankInTier": 1
        },
        {
          "animeMalId": 41457,
          "rankInTier": 2
        }
      ]
    }
  ]
}
```

**Response:**
```json
{
  "response": null,
  "returnValue": 0,
  "errorMessage": "Liste başarıyla kaydedildi.",
  "errored": false
}
```

**Hata Kodları:**
- `3004`: Liste bulunamadı
- `3005`: Liste kaydetme hatası

---

### 3. Liste Getir

Belirli bir listeyi detaylarıyla getirir.

**Endpoint:** `GET /api/list/{listId}`

**Headers:**
```http
token: <your-jwt-token>
```

**Response:**
```json
{
  "response": {
    "id": 123,
    "title": "My Top 100 Anime",
    "mode": "Tiered",
    "tiers": [
      {
        "id": 1,
        "title": "S",
        "color": "#FF7F7F",
        "order": 0,
        "items": [
          {
            "id": 1,
            "animeMalId": 48569,
            "rankInTier": 1,
            "title": "86 Part 2",
            "imageUrl": "https://cdn.myanimelist.net/..."
          }
        ]
      }
    ]
  },
  "returnValue": 0,
  "errorMessage": "Liste başarıyla çekildi.",
  "errored": false
}
```

**Not:** Anime detayları (title, imageUrl) Jikan API'den çekilir, bu yüzden biraz zaman alabilir.

**Hata Kodları:**
- `3004`: Liste bulunamadı
- `3006`: Liste çekme hatası

---

### 4. Tüm Listeleri Getir

Kullanıcının tüm listelerini getirir.

**Endpoint:** `GET /api/list/all`

**Headers:**
```http
token: <your-jwt-token>
```

**Response:**
```json
{
  "response": [
    {
      "id": 123,
      "title": "My Top 100 Anime",
      "mode": "Tiered",
      "createdAt": "2024-01-01T00:00:00",
      "modTime": "2024-01-15T00:00:00"
    }
  ],
  "returnValue": 0,
  "errorMessage": "Listeler başarıyla çekildi.",
  "errored": false
}
```

---

### 5. Liste Başlığını Güncelle

Liste başlığını günceller.

**Endpoint:** `PATCH /api/list/{listId}/title`

**Headers:**
```http
token: <your-jwt-token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "listId": 123,
  "title": "Yeni Başlık"
}
```

**Response:**
```json
{
  "response": null,
  "returnValue": 0,
  "errorMessage": "Liste başlığı başarıyla güncellendi.",
  "errored": false
}
```

**Hata Kodları:**
- `3014`: Liste ID'leri eşleşmiyor
- `3013`: Liste başlığı güncelleme hatası

---

### 6. Liste Sil

Listeyi siler.

**Endpoint:** `DELETE /api/list/{listId}`

**Headers:**
```http
token: <your-jwt-token>
```

**Response:**
```json
{
  "response": null,
  "returnValue": 0,
  "errorMessage": "Liste başarıyla silindi.",
  "errored": false
}
```

**Hata Kodları:**
- `3007`: Liste bulunamadı
- `3008`: Liste silme hatası

---

### 7. Fusion'a Çevir

Listeyi Fusion moduna çevirir (TierMaker + Ranked).

**Endpoint:** `POST /api/list/convert-to-fusion`

**Headers:**
```http
token: <your-jwt-token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "listId": 123
}
```

**Response:**
```json
{
  "response": null,
  "returnValue": 0,
  "errorMessage": "Liste başarıyla Fusion moduna çevrildi.",
  "errored": false
}
```

---

### 8. Ranked'e Çevir

Listeyi Ranked moduna çevirir (tek tier, sıralama).

**Endpoint:** `POST /api/list/convert-to-ranked`

**Headers:**
```http
token: <your-jwt-token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "listId": 123
}
```

**Response:**
```json
{
  "response": null,
  "returnValue": 0,
  "errorMessage": "Liste başarıyla Ranked moduna çevrildi.",
  "errored": false
}
```

**Not:** Ranked modunda sadece 1 tier olabilir. Tüm item'lar bu tier'a taşınır.

---

## Item Yönetimi

### 1. Item Ekle

Listeye yeni bir anime ekler.

**Endpoint:** `POST /api/list/item/add`

**Headers:**
```http
token: <your-jwt-token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "listId": 123,
  "tierId": 1,
  "animeMalId": 48569,
  "rankInTier": 1
}
```

**Parametreler:**
- `listId` (required): Liste ID'si
- `tierId` (required): Tier ID'si
- `animeMalId` (required): MAL anime ID'si
- `rankInTier` (optional): Tier içindeki sıra (null ise sona eklenir)

**Response:**
```json
{
  "response": null,
  "returnValue": 0,
  "errorMessage": "Item başarıyla eklendi.",
  "errored": false
}
```

**Hata Kodları:**
- `3009`: Item ekleme hatası
- `3010`: Duplicate item (aynı anime zaten listede)

---

### 2. Item Sil

Listeden bir anime siler.

**Endpoint:** `DELETE /api/list/item/remove`

**Headers:**
```http
token: <your-jwt-token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "itemId": 456
}
```

**Response:**
```json
{
  "response": null,
  "returnValue": 0,
  "errorMessage": "Item başarıyla silindi.",
  "errored": false
}
```

**Hata Kodları:**
- `3011`: Item bulunamadı
- `3012`: Item silme hatası

---

### 3. Toplu Item Ekle

Birden fazla animeyi toplu olarak ekler.

**Endpoint:** `POST /api/list/bulk-add-items`

**Headers:**
```http
token: <your-jwt-token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "listId": 123,
  "tierId": 1,
  "animeMalIds": [48569, 41457, 20],
  "skipDuplicates": true
}
```

**Parametreler:**
- `listId` (required): Liste ID'si
- `tierId` (required): Tier ID'si
- `animeMalIds` (required): MAL anime ID'leri array'i
- `skipDuplicates` (default: true): Duplicate'leri atla

**Response:**
```json
{
  "response": {
    "added": 3,
    "skipped": 0
  },
  "returnValue": 0,
  "errorMessage": "Toplu item ekleme tamamlandı.",
  "errored": false
}
```

---

### 4. Toplu Item Sil

Birden fazla animeyi toplu olarak siler.

**Endpoint:** `POST /api/list/bulk-remove-items`

**Headers:**
```http
token: <your-jwt-token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "listId": 123,
  "itemIds": [456, 457, 458]
}
```

**Response:**
```json
{
  "response": {
    "removed": 3
  },
  "returnValue": 0,
  "errorMessage": "Toplu item silme tamamlandı.",
  "errored": false
}
```

---

### 5. Duplicate Kontrol

Listede aynı anime'nin olup olmadığını kontrol eder.

**Endpoint:** `POST /api/list/check-duplicate`

**Headers:**
```http
token: <your-jwt-token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "listId": 123,
  "animeMalId": 48569
}
```

**Response:**
```json
{
  "response": {
    "isDuplicate": true,
    "existingItemId": 456,
    "existingTierId": 1,
    "existingTierTitle": "S"
  },
  "returnValue": 0,
  "errorMessage": "Duplicate kontrolü tamamlandı.",
  "errored": false
}
```

---

## Tier Yönetimi

### 1. Tier Ekle

Listeye yeni bir tier ekler.

**Endpoint:** `POST /api/list/tier/add`

**Headers:**
```http
token: <your-jwt-token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "listId": 123,
  "title": "SS",
  "color": "#FF0000",
  "order": 0
}
```

**Parametreler:**
- `listId` (required): Liste ID'si
- `title` (required): Tier başlığı
- `color` (optional, default: "#FFFFFF"): Tier rengi (hex)
- `order` (optional): Tier sırası (null ise sona eklenir)

**Response:**
```json
{
  "response": {
    "tierId": 5
  },
  "returnValue": 0,
  "errorMessage": "Tier başarıyla eklendi.",
  "errored": false
}
```

**Not:** Ranked modunda sadece 1 tier olabilir.

---

### 2. Tier Güncelle

Tier bilgilerini günceller.

**Endpoint:** `PUT /api/list/tier/update`

**Headers:**
```http
token: <your-jwt-token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "tierId": 5,
  "title": "SS+",
  "color": "#FF0000",
  "order": 0
}
```

**Parametreler:**
- `tierId` (required): Tier ID'si
- `title` (optional): Yeni başlık
- `color` (optional): Yeni renk
- `order` (optional): Yeni sıra

**Response:**
```json
{
  "response": null,
  "returnValue": 0,
  "errorMessage": "Tier başarıyla güncellendi.",
  "errored": false
}
```

---

### 3. Tier Sil

Tier'ı siler. Item'ları başka tier'a taşıyabilir veya silebilir.

**Endpoint:** `DELETE /api/list/tier/remove`

**Headers:**
```http
token: <your-jwt-token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "tierId": 5,
  "moveItemsToTierId": 1
}
```

**Parametreler:**
- `tierId` (required): Silinecek tier ID'si
- `moveItemsToTierId` (optional): Item'ları taşınacak tier ID'si (null ise item'lar silinir)

**Response:**
```json
{
  "response": null,
  "returnValue": 0,
  "errorMessage": "Tier başarıyla silindi.",
  "errored": false
}
```

**Not:** Ranked modunda son tier silinemez.

---

## Rank Yönetimi

### 1. Rank Değiştir

İki item'ın rank'ını değiştirir (swap).

**Endpoint:** `POST /api/list/swap-ranks`

**Headers:**
```http
token: <your-jwt-token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "listId": 123,
  "itemId1": 456,
  "itemId2": 457
}
```

**Response:**
```json
{
  "response": null,
  "returnValue": 0,
  "errorMessage": "Rank'lar başarıyla değiştirildi.",
  "errored": false
}
```

---

### 2. Rank Sıfırla

Tier içindeki tüm rank'ları sıfırlar (1'den başlayarak yeniden sıralar).

**Endpoint:** `POST /api/list/reset-ranks`

**Headers:**
```http
token: <your-jwt-token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "tierId": 1
}
```

**Response:**
```json
{
  "response": null,
  "returnValue": 0,
  "errorMessage": "Rank'lar başarıyla sıfırlandı.",
  "errored": false
}
```

---

## İstatistikler

### Liste İstatistikleri

Liste hakkında istatistikleri getirir.

**Endpoint:** `GET /api/list/{listId}/statistics`

**Headers:**
```http
token: <your-jwt-token>
```

**Response:**
```json
{
  "response": {
    "totalItems": 100,
    "totalTiers": 6,
    "itemsPerTier": {
      "S": 10,
      "A": 20,
      "B": 30
    },
    "uniqueAnimeCount": 100,
    "createdAt": "2024-01-01T00:00:00",
    "lastModified": "2024-01-15T00:00:00"
  },
  "returnValue": 0,
  "errorMessage": "İstatistikler başarıyla çekildi.",
  "errored": false
}
```

---

## Hata Kodları

### Genel Hatalar
- `1000`: Token geçersiz veya oturum süresi dolmuş
- `1001`: Geçersiz istek

### MAL Entegrasyon Hataları
- `2001`: URL oluşturulurken hata
- `2002`: MAL'dan token alınamadı
- `2003`: Callback hatası
- `2004`: Kullanıcı bulunamadı veya MAL hesabı bağlı değil
- `2005`: MAL listesi çekilemedi
- `2006`: Liste çekme hatası
- `2007`: MAL token süresi dolmuş ve yenilenemedi
- `2008`: Kullanıcı adı boş olamaz
- `2009`: Kullanıcının listesi çekilemedi veya liste boş
- `2010`: Liste çekme hatası
- `2011`: Kullanıcı adı boş olamaz (detaylı)
- `2012`: Detaylı liste çekilemedi veya boş
- `2013`: Detaylı liste çekme hatası

### Liste Yönetimi Hataları
- `3001`: Ranked modunda sadece bir tier olabilir
- `3002`: En az bir tier belirtilmelidir
- `3003`: Liste oluşturma hatası
- `3004`: Liste bulunamadı veya bu liste üzerinde yetkiniz yok
- `3005`: Liste kaydetme hatası
- `3006`: Liste çekme hatası
- `3007`: Liste bulunamadı
- `3008`: Liste silme hatası
- `3009`: Item ekleme hatası
- `3010`: Duplicate item
- `3011`: Item bulunamadı
- `3012`: Item silme hatası
- `3013`: Liste başlığı güncelleme hatası
- `3014`: Liste ID'leri eşleşmiyor

### Arama Hataları
- `6001`: Anime arama yapılamadı
- `6002`: Arama hatası

---

## Örnek Kullanım Senaryoları

### Senaryo 1: MAL Listesinden Tier Listesi Oluşturma

```javascript
// 1. MAL listesini çek
const malResponse = await fetch('/api/mal/get-list-by-username', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    username: 'Mamito_Aga',
    status: 2
  })
});
const malData = await malResponse.json();

// 2. Yeni liste oluştur
const createResponse = await fetch('/api/list/create', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'token': userToken
  },
  body: JSON.stringify({
    title: 'My MAL Top 100',
    mode: 1, // Tiered
    tiers: [
      { title: 'S', color: '#FF7F7F', order: 0 },
      { title: 'A', color: '#FFBF7F', order: 1 },
      { title: 'B', color: '#FFFF7F', order: 2 }
    ]
  })
});
const listData = await createResponse.json();
const listId = listData.response.listId;

// 3. MAL'dan gelen animeleri listeye ekle (score'a göre tier'lara dağıt)
const animeList = malData.response.data;
const tiers = [
  { id: 1, minScore: 9 }, // S tier
  { id: 2, minScore: 7 },  // A tier
  { id: 3, minScore: 5 }  // B tier
];

for (const anime of animeList) {
  const score = anime.list_status.score;
  let targetTierId = tiers.find(t => score >= t.minScore)?.id || tiers[tiers.length - 1].id;
  
  await fetch('/api/list/item/add', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'token': userToken
    },
    body: JSON.stringify({
      listId: listId,
      tierId: targetTierId,
      animeMalId: anime.node.id
    })
  });
}
```

### Senaryo 2: Anime Arama ve Listeye Ekleme

```javascript
// 1. Anime ara
const searchResponse = await fetch('/api/search/anime', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    query: 'naruto',
    page: 1,
    limit: 10
  })
});
const searchData = await searchResponse.json();

// 2. Bulunan animeleri listeye ekle
for (const anime of searchData.response.results) {
  await fetch('/api/list/item/add', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'token': userToken
    },
    body: JSON.stringify({
      listId: listId,
      tierId: tierId,
      animeMalId: anime.malId
    })
  });
}
```

### Senaryo 3: Detaylı MAL Listesi ile Gelişmiş Filtreleme

```javascript
// 1. Detaylı MAL listesini çek
const advancedResponse = await fetch('/api/mal/get-advanced-list-by-username', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    username: 'Mamito_Aga',
    status: 2
  })
});
const advancedData = await advancedResponse.json();

// 2. Genre'e göre filtrele
const actionAnime = advancedData.response.data.filter(anime => 
  anime.genres.some(genre => genre.name === 'Action')
);

// 3. Score'a göre sırala
const sortedAnime = actionAnime.sort((a, b) => 
  (b.anime_score_val || 0) - (a.anime_score_val || 0)
);

// 4. Yüksek score'luları listeye ekle
const topAnime = sortedAnime.slice(0, 10);
for (const anime of topAnime) {
  await fetch('/api/list/item/add', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'token': userToken
    },
    body: JSON.stringify({
      listId: listId,
      tierId: tierId,
      animeMalId: anime.anime_id
    })
  });
}
```

---

## Notlar

1. **Rate Limiting**: Jikan API rate limit'e sahiptir. Çok fazla istek atmayın.
2. **Proxy Fallback**: MAL load.json endpoint'leri için otomatik proxy fallback mekanizması vardır.
3. **Token Yenileme**: OAuth token'ları otomatik olarak yenilenir.
4. **Sayfalama**: MAL listeleri otomatik olarak tüm sayfalar çekilir (300 anime/sayfa).
5. **Relevance Sıralaması**: Arama sonuçları query'ye göre relevance sıralaması yapılır.

---

## Destek

Sorularınız için: [GitHub Issues](https://github.com/your-repo/issues)

