using Microsoft.AspNetCore.SignalR;
using KeremProject1backend.Models.DBs;
using Microsoft.AspNetCore.Authorization;
using KeremProject1backend.Infrastructure;
using KeremProject1backend.Services;
using Microsoft.EntityFrameworkCore;

namespace KeremProject1backend.Hubs;

[Authorize] // Require authentication for all hub methods
public class ChatHub : Hub
{
    private readonly ApplicationContext _context;
    private readonly SessionService _sessionService;

    public ChatHub(ApplicationContext context, SessionService sessionService)
    {
        _context = context;
        _sessionService = sessionService;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            // Join user-specific group for personal notifications
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
        }
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Join a conversation room (can be classroom-based or private)
    /// </summary>
    public async Task JoinConversation(int conversationId)
    {
        // Authorization: User must be part of the conversation
        var userId = int.Parse(Context.User!.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

        var conversation = await _context.Conversations
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == conversationId);

        if (conversation == null)
        {
            await Clients.Caller.SendAsync("Error", "Conversation not found");
            return;
        }

        // Check if user is member
        var isMember = conversation.Members.Any(m => m.UserId == userId);
        if (!isMember)
        {
            await Clients.Caller.SendAsync("Error", "Access denied to this conversation");
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"Conv_{conversationId}");
        await Clients.Caller.SendAsync("JoinedConversation", conversationId);
    }

    /// <summary>
    /// Leave a conversation room
    /// </summary>
    public async Task LeaveConversation(int conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Conv_{conversationId}");
        await Clients.Caller.SendAsync("LeftConversation", conversationId);
    }


    /// <summary>
    /// Typing indicator for real-time feedback
    /// </summary>
    public async Task UserTyping(int conversationId)
    {
        var userId = int.Parse(Context.User!.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users.FindAsync(userId);

        if (user != null)
        {
            await Clients.OthersInGroup($"Conv_{conversationId}").SendAsync("UserTyping", new
            {
                UserId = userId,
                UserName = user.FullName
            });
        }
    }

    /// <summary>
    /// Stop typing indicator
    /// </summary>
    public async Task UserStoppedTyping(int conversationId)
    {
        var userId = int.Parse(Context.User!.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

        await Clients.OthersInGroup($"Conv_{conversationId}").SendAsync("UserStoppedTyping", new
        {
            UserId = userId
        });
    }
}
