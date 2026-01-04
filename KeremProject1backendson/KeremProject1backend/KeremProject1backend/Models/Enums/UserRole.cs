namespace KeremProject1backend.Models.Enums;

public enum UserRole : byte
{
    AdminAdmin = 0, // Sistem kurucusu
    Admin = 1,      // Sistem yöneticisi
    User = 2        // Normal kullanıcı (Öğretmen/Öğrenci rolleri InstitutionUser'da)
}
