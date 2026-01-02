export interface GetUserLogsRequest {
  userId?: number;  // null ise kendi logları, admin için başka kullanıcı ID'si
  page?: number;  // Sayfa numarası (varsayılan: 1)
  limit?: number;  // Sayfa başına kayıt sayısı (varsayılan: 20)
  tableName?: string;  // Tablo adı filtresi (opsiyonel)
  action?: 'C' | 'U' | 'D';  // 'C'=Create, 'U'=Update, 'D'=Delete (opsiyonel)
  startDate?: string;  // Başlangıç tarihi (ISO format) (opsiyonel)
  endDate?: string;  // Bitiş tarihi (ISO format) (opsiyonel)
}

export interface GetAdminLogsRequest {
  page?: number;  // Sayfa numarası (varsayılan: 1)
  limit?: number;  // Sayfa başına kayıt sayısı (varsayılan: 50)
  userId?: number;  // Belirli kullanıcının logları (opsiyonel)
  tableName?: string;  // Tablo adı filtresi (opsiyonel)
  action?: 'C' | 'U' | 'D';  // 'C'=Create, 'U'=Update, 'D'=Delete (opsiyonel)
  startDate?: string;  // Başlangıç tarihi (ISO format) (opsiyonel)
  endDate?: string;  // Bitiş tarihi (ISO format) (opsiyonel)
}

