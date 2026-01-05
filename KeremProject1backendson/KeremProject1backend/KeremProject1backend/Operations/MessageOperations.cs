using KeremProject1backend.Core.Constants;
using KeremProject1backend.Infrastructure;
using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.DTOs;
using KeremProject1backend.Models.Enums;
using KeremProject1backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using KeremProject1backend.Hubs;

namespace KeremProject1backend.Operations;

public class MessageOperations
{
    private readonly ApplicationContext _context;
    private readonly SessionService _sessionService;
    private readonly IHubContext<ChatHub> _chatHub;
    private readonly CacheService _cacheService;
    private readonly AuditService _auditService;

    public MessageOperations(ApplicationContext context, SessionService sessionService, IHubContext<ChatHub> chatHub, CacheService cacheService, AuditService auditService)
    {
        _context = context;
        _sessionService = sessionService;
        _chatHub = chatHub;
        _cacheService = cacheService;
        _auditService = auditService;
    }

    public async Task<BaseResponse<int>> StartConversationAsync(int institutionId, int? classroomId, string title, bool isGroup)
    {
        var userId = _sessionService.GetUserId();

        var conversation = new Conversation
        {
            InstitutionId = institutionId,
            ClassroomId = classroomId,
            Title = title,
            IsGroup = isGroup,
            CreatedAt = DateTime.UtcNow,
            Institution = null!
        };

        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        // Add creator as member
        var member = new ConversationMember
        {
            ConversationId = conversation.Id,
            UserId = userId,
            IsAdmin = true,
            JoinedAt = DateTime.UtcNow,
            Conversation = null!,
            User = null!
        };
        _context.ConversationMembers.Add(member);
        await _context.SaveChangesAsync();

        return BaseResponse<int>.SuccessResponse(conversation.Id);
    }

    public async Task<BaseResponse<bool>> SendMessageAsync(int conversationId, string content, MessageType type = MessageType.Text, int? attachedExamId = null, int? attachedResultId = null)
    {
        var userId = _sessionService.GetUserId();

        // Security check: User must be member
        var isMember = await _context.ConversationMembers.AnyAsync(cm => cm.ConversationId == conversationId && cm.UserId == userId);
        if (!isMember) return BaseResponse<bool>.ErrorResponse("Unauthorized", ErrorCodes.AccessDenied);

        var message = new Message
        {
            ConversationId = conversationId,
            SenderId = userId,
            Content = content,
            Type = type,
            AttachedExamId = attachedExamId,
            AttachedExamResultId = attachedResultId,
            SentAt = DateTime.UtcNow,
            Conversation = null!,
            Sender = null!
        };

        _context.Messages.Add(message);
        
        // Update conversation LastMessageAt
        var conversation = await _context.Conversations.FindAsync(conversationId);
        if (conversation != null)
        {
            conversation.LastMessageAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // Invalidate cache
        await _cacheService.RemoveByPatternAsync("user_conversations_*");

        // Notify via SignalR
        await _chatHub.Clients.Group($"Conv_{conversationId}").SendAsync("ReceiveMessage", new
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            SenderId = message.SenderId,
            Content = message.Content,
            Type = message.Type,
            AttachedExamId = message.AttachedExamId,
            AttachedExamResultId = message.AttachedExamResultId,
            SentAt = message.SentAt
        });

        return BaseResponse<bool>.SuccessResponse(true);
    }

    public async Task<BaseResponse<List<MessageDto>>> GetMessagesAsync(int conversationId, int count = 50)
    {
        var userId = _sessionService.GetUserId();
        var isMember = await _context.ConversationMembers.AnyAsync(cm => cm.ConversationId == conversationId && cm.UserId == userId);
        if (!isMember) return BaseResponse<List<MessageDto>>.ErrorResponse("Unauthorized", ErrorCodes.AccessDenied);

        var messages = await _context.Messages
            .Include(m => m.Sender)
            .Where(m => m.ConversationId == conversationId && !m.IsDeleted)
            .OrderByDescending(m => m.SentAt)
            .Take(count)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                SenderId = m.SenderId,
                SenderName = m.Sender.FullName,
                Content = m.Content,
                Type = m.Type,
                AttachedExamId = m.AttachedExamId,
                AttachedExamResultId = m.AttachedExamResultId,
                SentAt = m.SentAt
            }).ToListAsync();

        return BaseResponse<List<MessageDto>>.SuccessResponse(messages.OrderBy(m => m.SentAt).ToList());
    }

    public async Task<BaseResponse<int>> SendToClassAsync(int classroomId, List<int> reportCardIds)
    {
        var userId = _sessionService.GetUserId();
        var classroom = await _context.Classrooms
            .Include(c => c.ClassConversation)
            .FirstOrDefaultAsync(c => c.Id == classroomId);

        if (classroom == null || classroom.ClassConversation == null)
            return BaseResponse<int>.ErrorResponse("Classroom or conversation not found", ErrorCodes.GenericError);

        var messagesList = new List<Message>();
        foreach (var reportId in reportCardIds)
        {
            messagesList.Add(new Message
            {
                ConversationId = classroom.ClassConversation.Id,
                SenderId = userId,
                Content = "Yeni sınav karneniz paylaşıldı.",
                Type = MessageType.ReportCard,
                AttachedExamResultId = reportId,
                SentAt = DateTime.UtcNow,
                Conversation = null!,
                Sender = null!
            });
        }

        _context.Messages.AddRange(messagesList);
        await _context.SaveChangesAsync();

        // SignalR Notification
        await _chatHub.Clients.Group($"Conv_{classroom.ClassConversation.Id}").SendAsync("BulkReportsReceived", messagesList.Count);

        return BaseResponse<int>.SuccessResponse(messagesList.Count);
    }

    public async Task<BaseResponse<List<ConversationDto>>> GetConversationsAsync(int userId, int page = 1, int limit = 20, bool forceRefresh = false)
    {
        // Cache key
        var cacheKey = $"user_conversations_{userId}_{page}_{limit}";
        if (!forceRefresh)
        {
            var cached = await _cacheService.GetAsync<List<ConversationDto>>(cacheKey);
            if (cached != null)
                return BaseResponse<List<ConversationDto>>.SuccessResponse(cached);
        }

        var conversations = await _context.ConversationMembers
            .Include(cm => cm.Conversation)
                .ThenInclude(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
            .Where(cm => cm.UserId == userId)
            .OrderByDescending(cm => cm.Conversation.LastMessageAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(cm => new ConversationDto
            {
                Id = cm.Conversation.Id,
                Title = cm.Conversation.Title ?? "Private Chat",
                IsGroup = cm.Conversation.IsGroup,
                Type = cm.Conversation.Type.ToString(),
                LastMessageAt = cm.Conversation.LastMessageAt,
                LastMessage = cm.Conversation.Messages
                    .OrderByDescending(m => m.SentAt)
                    .Select(m => new MessagePreviewDto
                    {
                        Content = m.Content,
                        SentAt = m.SentAt,
                        SenderName = m.Sender.FullName
                    })
                    .FirstOrDefault(),
                UnreadCount = _context.Messages.Count(m => 
                    m.ConversationId == cm.ConversationId && 
                    m.SentAt > (cm.LastReadAt ?? DateTime.MinValue) &&
                    m.SenderId != userId &&
                    !m.IsDeleted)
            })
            .ToListAsync();

        // Cache for 1 minute (conversations change frequently)
        if (!forceRefresh)
        {
            await _cacheService.SetAsync(cacheKey, conversations, TimeSpan.FromMinutes(1));
        }

        return BaseResponse<List<ConversationDto>>.SuccessResponse(conversations);
    }

    public async Task<BaseResponse<ConversationDetailDto>> GetConversationAsync(int conversationId, int userId)
    {
        var isMember = await _context.ConversationMembers.AnyAsync(cm => 
            cm.ConversationId == conversationId && cm.UserId == userId);

        if (!isMember)
            return BaseResponse<ConversationDetailDto>.ErrorResponse("Access denied", ErrorCodes.AccessDenied);

        var conversation = await _context.Conversations
            .Include(c => c.Members)
                .ThenInclude(m => m.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == conversationId);

        if (conversation == null)
            return BaseResponse<ConversationDetailDto>.ErrorResponse("Conversation not found", ErrorCodes.GenericError);

        var detail = new ConversationDetailDto
        {
            Id = conversation.Id,
            Title = conversation.Title,
            IsGroup = conversation.IsGroup,
            Type = conversation.Type.ToString(),
            CreatedAt = conversation.CreatedAt,
            Members = conversation.Members.Select(m => new ConversationMemberDto
            {
                UserId = m.UserId,
                UserName = m.User.FullName,
                IsAdmin = m.IsAdmin,
                JoinedAt = m.JoinedAt
            }).ToList()
        };

        return BaseResponse<ConversationDetailDto>.SuccessResponse(detail);
    }

    public async Task<BaseResponse<string>> UpdateConversationAsync(int conversationId, string title, int userId)
    {
        var conversation = await _context.Conversations
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == conversationId);

        if (conversation == null)
            return BaseResponse<string>.ErrorResponse("Conversation not found", ErrorCodes.GenericError);

        if (!conversation.IsGroup)
            return BaseResponse<string>.ErrorResponse("Only group conversations can be updated", ErrorCodes.AccessDenied);

        var isAdmin = await _context.ConversationMembers.AnyAsync(cm =>
            cm.ConversationId == conversationId &&
            cm.UserId == userId &&
            cm.IsAdmin);

        if (!isAdmin)
            return BaseResponse<string>.ErrorResponse("Only admins can update conversation", ErrorCodes.AccessDenied);

        conversation.Title = title;
        await _context.SaveChangesAsync();

        // Invalidate cache
        await _cacheService.RemoveByPatternAsync("user_conversations_*");

        await _auditService.LogAsync(userId, "ConversationUpdated", 
            System.Text.Json.JsonSerializer.Serialize(new { ConversationId = conversationId, Title = title }));

        return BaseResponse<string>.SuccessResponse("Conversation updated successfully");
    }

    public async Task<BaseResponse<string>> DeleteConversationAsync(int conversationId, int userId)
    {
        var conversation = await _context.Conversations
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == conversationId);

        if (conversation == null)
            return BaseResponse<string>.ErrorResponse("Conversation not found", ErrorCodes.GenericError);

        // Only private conversations can be deleted
        if (conversation.IsGroup)
            return BaseResponse<string>.ErrorResponse("Group conversations cannot be deleted", ErrorCodes.AccessDenied);

        // Remove user from conversation (soft delete by removing membership)
        var membership = await _context.ConversationMembers
            .FirstOrDefaultAsync(cm => cm.ConversationId == conversationId && cm.UserId == userId);

        if (membership != null)
        {
            _context.ConversationMembers.Remove(membership);
            await _context.SaveChangesAsync();
        }

        // Invalidate cache
        await _cacheService.RemoveByPatternAsync("user_conversations_*");

        await _auditService.LogAsync(userId, "ConversationDeleted", 
            System.Text.Json.JsonSerializer.Serialize(new { ConversationId = conversationId }));

        return BaseResponse<string>.SuccessResponse("Conversation deleted successfully");
    }

    public async Task<BaseResponse<string>> LeaveConversationAsync(int conversationId, int userId)
    {
        var conversation = await _context.Conversations.FindAsync(conversationId);
        if (conversation == null)
            return BaseResponse<string>.ErrorResponse("Conversation not found", ErrorCodes.GenericError);

        if (!conversation.IsGroup)
            return BaseResponse<string>.ErrorResponse("Cannot leave private conversation", ErrorCodes.AccessDenied);

        var membership = await _context.ConversationMembers
            .FirstOrDefaultAsync(cm => cm.ConversationId == conversationId && cm.UserId == userId);

        if (membership == null)
            return BaseResponse<string>.ErrorResponse("Not a member of this conversation", ErrorCodes.AccessDenied);

        _context.ConversationMembers.Remove(membership);
        await _context.SaveChangesAsync();

        // Invalidate cache
        await _cacheService.RemoveByPatternAsync("user_conversations_*");

        await _auditService.LogAsync(userId, "LeftConversation", 
            System.Text.Json.JsonSerializer.Serialize(new { ConversationId = conversationId }));

        return BaseResponse<string>.SuccessResponse("Left conversation successfully");
    }

    public async Task<BaseResponse<string>> DeleteMessageAsync(int messageId, int userId)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message == null)
            return BaseResponse<string>.ErrorResponse("Message not found", ErrorCodes.GenericError);

        // Only sender can delete
        if (message.SenderId != userId)
            return BaseResponse<string>.ErrorResponse("Can only delete your own messages", ErrorCodes.AccessDenied);

        message.IsDeleted = true;
        message.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Invalidate cache
        await _cacheService.RemoveByPatternAsync($"user_conversations_{userId}_*");

        await _auditService.LogAsync(userId, "MessageDeleted", 
            System.Text.Json.JsonSerializer.Serialize(new { MessageId = messageId }));

        return BaseResponse<string>.SuccessResponse("Message deleted successfully");
    }

    public async Task<BaseResponse<string>> UpdateMessageAsync(int messageId, string newContent, int userId)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message == null)
            return BaseResponse<string>.ErrorResponse("Message not found", ErrorCodes.GenericError);

        // Only sender can update
        if (message.SenderId != userId)
            return BaseResponse<string>.ErrorResponse("Can only update your own messages", ErrorCodes.AccessDenied);

        // Only text messages can be updated
        if (message.Type != MessageType.Text)
            return BaseResponse<string>.ErrorResponse("Only text messages can be updated", ErrorCodes.ValidationFailed);

        message.Content = newContent;
        await _context.SaveChangesAsync();

        // Invalidate cache
        await _cacheService.RemoveByPatternAsync($"user_conversations_{userId}_*");

        await _auditService.LogAsync(userId, "MessageUpdated", 
            System.Text.Json.JsonSerializer.Serialize(new { MessageId = messageId }));

        return BaseResponse<string>.SuccessResponse("Message updated successfully");
    }

    public async Task<BaseResponse<string>> MarkReadAsync(int conversationId, int userId)
    {
        var membership = await _context.ConversationMembers
            .FirstOrDefaultAsync(cm => cm.ConversationId == conversationId && cm.UserId == userId);

        if (membership == null)
            return BaseResponse<string>.ErrorResponse("Not a member of this conversation", ErrorCodes.AccessDenied);

        membership.LastReadAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Invalidate cache
        await _cacheService.RemoveByPatternAsync($"user_conversations_{userId}_*");

        return BaseResponse<string>.SuccessResponse("Marked as read");
    }

    public async Task<BaseResponse<List<MessageDto>>> SearchMessagesAsync(string query, int? conversationId, int userId, int page = 1, int limit = 20)
    {
        var messageQuery = _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Conversation)
                .ThenInclude(c => c.Members)
            .Where(m => !m.IsDeleted && m.Content.Contains(query))
            .AsQueryable();

        if (conversationId.HasValue)
        {
            // Check if user is member
            var isMember = await _context.ConversationMembers.AnyAsync(cm =>
                cm.ConversationId == conversationId.Value && cm.UserId == userId);
            if (!isMember)
                return BaseResponse<List<MessageDto>>.ErrorResponse("Access denied", ErrorCodes.AccessDenied);

            messageQuery = messageQuery.Where(m => m.ConversationId == conversationId.Value);
        }
        else
        {
            // Only search in user's conversations
            var userConversationIds = await _context.ConversationMembers
                .Where(cm => cm.UserId == userId)
                .Select(cm => cm.ConversationId)
                .ToListAsync();
            messageQuery = messageQuery.Where(m => userConversationIds.Contains(m.ConversationId));
        }

        var messages = await messageQuery
            .OrderByDescending(m => m.SentAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                SenderId = m.SenderId,
                SenderName = m.Sender.FullName,
                Content = m.Content,
                Type = m.Type,
                AttachedExamId = m.AttachedExamId,
                AttachedExamResultId = m.AttachedExamResultId,
                SentAt = m.SentAt
            })
            .ToListAsync();

        return BaseResponse<List<MessageDto>>.SuccessResponse(messages);
    }
}

public class ConversationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsGroup { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTime LastMessageAt { get; set; }
    public MessagePreviewDto? LastMessage { get; set; }
    public int UnreadCount { get; set; }
}

public class MessagePreviewDto
{
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public string SenderName { get; set; } = string.Empty;
}

public class ConversationDetailDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public bool IsGroup { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<ConversationMemberDto> Members { get; set; } = new();
}

public class ConversationMemberDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public DateTime JoinedAt { get; set; }
}

public class MessageDto
{
    public int Id { get; set; }
    public int SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public MessageType Type { get; set; }
    public int? AttachedExamId { get; set; }
    public int? AttachedExamResultId { get; set; }
    public DateTime SentAt { get; set; }
}
