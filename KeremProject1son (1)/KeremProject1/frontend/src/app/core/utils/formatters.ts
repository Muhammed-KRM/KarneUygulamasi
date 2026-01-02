// Formatlama fonksiyonları

/**
 * Tarihi göreceli formatta göster (örn: "2 saat önce")
 */
export function formatRelativeTime(date: string | Date): string {
  const now = new Date();
  const then = typeof date === 'string' ? new Date(date) : date;
  const diffInSeconds = Math.floor((now.getTime() - then.getTime()) / 1000);

  if (diffInSeconds < 60) {
    return 'Az önce';
  }

  const diffInMinutes = Math.floor(diffInSeconds / 60);
  if (diffInMinutes < 60) {
    return `${diffInMinutes} dakika önce`;
  }

  const diffInHours = Math.floor(diffInMinutes / 60);
  if (diffInHours < 24) {
    return `${diffInHours} saat önce`;
  }

  const diffInDays = Math.floor(diffInHours / 24);
  if (diffInDays < 7) {
    return `${diffInDays} gün önce`;
  }

  const diffInWeeks = Math.floor(diffInDays / 7);
  if (diffInWeeks < 4) {
    return `${diffInWeeks} hafta önce`;
  }

  const diffInMonths = Math.floor(diffInDays / 30);
  if (diffInMonths < 12) {
    return `${diffInMonths} ay önce`;
  }

  const diffInYears = Math.floor(diffInDays / 365);
  return `${diffInYears} yıl önce`;
}

/**
 * Dosya boyutunu formatla
 */
export function formatFileSize(bytes: number): string {
  if (bytes === 0) return '0 Bytes';

  const k = 1024;
  const sizes = ['Bytes', 'KB', 'MB', 'GB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));

  return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
}

/**
 * Puanı formatla (0-10 arası)
 */
export function formatScore(score: number): string {
  if (score === 0) return 'Puanlanmamış';
  return score.toFixed(1);
}

/**
 * Liste modunu Türkçe'ye çevir
 */
export function formatListMode(mode: string): string {
  const modeMap: { [key: string]: string } = {
    'Ranked': 'Sıralı',
    'Tiered': 'Kategorili',
    'Fusion': 'Birleşik'
  };
  return modeMap[mode] || mode;
}

/**
 * Bildirim tipini Türkçe'ye çevir
 */
export function formatNotificationType(type: string): string {
  const typeMap: { [key: string]: string } = {
    'like': 'Beğeni',
    'comment': 'Yorum',
    'follow': 'Takip',
    'mention': 'Bahsetme'
  };
  return typeMap[type] || type;
}

/**
 * Kategori adını formatla
 */
export function formatGenre(genre: string): string {
  return genre
    .split('-')
    .map(word => word.charAt(0).toUpperCase() + word.slice(1).toLowerCase())
    .join(' ');
}

