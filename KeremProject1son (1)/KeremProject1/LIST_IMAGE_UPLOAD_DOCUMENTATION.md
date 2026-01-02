# Liste Görseli Yükleme - Frontend Kullanım Kılavuzu

## Genel Bakış

Listeler için görsel yükleme özelliği eklendi. Artık her liste için bir görsel ekleyebilir ve bu görsel liste listelerinde (GetAllLists, GetUserPublicLists, GetPublicLists) görüntülenir.

## Endpoint'ler

### 1. Liste Görseli Yükleme

**Endpoint:** `POST /api/list/{listId}/upload-image`

**Headers:**
```
Token: <kullanıcı_token>
Content-Type: multipart/form-data (browser otomatik ekler)
```

**Request:**
- FormData ile dosya gönder
- Field adı: `file`
- Dosya formatları: JPG, JPEG, PNG, GIF, WEBP
- Max dosya boyutu: 5MB

**Response:**
```json
{
  "response": {
    "imageLink": "https://localhost:7132/api/file/download?filename=0000001_20251121162929_list_image.jpg&type=3&sessionno=1&signature=...",
    "fileName": "0000001_20251121162929_list_image.jpg"
  },
  "returnValue": 0,
  "errorMessage": "Liste görseli başarıyla yüklendi.",
  "errored": false
}
```

**Hata Kodları:**
- `3101`: Dosya seçilmedi
- `3102`: Geçersiz dosya tipi
- `3103`: Dosya boyutu çok büyük (max 5MB)
- `3104`: Liste bulunamadı
- `3105`: Bu liste üzerinde yetkiniz yok
- `3106`: Liste görseli yüklenirken hata

### 2. Liste Görseli Getirme

Liste görselleri artık tüm liste döndüren endpoint'lerde otomatik olarak gelir:

- `GET /api/list/all` - `ListImageLink` alanı eklendi
- `GET /api/list/user/{userId}` - `ListImageLink` alanı eklendi
- `GET /api/share/public` - `ListImageLink` alanı eklendi

**Response Örneği (GetAllLists):**
```json
{
  "response": [
    {
      "id": 1,
      "title": "En İyi Anime'ler",
      "mode": "Tiered",
      "createdAt": "2025-11-21T10:00:00",
      "modTime": "2025-11-21T16:29:29",
      "tierCount": 6,
      "itemCount": 50,
      "userId": 1,
      "listImageLink": "0000001_20251121162929_list_image.jpg"
    }
  ],
  "errored": false
}
```

**Not:** `listImageLink` null olabilir (görsel yüklenmemişse). Görsel linkini oluşturmak için FileService kullanılmalı.

## Frontend Kullanım Örnekleri

### JavaScript (Vanilla)

```javascript
// Liste görseli yükleme
async function uploadListImage(listId, file, token) {
  const formData = new FormData();
  formData.append('file', file);

  const response = await fetch(`https://localhost:7132/api/list/${listId}/upload-image`, {
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
    const listId = 1; // Liste ID'si
    const result = await uploadListImage(listId, file, token);
    
    if (!result.errored) {
      console.log('Liste görseli:', result.response.imageLink);
      // Liste görselini güncelle
      updateListImageDisplay(result.response.imageLink);
    } else {
      alert('Hata: ' + result.errorMessage);
    }
  }
});

// Liste görseli linkini oluşturma (listImageLink'ten)
function getListImageUrl(listImageLink, token) {
  if (!listImageLink) return null;
  
  // FileService'in GenerateFileLinkWithSecret metodunu kullanarak link oluştur
  // Backend'den gelen listImageLink sadece dosya adı, tam URL'yi oluşturmalısınız
  const baseUrl = 'https://localhost:7132/api/file/download';
  // Signature oluşturma için backend'e istek yapabilir veya
  // Backend'den gelen imageLink'i direkt kullanabilirsiniz (upload sonrası dönen)
  
  // Alternatif: Backend'den direkt imageLink döndüğü için onu kullanın
  return null; // listImageLink null ise
}

// Liste listesini getir ve görselleri göster
async function loadLists(token) {
  const response = await fetch('https://localhost:7132/api/list/all', {
    headers: { 'Token': token }
  });
  
  const data = await response.json();
  if (!data.errored) {
    data.response.forEach(list => {
      // list.listImageLink varsa görseli göster
      if (list.listImageLink) {
        // Görsel linkini oluştur (upload sonrası dönen imageLink formatında)
        // Veya backend'den direkt imageLink döndürmesini sağlayın
        displayListWithImage(list);
      }
    });
  }
}
```

### React (TypeScript)

```tsx
import React, { useState } from 'react';

interface ListImageUploadProps {
  listId: number;
  token: string;
  onUploadSuccess?: (imageLink: string) => void;
}

const ListImageUpload: React.FC<ListImageUploadProps> = ({ listId, token, onUploadSuccess }) => {
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    // Validasyon
    const allowedTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'];
    if (!allowedTypes.includes(file.type)) {
      setError('Sadece resim dosyaları kabul edilir!');
      return;
    }

    if (file.size > 5 * 1024 * 1024) {
      setError('Max 5MB!');
      return;
    }

    setUploading(true);
    setError(null);

    const formData = new FormData();
    formData.append('file', file);

    try {
      const response = await fetch(`/api/list/${listId}/upload-image`, {
        method: 'POST',
        headers: { 'Token': token },
        body: formData
      });

      const data = await response.json();
      
      if (!data.errored) {
        onUploadSuccess?.(data.response.imageLink);
        alert('Liste görseli yüklendi!');
      } else {
        setError(data.errorMessage);
      }
    } catch (err) {
      setError('Yükleme sırasında hata oluştu.');
    } finally {
      setUploading(false);
    }
  };

  return (
    <div>
      <input
        type="file"
        accept="image/*"
        onChange={handleFileChange}
        disabled={uploading}
      />
      {error && <div style={{ color: 'red' }}>{error}</div>}
      {uploading && <div>Yükleniyor...</div>}
    </div>
  );
};

// Liste kartı komponenti
interface ListCardProps {
  list: {
    id: number;
    title: string;
    listImageLink: string | null;
    // ... diğer alanlar
  };
  token: string;
}

const ListCard: React.FC<ListCardProps> = ({ list, token }) => {
  // listImageLink'ten görsel URL'si oluştur
  // Not: Backend'den direkt imageLink döndürmesi daha iyi olur
  const getImageUrl = () => {
    if (!list.listImageLink) return null;
    // Backend'den gelen listImageLink sadece dosya adı
    // Tam URL'yi oluşturmak için backend'e istek yapabilirsiniz
    // Veya backend'i güncelleyerek direkt imageLink döndürmesini sağlayın
    return null;
  };

  return (
    <div className="list-card">
      {list.listImageLink && (
        <img 
          src={getImageUrl() || '/placeholder-list.png'} 
          alt={list.title}
          onError={(e) => {
            e.currentTarget.src = '/placeholder-list.png';
          }}
        />
      )}
      <h3>{list.title}</h3>
    </div>
  );
};

export default ListImageUpload;
```

### Vue.js

```vue
<template>
  <div>
    <input
      type="file"
      accept="image/*"
      @change="handleFileUpload"
      :disabled="uploading"
    />
    <div v-if="error" style="color: red">{{ error }}</div>
    <div v-if="uploading">Yükleniyor...</div>
  </div>
</template>

<script setup>
import { ref } from 'vue';

const props = defineProps({
  listId: {
    type: Number,
    required: true
  },
  token: {
    type: String,
    required: true
  }
});

const emit = defineEmits(['upload-success']);

const uploading = ref(false);
const error = ref(null);

const handleFileUpload = async (e) => {
  const file = e.target.files[0];
  if (!file) return;

  // Validasyon
  const allowedTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'];
  if (!allowedTypes.includes(file.type)) {
    error.value = 'Sadece resim dosyaları kabul edilir!';
    return;
  }

  if (file.size > 5 * 1024 * 1024) {
    error.value = 'Max 5MB!';
    return;
  }

  uploading.value = true;
  error.value = null;

  const formData = new FormData();
  formData.append('file', file);

  try {
    const response = await fetch(`/api/list/${props.listId}/upload-image`, {
      method: 'POST',
      headers: { 'Token': props.token },
      body: formData
    });

    const data = await response.json();
    
    if (!data.errored) {
      emit('upload-success', data.response.imageLink);
      alert('Liste görseli yüklendi!');
    } else {
      error.value = data.errorMessage;
    }
  } catch (err) {
    error.value = 'Yükleme sırasında hata oluştu.';
  } finally {
    uploading.value = false;
  }
};
</script>
```

## Önemli Notlar

1. **Görsel Link Formatı:** Backend'den gelen `listImageLink` sadece dosya adıdır. Tam URL'yi oluşturmak için:
   - Upload sonrası dönen `imageLink`'i kullanın (en kolay yol)
   - Veya backend'i güncelleyerek GetAllLists gibi endpoint'lerden direkt `imageLink` döndürmesini sağlayın

2. **Görsel Yoksa:** `listImageLink` null olabilir. Bu durumda placeholder görsel gösterin.

3. **Güvenlik:** Görsel linkleri signature ile korunur. Backend'den dönen `imageLink`'i direkt kullanın.

4. **Dosya Yönetimi:** Eski görsel otomatik olarak silinir. Yeni görsel yüklendiğinde eski görsel dosya sisteminden kaldırılır.

## Backend İyileştirme Önerisi

GetAllLists ve diğer liste döndüren endpoint'lerden direkt `imageLink` döndürmek için backend'i güncelleyebilirsiniz:

```csharp
// GetAllLists metodunda
ListImageLink = !string.IsNullOrEmpty(l.ListImageLink) 
    ? FileService.GenerateFileLinkWithSecret(l.ListImageLink, FileType.List, session, downloadServiceLink, secretKey)
    : null
```

Bu şekilde frontend'de ekstra işlem yapmanıza gerek kalmaz.

