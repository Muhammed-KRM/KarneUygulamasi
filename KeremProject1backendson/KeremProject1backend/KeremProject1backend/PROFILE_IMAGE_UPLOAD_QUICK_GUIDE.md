# Profil Resmi Yükleme - Hızlı Kullanım Kılavuzu

## Endpoint
```
POST /api/user/upload-image
```

## Headers
```
Token: <kullanıcı_token>
```

## Request
**FormData** ile dosya gönder:
- Field adı: `file`
- Content-Type: `multipart/form-data` (browser otomatik ekler)

## JavaScript Örneği

```javascript
async function uploadProfileImage(file, token) {
  const formData = new FormData();
  formData.append('file', file);

  const response = await fetch('https://localhost:7132/api/user/upload-image', {
    method: 'POST',
    headers: {
      'Token': token
      // Content-Type header'ını EKLEMEYİN!
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
      console.log('Resim linki:', result.response.imageLink);
      // Profil resmini güncelle
      document.getElementById('profile-img').src = result.response.imageLink;
    }
  }
});
```

## React Örneği

```tsx
const handleFileUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
  const file = e.target.files?.[0];
  if (!file) return;

  // Validasyon
  if (!['image/jpeg', 'image/png', 'image/gif', 'image/webp'].includes(file.type)) {
    alert('Sadece resim dosyaları!');
    return;
  }
  if (file.size > 5 * 1024 * 1024) {
    alert('Max 5MB!');
    return;
  }

  const formData = new FormData();
  formData.append('file', file);

  const response = await fetch('/api/user/upload-image', {
    method: 'POST',
    headers: { 'Token': token },
    body: formData
  });

  const data = await response.json();
  if (!data.errored) {
    setImageUrl(data.response.imageLink);
  }
};
```

## Özellikler
- ✅ Formatlar: JPG, JPEG, PNG, GIF, WEBP
- ✅ Max boyut: 5MB
- ✅ Eski resim otomatik silinir
- ✅ Güvenli download linki döner

## Response
```json
{
  "response": {
    "imageLink": "https://localhost:7123/api/file/download?...",
    "fileName": "user_5_1234567890.jpg"
  },
  "errored": false
}
```

## Önemli Notlar
1. **Content-Type header'ını manuel eklemeyin** - Browser otomatik ekler
2. **FormData kullanın** - JSON değil!
3. **Token header'ı gerekli**
4. **Dosya validasyonu** frontend'de de yapın

