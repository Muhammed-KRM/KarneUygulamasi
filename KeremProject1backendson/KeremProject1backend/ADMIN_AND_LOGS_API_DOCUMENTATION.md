# Admin Kullanıcı Yönetimi ve Log API Dokümantasyonu

Bu dokümantasyon, admin kullanıcı yönetimi ve log görüntüleme fonksiyonlarının frontend'de nasıl kullanılacağını açıklar.

## 📋 İçindekiler

1. [Admin Kullanıcı Yönetimi](#admin-kullanıcı-yönetimi)
   - [Kullanıcı Güncelleme (Admin)](#1-kullanıcı-güncelleme-admin)
   - [Kullanıcı Listeleme (Admin)](#2-kullanıcı-listeleme-admin)
   - [Kullanıcı Silme](#3-kullanıcı-silme)
2. [Log Fonksiyonları](#log-fonksiyonları)
   - [Kendi Loglarını Görme](#1-kendi-loglarını-görme)
   - [Admin - Başka Kullanıcının Loglarını Görme](#2-admin---başka-kullanıcının-loglarını-görme)
   - [Admin - Tüm Logları Görme](#3-admin---tüm-logları-görme)
3. [Frontend Kullanım Örnekleri](#frontend-kullanım-örnekleri)
4. [TypeScript Interface'leri](#typescript-interfaces)

---

## Admin Kullanıcı Yönetimi

### 1. Kullanıcı Güncelleme (Admin)

**Endpoint:** `PUT /api/user/update`

**Açıklama:** Admin, başka kullanıcıların bilgilerini güncelleyebilir. Normal kullanıcılar sadece kendi bilgilerini güncelleyebilir.

**Headers:**
```
Token: <admin_token>
Content-Type: application/json
```

**Request Body:**
```typescript
{
  targetUserId?: number;      // null veya yoksa kendi hesabı, admin için başka kullanıcı ID'si
  username?: string;          // Yeni kullanıcı adı
  malUsername?: string;        // Yeni MAL kullanıcı adı
  role?: number;              // Admin için: 0=User, 1=Admin, 2=AdminAdmin
  state?: boolean;            // Admin için: true=aktif, false=pasif
}
```

**Response:**
```json
{
  "response": null,
  "returnValue": 0,
  "errorMessage": "Kullanıcı bilgileri başarıyla güncellendi.",
  "errored": false,
  "userId": 1
}
```

**Frontend Kullanımı:**

```typescript
// Kendi hesabını güncelleme (normal kullanıcı)
async function updateMyProfile(token: string, username?: string, malUsername?: string) {
  const response = await fetch('https://localhost:7132/api/user/update', {
    method: 'PUT',
    headers: {
      'Token': token,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      username: username,
      malUsername: malUsername
    })
  });
  const data = await response.json();
  return data;
}

// Admin - Başka kullanıcıyı güncelleme
async function updateUserAsAdmin(
  adminToken: string, 
  targetUserId: number,
  updates: {
    username?: string;
    malUsername?: string;
    role?: number;
    state?: boolean;
  }
) {
  const response = await fetch('https://localhost:7132/api/user/update', {
    method: 'PUT',
    headers: {
      'Token': adminToken,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      targetUserId: targetUserId,
      ...updates
    })
  });
  const data = await response.json();
  return data;
}

// Kullanım örneği
// Normal kullanıcı
await updateMyProfile(userToken, 'yeni_username', 'yeni_mal_username');

// Admin - Kullanıcıyı pasif yap
await updateUserAsAdmin(adminToken, 5, { state: false });

// Admin - Kullanıcıyı Admin yap (sadece AdminAdmin yapabilir)
await updateUserAsAdmin(adminToken, 5, { role: 1 });
```

**React Component Örneği:**

```tsx
function AdminUserEditModal({ userId, onClose }: { userId: number; onClose: () => void }) {
  const [username, setUsername] = useState('');
  const [malUsername, setMalUsername] = useState('');
  const [role, setRole] = useState(0);
  const [state, setState] = useState(true);
  const token = localStorage.getItem('token');

  const handleUpdate = async () => {
    try {
      const response = await updateUserAsAdmin(token!, userId, {
        username: username || undefined,
        malUsername: malUsername || undefined,
        role: role,
        state: state
      });

      if (!response.errored) {
        alert('Kullanıcı başarıyla güncellendi!');
        onClose();
      } else {
        alert(`Hata: ${response.errorMessage}`);
      }
    } catch (error) {
      console.error('Güncelleme hatası:', error);
    }
  };

  return (
    <div className="modal">
      <h2>Kullanıcı Düzenle</h2>
      <input
        type="text"
        placeholder="Kullanıcı adı"
        value={username}
        onChange={(e) => setUsername(e.target.value)}
      />
      <input
        type="text"
        placeholder="MAL kullanıcı adı"
        value={malUsername}
        onChange={(e) => setMalUsername(e.target.value)}
      />
      <select value={role} onChange={(e) => setRole(Number(e.target.value))}>
        <option value={0}>User</option>
        <option value={1}>Admin</option>
        <option value={2}>AdminAdmin</option>
      </select>
      <label>
        <input
          type="checkbox"
          checked={state}
          onChange={(e) => setState(e.target.checked)}
        />
        Aktif
      </label>
      <button onClick={handleUpdate}>Güncelle</button>
      <button onClick={onClose}>İptal</button>
    </div>
  );
}
```

---

### 2. Kullanıcı Listeleme (Admin)

**Endpoint:** `GET /api/user/all`

**Açıklama:** Tüm kullanıcıları listeler. Admin kontrolü eklendi, admin daha detaylı bilgiler görebilir.

**Headers (Opsiyonel):**
```
Token: <admin_token>  // Admin ise detaylı bilgiler
```

**Query Parameters:**
```
page=1          // Sayfa numarası
limit=20         // Sayfa başına kayıt sayısı
searchQuery=     // Arama sorgusu (opsiyonel)
isActive=true    // Aktif/pasif filtresi (opsiyonel)
```

**Response:**
```json
{
  "response": {
    "users": [
      {
        "id": 5,
        "username": "kerem123",
        "userImageLink": "https://...",
        "malUsername": "kerem_mal",
        "totalLists": 10,
        "totalFollowers": 25,
        "modTime": "2025-01-21T10:00:00"
      }
    ],
    "totalCount": 100,
    "page": 1,
    "limit": 20,
    "totalPages": 5
  },
  "returnValue": 0,
  "errorMessage": "Kullanıcılar başarıyla getirildi.",
  "errored": false
}
```

**Frontend Kullanımı:**

```typescript
async function getAllUsers(
  token?: string,
  page: number = 1,
  limit: number = 20,
  searchQuery?: string,
  isActive?: boolean
) {
  const params = new URLSearchParams({
    page: page.toString(),
    limit: limit.toString()
  });
  
  if (searchQuery) params.append('searchQuery', searchQuery);
  if (isActive !== undefined) params.append('isActive', isActive.toString());

  const headers: HeadersInit = {};
  if (token) headers['Token'] = token;

  const response = await fetch(
    `https://localhost:7132/api/user/all?${params.toString()}`,
    {
      method: 'GET',
      headers
    }
  );
  const data = await response.json();
  return data.response;
}

// Kullanım örneği
const users = await getAllUsers(adminToken, 1, 20, 'kerem', true);
console.log(users.users); // Kullanıcı listesi
console.log(users.totalCount); // Toplam kullanıcı sayısı
```

**React Component Örneği:**

```tsx
function AdminUserList() {
  const [users, setUsers] = useState([]);
  const [page, setPage] = useState(1);
  const [searchQuery, setSearchQuery] = useState('');
  const token = localStorage.getItem('token');

  useEffect(() => {
    async function loadUsers() {
      const data = await getAllUsers(token || undefined, page, 20, searchQuery || undefined);
      setUsers(data.users);
    }
    loadUsers();
  }, [page, searchQuery]);

  return (
    <div>
      <h1>Kullanıcı Yönetimi</h1>
      <input
        type="text"
        placeholder="Kullanıcı ara..."
        value={searchQuery}
        onChange={(e) => setSearchQuery(e.target.value)}
      />
      <table>
        <thead>
          <tr>
            <th>ID</th>
            <th>Kullanıcı Adı</th>
            <th>MAL Username</th>
            <th>Liste Sayısı</th>
            <th>Takipçi</th>
            <th>İşlemler</th>
          </tr>
        </thead>
        <tbody>
          {users.map(user => (
            <tr key={user.id}>
              <td>{user.id}</td>
              <td>{user.username}</td>
              <td>{user.malUsername || '-'}</td>
              <td>{user.totalLists}</td>
              <td>{user.totalFollowers}</td>
              <td>
                <button onClick={() => editUser(user.id)}>Düzenle</button>
                <button onClick={() => deleteUser(user.id)}>Sil</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      <div>
        <button onClick={() => setPage(p => Math.max(1, p - 1))}>Önceki</button>
        <span>Sayfa {page}</span>
        <button onClick={() => setPage(p => p + 1)}>Sonraki</button>
      </div>
    </div>
  );
}
```

---

### 3. Kullanıcı Silme

**Endpoint:** `DELETE /api/user`

**Açıklama:** Kullanıcı silme (soft delete veya hard delete). Admin veya kendi hesabını silebilir.

**Headers:**
```
Token: <token>
Content-Type: application/json
```

**Request Body:**
```typescript
{
  userId: number;        // Silinecek kullanıcı ID'si
  hardDelete: boolean;   // true ise kalıcı sil, false ise soft delete (State = false)
  password?: string;     // Kendi hesabını silerken şifre doğrulama için
}
```

**Frontend Kullanımı:**

```typescript
async function deleteUser(
  token: string,
  userId: number,
  hardDelete: boolean = false,
  password?: string
) {
  const response = await fetch('https://localhost:7132/api/user', {
    method: 'DELETE',
    headers: {
      'Token': token,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      userId: userId,
      hardDelete: hardDelete,
      password: password
    })
  });
  const data = await response.json();
  return data;
}

// Kullanım örneği
// Soft delete (hesabı pasif yap)
await deleteUser(adminToken, 5, false);

// Hard delete (kalıcı sil)
await deleteUser(adminToken, 5, true);
```

---

## Log Fonksiyonları

### 1. Kendi Loglarını Görme

**Endpoint:** `POST /api/log/user`

**Açıklama:** Kullanıcının kendi loglarını görüntüler. Admin, başka kullanıcının loglarını da görebilir.

**Headers:**
```
Token: <token>
Content-Type: application/json
```

**Request Body:**
```typescript
{
  userId?: number;           // null ise kendi logları, admin için başka kullanıcı ID'si
  page: number;             // Sayfa numarası (varsayılan: 1)
  limit: number;            // Sayfa başına kayıt sayısı (varsayılan: 20)
  tableName?: string;       // Tablo adı filtresi (opsiyonel)
  action?: string;          // 'C'=Create, 'U'=Update, 'D'=Delete (opsiyonel)
  startDate?: string;       // Başlangıç tarihi (ISO format) (opsiyonel)
  endDate?: string;         // Bitiş tarihi (ISO format) (opsiyonel)
}
```

**Response:**
```json
{
  "response": {
    "logs": [
      {
        "id": 1,
        "tableName": "AppUsers",
        "oldValue": "{\"username\":\"eski_username\"}",
        "newValue": "{\"username\":\"yeni_username\"}",
        "action": "U",
        "actionName": "Güncellendi",
        "oldModUser": null,
        "oldModUsername": null,
        "oldModTime": null,
        "modUser": 5,
        "modUsername": "kerem123",
        "modTime": "2025-01-21T10:00:00"
      }
    ],
    "totalCount": 50,
    "page": 1,
    "limit": 20,
    "totalPages": 3
  },
  "returnValue": 0,
  "errorMessage": "Loglar başarıyla getirildi.",
  "errored": false,
  "userId": 5
}
```

**Frontend Kullanımı:**

```typescript
async function getUserLogs(
  token: string,
  options: {
    userId?: number;
    page?: number;
    limit?: number;
    tableName?: string;
    action?: 'C' | 'U' | 'D';
    startDate?: Date;
    endDate?: Date;
  } = {}
) {
  const body: any = {
    page: options.page || 1,
    limit: options.limit || 20
  };

  if (options.userId !== undefined) body.userId = options.userId;
  if (options.tableName) body.tableName = options.tableName;
  if (options.action) body.action = options.action;
  if (options.startDate) body.startDate = options.startDate.toISOString();
  if (options.endDate) body.endDate = options.endDate.toISOString();

  const response = await fetch('https://localhost:7132/api/log/user', {
    method: 'POST',
    headers: {
      'Token': token,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(body)
  });
  const data = await response.json();
  return data.response;
}

// Kullanım örnekleri
// Kendi loglarını getir
const myLogs = await getUserLogs(userToken);

// Son 7 günün logları
const lastWeek = new Date();
lastWeek.setDate(lastWeek.getDate() - 7);
const recentLogs = await getUserLogs(userToken, {
  startDate: lastWeek,
  endDate: new Date()
});

// Sadece güncelleme logları
const updateLogs = await getUserLogs(userToken, {
  action: 'U'
});

// Admin - Başka kullanıcının logları
const userLogs = await getUserLogs(adminToken, {
  userId: 5
});
```

**React Component Örneği:**

```tsx
function MyLogsPage() {
  const [logs, setLogs] = useState([]);
  const [page, setPage] = useState(1);
  const [filter, setFilter] = useState({
    tableName: '',
    action: '',
    startDate: '',
    endDate: ''
  });
  const token = localStorage.getItem('token');

  useEffect(() => {
    async function loadLogs() {
      const options: any = {
        page: page,
        limit: 20
      };

      if (filter.tableName) options.tableName = filter.tableName;
      if (filter.action) options.action = filter.action;
      if (filter.startDate) options.startDate = new Date(filter.startDate);
      if (filter.endDate) options.endDate = new Date(filter.endDate);

      const data = await getUserLogs(token!, options);
      setLogs(data.logs);
    }
    loadLogs();
  }, [page, filter]);

  return (
    <div>
      <h1>Loglarım</h1>
      
      {/* Filtreler */}
      <div className="filters">
        <select
          value={filter.action}
          onChange={(e) => setFilter({ ...filter, action: e.target.value })}
        >
          <option value="">Tüm Aksiyonlar</option>
          <option value="C">Oluşturuldu</option>
          <option value="U">Güncellendi</option>
          <option value="D">Silindi</option>
        </select>
        <input
          type="text"
          placeholder="Tablo adı"
          value={filter.tableName}
          onChange={(e) => setFilter({ ...filter, tableName: e.target.value })}
        />
        <input
          type="date"
          value={filter.startDate}
          onChange={(e) => setFilter({ ...filter, startDate: e.target.value })}
        />
        <input
          type="date"
          value={filter.endDate}
          onChange={(e) => setFilter({ ...filter, endDate: e.target.value })}
        />
      </div>

      {/* Log listesi */}
      <table>
        <thead>
          <tr>
            <th>Tarih</th>
            <th>Tablo</th>
            <th>Aksiyon</th>
            <th>Kullanıcı</th>
            <th>Detay</th>
          </tr>
        </thead>
        <tbody>
          {logs.map(log => (
            <tr key={log.id}>
              <td>{new Date(log.modTime).toLocaleString('tr-TR')}</td>
              <td>{log.tableName}</td>
              <td>{log.actionName}</td>
              <td>{log.modUsername}</td>
              <td>
                <button onClick={() => showLogDetail(log)}>Detay</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      {/* Sayfalama */}
      <div>
        <button onClick={() => setPage(p => Math.max(1, p - 1))}>Önceki</button>
        <span>Sayfa {page}</span>
        <button onClick={() => setPage(p => p + 1)}>Sonraki</button>
      </div>
    </div>
  );
}
```

---

### 2. Admin - Başka Kullanıcının Loglarını Görme

**Endpoint:** `POST /api/log/user`

**Açıklama:** Admin, başka kullanıcının loglarını görebilir. Aynı endpoint, sadece `userId` parametresi ile kullanılır.

**Frontend Kullanımı:**

```typescript
// Admin - Belirli kullanıcının loglarını getir
async function getUserLogsAsAdmin(
  adminToken: string,
  targetUserId: number,
  page: number = 1,
  limit: number = 20
) {
  const data = await getUserLogs(adminToken, {
    userId: targetUserId,
    page: page,
    limit: limit
  });
  return data;
}

// Kullanım örneği
const user5Logs = await getUserLogsAsAdmin(adminToken, 5);
```

**React Component Örneği:**

```tsx
function AdminUserLogsModal({ userId, onClose }: { userId: number; onClose: () => void }) {
  const [logs, setLogs] = useState([]);
  const [page, setPage] = useState(1);
  const token = localStorage.getItem('token');

  useEffect(() => {
    async function loadLogs() {
      const data = await getUserLogs(token!, {
        userId: userId,
        page: page,
        limit: 20
      });
      setLogs(data.logs);
    }
    loadLogs();
  }, [userId, page]);

  return (
    <div className="modal">
      <h2>Kullanıcı #{userId} Logları</h2>
      <table>
        <thead>
          <tr>
            <th>Tarih</th>
            <th>Tablo</th>
            <th>Aksiyon</th>
            <th>Detay</th>
          </tr>
        </thead>
        <tbody>
          {logs.map(log => (
            <tr key={log.id}>
              <td>{new Date(log.modTime).toLocaleString('tr-TR')}</td>
              <td>{log.tableName}</td>
              <td>{log.actionName}</td>
              <td>
                <button onClick={() => showLogDetail(log)}>Detay</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      <button onClick={onClose}>Kapat</button>
    </div>
  );
}
```

---

### 3. Admin - Tüm Logları Görme

**Endpoint:** `POST /api/log/admin/all`

**Açıklama:** Admin, sistemdeki tüm logları görüntüleyebilir. Sadece Admin ve AdminAdmin kullanabilir.

**Headers:**
```
Token: <admin_token>
Content-Type: application/json
```

**Request Body:**
```typescript
{
  page: number;             // Sayfa numarası (varsayılan: 1)
  limit: number;            // Sayfa başına kayıt sayısı (varsayılan: 50)
  userId?: number;          // Belirli kullanıcının logları (opsiyonel)
  tableName?: string;       // Tablo adı filtresi (opsiyonel)
  action?: string;          // 'C'=Create, 'U'=Update, 'D'=Delete (opsiyonel)
  startDate?: string;       // Başlangıç tarihi (ISO format) (opsiyonel)
  endDate?: string;         // Bitiş tarihi (ISO format) (opsiyonel)
}
```

**Response:**
```json
{
  "response": {
    "logs": [
      {
        "id": 1,
        "tableName": "AppUsers",
        "oldValue": "...",
        "newValue": "...",
        "action": "U",
        "actionName": "Güncellendi",
        "modUser": 5,
        "modUsername": "kerem123",
        "modTime": "2025-01-21T10:00:00"
      }
    ],
    "totalCount": 1000,
    "page": 1,
    "limit": 50,
    "totalPages": 20
  },
  "returnValue": 0,
  "errorMessage": "Tüm loglar başarıyla getirildi.",
  "errored": false,
  "userId": 1
}
```

**Frontend Kullanımı:**

```typescript
async function getAdminLogs(
  adminToken: string,
  options: {
    page?: number;
    limit?: number;
    userId?: number;
    tableName?: string;
    action?: 'C' | 'U' | 'D';
    startDate?: Date;
    endDate?: Date;
  } = {}
) {
  const body: any = {
    page: options.page || 1,
    limit: options.limit || 50
  };

  if (options.userId !== undefined) body.userId = options.userId;
  if (options.tableName) body.tableName = options.tableName;
  if (options.action) body.action = options.action;
  if (options.startDate) body.startDate = options.startDate.toISOString();
  if (options.endDate) body.endDate = options.endDate.toISOString();

  const response = await fetch('https://localhost:7132/api/log/admin/all', {
    method: 'POST',
    headers: {
      'Token': adminToken,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(body)
  });
  const data = await response.json();
  return data.response;
}

// Kullanım örnekleri
// Tüm loglar
const allLogs = await getAdminLogs(adminToken);

// Belirli kullanıcının logları
const userLogs = await getAdminLogs(adminToken, {
  userId: 5
});

// Son 24 saat
const yesterday = new Date();
yesterday.setDate(yesterday.getDate() - 1);
const recentLogs = await getAdminLogs(adminToken, {
  startDate: yesterday,
  endDate: new Date()
});

// Sadece AppUsers tablosundaki güncellemeler
const userUpdates = await getAdminLogs(adminToken, {
  tableName: 'AppUsers',
  action: 'U'
});
```

**React Component Örneği:**

```tsx
function AdminLogsPage() {
  const [logs, setLogs] = useState([]);
  const [page, setPage] = useState(1);
  const [filters, setFilters] = useState({
    userId: '',
    tableName: '',
    action: '',
    startDate: '',
    endDate: ''
  });
  const token = localStorage.getItem('token');

  useEffect(() => {
    async function loadLogs() {
      const options: any = {
        page: page,
        limit: 50
      };

      if (filters.userId) options.userId = Number(filters.userId);
      if (filters.tableName) options.tableName = filters.tableName;
      if (filters.action) options.action = filters.action;
      if (filters.startDate) options.startDate = new Date(filters.startDate);
      if (filters.endDate) options.endDate = new Date(filters.endDate);

      const data = await getAdminLogs(token!, options);
      setLogs(data.logs);
    }
    loadLogs();
  }, [page, filters]);

  return (
    <div>
      <h1>Admin - Tüm Loglar</h1>
      
      {/* Filtreler */}
      <div className="filters">
        <input
          type="number"
          placeholder="Kullanıcı ID"
          value={filters.userId}
          onChange={(e) => setFilters({ ...filters, userId: e.target.value })}
        />
        <input
          type="text"
          placeholder="Tablo adı"
          value={filters.tableName}
          onChange={(e) => setFilters({ ...filters, tableName: e.target.value })}
        />
        <select
          value={filters.action}
          onChange={(e) => setFilters({ ...filters, action: e.target.value })}
        >
          <option value="">Tüm Aksiyonlar</option>
          <option value="C">Oluşturuldu</option>
          <option value="U">Güncellendi</option>
          <option value="D">Silindi</option>
        </select>
        <input
          type="date"
          value={filters.startDate}
          onChange={(e) => setFilters({ ...filters, startDate: e.target.value })}
        />
        <input
          type="date"
          value={filters.endDate}
          onChange={(e) => setFilters({ ...filters, endDate: e.target.value })}
        />
      </div>

      {/* Log listesi */}
      <table>
        <thead>
          <tr>
            <th>ID</th>
            <th>Tarih</th>
            <th>Tablo</th>
            <th>Aksiyon</th>
            <th>Kullanıcı</th>
            <th>Detay</th>
          </tr>
        </thead>
        <tbody>
          {logs.map(log => (
            <tr key={log.id}>
              <td>{log.id}</td>
              <td>{new Date(log.modTime).toLocaleString('tr-TR')}</td>
              <td>{log.tableName}</td>
              <td>{log.actionName}</td>
              <td>{log.modUsername} (#{log.modUser})</td>
              <td>
                <button onClick={() => showLogDetail(log)}>Detay</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      {/* Sayfalama */}
      <div>
        <button onClick={() => setPage(p => Math.max(1, p - 1))}>Önceki</button>
        <span>Sayfa {page}</span>
        <button onClick={() => setPage(p => p + 1)}>Sonraki</button>
      </div>
    </div>
  );
}
```

---

## TypeScript Interfaces

```typescript
// Request Interfaces
interface UpdateUserRequest {
  targetUserId?: number;
  username?: string;
  malUsername?: string;
  role?: number;  // 0=User, 1=Admin, 2=AdminAdmin
  state?: boolean;
}

interface GetUserLogsRequest {
  userId?: number;
  page?: number;
  limit?: number;
  tableName?: string;
  action?: 'C' | 'U' | 'D';
  startDate?: string;  // ISO format
  endDate?: string;    // ISO format
}

interface GetAdminLogsRequest {
  page?: number;
  limit?: number;
  userId?: number;
  tableName?: string;
  action?: 'C' | 'U' | 'D';
  startDate?: string;
  endDate?: string;
}

// Response Interfaces
interface LogDto {
  id: number;
  tableName: string;
  oldValue?: string;
  newValue?: string;
  action: 'C' | 'U' | 'D';
  actionName: string;
  oldModUser?: number;
  oldModUsername?: string;
  oldModTime?: string;
  modUser: number;
  modUsername: string;
  modTime: string;
}

interface LogListResponse {
  logs: LogDto[];
  totalCount: number;
  page: number;
  limit: number;
  totalPages: number;
}

interface UserSummaryDto {
  id: number;
  username: string;
  userImageLink: string;
  malUsername?: string;
  totalLists: number;
  totalFollowers: number;
  modTime: string;
}

interface UserListResponse {
  users: UserSummaryDto[];
  totalCount: number;
  page: number;
  limit: number;
  totalPages: number;
}
```

---

## Özet Tablo

| Endpoint | Method | Amaç | Token Gerekli? | Admin Gerekli? |
|----------|--------|------|----------------|----------------|
| `/api/user/update` | PUT | Kullanıcı güncelleme | ✅ Evet | ❌ (Admin için ek özellikler) |
| `/api/user/all` | GET | Tüm kullanıcıları listele | ❌ Opsiyonel | ❌ (Admin için detaylı bilgiler) |
| `/api/user` | DELETE | Kullanıcı silme | ✅ Evet | ❌ (Admin veya kendi hesabı) |
| `/api/log/user` | POST | Kullanıcı loglarını görme | ✅ Evet | ❌ (Admin başka kullanıcının loglarını görebilir) |
| `/api/log/admin/all` | POST | Tüm logları görme | ✅ Evet | ✅ Evet |

---

## Önemli Notlar

1. **Admin Yetkileri:**
   - Admin, başka kullanıcıları güncelleyebilir (`targetUserId` parametresi ile)
   - Admin, kullanıcıların rolünü değiştirebilir (ama AdminAdmin sadece AdminAdmin yapabilir)
   - Admin, kullanıcıları pasif/aktif yapabilir (`state` parametresi ile)

2. **Log Filtreleme:**
   - Tüm log endpoint'leri tarih aralığı, tablo adı ve aksiyon tipi ile filtrelenebilir
   - Sayfalama her zaman mevcuttur

3. **Güvenlik:**
   - Tüm admin işlemleri token ile doğrulanır
   - Admin kontrolü backend'de yapılır
   - Normal kullanıcılar sadece kendi verilerini görebilir/düzenleyebilir

---

**Son Güncelleme:** 2025-01-21

