using Microsoft.AspNetCore.SignalR;

namespace KeremProject1backend.Hubs;

public class NotificationHub : Hub
{
    // Real-time system alerts (e.g., "Exam Results Ready")

    public override async Task OnConnectedAsync()
    {
        // Join a group specific to the user ID
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
        }
        await base.OnConnectedAsync();
    }
}
