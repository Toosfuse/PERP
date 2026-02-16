using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using ERP.Data;
using ERP.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ERP.Hubs
{
    [AllowAnonymous]
    public class ChatHub : Hub
    {
        private readonly ERPContext _context;

        public ChatHub(ERPContext context)
        {
            _context = context;
        }

        private string GetUserId() => Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        private string GetGuestToken()
        {
            var httpContext = Context.GetHttpContext();
            return httpContext?.Request.Cookies["GuestToken"];
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            var guestToken = GetGuestToken();
            
            if (!string.IsNullOrEmpty(guestToken))
            {
                // مهمان
                var guest = await _context.GuestUsers
                    .FirstOrDefaultAsync(g => g.UniqueToken == guestToken && g.IsActive && g.ExpiryDate > DateTime.Now);
                
                if (guest != null)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{guestToken}");
                }
            }
            else if (userId != null)
            {
                // کارمند
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.IsOnline = true;
                    user.LastSeen = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
                
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
                await Groups.AddToGroupAsync(Context.ConnectionId, "Employees");
                
                var allowedUsers = await _context.ChatAccesses
                    .Where(ca => ca.AllowedUserId == userId && !ca.IsBlocked)
                    .Select(ca => ca.UserId)
                    .ToListAsync();
                
                foreach (var allowedUserId in allowedUsers)
                {
                    await Clients.Group($"User_{allowedUserId}").SendAsync("UserOnline", userId);
                }
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = GetUserId();
            if (userId != null)
            {
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.IsOnline = false;
                    user.LastSeen = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
                
                // اطلاع رسانی فقط به کاربرانی که دسترسی دارند
                var allowedUsers = await _context.ChatAccesses
                    .Where(ca => ca.AllowedUserId == userId && !ca.IsBlocked)
                    .Select(ca => ca.UserId)
                    .ToListAsync();
                
                foreach (var allowedUserId in allowedUsers)
                {
                    await Clients.Group($"User_{allowedUserId}").SendAsync("UserOffline", userId);
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task MarkAsRead(string senderId)
        {
            var receiverId = GetUserId();
            if (string.IsNullOrEmpty(receiverId)) return;
            
            // بررسی دسترسی
            var hasAccess = await _context.ChatAccesses
                .AnyAsync(ca => ca.UserId == receiverId && ca.AllowedUserId == senderId && !ca.IsBlocked);
            
            if (!hasAccess) return;
            
            var unreadMessages = await _context.ChatMessages
                .Where(m => m.SenderId == senderId && m.ReceiverId == receiverId && !m.IsRead)
                .ToListAsync();

            unreadMessages.ForEach(m => {
                m.IsRead = true;
                m.ReadAt = DateTime.Now;
            });
            await _context.SaveChangesAsync();
            
            await Clients.Group($"User_{senderId}").SendAsync("MessagesRead", receiverId);
        }

        public async Task SendTyping(string receiverId)
        {
            var senderId = GetUserId();
            if (senderId != null)
            {
                // بررسی دسترسی
                var hasAccess = await _context.ChatAccesses
                    .AnyAsync(ca => ca.UserId == senderId && ca.AllowedUserId == receiverId && !ca.IsBlocked);
                
                if (!hasAccess) return;
                
                await Clients.Group($"User_{receiverId}").SendAsync("UserTyping", senderId);
            }
        }

        public async Task StopTyping(string receiverId)
        {
            var senderId = GetUserId();
            if (senderId != null)
            {
                // بررسی دسترسی
                var hasAccess = await _context.ChatAccesses
                    .AnyAsync(ca => ca.UserId == senderId && ca.AllowedUserId == receiverId && !ca.IsBlocked);
                
                if (!hasAccess) return;
                
                await Clients.Group($"User_{receiverId}").SendAsync("UserStoppedTyping", senderId);
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
            // ارسال فقط به اعضای گروه
            await Clients.Group($"group-{groupId}").SendAsync("GroupMemberCountUpdated", groupId, memberCount);
        }

        public async Task NotifyGroupMessageEdited(int groupId, int messageId, string message)
        {
            await Clients.Group($"group-{groupId}").SendAsync("GroupMessageEdited", new { id = messageId, message = message });
        }

        public async Task NotifyUnreadCount(string userId, int count)
        {
            await Clients.Group($"User_{userId}").SendAsync("UpdateUnreadCount", count);
        }
        
        public async Task NotifyNewConversation(string userId, string otherUserId, string userName, string userImage, string lastMessage)
        {
            await Clients.Group($"User_{userId}").SendAsync("NewConversation", new
            {
                userId = otherUserId,
                userName = userName,
                userImage = userImage,
                lastMessage = lastMessage
            });
        }
        
        // متد جدید برای ارسال پیام با بررسی دسترسی
        public async Task SendMessage(string receiverId, string message)
        {
            var senderId = GetUserId();
            if (string.IsNullOrEmpty(senderId)) return;
            
            // بررسی دسترسی
            var hasAccess = await _context.ChatAccesses
                .AnyAsync(ca => ca.UserId == senderId && ca.AllowedUserId == receiverId && !ca.IsBlocked);
            
            if (!hasAccess) return;
            
            // ذخیره پیام
            var chatMessage = new ChatMessage
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Message = message,
                SentAt = DateTime.Now
            };
            
            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();
            
            // ارسال فقط به گیرنده
            await Clients.Group($"User_{receiverId}").SendAsync("ReceiveMessage", new
            {
                id = chatMessage.Id,
                senderId = senderId,
                message = message,
                sentAt = chatMessage.SentAt
            });
            
            // ارسال تایید به فرستنده
            await Clients.Caller.SendAsync("MessageSent", new
            {
                id = chatMessage.Id,
                receiverId = receiverId,
                message = message,
                sentAt = chatMessage.SentAt
            });
        }
    }
}
