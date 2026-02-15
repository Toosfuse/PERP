using ERP.Data;
using ERP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;

namespace ERP.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class ChatMonitoringController : Controller
    {
        private readonly ERPContext _context;

        public ChatMonitoringController(ERPContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
        public DateTime ShamsiStringToMiladi(string shamsiDate)
        {
            // جدا کردن سال، ماه، روز (فرمت yyyy/MM/dd یا yyyy/M/d)
            string[] parts = shamsiDate.Replace("۰", "0").Replace("۱", "1") // تبدیل ارقام فارسی به انگلیسی اگر لازم بود
                                       .Replace("۲", "2").Replace("۳", "3").Replace("۴", "4")
                                       .Replace("۵", "5").Replace("۶", "6").Replace("۷", "7")
                                       .Replace("۸", "8").Replace("۹", "9")
                                       .Split('/', '-');

            int year = int.Parse(parts[0]);
            int month = int.Parse(parts[1]);
            int day = int.Parse(parts[2]);

            PersianCalendar pc = new PersianCalendar();
            return pc.ToDateTime(year, month, day, 0, 0, 0, 0);
        }
        [HttpGet]
        public async Task<IActionResult> GetActiveConversations(string fromDate, string toDate, bool? isOnline, bool? isRead)
        {
            try
            {
                var query = _context.ChatMessages.AsQueryable();

                if (!string.IsNullOrEmpty(fromDate))
                    query = query.Where(m => m.SentAt.Date >= ShamsiStringToMiladi(fromDate).Date);
                if (!string.IsNullOrEmpty(toDate))
                    query = query.Where(m => m.SentAt.Date <= ShamsiStringToMiladi(toDate).Date);
                if (isRead.HasValue)
                    query = query.Where(m => m.IsRead == isRead.Value);

                var conversations = await query
                    .Select(m => new { m.SenderId, m.ReceiverId, m.SentAt, m.Message, m.IsRead })
                    .ToListAsync();

                var grouped = conversations
                    .GroupBy(m => new { 
                        User1 = string.Compare(m.SenderId, m.ReceiverId) < 0 ? m.SenderId : m.ReceiverId,
                        User2 = string.Compare(m.SenderId, m.ReceiverId) < 0 ? m.ReceiverId : m.SenderId
                    })
                    .Select(g => new {
                        user1Id = g.Key.User1,
                        user2Id = g.Key.User2,
                        lastMessage = g.OrderByDescending(m => m.SentAt).FirstOrDefault(),
                        messageCount = g.Count(),
                        unreadCount = g.Count(m => !m.IsRead)
                    })
                    .OrderByDescending(c => c.lastMessage.SentAt)
                    .Take(50)
                    .ToList();

                var result = new List<object>();
                foreach (var conv in grouped)
                {
                    object user1Data = null, user2Data = null;

                    
                        var guest1 = await _context.GuestUsers.FirstOrDefaultAsync(g => g.UniqueToken == conv.user1Id && g.IsActive);
                        if (guest1 != null)
                        {
                            user1Data = new {
                                id = conv.user1Id,
                                name = (guest1.FirstName + " " + guest1.LastName) ?? "مهمان",
                                image = string.IsNullOrEmpty(guest1.Image) ? "/UserImage/Male.png" : "/UserImage/" + guest1.Image,
                                isOnline = false,
                                isGuest = true
                            };
                        }
                    
                    
                        var user1 = await _context.Users.FindAsync(conv.user1Id);
                        if (user1 != null)
                        {
                            user1Data = new {
                                id = user1.Id,
                                name = user1.FirstName + " " + user1.LastName,
                                image = string.IsNullOrEmpty(user1.Image) ? "/UserImage/Male.png" : "/UserImage/" + user1.Image,
                                isOnline = user1.IsOnline,
                                isGuest = false
                            };
                        }
                    

                    
                        var guest2 = await _context.GuestUsers.FirstOrDefaultAsync(g => g.UniqueToken == conv.user2Id && g.IsActive);
                        if (guest2 != null)
                        {
                            user2Data = new {
                                id = conv.user2Id,
                                name = (guest2.FirstName + " " + guest2.LastName) ?? "مهمان",
                                image = string.IsNullOrEmpty(guest2.Image) ? "/UserImage/Male.png" : "/UserImage/" + guest2.Image,
                                isOnline = false,
                                isGuest = true
                            };
                        }
                   
            
                        var user2 = await _context.Users.FindAsync(conv.user2Id);
                        if (user2 != null)
                        {
                            user2Data = new {
                                id = user2.Id,
                                name = user2.FirstName + " " + user2.LastName,
                                image = string.IsNullOrEmpty(user2.Image) ? "/UserImage/Male.png" : "/UserImage/" + user2.Image,
                                isOnline = user2.IsOnline,
                                isGuest = false
                            };
                        }
                   
                    if (user1Data != null && user2Data != null)
                    {
                        result.Add(new {
                            user1 = user1Data,
                            user2 = user2Data,
                            lastMessage = conv.lastMessage?.Message,
                            lastMessageTime = conv.lastMessage?.SentAt,
                            messageCount = conv.messageCount,
                            unreadCount = conv.unreadCount
                        });
                    }
                }

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetConversationMessages(string user1Id, string user2Id, string? fromDate, string? toDate, bool? isRead)
        {
            var query = _context.ChatMessages
                .Where(m => (m.SenderId == user1Id && m.ReceiverId == user2Id) ||
                           (m.SenderId == user2Id && m.ReceiverId == user1Id));

            if (!string.IsNullOrEmpty(fromDate))
                query = query.Where(m => m.SentAt.Date >= ShamsiStringToMiladi(fromDate).Date);
            if (!string.IsNullOrEmpty(toDate))
                query = query.Where(m => m.SentAt.Date <= ShamsiStringToMiladi(toDate).Date);
            if (isRead.HasValue)
                query = query.Where(m => m.IsRead == isRead.Value);

            var messages = await query
                .OrderBy(m => m.SentAt)
                .Select(m => new {
                    id = m.Id,
                    senderId = m.SenderId,
                    receiverId = m.ReceiverId,
                    message = m.Message,
                    sentAt = m.SentAt,
                    isRead = m.IsRead,
                    isDelivered = m.IsDelivered,
                    attachmentPath = m.AttachmentPath,
                    attachmentName = m.AttachmentName,
                    isEdited = m.IsEdited,
                    editedAt = m.EditedAt,
                    replyToMessageId = m.ReplyToMessageId,
                    replyToMessage = m.ReplyToMessage,
                    replyToSenderName = m.ReplyToSenderName,
                    isDeletedBySender = m.IsDeletedBySender,
                    isDeletedByReceiver = m.IsDeletedByReceiver,
                    deletedAt = m.DeletedAt
                })
                .ToListAsync();

            return Json(messages);
        }

        [HttpGet]
        public async Task<IActionResult> SearchConversations(string query)
        {
            if (string.IsNullOrEmpty(query))
                return Json(new List<object>());

            var users = await _context.Users
                .Where(u => u.FirstName.Contains(query) || u.LastName.Contains(query))
                .Select(u => new {
                    id = u.Id,
                    name = u.FirstName + " " + u.LastName,
                    image = string.IsNullOrEmpty(u.Image) ? "/UserImage/Male.png" : "/UserImage/" + u.Image
                })
                .Take(10)
                .ToListAsync();

            return Json(users);
        }

        [HttpGet]
        public async Task<IActionResult> GetStatistics()
        {
            var totalMessages = await _context.ChatMessages.CountAsync();
            var totalUsers = await _context.Users.CountAsync();
            var activeConversations = await _context.ChatMessages
                .GroupBy(m => new { 
                    User1 = m.SenderId.CompareTo(m.ReceiverId) < 0 ? m.SenderId : m.ReceiverId,
                    User2 = m.SenderId.CompareTo(m.ReceiverId) < 0 ? m.ReceiverId : m.SenderId
                })
                .CountAsync();

            var last24Hours = DateTime.Now.AddHours(-24);
            var messagesLast24h = await _context.ChatMessages
                .Where(m => m.SentAt >= last24Hours)
                .CountAsync();

            var topUsers = await _context.ChatMessages
                .GroupBy(m => m.SenderId)
                .Select(g => new {
                    userId = g.Key,
                    count = g.Count()
                })
                .OrderByDescending(x => x.count)
                .Take(5)
                .ToListAsync();

            var topUsersData = new List<object>();
            foreach (var user in topUsers)
            {
                // بررسی مهمان یا کاربر
                if (user.userId.Contains("-"))
                {
                    var guest = await _context.GuestUsers.FirstOrDefaultAsync(g => g.UniqueToken == user.userId && g.IsActive);
                    if (guest != null)
                    {
                        topUsersData.Add(new {
                            name = (guest.FirstName + " " + guest.LastName) ?? "مهمان",
                            count = user.count
                        });
                    }
                }
                else
                {
                    var u = await _context.Users.FindAsync(user.userId);
                    if (u != null)
                    {
                        topUsersData.Add(new {
                            name = u.FirstName + " " + u.LastName,
                            count = user.count
                        });
                    }
                }
            }

            return Json(new {
                totalMessages,
                totalUsers,
                activeConversations,
                messagesLast24h,
                topUsers = topUsersData
            });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            var message = await _context.ChatMessages.FindAsync(messageId);
            if (message == null)
                return Json(new { success = false, error = "پیام یافت نشد" });

            _context.ChatMessages.Remove(message);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> SearchInMessages(string query)
        {
            if (string.IsNullOrEmpty(query))
                return Json(new List<object>());

            var messages = await _context.ChatMessages
                .Where(m => m.Message.Contains(query))
                .OrderByDescending(m => m.SentAt)
                .Take(50)
                .Select(m => new {
                    id = m.Id,
                    senderId = m.SenderId,
                    receiverId = m.ReceiverId,
                    message = m.Message,
                    sentAt = m.SentAt
                })
                .ToListAsync();

            return Json(messages);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserConversations(string userId)
        {
            var conversations = await _context.ChatMessages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Select(g => new {
                    otherUserId = g.Key,
                    messageCount = g.Count(),
                    lastMessage = g.OrderByDescending(m => m.SentAt).FirstOrDefault()
                })
                .ToListAsync();

            var result = new List<object>();
            foreach (var conv in conversations)
            {
                object userData = null;

                // بررسی مهمان یا کاربر
                if (conv.otherUserId.Contains("-"))
                {
                    var guest = await _context.GuestUsers.FirstOrDefaultAsync(g => g.UniqueToken == conv.otherUserId && g.IsActive);
                    if (guest != null)
                    {
                        userData = new {
                            id = conv.otherUserId,
                            name = (guest.FirstName + " " + guest.LastName) ?? "مهمان",
                            image = string.IsNullOrEmpty(guest.Image) ? "/UserImage/Male.png" : "/UserImage/" + guest.Image,
                            isGuest = true
                        };
                    }
                }
                else
                {
                    var user = await _context.Users.FindAsync(conv.otherUserId);
                    if (user != null)
                    {
                        userData = new {
                            id = user.Id,
                            name = user.FirstName + " " + user.LastName,
                            image = string.IsNullOrEmpty(user.Image) ? "/UserImage/Male.png" : "/UserImage/" + user.Image,
                            isGuest = false
                        };
                    }
                }

                if (userData != null)
                {
                    result.Add(new {
                        user = userData,
                        messageCount = conv.messageCount,
                        lastMessage = conv.lastMessage?.Message,
                        lastMessageTime = conv.lastMessage?.SentAt
                    });
                }
            }

            return Json(result);
        }
    }
}
