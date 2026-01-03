using Microsoft.AspNetCore.SignalR;

namespace KeremProject1backend.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task SendToGroup(string groupName, string user, string message)
        {
            await Clients.Group(groupName).SendAsync("ReceiveGroupMessage", user, message);
        }
    }
}
