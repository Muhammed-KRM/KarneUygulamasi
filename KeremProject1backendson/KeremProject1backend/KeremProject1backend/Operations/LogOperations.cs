using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.Requests;
using KeremProject1backend.Models.Responses;
using KeremProject1backend.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KeremProject1backend.Operations
{
    public static class LogOperations
    {
        /// <summary>
        /// Kullanıcının kendi loglarını getir (veya Admin başka kullanıcının loglarını)
        /// </summary>
        public static async Task<BaseResponse> GetUserLogs(
            GetUserLogsRequest request,
            ClaimsPrincipal session,
            ApplicationContext context)
        {
            BaseResponse response = new();

            try
            {
                int currentUserId = SessionService.GetUserId(session);
                var currentUser = await context.AppUsers.FindAsync(currentUserId);
                if (currentUser == null)
                    return response.GenerateError(7001, "Kullanıcı bulunamadı.");

                // Hangi kullanıcının logları getirilecek?
                int targetUserId = request.UserId ?? currentUserId;
                bool isAdmin = currentUser.UserRole == UserRole.Admin || currentUser.UserRole == UserRole.AdminAdmin;
                bool isOwnAccount = targetUserId == currentUserId;

                // Yetki kontrolü: Admin veya kendi logları
                if (!isAdmin && !isOwnAccount)
                    return response.GenerateError(7002, "Bu işlem için yetkiniz yok.");

                var query = context.DataLogs.AsNoTracking()
                    .Where(l => l.ModUser == targetUserId);

                // Filtreleme
                if (!string.IsNullOrWhiteSpace(request.TableName))
                {
                    query = query.Where(l => l.TableName == request.TableName);
                }

                if (request.Action.HasValue)
                {
                    query = query.Where(l => l.Action == request.Action.Value);
                }

                if (request.StartDate.HasValue)
                {
                    query = query.Where(l => l.ModTime >= request.StartDate.Value);
                }

                if (request.EndDate.HasValue)
                {
                    query = query.Where(l => l.ModTime <= request.EndDate.Value);
                }

                // Toplam sayı
                int totalCount = await query.CountAsync();

                // Sayfalama
                var logs = await query
                    .OrderByDescending(l => l.ModTime)
                    .Skip((request.Page - 1) * request.Limit)
                    .Take(request.Limit)
                    .ToListAsync();

                // Kullanıcı adlarını al
                var userIds = logs.Select(l => l.ModUser).Distinct().ToList();
                var oldUserIds = logs.Where(l => l.OldModUser.HasValue).Select(l => l.OldModUser!.Value).Distinct().ToList();
                userIds.AddRange(oldUserIds);

                var users = await context.AppUsers
                    .AsNoTracking()
                    .Where(u => userIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id, u => u.UserName);

                // Log DTO'ları oluştur
                var logDtos = logs.Select(l => new LogDto
                {
                    Id = l.Id,
                    TableName = l.TableName,
                    OldValue = l.OldValue,
                    NewValue = l.NewValue,
                    Action = l.Action,
                    ActionName = l.Action switch
                    {
                        'C' => "Oluşturuldu",
                        'U' => "Güncellendi",
                        'D' => "Silindi",
                        _ => "Bilinmeyen"
                    },
                    OldModUser = l.OldModUser,
                    OldModUsername = l.OldModUser.HasValue && users.ContainsKey(l.OldModUser.Value) 
                        ? users[l.OldModUser.Value] 
                        : null,
                    OldModTime = l.OldModTime,
                    ModUser = l.ModUser,
                    ModUsername = users.ContainsKey(l.ModUser) ? users[l.ModUser] : "Bilinmeyen",
                    ModTime = l.ModTime
                }).ToList();

                var logResponse = new LogListResponse
                {
                    Logs = logDtos,
                    TotalCount = totalCount,
                    Page = request.Page,
                    Limit = request.Limit,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)request.Limit)
                };

                response.Response = logResponse;
                response.SetUserID(currentUserId);
                return response.GenerateSuccess("Loglar başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return response.GenerateError(7003, $"Loglar getirilirken hata: {ex.Message}");
            }
        }

        /// <summary>
        /// Admin için tüm logları getir
        /// </summary>
        public static async Task<BaseResponse> GetAdminLogs(
            GetAdminLogsRequest request,
            ClaimsPrincipal session,
            ApplicationContext context)
        {
            BaseResponse response = new();

            try
            {
                int currentUserId = SessionService.GetUserId(session);
                var currentUser = await context.AppUsers.FindAsync(currentUserId);
                if (currentUser == null)
                    return response.GenerateError(7004, "Kullanıcı bulunamadı.");

                // Admin kontrolü
                bool isAdmin = currentUser.UserRole == UserRole.Admin || currentUser.UserRole == UserRole.AdminAdmin;
                if (!isAdmin)
                    return response.GenerateError(7005, "Bu işlem için Admin yetkisi gereklidir.");

                var query = context.DataLogs.AsNoTracking();

                // Filtreleme
                if (request.UserId.HasValue)
                {
                    query = query.Where(l => l.ModUser == request.UserId.Value);
                }

                if (!string.IsNullOrWhiteSpace(request.TableName))
                {
                    query = query.Where(l => l.TableName == request.TableName);
                }

                if (request.Action.HasValue)
                {
                    query = query.Where(l => l.Action == request.Action.Value);
                }

                if (request.StartDate.HasValue)
                {
                    query = query.Where(l => l.ModTime >= request.StartDate.Value);
                }

                if (request.EndDate.HasValue)
                {
                    query = query.Where(l => l.ModTime <= request.EndDate.Value);
                }

                // Toplam sayı
                int totalCount = await query.CountAsync();

                // Sayfalama
                var logs = await query
                    .OrderByDescending(l => l.ModTime)
                    .Skip((request.Page - 1) * request.Limit)
                    .Take(request.Limit)
                    .ToListAsync();

                // Kullanıcı adlarını al
                var userIds = logs.Select(l => l.ModUser).Distinct().ToList();
                var oldUserIds = logs.Where(l => l.OldModUser.HasValue).Select(l => l.OldModUser!.Value).Distinct().ToList();
                userIds.AddRange(oldUserIds);

                var users = await context.AppUsers
                    .AsNoTracking()
                    .Where(u => userIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id, u => u.UserName);

                // Log DTO'ları oluştur
                var logDtos = logs.Select(l => new LogDto
                {
                    Id = l.Id,
                    TableName = l.TableName,
                    OldValue = l.OldValue,
                    NewValue = l.NewValue,
                    Action = l.Action,
                    ActionName = l.Action switch
                    {
                        'C' => "Oluşturuldu",
                        'U' => "Güncellendi",
                        'D' => "Silindi",
                        _ => "Bilinmeyen"
                    },
                    OldModUser = l.OldModUser,
                    OldModUsername = l.OldModUser.HasValue && users.ContainsKey(l.OldModUser.Value) 
                        ? users[l.OldModUser.Value] 
                        : null,
                    OldModTime = l.OldModTime,
                    ModUser = l.ModUser,
                    ModUsername = users.ContainsKey(l.ModUser) ? users[l.ModUser] : "Bilinmeyen",
                    ModTime = l.ModTime
                }).ToList();

                var logResponse = new LogListResponse
                {
                    Logs = logDtos,
                    TotalCount = totalCount,
                    Page = request.Page,
                    Limit = request.Limit,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)request.Limit)
                };

                response.Response = logResponse;
                response.SetUserID(currentUserId);
                return response.GenerateSuccess("Tüm loglar başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return response.GenerateError(7006, $"Loglar getirilirken hata: {ex.Message}");
            }
        }
    }
}

