# Profil Resmi Yükleme Kullanım Kılavuzu

## Endpoint

**URL:** `POST /api/user/upload-image`

**Headers:**
```
Token: <kullanıcı_token>
Content-Type: multipart/form-data (otomatik - browser ekler)
```

## Özellikler

- ✅ Desteklenen formatlar: JPG, JPEG, PNG, GIF, WEBP
- ✅ Maksimum dosya boyutu: 5MB
- ✅ Eski resim otomatik silinir
- ✅ Güvenli download linki döner

## Frontend Kullanımı

### Basit JavaScript Örneği

```javascript
async function uploadProfileImage(file, token) {
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

// Kullanım
const fileInput = document.querySelector('input[type="file"]');
fileInput.addEventListener('change', async (e) => {
  const file = e.target.files[0];
  if (file) {
    const token = localStorage.getItem('token');
    const result = await uploadProfileImage(file, token);
    
    if (!result.errored) {
      console.log('Yeni resim linki:', result.response.imageLink);
      // Profil resmini UI'da güncelle
      document.getElementById('profile-image').src = result.response.imageLink;
    } else {
      alert('Hata: ' + result.errorMessage);
    }
  }
});
```

### React Örneği

```tsx
function ProfileImageUpload() {
  const [imageUrl, setImageUrl] = useState('');
  const [uploading, setUploading] = useState(false);
  const token = localStorage.getItem('token');

  const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    // Dosya validasyonu
    const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'];
    if (!allowedTypes.includes(file.type)) {
      alert('Sadece resim dosyaları kabul edilir!');
      return;
    }

    if (file.size > 5 * 1024 * 1024) {
      alert('Dosya boyutu 5MB\'dan küçük olmalıdır!');
      return;
    }

    setUploading(true);
    try {
      const formData = new FormData();
      formData.append('file', file);

      const response = await fetch('https://localhost:7132/api/user/upload-image', {
        method: 'POST',
        headers: {
          'Token': token!
        },
        body: formData
      });

      const data = await response.json();
      
      if (!data.errored) {
        setImageUrl(data.response.imageLink);
        alert('Profil resmi başarıyla yüklendi!');
      } else {
        alert('Hata: ' + data.errorMessage);
      }
    } catch (error) {
      console.error('Yükleme hatası:', error);
      alert('Bir hata oluştu!');
    } finally {
      setUploading(false);
    }
  };

  return (
    <div>
      {imageUrl && <img src={imageUrl} alt="Profil" style={{width: 100, height: 100, borderRadius: '50%'}} />}
      <input
        type="file"
        accept="image/jpeg,image/jpg,image/png,image/gif,image/webp"
        onChange={handleFileChange}
        disabled={uploading}
      />
      {uploading && <p>Yükleniyor...</p>}
    </div>
  );
}
```

### Axios ile Kullanım

```typescript
import axios from 'axios';

async function uploadProfileImage(file: File, token: string) {
  const formData = new FormData();
  formData.append('file', file);

  const response = await axios.post(
    'https://localhost:7132/api/user/upload-image',
    formData,
    {
      headers: {
        'Token': token,
        'Content-Type': 'multipart/form-data'
      }
    }
  );

  return response.data;
}
```

## Response Formatı

**Başarılı:**
```json
{
  "response": {
    "imageLink": "https://localhost:7123/api/file/download?filename=user_5_1234567890.jpg&...",
    "fileName": "user_5_1234567890.jpg"
  },
  "returnValue": 0,
  "errorMessage": "Profil resmi başarıyla yüklendi.",
  "errored": false,
  "userId": 5
}
```

**Hata:**
```json
{
  "response": null,
  "returnValue": 2001,
  "errorMessage": "Dosya seçilmedi.",
  "errored": true
}
```

## Hata Kodları

- `2001`: Dosya seçilmedi
- `2002`: Geçersiz dosya tipi
- `2003`: Dosya boyutu çok büyük (max 5MB)
- `2004`: Kullanıcı bulunamadı
- `2005`: Dosya kaydedilemedi
- `2006`: Yükleme hatası

## Önemli Notlar

1. **Content-Type Header:** Browser otomatik olarak `multipart/form-data` ekler, manuel eklemeyin
2. **Token Gerekli:** Her istekte token header'ı gönderilmeli
3. **Dosya Validasyonu:** Frontend'de de validasyon yapın (tip ve boyut)
4. **Güvenli Link:** Response'daki `imageLink` güvenli download linkidir, direkt kullanılabilir

## HTML Form Örneği

```html
<form id="upload-form">
  <input type="file" id="file-input" accept="image/*" />
  <button type="submit">Yükle</button>
</form>

<script>
document.getElementById('upload-form').addEventListener('submit', async (e) => {
  e.preventDefault();
  const file = document.getElementById('file-input').files[0];
  const token = localStorage.getItem('token');
  
  const formData = new FormData();
  formData.append('file', file);
  
  const response = await fetch('/api/user/upload-image', {
    method: 'POST',
    headers: { 'Token': token },
    body: formData
  });
  
  const data = await response.json();
  if (!data.errored) {
    alert('Başarılı!');
  }
});
</script>
```

