using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using ERP.Data;
using ERP.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ERP.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ERPContext _context;

        public ChatHub(ERPContext context)
        {
            _context = context;
        }

        private string GetUserId() => Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            if (userId != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
                await Clients.All.SendAsync("UserOnline", userId);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = GetUserId();
            if (userId != null)
            {
                await Clients.All.SendAsync("UserOffline", userId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task MarkAsRead(string senderId)
        {
            var receiverId = GetUserId();
            if (string.IsNullOrEmpty(receiverId)) return;
            
            var unreadMessages = await _context.ChatMessages
                .Where(m => m.SenderId == senderId && m.ReceiverId == receiverId && !m.IsRead)
                .ToListAsync();

            unreadMessages.ForEach(m => {
                m.IsRead = true;
                m.ReadAt = DateTime.Now;
            });
            await _context.SaveChangesAsync();
            
            await Clients.Group(senderId).SendAsync("MessagesRead", receiverId);
        }

        public async Task SendTyping(string receiverId)
        {
            var senderId = GetUserId();
            if (senderId != null)
            {
                await Clients.Group(receiverId).SendAsync("UserTyping", senderId);
            }
        }

        public async Task StopTyping(string receiverId)
        {
            var senderId = GetUserId();
            if (senderId != null)
            {
                await Clients.Group(receiverId).SendAsync("UserStoppedTyping", senderId);
            }
        }

        public async Task JoinGroup(int groupId)
        {
            var userId = GetUserId();
            if (userId != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"group-{groupId}");
            }
        }

        public async Task LeaveGroup(int groupId)
        {
            var userId = GetUserId();
            if (userId != null)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"group-{groupId}");
            }
        }

        public async Task SendGroupMessage(int groupId, string message)
        {
            var senderId = GetUserId();
            if (senderId != null)
            {
                await Clients.Group($"group-{groupId}").SendAsync("ReceiveGroupMessage", new
                {
                    senderId = senderId,
                    message = message,
                    sentAt = DateTime.Now.ToString("HH:mm")
                });
            }
        }

        public async Task NotifyGroupMessageDeleted(int groupId, int messageId)
        {
            await Clients.Group($"group-{groupId}").SendAsync("GroupMessageDeleted", new { id = messageId });
        }

        public async Task NotifyGroupMemberCountUpdated(int groupId, int memberCount)
        {
            await Clients.All.SendAsync("GroupMemberCountUpdated", groupId, memberCount);
        }

        public async Task NotifyGroupMessageEdited(int groupId, int messageId, string message)
        {
            await Clients.Group($"group-{groupId}").SendAsync("GroupMessageEdited", new { id = messageId, message = message });
        }
    }
}
