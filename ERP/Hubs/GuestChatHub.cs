using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using ERP.Data;
using ERP.Models;
using Microsoft.EntityFrameworkCore;

namespace ERP.Hubs
{
    [AllowAnonymous]
    public class GuestChatHub : Hub
    {
        private readonly ERPContext _context;

        public GuestChatHub(ERPContext context)
        {
            _context = context;
        }

        private int? GetGuestUserId()
        {
            var guestIdClaim = Context.User?.FindFirst("GuestUserId")?.Value;
            return guestIdClaim != null ? int.Parse(guestIdClaim) : null;
        }

        public override async Task OnConnectedAsync()
        {
            var guestUserId = GetGuestUserId();
            if (guestUserId.HasValue)
            {
                var guest = await _context.GuestUsers.FindAsync(guestUserId.Value);
                if (guest != null && guest.IsActive && guest.ExpiryDate > DateTime.Now)
                {
                    guest.LastActivity = DateTime.Now;
                    await _context.SaveChangesAsync();

                    // اضافه کردن به گروه شخصی مهمان
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"Guest_{guestUserId}");

                    // اطلاع رسانی به کارمند مسئول
                    var access = await _context.GuestChatAccesses
                        .FirstOrDefaultAsync(gca => gca.GuestUserId == guestUserId.Value);

                    if (access != null)
                    {
                        await Clients.Group($"User_{access.AllowedUserId}")
                            .SendAsync("GuestOnline", guestUserId.Value);
                    }
                }
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var guestUserId = GetGuestUserId();
            if (guestUserId.HasValue)
            {
                var access = await _context.GuestChatAccesses
                    .FirstOrDefaultAsync(gca => gca.GuestUserId == guestUserId.Value);

                if (access != null)
                {
                    await Clients.Group($"User_{access.AllowedUserId}")
                        .SendAsync("GuestOffline", guestUserId.Value);
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessageToEmployee(string message)
        {
            var guestUserId = GetGuestUserId();
            if (!guestUserId.HasValue) return;

            var guest = await _context.GuestUsers.FindAsync(guestUserId.Value);
            if (guest == null || !guest.IsActive || guest.ExpiryDate < DateTime.Now) return;

            // پیدا کردن کارمند مسئول
            var access = await _context.GuestChatAccesses
                .FirstOrDefaultAsync(gca => gca.GuestUserId == guestUserId.Value);

            if (access == null) return;

            // ذخیره پیام
            var chatMessage = new ChatMessage
            {
                SenderId = $"Guest_{guestUserId}",
                ReceiverId = access.AllowedUserId,
                Message = message,
                SentAt = DateTime.Now
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            // ارسال فقط به کارمند مسئول
            await Clients.Group($"User_{access.AllowedUserId}").SendAsync("ReceiveGuestMessage", new
            {
                id = chatMessage.Id,
                guestUserId = guestUserId.Value,
                guestName = $"{guest.FirstName} {guest.LastName}".Trim(),
                message = message,
                sentAt = chatMessage.SentAt
            });

            // تایید به مهمان
            await Clients.Caller.SendAsync("MessageSent", new
            {
                id = chatMessage.Id,
                message = message,
                sentAt = chatMessage.SentAt
            });
        }

        public async Task SendTypingToEmployee()
        {
            var guestUserId = GetGuestUserId();
            if (!guestUserId.HasValue) return;

            var access = await _context.GuestChatAccesses
                .FirstOrDefaultAsync(gca => gca.GuestUserId == guestUserId.Value);

            if (access != null)
            {
                await Clients.Group($"User_{access.AllowedUserId}")
                    .SendAsync("GuestTyping", guestUserId.Value);
            }
        }

        public async Task StopTypingToEmployee()
        {
            var guestUserId = GetGuestUserId();
            if (!guestUserId.HasValue) return;

            var access = await _context.GuestChatAccesses
                .FirstOrDefaultAsync(gca => gca.GuestUserId == guestUserId.Value);

            if (access != null)
            {
                await Clients.Group($"User_{access.AllowedUserId}")
                    .SendAsync("GuestStoppedTyping", guestUserId.Value);
            }
        }

        // متد برای کارمند برای ارسال پیام به مهمان
        public async Task SendMessageToGuest(int guestUserId, string message)
        {
            var employeeId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(employeeId)) return;

            // بررسی دسترسی
            var hasAccess = await _context.GuestChatAccesses
                .AnyAsync(gca => gca.GuestUserId == guestUserId && gca.AllowedUserId == employeeId);

            if (!hasAccess) return;

            var guest = await _context.GuestUsers.FindAsync(guestUserId);
            if (guest == null || !guest.IsActive) return;

            // ذخیره پیام
            var chatMessage = new ChatMessage
            {
                SenderId = employeeId,
                ReceiverId = $"Guest_{guestUserId}",
                Message = message,
                SentAt = DateTime.Now
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            // ارسال فقط به مهمان
            await Clients.Group($"Guest_{guestUserId}").SendAsync("ReceiveEmployeeMessage", new
            {
                id = chatMessage.Id,
                message = message,
                sentAt = chatMessage.SentAt
            });

            // تایید به کارمند
            await Clients.Caller.SendAsync("MessageSentToGuest", new
            {
                id = chatMessage.Id,
                guestUserId = guestUserId,
                message = message,
                sentAt = chatMessage.SentAt
            });
        }
    }
}
