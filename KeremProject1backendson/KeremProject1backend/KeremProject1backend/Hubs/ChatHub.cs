using Microsoft.AspNetCore.SignalR;
using KeremProject1backend.Models.DBs;

namespace KeremProject1backend.Hubs;

public class ChatHub : Hub
{
    // Real-time group messaging for classrooms or private chats

    public async Task JoinConversation(int conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Conv_{conversationId}");
    }

    public async Task LeaveConversation(int conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Conv_{conversationId}");
    }
}
