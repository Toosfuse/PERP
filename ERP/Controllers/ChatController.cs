using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ERP.Data;
using ERP.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using ERP.Hubs;
using ERP.Services;
using static Ganss.Xss.HtmlSanitizer;
using Ganss.Xss;

namespace ERP.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ERPContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IServices _services;

        public ChatController(ERPContext context, IWebHostEnvironment environment, IHubContext<ChatHub> hubContext,IServices services)
        {
            _context = context;
            _environment = environment;
            _hubContext = hubContext;
            _services = services;
        }

        public async Task<IActionResult> Index()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var users = await GetChatUsers(currentUserId);
            return View(users);
        }

        [HttpGet]
        public IActionResult GetCurrentUserName()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            return Json(new { name = user != null ? user.FirstName + " " + user.LastName : "کاربر" });
        }

        [HttpGet]
        public async Task<IActionResult> GetMessages(string userId, int page = 1, int pageSize = 50)
        {
            if (string.IsNullOrEmpty(userId)) return BadRequest();
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var messages = await _context.ChatMessages
                .Where(m => (m.SenderId == currentUserId && m.ReceiverId == userId && !m.IsDeletedBySender) ||
                           (m.SenderId == userId && m.ReceiverId == currentUserId && !m.IsDeletedByReceiver))
                .OrderByDescending(m => m.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .OrderBy(m => m.SentAt)
                .Select(m => new {
                    id = m.Id,
                    senderId = m.SenderId,
                    receiverId = m.ReceiverId,
                    message = m.Message,
                    sentAt = m.SentAt.ToString("HH:mm"),
                    dateAt = _services.iGregorianToPersian(m.SentAt),
                    isDelivered = m.IsDelivered,
                    isRead = m.IsRead,
                    readAt = m.ReadAt,
                    isEdited = m.IsEdited,
                    editedAt = m.EditedAt,
                    attachmentPath = m.AttachmentPath,
                    attachmentName = m.AttachmentName,
                    replyToMessageId = m.ReplyToMessageId,
                    replyToMessage = m.ReplyToMessage,
                    replyToSenderName = m.ReplyToSenderName,
                    forwardedFromMessageId = m.ForwardedFromMessageId,
                    isMine = m.SenderId == currentUserId,
                    isDeleted = (m.SenderId == currentUserId && m.IsDeletedBySender) || (m.SenderId == userId && m.IsDeletedByReceiver)
                })
                .ToListAsync();

            return Json(messages);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(string receiverId, string message, string attachmentPath, string attachmentName, int? replyToMessageId)
        {
            try
            {
                if (string.IsNullOrEmpty(receiverId))
                    return Json(new { success = false, error = "گیرنده مشخص نشده" });

                if (string.IsNullOrEmpty(message) && string.IsNullOrEmpty(attachmentPath))
                    return Json(new { success = false, error = "پیام یا فایل الزامی است" });

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                string replyToMessage = null;
                string replyToSenderName = null;
                
                if (replyToMessageId.HasValue)
                {
                    var repliedMessage = await _context.ChatMessages.FindAsync(replyToMessageId.Value);
                    if (repliedMessage != null)
                    {
                        replyToMessage = repliedMessage.Message;
                        var sender = await _context.Users.FindAsync(repliedMessage.SenderId);
                        replyToSenderName = sender?.FirstName + " " + sender?.LastName;
                    }
                }
                
                var sanitizer = new HtmlSanitizer();
                var sanitizedMessage = sanitizer.Sanitize(message);
                
                var chatMessage = new ChatMessage
                {
                    SenderId = currentUserId,
                    ReceiverId = receiverId,
                    Message = sanitizedMessage,
                    SentAt = DateTime.Now,
                    IsRead = false,
                    AttachmentPath = attachmentPath,
                    AttachmentName = attachmentName,
                    ReplyToMessageId = replyToMessageId,
                    ReplyToMessage = replyToMessage,
                    ReplyToSenderName = replyToSenderName
                };
                
                _context.ChatMessages.Add(chatMessage);
                await _context.SaveChangesAsync();

                var messageData = new
                {
                    id = chatMessage.Id,
                    senderId = currentUserId,
                    receiverId = receiverId,
                    message = sanitizedMessage,
                    sentAt = chatMessage.SentAt.ToString("HH:mm"),
                    dateAt= _services.iGregorianToPersian(chatMessage.SentAt),
                    isDelivered = false,
                    isRead = false,
                    attachmentPath = attachmentPath,
                    attachmentName = attachmentName,
                    replyToMessageId = replyToMessageId,
                    replyToMessage = replyToMessage,
                    replyToSenderName = replyToSenderName
                };

                await _hubContext.Clients.User(receiverId).SendAsync("ReceiveMessage", messageData);
                await _hubContext.Clients.User(currentUserId).SendAsync("ReceiveMessage", messageData);
                
                return Json(new { success = true, messageId = chatMessage.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "خطا در ارسال پیام" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var unreadMessages = await _context.ChatMessages
                .Where(m => m.SenderId == userId && m.ReceiverId == currentUserId && !m.IsRead)
                .ToListAsync();
            
            unreadMessages.ForEach(m => {
                m.IsRead = true;
                m.ReadAt = DateTime.Now;
            });
            await _context.SaveChangesAsync();
            
            await _hubContext.Clients.User(userId).SendAsync("MessagesRead", currentUserId);
            
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsDelivered(int messageId)
        {
            var message = await _context.ChatMessages.FindAsync(messageId);
            if (message != null && !message.IsDelivered)
            {
                message.IsDelivered = true;
                message.DeliveredAt = DateTime.Now;
                await _context.SaveChangesAsync();
                
                await _hubContext.Clients.User(message.SenderId).SendAsync("MessageDelivered", new { id = messageId });
            }
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMessage(int messageId, string newMessage)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var message = await _context.ChatMessages.FindAsync(messageId);
            
            if (message == null || message.SenderId != currentUserId)
                return Json(new { success = false, error = "پیام یافت نشد" });
            
            if (message.IsRead)
                return Json(new { success = false, error = "پیام خوانده شده قابل ویرایش نیست" });
            
            var sanitizer = new HtmlSanitizer();
            var sanitizedMessage = sanitizer.Sanitize(newMessage);
            
            message.Message = sanitizedMessage;
            message.IsEdited = true;
            message.EditedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            
            await _hubContext.Clients.User(message.ReceiverId).SendAsync("MessageEdited", new { 
                id = messageId, 
                message = sanitizedMessage,
                editedAt = message.EditedAt
            });
            
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForwardMessage(int messageId, string receiverId)
        {
            try
            {
                if (string.IsNullOrEmpty(receiverId))
                    return Json(new { success = false, error = "گیرنده مشخص نشده" });

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var originalMessage = await _context.ChatMessages.FindAsync(messageId);
                
                if (originalMessage == null)
                    return Json(new { success = false, error = "پیام یافت نشد" });

                var sanitizer = new HtmlSanitizer();
                var sanitizedMessage = sanitizer.Sanitize(originalMessage.Message);
                
                var forwardedMessage = new ChatMessage
                {
                    SenderId = currentUserId,
                    ReceiverId = receiverId,
                    Message = sanitizedMessage,
                    SentAt = DateTime.Now,
                    IsRead = false,
                    AttachmentPath = originalMessage.AttachmentPath,
                    AttachmentName = originalMessage.AttachmentName,
                    ForwardedFromMessageId = messageId
                };
                
                _context.ChatMessages.Add(forwardedMessage);
                await _context.SaveChangesAsync();

                var messageData = new
                {
                    id = forwardedMessage.Id,
                    senderId = currentUserId,
                    receiverId = receiverId,
                    message = sanitizedMessage,
                    sentAt = forwardedMessage.SentAt.ToString("HH:mm"),
                    dateAt = _services.iGregorianToPersian(forwardedMessage.SentAt),
                    isDelivered = false,
                    isRead = false,
                    attachmentPath = forwardedMessage.AttachmentPath,
                    attachmentName = forwardedMessage.AttachmentName,
                    forwardedFromMessageId = messageId
                };

                await _hubContext.Clients.User(receiverId).SendAsync("ReceiveMessage", messageData);
                await _hubContext.Clients.User(currentUserId).SendAsync("ReceiveMessage", messageData);
                
                return Json(new { success = true, messageId = forwardedMessage.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "خطا در فوروارد پیام" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteChat(string userId, bool permanent = false)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (permanent)
            {
                var messages = await _context.ChatMessages
                    .Where(m => (m.SenderId == currentUserId && m.ReceiverId == userId) ||
                               (m.SenderId == userId && m.ReceiverId == currentUserId))
                    .ToListAsync();
                
                _context.ChatMessages.RemoveRange(messages);
            }
            else
            {
                var messages = await _context.ChatMessages
                    .Where(m => (m.SenderId == currentUserId && m.ReceiverId == userId) ||
                               (m.SenderId == userId && m.ReceiverId == currentUserId))
                    .ToListAsync();
                
                foreach (var msg in messages)
                {
                    if (msg.SenderId == currentUserId)
                        msg.IsDeletedBySender = true;
                    if (msg.ReceiverId == currentUserId)
                        msg.IsDeletedByReceiver = true;
                    msg.DeletedAt = DateTime.Now;
                }
            }
            
            await _context.SaveChangesAsync();
            
            // بررسی اینکه آیا پیامی برای نمایش باقی مانده یا نه
            var hasVisibleMessages = await _context.ChatMessages
                .AnyAsync(m => ((m.SenderId == currentUserId && m.ReceiverId == userId && !m.IsDeletedBySender) ||
                               (m.SenderId == userId && m.ReceiverId == currentUserId && !m.IsDeletedByReceiver)));
            
            return Json(new { success = true, shouldRemoveUser = !hasVisibleMessages });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var message = await _context.ChatMessages.FindAsync(messageId);
            
            if (message == null || message.SenderId != currentUserId)
                return Json(new { success = false, error = "پیام یافت نشد" });
            
            _context.ChatMessages.Remove(message);
            await _context.SaveChangesAsync();
            
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreChat(string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var messages = await _context.ChatMessages
                .Where(m => (m.SenderId == currentUserId && m.ReceiverId == userId) ||
                           (m.SenderId == userId && m.ReceiverId == currentUserId))
                .ToListAsync();
            
            foreach (var msg in messages)
            {
                if (msg.SenderId == currentUserId)
                    msg.IsDeletedBySender = false;
                if (msg.ReceiverId == currentUserId)
                    msg.IsDeletedByReceiver = false;
                msg.DeletedAt = null;
            }
            
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return Json(new { success = false, error = "فایل انتخاب نشده" });

                const long maxFileSize = 20 * 1024 * 1024;
                if (file.Length > maxFileSize)
                    return Json(new { success = false, error = "حجم فایل نباید بیشتر از 20 مگابایت باشد" });

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".txt", ".zip", ".rar", ".xml" ,".xlsx" ,".xls"};
                var extension = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                    return Json(new { success = false, error = "نوع فایل مجاز نیست" });

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "chat");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + extension;
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return Json(new
                {
                    success = true,
                    path = $"/uploads/chat/{fileName}",
                    name = file.FileName
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "خطا در آپلود فایل" });
            }
        }

        //[HttpPost]
        //public async Task<IActionResult> UploadFile(IFormFile file)
        //{
        //    try
        //    {
        //        if (file == null || file.Length == 0)
        //            return Json(new { success = false, error = "فایل انتخاب نشده" });

        //        const long maxFileSize = 20 * 1024 * 1024;
        //        if (file.Length > maxFileSize)
        //            return Json(new { success = false, error = "حجم فایل نباید بیشتر از 20 مگابایت باشد" });

        //        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "chat");
        //        Directory.CreateDirectory(uploadsFolder);

        //        var extension = Path.GetExtension(file.FileName);
        //        var fileName = Guid.NewGuid() + extension;
        //        var filePath = Path.Combine(uploadsFolder, fileName);

        //        using (var stream = new FileStream(filePath, FileMode.Create))
        //        {
        //            await file.CopyToAsync(stream);
        //        }

        //        return Json(new
        //        {
        //            success = true,
        //            path = $"/uploads/chat/{fileName}",
        //            name = file.FileName
        //        });
        //    }
        //    catch
        //    {
        //        return Json(new { success = false, error = "خطا در آپلود فایل" });
        //    }
        //}


        [HttpGet]
        public async Task<IActionResult> SearchUsers(string query)
        {
            if (string.IsNullOrEmpty(query)) return Json(new List<object>());
            
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var users = await _context.Users
                .Where(u => u.Id != currentUserId && 
                       (u.FirstName.Contains(query) || u.LastName.Contains(query)))
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
        public async Task<IActionResult> GetUserInfo(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return BadRequest();
            
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();
            
            return Json(new {
                id = user.Id,
                name = user.FirstName + " " + user.LastName,
                image = string.IsNullOrEmpty(user.Image) ? "/UserImage/Male.png" : "/UserImage/" + user.Image,
                isOnline = false
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var users = await _context.Users
                .Where(u => u.Id != currentUserId)
                .Select(u => new {
                    id = u.Id,
                    name = u.FirstName + " " + u.LastName,
                    image = string.IsNullOrEmpty(u.Image) ? "/UserImage/Male.png" : "/UserImage/" + u.Image
                })
                .ToListAsync();
            
            return Json(users);
        }

        private async Task<List<ChatUser>> GetChatUsers(string currentUserId)
        {
            var userMessages = await _context.Users
                .Where(u => u.Id != currentUserId)
                .Select(u => new {
                    User = u,
                    LastMessage = _context.ChatMessages
                        .Where(m => ((m.SenderId == u.Id && m.ReceiverId == currentUserId && !m.IsDeletedByReceiver) || 
                                   (m.SenderId == currentUserId && m.ReceiverId == u.Id && !m.IsDeletedBySender)))
                        .OrderByDescending(m => m.SentAt)
                        .FirstOrDefault(),
                    UnreadCount = _context.ChatMessages
                        .Count(m => m.SenderId == u.Id && m.ReceiverId == currentUserId && !m.IsRead && !m.IsDeletedByReceiver)
                })
                .Where(x => x.LastMessage != null)
                .ToListAsync();

            return userMessages.Select(x => new ChatUser
            {
                Id = x.User.Id,
                Name = x.User.FirstName + " " + x.User.LastName,
                Image = string.IsNullOrEmpty(x.User.Image) ? "/UserImage/Male.png" : "/UserImage/" + x.User.Image,
                IsOnline = false,
                UnreadCount = x.UnreadCount,
                LastMessage = x.LastMessage?.Message,
                LastMessageTime = x.LastMessage?.SentAt
            })
            .OrderByDescending(u => u.LastMessageTime)
            .ToList();
        }
    }
}
