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

    public MessageOperations(ApplicationContext context, SessionService sessionService, IHubContext<ChatHub> chatHub)
    {
        _context = context;
        _sessionService = sessionService;
        _chatHub = chatHub;
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
        await _context.SaveChangesAsync();

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
            .Where(m => m.ConversationId == conversationId)
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
