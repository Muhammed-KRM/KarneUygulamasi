using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace KeremProject1backend.Hubs;

[Authorize]
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

    /// <summary>
    /// Join institution-wide notification group
    /// </summary>
    public async Task JoinInstitutionGroup(int institutionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Inst_{institutionId}");
    }

    /// <summary>
    /// Leave institution group
    /// </summary>
    public async Task LeaveInstitutionGroup(int institutionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Inst_{institutionId}");
    }

    /// <summary>
    /// Join classroom-specific notification group
    /// </summary>
    public async Task JoinClassroomGroup(int classroomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Class_{classroomId}");
    }

    /// <summary>
    /// Leave classroom group
    /// </summary>
    public async Task LeaveClassroomGroup(int classroomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Class_{classroomId}");
    }
}
