# Kullanıcı Listeleri API Dokümantasyonu

Bu dokümantasyon, başkalarının listelerini görüntülemek ve kullanıcı profillerindeki listelere erişmek için kullanılan endpoint'leri açıklar.

## 📋 İçindekiler

1. [Genel Bakış](#genel-bakış)
2. [Endpoint'ler](#endpointler)
   - [Kendi Listelerini Getir](#1-kendi-listelerini-getir)
   - [Bir Kullanıcının Public Listelerini Getir](#2-bir-kullanıcının-public-listelerini-getir)
   - [Liste Detaylarını Getir (Public Listeler Dahil)](#3-liste-detaylarını-getir-public-listeler-dahil)
   - [Tüm Public Listeleri Keşfet](#4-tüm-public-listeleri-keşfet)
3. [Frontend Kullanım Örnekleri](#frontend-kullanım-örnekleri)
4. [Response Formatları](#response-formatları)

---

## Genel Bakış

Sistemde iki tür liste erişimi vardır:

1. **Kendi Listeleriniz**: `/api/list/all` - Sadece kendi oluşturduğunuz listeler
2. **Başkalarının Public Listeleri**: `/api/list/user/{userId}` - Bir kullanıcının paylaşıma açık listeleri
3. **Public Liste Detayları**: `/api/list/{listId}` - Public listeler artık sahibi olmasa bile görüntülenebilir
4. **Keşfet**: `/api/share/public` - Tüm public listeleri keşfet

---

## Endpoint'ler

### 1. Kendi Listelerini Getir

**Endpoint:** `GET /api/list/all`

**Açıklama:** Kullanıcının kendi oluşturduğu tüm listelerini getirir.

**Headers:**
```
Token: <kullanıcı_token>
```

**Response:**
```json
{
  "response": [
    {
      "id": 16,
      "title": "kuyuktfy",
      "mode": "Ranked",
      "createdAt": "2025-11-21T10:08:04.407586",
      "modTime": "2025-11-21T10:09:24.3814044",
      "tierCount": 1,
      "itemCount": 3,
      "userId": 5  // ✅ YENİ: Liste sahibinin userId'si
    }
  ],
  "returnValue": 0,
  "errorMessage": "Listeler başarıyla çekildi.",
  "errored": false
}
```

**Frontend Kullanımı:**
```typescript
// Kendi listelerini getir
async function getMyLists(token: string) {
  const response = await fetch('https://localhost:7132/api/list/all', {
    method: 'GET',
    headers: {
      'Token': token
    }
  });
  const data = await response.json();
  return data.response; // Array of lists
}
```

**Kullanım Senaryosu:**
- Kullanıcı profil sayfasında "Benim Listelerim" sekmesi
- Dashboard'da kullanıcının tüm listelerini gösterme

---

### 2. Bir Kullanıcının Public Listelerini Getir

**Endpoint:** `GET /api/list/user/{userId}`

**Açıklama:** Belirtilen kullanıcının tüm **public** (paylaşıma açık) listelerini getirir. Bu endpoint profil sayfasında kullanılır.

**Headers (Opsiyonel):**
```
Token: <kullanıcı_token>  // Giriş yapmışsa beğeni durumunu gösterir
```

**Path Parameters:**
- `userId` (int): Liste sahibinin kullanıcı ID'si

**Response:**
```json
{
  "response": {
    "lists": [
      {
        "id": 16,
        "title": "En İyi Anime'ler",
        "mode": "Ranked",
        "createdAt": "2025-11-21T10:08:04.407586",
        "modTime": "2025-11-21T10:09:24.3814044",
        "tierCount": 1,
        "itemCount": 25,
        "viewCount": 150,
        "likeCount": 12,
        "authorUsername": "kerem123",
        "authorId": 5,
        "isLiked": false  // Giriş yapmışsa beğeni durumu
      }
    ],
    "userId": 5,
    "username": "kerem123",
    "totalCount": 3
  },
  "returnValue": 0,
  "errorMessage": "Kullanıcının public listeleri başarıyla getirildi.",
  "errored": false
}
```

**Frontend Kullanımı:**
```typescript
// Bir kullanıcının public listelerini getir
async function getUserPublicLists(userId: number, token?: string) {
  const headers: HeadersInit = {};
  if (token) {
    headers['Token'] = token;
  }

  const response = await fetch(
    `https://localhost:7132/api/list/user/${userId}`,
    {
      method: 'GET',
      headers
    }
  );
  const data = await response.json();
  return data.response;
}

// Kullanım örneği
const userLists = await getUserPublicLists(5, userToken);
console.log(userLists.lists); // Kullanıcının public listeleri
console.log(userLists.username); // "kerem123"
```

**Kullanım Senaryoları:**
1. **Profil Sayfası**: Bir kullanıcının profiline girildiğinde, o kullanıcının public listelerini göstermek
2. **Keşfet Sayfası**: Bir kullanıcıya tıklandığında, o kullanıcının listelerini göstermek
3. **Liste Detayı**: Bir listede "Yazarın Diğer Listeleri" bölümünde kullanılabilir

**Örnek React Component:**
```tsx
function UserProfileLists({ userId }: { userId: number }) {
  const [lists, setLists] = useState([]);
  const token = localStorage.getItem('token');

  useEffect(() => {
    async function fetchLists() {
      const data = await getUserPublicLists(userId, token || undefined);
      setLists(data.lists);
    }
    fetchLists();
  }, [userId]);

  return (
    <div>
      <h2>Public Listeler</h2>
      {lists.map(list => (
        <div key={list.id}>
          <h3>{list.title}</h3>
          <p>{list.itemCount} anime</p>
          <p>👁️ {list.viewCount} | ❤️ {list.likeCount}</p>
        </div>
      ))}
    </div>
  );
}
```

---

### 3. Liste Detaylarını Getir (Public Listeler Dahil)

**Endpoint:** `GET /api/list/{listId}`

**Açıklama:** Liste detaylarını getirir. Artık **public listeler sahibi olmasa bile görüntülenebilir**. Token opsiyoneldir (giriş yapmamış kullanıcılar da public listeleri görebilir).

**Headers (Opsiyonel):**
```
Token: <kullanıcı_token>  // Giriş yapmışsa sahip bilgisi gösterilir
```

**Path Parameters:**
- `listId` (int): Liste ID'si

**Response:**
```json
{
  "response": {
    "list": {
      "id": 16,
      "title": "En İyi Anime'ler",
      "mode": "Ranked",
      "tiers": [
        {
          "id": 1,
          "title": "Ranked",
          "color": "#FFFFFF",
          "order": 0,
          "items": [
            {
              "id": 1,
              "animeMalId": 20,
              "rankInTier": 1,
              "title": "Naruto",
              "imageUrl": "https://cdn.myanimelist.net/..."
            }
          ]
        }
      ]
    },
    "ownerId": 5,           // ✅ YENİ: Liste sahibinin ID'si
    "ownerUsername": "kerem123",  // ✅ YENİ: Liste sahibinin kullanıcı adı
    "isPublic": true,       // ✅ YENİ: Liste public mi?
    "isOwner": false        // ✅ YENİ: Bu liste sizin mi? (token varsa)
  },
  "returnValue": 0,
  "errorMessage": "Liste başarıyla çekildi.",
  "errored": false
}
```

**Frontend Kullanımı:**
```typescript
// Liste detaylarını getir (token opsiyonel)
async function getListDetails(listId: number, token?: string) {
  const headers: HeadersInit = {};
  if (token) {
    headers['Token'] = token;
  }

  const response = await fetch(
    `https://localhost:7132/api/list/${listId}`,
    {
      method: 'GET',
      headers
    }
  );
  const data = await response.json();
  return data.response;
}

// Kullanım örneği
const listDetails = await getListDetails(16, userToken);
console.log(listDetails.list); // Liste detayları
console.log(listDetails.ownerUsername); // "kerem123"
console.log(listDetails.isOwner); // false (başkasının listesi)
```

**Kullanım Senaryoları:**
1. **Kendi Listelerini Düzenleme**: `isOwner: true` ise düzenleme butonları göster
2. **Başkalarının Listelerini Görüntüleme**: `isOwner: false` ise sadece görüntüleme modu
3. **Yazar Bilgisi**: `ownerUsername` ile "Yazarın Diğer Listeleri" linki oluştur
4. **Public/Private Kontrolü**: `isPublic` ile paylaşım durumunu göster

**Örnek React Component:**
```tsx
function ListDetails({ listId }: { listId: number }) {
  const [listData, setListData] = useState(null);
  const token = localStorage.getItem('token');

  useEffect(() => {
    async function fetchList() {
      const data = await getListDetails(listId, token || undefined);
      setListData(data);
    }
    fetchList();
  }, [listId]);

  if (!listData) return <div>Yükleniyor...</div>;

  return (
    <div>
      <h1>{listData.list.title}</h1>
      
      {/* Yazar bilgisi */}
      <div>
        <p>Yazar: {listData.ownerUsername}</p>
        {!listData.isOwner && (
          <Link to={`/user/${listData.ownerId}`}>
            Yazarın Diğer Listeleri
          </Link>
        )}
      </div>

      {/* Düzenleme butonu (sadece sahip için) */}
      {listData.isOwner && (
        <button>Listeyi Düzenle</button>
      )}

      {/* Liste içeriği */}
      {listData.list.tiers.map(tier => (
        <div key={tier.id}>
          <h2>{tier.title}</h2>
          {tier.items.map(item => (
            <div key={item.id}>
              <img src={item.imageUrl} alt={item.title} />
              <span>{item.title}</span>
            </div>
          ))}
        </div>
      ))}
    </div>
  );
}
```

---

### 4. Tüm Public Listeleri Keşfet

**Endpoint:** `GET /api/share/public`

**Açıklama:** Tüm public listeleri beğeni ve görüntülenme sayısına göre sıralı şekilde getirir. Keşfet sayfası için kullanılır.

**Query Parameters:**
- `page` (int, opsiyonel): Sayfa numarası (varsayılan: 1)
- `limit` (int, opsiyonel): Sayfa başına liste sayısı (varsayılan: 20)

**Response:**
```json
{
  "response": {
    "lists": [
      {
        "id": 16,
        "title": "En İyi Anime'ler",
        "mode": "Ranked",
        "authorUsername": "kerem123",
        "authorId": 5,
        "viewCount": 150,
        "likeCount": 12,
        "createdAt": "2025-11-21T10:08:04.407586",
        "isLiked": false
      }
    ],
    "totalCount": 50,
    "page": 1,
    "totalPages": 3
  },
  "returnValue": 0,
  "errorMessage": "Public listeler başarıyla getirildi.",
  "errored": false
}
```

**Frontend Kullanımı:**
```typescript
// Keşfet sayfası için public listeleri getir
async function getPublicLists(page: number = 1, limit: number = 20) {
  const response = await fetch(
    `https://localhost:7132/api/share/public?page=${page}&limit=${limit}`,
    {
      method: 'GET'
    }
  );
  const data = await response.json();
  return data.response;
}
```

**Kullanım Senaryosu:**
- Keşfet/Explore sayfasında popüler listeleri gösterme
- "En Çok Beğenilen Listeler" bölümü

---

## Frontend Kullanım Örnekleri

### Senaryo 1: Profil Sayfası

Bir kullanıcının profiline girildiğinde, o kullanıcının public listelerini göstermek:

```typescript
// Profil sayfası component'i
function UserProfile({ userId }: { userId: number }) {
  const [userLists, setUserLists] = useState(null);
  const token = localStorage.getItem('token');

  useEffect(() => {
    async function loadUserLists() {
      const data = await getUserPublicLists(userId, token || undefined);
      setUserLists(data);
    }
    loadUserLists();
  }, [userId]);

  if (!userLists) return <div>Yükleniyor...</div>;

  return (
    <div>
      <h1>{userLists.username} Profili</h1>
      <h2>Public Listeler ({userLists.totalCount})</h2>
      
      {userLists.lists.map(list => (
        <Link key={list.id} to={`/list/${list.id}`}>
          <div>
            <h3>{list.title}</h3>
            <p>{list.itemCount} anime</p>
            <p>👁️ {list.viewCount} | ❤️ {list.likeCount}</p>
          </div>
        </Link>
      ))}
    </div>
  );
}
```

### Senaryo 2: Liste Detayında Yazar Bilgisi

Bir liste detayında, yazarın diğer listelerine link vermek:

```typescript
function ListDetailPage({ listId }: { listId: number }) {
  const [listData, setListData] = useState(null);
  const token = localStorage.getItem('token');

  useEffect(() => {
    async function loadList() {
      const data = await getListDetails(listId, token || undefined);
      setListData(data);
    }
    loadList();
  }, [listId]);

  if (!listData) return <div>Yükleniyor...</div>;

  return (
    <div>
      <h1>{listData.list.title}</h1>
      
      {/* Yazar bilgisi */}
      <div className="author-info">
        <p>Yazar: {listData.ownerUsername}</p>
        {!listData.isOwner && (
          <Link to={`/user/${listData.ownerId}`}>
            {listData.ownerUsername}'in Diğer Listeleri →
          </Link>
        )}
      </div>

      {/* Liste içeriği */}
      {/* ... */}
    </div>
  );
}
```

### Senaryo 3: Keşfet Sayfası

Keşfet sayfasında popüler listeleri göstermek:

```typescript
function ExplorePage() {
  const [publicLists, setPublicLists] = useState([]);
  const [page, setPage] = useState(1);

  useEffect(() => {
    async function loadLists() {
      const data = await getPublicLists(page, 20);
      setPublicLists(data.lists);
    }
    loadLists();
  }, [page]);

  return (
    <div>
      <h1>Keşfet</h1>
      <h2>Popüler Listeler</h2>
      
      {publicLists.map(list => (
        <Link key={list.id} to={`/list/${list.id}`}>
          <div>
            <h3>{list.title}</h3>
            <p>Yazar: {list.authorUsername}</p>
            <p>👁️ {list.viewCount} | ❤️ {list.likeCount}</p>
          </div>
        </Link>
      ))}
    </div>
  );
}
```

---

## Response Formatları

### Liste Özeti (List Summary)
```typescript
interface ListSummary {
  id: number;
  title: string;
  mode: string; // "Ranked", "Tiered", "Fusion"
  createdAt: string;
  modTime: string;
  tierCount: number;
  itemCount: number;
  userId?: number; // Kendi listelerinde
  viewCount?: number; // Public listelerde
  likeCount?: number; // Public listelerde
  authorUsername?: string; // Public listelerde
  authorId?: number; // Public listelerde
  isLiked?: boolean; // Giriş yapmışsa
}
```

### Liste Detayı (List Detail)
```typescript
interface ListDetailResponse {
  list: {
    id: number;
    title: string;
    mode: string;
    tiers: TierDto[];
  };
  ownerId: number;
  ownerUsername: string;
  isPublic: boolean;
  isOwner: boolean;
}
```

---

## Özet

| Endpoint | Amaç | Token Gerekli? | Kullanım Senaryosu |
|----------|------|----------------|---------------------|
| `GET /api/list/all` | Kendi listelerini getir | ✅ Evet | Dashboard, "Benim Listelerim" |
| `GET /api/list/user/{userId}` | Bir kullanıcının public listelerini getir | ❌ Opsiyonel | Profil sayfası |
| `GET /api/list/{listId}` | Liste detaylarını getir (public dahil) | ❌ Opsiyonel | Liste detay sayfası |
| `GET /api/share/public` | Tüm public listeleri keşfet | ❌ Hayır | Keşfet/Explore sayfası |

---

## Önemli Notlar

1. **Public Liste Kontrolü**: Bir liste `isPublic: true` ise, sahibi olmasa bile görüntülenebilir.
2. **Token Opsiyoneldir**: Public listeler için token gerekmez, ancak token varsa beğeni durumu ve sahip bilgisi gösterilir.
3. **userId Bilgisi**: Artık tüm response'larda `userId` veya `authorId` bilgisi mevcut, böylece frontend'de "Yazarın Diğer Listeleri" linki oluşturulabilir.
4. **Beğeni Durumu**: Giriş yapmış kullanıcılar için `isLiked` bilgisi gösterilir.

---

**Son Güncelleme:** 2025-01-21

