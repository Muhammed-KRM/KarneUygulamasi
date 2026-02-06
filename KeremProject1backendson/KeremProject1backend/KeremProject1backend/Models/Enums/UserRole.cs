namespace KeremProject1backend.Models.Enums;

public enum UserRole : byte
{
    AdminAdmin = 0,        // Sistem kurucusu
    Admin = 1,             // Sistem yöneticisi
    Manager = 2,           // Kurum yöneticisi (Dershane yöneticisi)
    Teacher = 3,           // Öğretmen (kuruma bağlı veya bağımsız)
    Student = 4,           // Öğrenci (kuruma bağlı veya bağımsız)
    StandaloneTeacher = 5, // Bağımsız öğretmen (kuruma bağlı değil)
    StandaloneStudent = 6, // Bağımsız öğrenci (kuruma bağlı değil)
    InstitutionOwner = 7   // Dershane sahibi (kurum başvurusu yapabilir, manager oluşturabilir)
}
