namespace KeremProject1backend.Models.Enums;

public enum ProfileVisibility : byte
{
    PublicToAll = 1,    // Herkes görebilir
    TeachersOnly = 2,   // Sadece öğretmenler
    Private = 3         // Sadece kendisi
}
