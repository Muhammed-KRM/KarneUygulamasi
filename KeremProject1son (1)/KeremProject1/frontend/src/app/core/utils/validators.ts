// Validasyon fonksiyonları

/**
 * Email validasyonu
 */
export function isValidEmail(email: string): boolean {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailRegex.test(email);
}

/**
 * Kullanıcı adı validasyonu
 */
export function isValidUsername(username: string): boolean {
  const minLength = 3;
  const maxLength = 50;
  const usernameRegex = /^[a-zA-Z0-9_]+$/;
  
  return (
    username.length >= minLength &&
    username.length <= maxLength &&
    usernameRegex.test(username)
  );
}

/**
 * Şifre validasyonu
 */
export function isValidPassword(password: string): boolean {
  return password.length >= 6;
}

/**
 * Liste başlığı validasyonu
 */
export function isValidListTitle(title: string): boolean {
  return title.trim().length > 0 && title.trim().length <= 100;
}

/**
 * Tier başlığı validasyonu
 */
export function isValidTierTitle(title: string): boolean {
  return title.trim().length > 0 && title.trim().length <= 50;
}

/**
 * Yorum içeriği validasyonu
 */
export function isValidComment(content: string): boolean {
  return content.trim().length > 0 && content.trim().length <= 500;
}

/**
 * URL validasyonu
 */
export function isValidUrl(url: string): boolean {
  try {
    new URL(url);
    return true;
  } catch {
    return false;
  }
}

/**
 * Dosya tipi validasyonu
 */
export function isValidImageType(file: File): boolean {
  const allowedTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'];
  return allowedTypes.includes(file.type);
}

/**
 * Dosya boyutu validasyonu
 */
export function isValidFileSize(file: File, maxSize: number = 5 * 1024 * 1024): boolean {
  return file.size <= maxSize;
}

/**
 * Hex renk kodu validasyonu
 */
export function isValidHexColor(color: string): boolean {
  return /^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$/.test(color);
}

