# Kullanıcı Profil ve Ayarlar API Dokümantasyonu

Bu dokümantasyon, kullanıcının kendi profilini ve ayarlarını yönetmek için kullanılan tüm endpoint'leri ve frontend kullanım örneklerini içerir.

## 📋 İçindekiler

1. [Profil Bilgileri](#profil-bilgileri)
   - [Kendi Profilimi Getir](#1-kendi-profilimi-getir)
   - [Profil Güncelle](#2-profil-güncelle)
   - [Profil Resmi Yükle](#3-profil-resmi-yükle)
2. [Şifre Yönetimi](#şifre-yönetimi)
   - [Şifre Değiştir](#1-şifre-değiştir)
3. [MAL (MyAnimeList) Entegrasyonu](#mal-myanimelist-entegrasyonu)
   - [MAL Bağlantı URL'i Al](#1-mal-bağlantı-url-i-al)
   - [MAL Callback İşle](#2-mal-callback-işle)
   - [MAL Bağlantısını Kaldır](#3-mal-bağlantısını-kaldır)
4. [Hesap Yönetimi](#hesap-yönetimi)
   - [Hesabı Sil](#1-hesabı-sil)
5. [Frontend Kullanım Örnekleri](#frontend-kullanım-örnekleri)
   - [Tam Ayarlar Sayfası Örneği](#tam-ayarlar-sayfası-örneği)

---

## Profil Bilgileri

### 1. Kendi Profilimi Getir

**Endpoint:** `GET /api/user/me`

**Açıklama:** Kullanıcının kendi profil bilgilerini getirir. Token'dan otomatik olarak kullanıcı ID'si alınır.

**Headers:**
```
Token: <kullanıcı_token>
```

**Response:**
```json
{
  "response": {
    "id": 5,
    "username": "kerem123",
    "role": "User",
    "userImageLink": "https://localhost:7123/api/file/download?filename=...",
    "malUsername": "kerem_mal",
    "modTime": "2025-01-21T10:00:00"
  },
  "returnValue": 0,
  "errorMessage": "Kullanıcı bilgileri başarıyla getirildi.",
  "errored": false,
  "userId": 5
}
```

**Frontend Kullanımı:**

```typescript
async function getMyProfile(token: string) {
  const response = await fetch('https://localhost:7132/api/user/me', {
    method: 'GET',
    headers: {
      'Token': token
    }
  });
  const data = await response.json();
  return data.response;
}

// Kullanım örneği
const profile = await getMyProfile(userToken);
console.log(profile.username); // "kerem123"
console.log(profile.malUsername); // "kerem_mal"
```

---

### 2. Profil Güncelle

**Endpoint:** `PUT /api/user/profile`

**Açıklama:** Kullanıcının kendi profil bilgilerini günceller (kullanıcı adı ve MAL kullanıcı adı).

**Headers:**
```
Token: <kullanıcı_token>
Content-Type: application/json
```

**Request Body:**
```typescript
{
  username?: string;      // Yeni kullanıcı adı (3-50 karakter)
  malUsername?: string;   // Yeni MAL kullanıcı adı
}
```

**Response:**
```json
{
  "response": null,
  "returnValue": 0,
  "errorMessage": "Profil başarıyla güncellendi.",
  "errored": false,
  "userId": 5
}
```

**Frontend Kullanımı:**

```typescript
async function updateProfile(
  token: string,
  username?: string,
  malUsername?: string
) {
  const body: any = {};
  if (username) body.username = username;
  if (malUsername !== undefined) body.malUsername = malUsername;

  const response = await fetch('https://localhost:7132/api/user/profile', {
    method: 'PUT',
    headers: {
      'Token': token,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(body)
  });
  const data = await response.json();
  return data;
}

// Kullanım örnekleri
// Sadece kullanıcı adını güncelle
await updateProfile(userToken, 'yeni_username');

// Sadece MAL username'i güncelle
await updateProfile(userToken, undefined, 'yeni_mal_username');

// Her ikisini de güncelle
await updateProfile(userToken, 'yeni_username', 'yeni_mal_username');
```

**React Component Örneği:**

```tsx
function ProfileSettingsForm() {
  const [username, setUsername] = useState('');
  const [malUsername, setMalUsername] = useState('');
  const [loading, setLoading] = useState(false);
  const token = localStorage.getItem('token');

  useEffect(() => {
    // Profil bilgilerini yükle
    async function loadProfile() {
      const profile = await getMyProfile(token!);
      setUsername(profile.username);
      setMalUsername(profile.malUsername || '');
    }
    loadProfile();
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    
    try {
      const result = await updateProfile(token!, username, malUsername);
      if (!result.errored) {
        alert('Profil başarıyla güncellendi!');
      } else {
        alert(`Hata: ${result.errorMessage}`);
      }
    } catch (error) {
      console.error('Güncelleme hatası:', error);
      alert('Bir hata oluştu!');
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <div>
        <label>Kullanıcı Adı</label>
        <input
          type="text"
          value={username}
          onChange={(e) => setUsername(e.target.value)}
          required
          minLength={3}
          maxLength={50}
        />
      </div>
      <div>
        <label>MAL Kullanıcı Adı</label>
        <input
          type="text"
          value={malUsername}
          onChange={(e) => setMalUsername(e.target.value)}
          maxLength={50}
        />
      </div>
      <button type="submit" disabled={loading}>
        {loading ? 'Kaydediliyor...' : 'Kaydet'}
      </button>
    </form>
  );
}
```

---

### 3. Profil Resmi Yükle

**Endpoint:** `POST /api/user/upload-image`

**Açıklama:** Kullanıcının profil resmini yükler. Eski resim otomatik olarak silinir.

**Headers:**
```
Token: <kullanıcı_token>
Content-Type: multipart/form-data
```

**Request Body (FormData):**
```typescript
{
  file: File  // Resim dosyası (jpg, jpeg, png, gif, webp, max 5MB)
}
```

**Response:**
```json
{
  "response": {
    "imageLink": "https://localhost:7123/api/file/download?filename=...",
    "fileName": "user_5_1234567890.jpg"
  },
  "returnValue": 0,
  "errorMessage": "Profil resmi başarıyla yüklendi.",
  "errored": false,
  "userId": 5
}
```

**Frontend Kullanımı:**

```typescript
async function uploadProfileImage(token: string, file: File) {
  const formData = new FormData();
  formData.append('file', file);

  const response = await fetch('https://localhost:7132/api/user/upload-image', {
    method: 'POST',
    headers: {
      'Token': token
      // Content-Type header'ını EKLEMEYİN - browser otomatik ekler
    },
    body: formData
  });
  const data = await response.json();
  return data;
}

// Kullanım örneği
const fileInput = document.querySelector('input[type="file"]') as HTMLInputElement;
fileInput.addEventListener('change', async (e) => {
  const file = (e.target as HTMLInputElement).files?.[0];
  if (file) {
    // Dosya tipi kontrolü
    const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'];
    if (!allowedTypes.includes(file.type)) {
      alert('Sadece resim dosyaları kabul edilir!');
      return;
    }

    // Dosya boyutu kontrolü (5MB)
    if (file.size > 5 * 1024 * 1024) {
      alert('Dosya boyutu 5MB\'dan küçük olmalıdır!');
      return;
    }

    const result = await uploadProfileImage(userToken, file);
    if (!result.errored) {
      console.log('Yeni profil resmi:', result.response.imageLink);
      // Profil resmini UI'da güncelle
    } else {
      alert(`Hata: ${result.errorMessage}`);
    }
  }
});
```

**React Component Örneği:**

```tsx
function ProfileImageUpload() {
  const [imageUrl, setImageUrl] = useState('');
  const [uploading, setUploading] = useState(false);
  const token = localStorage.getItem('token');

  useEffect(() => {
    // Mevcut profil resmini yükle
    async function loadProfile() {
      const profile = await getMyProfile(token!);
      setImageUrl(profile.userImageLink || '');
    }
    loadProfile();
  }, []);

  const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    // Dosya validasyonu
    const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'];
    if (!allowedTypes.includes(file.type)) {
      alert('Sadece resim dosyaları kabul edilir! (jpg, jpeg, png, gif, webp)');
      return;
    }

    if (file.size > 5 * 1024 * 1024) {
      alert('Dosya boyutu 5MB\'dan küçük olmalıdır!');
      return;
    }

    setUploading(true);
    try {
      const result = await uploadProfileImage(token!, file);
      if (!result.errored) {
        setImageUrl(result.response.imageLink);
        alert('Profil resmi başarıyla yüklendi!');
      } else {
        alert(`Hata: ${result.errorMessage}`);
      }
    } catch (error) {
      console.error('Yükleme hatası:', error);
      alert('Bir hata oluştu!');
    } finally {
      setUploading(false);
    }
  };

  return (
    <div className="profile-image-upload">
      <div className="image-preview">
        {imageUrl ? (
          <img src={imageUrl} alt="Profil resmi" />
        ) : (
          <div className="placeholder">Profil resmi yok</div>
        )}
      </div>
      <label className="upload-button">
        {uploading ? 'Yükleniyor...' : 'Resim Seç'}
        <input
          type="file"
          accept="image/jpeg,image/jpg,image/png,image/gif,image/webp"
          onChange={handleFileChange}
          disabled={uploading}
          style={{ display: 'none' }}
        />
      </label>
    </div>
  );
}
```

---

## Şifre Yönetimi

### 1. Şifre Değiştir

**Endpoint:** `POST /api/user/change-password`

**Açıklama:** Kullanıcının şifresini değiştirir. Mevcut şifre doğrulaması yapılır.

**Headers:**
```
Token: <kullanıcı_token>
Content-Type: application/json
```

**Request Body:**
```typescript
{
  currentPassword: string;  // Mevcut şifre
  newPassword: string;       // Yeni şifre (minimum 6 karakter)
}
```

**Response:**
```json
{
  "response": null,
  "returnValue": 0,
  "errorMessage": "Şifre başarıyla değiştirildi.",
  "errored": false,
  "userId": 5
}
```

**Hata Durumları:**
- `2015`: "Mevcut şifre hatalı."
- `2016`: Şifre değiştirilirken hata

**Frontend Kullanımı:**

```typescript
async function changePassword(
  token: string,
  currentPassword: string,
  newPassword: string
) {
  const response = await fetch('https://localhost:7132/api/user/change-password', {
    method: 'POST',
    headers: {
      'Token': token,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      currentPassword: currentPassword,
      newPassword: newPassword
    })
  });
  const data = await response.json();
  return data;
}

// Kullanım örneği
const result = await changePassword(userToken, 'eski_sifre', 'yeni_sifre123');
if (!result.errored) {
  alert('Şifre başarıyla değiştirildi!');
} else {
  if (result.returnValue === 2015) {
    alert('Mevcut şifre hatalı!');
  } else {
    alert(`Hata: ${result.errorMessage}`);
  }
}
```

**React Component Örneği:**

```tsx
function ChangePasswordForm() {
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const token = localStorage.getItem('token');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    // Validasyon
    if (newPassword.length < 6) {
      setError('Yeni şifre en az 6 karakter olmalıdır!');
      return;
    }

    if (newPassword !== confirmPassword) {
      setError('Yeni şifreler eşleşmiyor!');
      return;
    }

    setLoading(true);
    try {
      const result = await changePassword(token!, currentPassword, newPassword);
      if (!result.errored) {
        alert('Şifre başarıyla değiştirildi!');
        setCurrentPassword('');
        setNewPassword('');
        setConfirmPassword('');
      } else {
        if (result.returnValue === 2015) {
          setError('Mevcut şifre hatalı!');
        } else {
          setError(result.errorMessage);
        }
      }
    } catch (error) {
      console.error('Şifre değiştirme hatası:', error);
      setError('Bir hata oluştu!');
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <h2>Şifre Değiştir</h2>
      {error && <div className="error">{error}</div>}
      <div>
        <label>Mevcut Şifre</label>
        <input
          type="password"
          value={currentPassword}
          onChange={(e) => setCurrentPassword(e.target.value)}
          required
        />
      </div>
      <div>
        <label>Yeni Şifre</label>
        <input
          type="password"
          value={newPassword}
          onChange={(e) => setNewPassword(e.target.value)}
          required
          minLength={6}
        />
      </div>
      <div>
        <label>Yeni Şifre (Tekrar)</label>
        <input
          type="password"
          value={confirmPassword}
          onChange={(e) => setConfirmPassword(e.target.value)}
          required
          minLength={6}
        />
      </div>
      <button type="submit" disabled={loading}>
        {loading ? 'Değiştiriliyor...' : 'Şifreyi Değiştir'}
      </button>
    </form>
  );
}
```

---

## MAL (MyAnimeList) Entegrasyonu

### 1. MAL Bağlantı URL'i Al

**Endpoint:** `GET /api/mal/get-auth-url`

**Açıklama:** MyAnimeList hesabını bağlamak için OAuth URL'ini döndürür.

**Headers:**
```
Token: <kullanıcı_token>
```

**Response:**
```json
{
  "response": {
    "authUrl": "https://myanimelist.net/v1/oauth2/authorize?response_type=code&client_id=...&redirect_uri=..."
  },
  "returnValue": 0,
  "errorMessage": "MAL bağlantı URL'i başarıyla oluşturuldu.",
  "errored": false
}
```

**Frontend Kullanımı:**

```typescript
async function getMalAuthUrl(token: string) {
  const response = await fetch('https://localhost:7132/api/mal/get-auth-url', {
    method: 'GET',
    headers: {
      'Token': token
    }
  });
  const data = await response.json();
  return data.response?.authUrl;
}

// Kullanım örneği
const authUrl = await getMalAuthUrl(userToken);
if (authUrl) {
  // Kullanıcıyı MAL OAuth sayfasına yönlendir
  window.location.href = authUrl;
}
```

---

### 2. MAL Callback İşle

**Endpoint:** `GET /api/mal/callback?code={code}`

**Açıklama:** MAL OAuth callback'ini işler ve kullanıcının MAL hesabını bağlar. Bu endpoint genellikle MAL'den yönlendirme sonrası otomatik çağrılır.

**Query Parameters:**
- `code`: OAuth authorization code

**Headers:**
```
Token: <kullanıcı_token>
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

**Frontend Kullanımı:**

```typescript
// Callback sayfasında (örn: /mal/callback)
async function handleMalCallback(token: string, code: string) {
  const response = await fetch(
    `https://localhost:7132/api/mal/callback?code=${encodeURIComponent(code)}`,
    {
      method: 'GET',
      headers: {
        'Token': token
      }
    }
  );
  const data = await response.json();
  return data;
}

// React Router ile callback sayfası
function MalCallbackPage() {
  const [token] = useState(localStorage.getItem('token'));
  const searchParams = new URLSearchParams(window.location.search);
  const code = searchParams.get('code');

  useEffect(() => {
    if (code && token) {
      handleMalCallback(token, code).then(result => {
        if (!result.errored) {
          alert('MAL hesabı başarıyla bağlandı!');
          // Ana sayfaya yönlendir
          window.location.href = '/settings';
        } else {
          alert(`Hata: ${result.errorMessage}`);
        }
      });
    }
  }, [code, token]);

  return <div>MAL bağlantısı işleniyor...</div>;
}
```

---

### 3. MAL Bağlantısını Kaldır

**Endpoint:** `PUT /api/user/profile` (malUsername'i null yap)

**Açıklama:** MAL bağlantısını kaldırmak için `malUsername`'i boş string veya null yapın.

**Frontend Kullanımı:**

```typescript
// MAL bağlantısını kaldır
await updateProfile(userToken, undefined, '');
```

---

## Hesap Yönetimi

### 1. Hesabı Sil

**Endpoint:** `DELETE /api/user`

**Açıklama:** Kullanıcının kendi hesabını siler. Soft delete (varsayılan) veya hard delete yapılabilir.

**Headers:**
```
Token: <kullanıcı_token>
Content-Type: application/json
```

**Request Body:**
```typescript
{
  userId: number;        // Kendi kullanıcı ID'si
  hardDelete: boolean;   // true ise kalıcı sil, false ise soft delete (State = false)
  password?: string;     // Hard delete için şifre doğrulama
}
```

**Response:**
```json
{
  "response": null,
  "returnValue": 0,
  "errorMessage": "Kullanıcı hesabı devre dışı bırakıldı.", // veya "Kullanıcı kalıcı olarak silindi."
  "errored": false,
  "userId": 5
}
```

**Frontend Kullanımı:**

```typescript
async function deleteAccount(
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
const result = await deleteAccount(userToken, currentUserId, false);
if (!result.errored) {
  alert('Hesabınız devre dışı bırakıldı.');
  // Çıkış yap ve login sayfasına yönlendir
  localStorage.removeItem('token');
  window.location.href = '/login';
}
```

**React Component Örneği:**

```tsx
function DeleteAccountSection() {
  const [showConfirm, setShowConfirm] = useState(false);
  const [password, setPassword] = useState('');
  const [hardDelete, setHardDelete] = useState(false);
  const [loading, setLoading] = useState(false);
  const token = localStorage.getItem('token');
  const userId = parseInt(localStorage.getItem('userId') || '0');

  const handleDelete = async () => {
    if (hardDelete && !password) {
      alert('Kalıcı silme için şifre gerekli!');
      return;
    }

    setLoading(true);
    try {
      const result = await deleteAccount(token!, userId, hardDelete, password || undefined);
      if (!result.errored) {
        alert(hardDelete ? 'Hesabınız kalıcı olarak silindi.' : 'Hesabınız devre dışı bırakıldı.');
        localStorage.removeItem('token');
        localStorage.removeItem('userId');
        window.location.href = '/login';
      } else {
        alert(`Hata: ${result.errorMessage}`);
      }
    } catch (error) {
      console.error('Hesap silme hatası:', error);
      alert('Bir hata oluştu!');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="delete-account-section">
      <h2>Hesabı Sil</h2>
      <p>Hesabınızı silmek istediğinizden emin misiniz? Bu işlem geri alınamaz.</p>
      
      <label>
        <input
          type="checkbox"
          checked={hardDelete}
          onChange={(e) => setHardDelete(e.target.checked)}
        />
        Kalıcı olarak sil (tüm veriler silinir)
      </label>

      {hardDelete && (
        <div>
          <label>Şifre (Kalıcı silme için gerekli)</label>
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
        </div>
      )}

      {!showConfirm ? (
        <button onClick={() => setShowConfirm(true)} className="danger">
          Hesabı Sil
        </button>
      ) : (
        <div>
          <p>Bu işlemi onaylıyor musunuz?</p>
          <button onClick={handleDelete} disabled={loading} className="danger">
            {loading ? 'Siliniyor...' : 'Evet, Sil'}
          </button>
          <button onClick={() => setShowConfirm(false)}>İptal</button>
        </div>
      )}
    </div>
  );
}
```

---

## Frontend Kullanım Örnekleri

### Tam Ayarlar Sayfası Örneği

```tsx
function SettingsPage() {
  const [activeTab, setActiveTab] = useState<'profile' | 'password' | 'mal' | 'account'>('profile');
  const [profile, setProfile] = useState<any>(null);
  const token = localStorage.getItem('token');

  useEffect(() => {
    async function loadProfile() {
      const data = await getMyProfile(token!);
      setProfile(data);
    }
    loadProfile();
  }, []);

  return (
    <div className="settings-page">
      <h1>Ayarlar</h1>
      
      {/* Tab Navigation */}
      <div className="tabs">
        <button
          className={activeTab === 'profile' ? 'active' : ''}
          onClick={() => setActiveTab('profile')}
        >
          Profil
        </button>
        <button
          className={activeTab === 'password' ? 'active' : ''}
          onClick={() => setActiveTab('password')}
        >
          Şifre
        </button>
        <button
          className={activeTab === 'mal' ? 'active' : ''}
          onClick={() => setActiveTab('mal')}
        >
          MyAnimeList
        </button>
        <button
          className={activeTab === 'account' ? 'active' : ''}
          onClick={() => setActiveTab('account')}
        >
          Hesap
        </button>
      </div>

      {/* Tab Content */}
      <div className="tab-content">
        {activeTab === 'profile' && (
          <div>
            <ProfileImageUpload />
            <ProfileSettingsForm />
          </div>
        )}
        
        {activeTab === 'password' && (
          <ChangePasswordForm />
        )}
        
        {activeTab === 'mal' && (
          <MalIntegrationSection />
        )}
        
        {activeTab === 'account' && (
          <DeleteAccountSection />
        )}
      </div>
    </div>
  );
}

// MAL Entegrasyonu Bölümü
function MalIntegrationSection() {
  const [profile, setProfile] = useState<any>(null);
  const [connecting, setConnecting] = useState(false);
  const token = localStorage.getItem('token');

  useEffect(() => {
    async function loadProfile() {
      const data = await getMyProfile(token!);
      setProfile(data);
    }
    loadProfile();
  }, []);

  const handleConnect = async () => {
    setConnecting(true);
    try {
      const authUrl = await getMalAuthUrl(token!);
      if (authUrl) {
        window.location.href = authUrl;
      }
    } catch (error) {
      console.error('MAL bağlantı hatası:', error);
      alert('Bir hata oluştu!');
    } finally {
      setConnecting(false);
    }
  };

  const handleDisconnect = async () => {
    if (confirm('MAL bağlantısını kaldırmak istediğinizden emin misiniz?')) {
      await updateProfile(token!, undefined, '');
      const data = await getMyProfile(token!);
      setProfile(data);
      alert('MAL bağlantısı kaldırıldı.');
    }
  };

  return (
    <div className="mal-integration">
      <h2>MyAnimeList Entegrasyonu</h2>
      {profile?.malUsername ? (
        <div>
          <p>Bağlı MAL Hesabı: <strong>{profile.malUsername}</strong></p>
          <button onClick={handleDisconnect} className="danger">
            Bağlantıyı Kaldır
          </button>
        </div>
      ) : (
        <div>
          <p>MAL hesabınızı bağlayarak listelerinizi otomatik olarak içe aktarabilirsiniz.</p>
          <button onClick={handleConnect} disabled={connecting}>
            {connecting ? 'Bağlanıyor...' : 'MAL Hesabını Bağla'}
          </button>
        </div>
      )}
    </div>
  );
}
```

---

## Özet Tablo

| Endpoint | Method | Amaç | Token Gerekli? |
|----------|--------|------|----------------|
| `/api/user/me` | GET | Kendi profil bilgilerini getir | ✅ Evet |
| `/api/user/profile` | PUT | Profil güncelle (username, malUsername) | ✅ Evet |
| `/api/user/upload-image` | POST | Profil resmi yükle | ✅ Evet |
| `/api/user/change-password` | POST | Şifre değiştir | ✅ Evet |
| `/api/mal/get-auth-url` | GET | MAL bağlantı URL'i al | ✅ Evet |
| `/api/mal/callback` | GET | MAL callback işle | ✅ Evet |
| `/api/user` | DELETE | Hesabı sil | ✅ Evet |

---

## Önemli Notlar

1. **Profil Resmi:**
   - Maksimum dosya boyutu: 5MB
   - İzin verilen formatlar: jpg, jpeg, png, gif, webp
   - Eski resim otomatik olarak silinir

2. **Şifre Değiştirme:**
   - Mevcut şifre doğrulaması yapılır
   - Yeni şifre minimum 6 karakter olmalıdır

3. **MAL Entegrasyonu:**
   - OAuth 2.0 kullanılır
   - Callback URL'i `appsettings.json`'da tanımlı olmalıdır
   - Bağlantıyı kaldırmak için `malUsername`'i boş string yapın

4. **Hesap Silme:**
   - Soft delete (varsayılan): Hesap pasif yapılır, veriler korunur
   - Hard delete: Tüm veriler kalıcı olarak silinir (şifre gerekli)

---

**Son Güncelleme:** 2025-01-21

