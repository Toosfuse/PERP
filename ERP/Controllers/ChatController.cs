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
    public class ChatController : Controller
    {
        private readonly ERPContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IServices _services;
        private readonly ISmsService _smsService;

        public ChatController(ERPContext context, IWebHostEnvironment environment, IHubContext<ChatHub> hubContext, IServices services, ISmsService smsService)
        {
            _context = context;
            _environment = environment;
            _hubContext = hubContext;
            _services = services;
            _smsService = smsService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var guestToken = Request.Cookies["GuestToken"];
            if (!string.IsNullOrEmpty(guestToken))
            {
                var guest = await _context.GuestUsers
                    .FirstOrDefaultAsync(g => g.UniqueToken == guestToken && g.IsActive && g.ExpiryDate > DateTime.Now);

                if (guest != null)
                {
                    if (!guest.GroupId.HasValue)
                        return RedirectToAction("GuestLogin");

                    ViewBag.GuestToken = guestToken;

                    var userIdsInGroup = await _context.UserGroups
                        .Where(ug => ug.GroupID == guest.GroupId.Value)
                        .Select(ug => ug.UserID)
                        .ToListAsync();

                    var users = await _context.Users
                        .Where(u => userIdsInGroup.Contains(u.Id))
                        .Select(u => new ChatUser
                        {
                            Id = u.Id,
                            Name = u.FirstName + " " + u.LastName,
                            Image = string.IsNullOrEmpty(u.Image) ? "/UserImage/Male.png" : "/UserImage/" + u.Image,
                            IsOnline = false,
                            UnreadCount = 0,
                            LastMessage = null,
                            LastMessageTime = null
                        })
                        .ToListAsync();

                    return View(users);
                }
                else
                {
                    return RedirectToAction("GuestLogin");
                }
            }

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("GuestLogin");
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var chatUsers = await GetChatUsersInternal(currentUserId);
            return View(chatUsers);
        }

        [AllowAnonymous]
        public IActionResult GuestLogin()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> UserChat()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var users = await GetChatUsersInternal(currentUserId);
            return View(users);
        }

        [HttpGet]
        public IActionResult GetCurrentUserName()
        {
            var guestToken = Request.Cookies["GuestToken"];
            if (!string.IsNullOrEmpty(guestToken))
            {
                var guest = _context.GuestUsers.FirstOrDefault(g => g.UniqueToken == guestToken && g.IsActive);
                if (guest != null && guest.ExpiryDate > DateTime.Now)
                {
                    return Json(new { name = (guest.FirstName + " " + guest.LastName) ?? "مهمان", isGuest = true });
                }
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            return Json(new { name = user != null ? user.FirstName + " " + user.LastName : "کاربر", isGuest = false });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendGuestVerificationCode(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber) || phoneNumber.Length != 11)
                return Json(new { success = false, error = "شماره تماس معتبر نیست" });

            var recentCode = await _context.GuestVerificationCodes
                .Where(c => c.PhoneNumber == phoneNumber && c.CreatedDate > DateTime.Now.AddMinutes(-2))
                .FirstOrDefaultAsync();

            //if (recentCode != null)
            //    return Json(new { success = false, error = "کد قبلی هنوز معتبر است" });

            //var code = new Random().Next(10000, 99999).ToString();
            var code = "12345";

            var verificationCode = new GuestVerificationCode
            {
                PhoneNumber = phoneNumber,
                Code = code,
                CreatedDate = DateTime.Now,
                ExpiryDate = DateTime.Now.AddMinutes(5),
                IsUsed = false,
                AttemptCount = 0
            };

            _context.GuestVerificationCodes.Add(verificationCode);
            await _context.SaveChangesAsync();

            //await _smsService.SendVerificationCode(phoneNumber, code);

            return Json(new { success = true });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetGroups()
        {
            var groups = await _context.Groups
                .Where(g => g.GroupID != 4)
                .Select(g => new { id = g.GroupID, name = g.Name })
                .ToListAsync();
            return Json(groups);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyGuestCode(string phoneNumber, string code, string firstName, string lastName, int groupId)
        {
            var verificationCode = await _context.GuestVerificationCodes
                .Where(c => c.PhoneNumber == phoneNumber && c.Code == code && !c.IsUsed && c.ExpiryDate > DateTime.Now)
                .FirstOrDefaultAsync();

            if (verificationCode == null)
                return Json(new { success = false, error = "کد تایید نامعتبر یا منقضی شده است" });

            if (verificationCode.AttemptCount >= 3)
                return Json(new { success = false, error = "تعداد تلاش بیش از حد مجاز" });

            verificationCode.IsUsed = true;
            verificationCode.AttemptCount++;

            var existingGuest = await _context.GuestUsers
                .FirstOrDefaultAsync(g => g.PhoneNumber == phoneNumber);

            if (existingGuest != null)
            {
                existingGuest.LastActivity = DateTime.Now;
                existingGuest.ExpiryDate = DateTime.Now.AddHours(2);
                existingGuest.GroupId = groupId;
                existingGuest.FirstName = firstName;
                existingGuest.LastName = lastName;
                existingGuest.IsActive = true;
                await _context.SaveChangesAsync();

                Response.Cookies.Append("GuestToken", existingGuest.UniqueToken, new CookieOptions
                {
                    HttpOnly = true,
                    Expires = existingGuest.ExpiryDate
                });

                return Json(new { success = true, token = existingGuest.UniqueToken });
            }

            var guestUser = new GuestUser
            {
                PhoneNumber = phoneNumber,
                FirstName = firstName,
                LastName = lastName,
                Image = "Male.png",
                UniqueToken = Guid.NewGuid().ToString(),
                CreatedDate = DateTime.Now,
                ExpiryDate = DateTime.Now.AddHours(2),
                IsActive = true,
                LastActivity = DateTime.Now,
                GroupId = groupId
            };

            _context.GuestUsers.Add(guestUser);
            await _context.SaveChangesAsync();

            Response.Cookies.Append("GuestToken", guestUser.UniqueToken, new CookieOptions
            {
                HttpOnly = true,
                Expires = guestUser.ExpiryDate
            });

            return Json(new { success = true, token = guestUser.UniqueToken });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuestDirectLogin(string phoneNumber, string firstName, string lastName, string image, int groupId)
        {
            try
            {
                var existingGuest = await _context.GuestUsers
                    .FirstOrDefaultAsync(g => g.PhoneNumber == phoneNumber);

                if (existingGuest != null)
                {
                    existingGuest.LastActivity = DateTime.Now;
                    existingGuest.ExpiryDate = DateTime.Now.AddHours(2);
                    existingGuest.GroupId = groupId;
                    existingGuest.FirstName = firstName;
                    existingGuest.LastName = lastName;
                    existingGuest.IsActive = true;
                    await _context.SaveChangesAsync();

                    Response.Cookies.Append("GuestToken", existingGuest.UniqueToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Expires = existingGuest.ExpiryDate
                    });

                    return Json(new { success = true, token = existingGuest.UniqueToken });
                }

                var guestUser = new GuestUser
                {
                    PhoneNumber = phoneNumber,
                    FirstName = firstName,
                    LastName = lastName,
                    Image = image ?? "Male.png",
                    UniqueToken = Guid.NewGuid().ToString(),
                    CreatedDate = DateTime.Now,
                    ExpiryDate = DateTime.Now.AddHours(2),
                    IsActive = true,
                    LastActivity = DateTime.Now,
                    GroupId = groupId
                };

                _context.GuestUsers.Add(guestUser);
                await _context.SaveChangesAsync();

                Response.Cookies.Append("GuestToken", guestUser.UniqueToken, new CookieOptions
                {
                    HttpOnly = true,
                    Expires = guestUser.ExpiryDate
                });

                return Json(new { success = true, token = guestUser.UniqueToken });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "خطا در ورود: " + ex.Message });
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetGuestAllowedUsers()
        {
            var guestToken = Request.Cookies["GuestToken"];
            if (string.IsNullOrEmpty(guestToken))
                return Json(new { success = false, error = "احراز هویت نشده" });

            var guest = await _context.GuestUsers
                .FirstOrDefaultAsync(g => g.UniqueToken == guestToken && g.IsActive && g.ExpiryDate > DateTime.Now);

            if (guest == null)
                return Json(new { success = false, error = "دسترسی منقضی شده" });

            guest.LastActivity = DateTime.Now;
            await _context.SaveChangesAsync();

            var allowedUserIds = await _context.GuestChatAccesses
                .Where(a => a.GuestUserId == guest.Id)
                .Select(a => a.AllowedUserId)
                .ToListAsync();

            var users = await _context.Users
                .Where(u => allowedUserIds.Contains(u.Id))
                .Select(u => new {
                    id = u.Id,
                    name = u.FirstName + " " + u.LastName,
                    image = string.IsNullOrEmpty(u.Image) ? "/UserImage/Male.png" : "/UserImage/" + u.Image
                })
                .ToListAsync();

            return Json(new { success = true, users });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetMessages(string userId, int page = 1, int pageSize = 50)
        {
            if (string.IsNullOrEmpty(userId)) return BadRequest();

            var guestToken = Request.Cookies["GuestToken"];
            string currentUserId;
            
            if (!string.IsNullOrEmpty(guestToken))
            {
                var guest = await _context.GuestUsers
                    .FirstOrDefaultAsync(g => g.UniqueToken == guestToken && g.IsActive && g.ExpiryDate > DateTime.Now);
                if (guest != null)
                    currentUserId = guestToken;
                else
                    return Json(new List<object>());
            }
            else
            {
                currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
            
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
                    isMine = m.SenderId == currentUserId
                })
                .ToListAsync();

            return Json(messages);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(string receiverId, string message, string attachmentPath, string attachmentName, int? replyToMessageId)
        {
            try
            
            {
                if (string.IsNullOrEmpty(receiverId))
                    return Json(new { success = false, error = "گیرنده مشخص نشده" });

                if (string.IsNullOrEmpty(message) && string.IsNullOrEmpty(attachmentPath))
                    return Json(new { success = false, error = "پیام یا فایل الزامی است" });

                var guestToken = Request.Cookies["GuestToken"];
                if (!string.IsNullOrEmpty(guestToken))
                {
                    var guest = await _context.GuestUsers
                        .FirstOrDefaultAsync(g => g.UniqueToken == guestToken && g.IsActive && g.ExpiryDate > DateTime.Now);

                    if (guest != null)
                    {
                        var guestSanitizer = new HtmlSanitizer();
                        var guestSanitizedMessage = guestSanitizer.Sanitize(message);

                        var guestMessage = new ChatMessage
                        {
                            SenderId = guestToken,
                            ReceiverId = receiverId,
                            Message = guestSanitizedMessage,
                            SentAt = DateTime.Now,
                            IsRead = false,
                            AttachmentPath = attachmentPath,
                            AttachmentName = attachmentName
                        };

                        _context.ChatMessages.Add(guestMessage);
                        await _context.SaveChangesAsync();

                        var guestMessageData = new
                        {
                            id = guestMessage.Id,
                            senderId = guestToken,
                            receiverId = receiverId,
                            message = guestSanitizedMessage,
                            sentAt = guestMessage.SentAt.ToString("HH:mm"),
                            dateAt = _services.iGregorianToPersian(guestMessage.SentAt),
                            isDelivered = false,
                            isRead = false,
                            attachmentPath = attachmentPath,
                            attachmentName = attachmentName
                        };

                        await _hubContext.Clients.All.SendAsync("ReceiveMessage", guestMessageData);

                        return Json(new { success = true, messageId = guestMessage.Id });
                    }
                }

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                // Check if receiver is a guest
                var isReceiverGuest = await _context.GuestUsers
                    .AnyAsync(g => g.UniqueToken == receiverId && g.IsActive);
                
                if (!isReceiverGuest)
                {
                    // Check access only for company users
                    var hasAccess = await _context.ChatAccesses
                        .AnyAsync(c => c.UserId == currentUserId && c.AllowedUserId == receiverId && !c.IsBlocked);
                    
                    if (!hasAccess)
                        return Json(new { success = false, error = "شما مجاز به ارسال پیام به این کاربر نیستید" });
                }
                
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

                await _hubContext.Clients.All.SendAsync("ReceiveMessage", messageData);
                
                var unreadCount = await _context.ChatMessages
                    .CountAsync(m => m.ReceiverId == receiverId && !m.IsRead && !m.IsDeletedByReceiver);
                await _hubContext.Clients.User(receiverId).SendAsync("UpdateUnreadCount", unreadCount);
                
                return Json(new { success = true, messageId = chatMessage.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "خطا در ارسال پیام" });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(string userId)
        {
            var guestToken = Request.Cookies["GuestToken"];
            string currentUserId;
            
            if (!string.IsNullOrEmpty(guestToken))
            {
                var guest = await _context.GuestUsers
                    .FirstOrDefaultAsync(g => g.UniqueToken == guestToken && g.IsActive && g.ExpiryDate > DateTime.Now);
                if (guest != null)
                    currentUserId = guestToken;
                else
                    return Json(new { success = false });
            }
            else
            {
                currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
            
            var unreadMessages = await _context.ChatMessages
                .Where(m => m.SenderId == userId && m.ReceiverId == currentUserId && !m.IsRead)
                .ToListAsync();
            
            unreadMessages.ForEach(m => {
                m.IsRead = true;
                m.ReadAt = DateTime.Now;
            });
            await _context.SaveChangesAsync();
            
            await _hubContext.Clients.All.SendAsync("MessagesRead", userId, currentUserId);
            
            return Json(new { success = true });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsDelivered(int messageId)
        {
            var message = await _context.ChatMessages.FindAsync(messageId);
            if (message != null && !message.IsDelivered)
            {
                message.IsDelivered = true;
                message.DeliveredAt = DateTime.Now;
                
                if (message.IsRead && message.ReadAt.HasValue)
                {
                    message.DeliveredAt = message.ReadAt.Value.AddSeconds(-1);
                }
                
                await _context.SaveChangesAsync();
                
                await _hubContext.Clients.All.SendAsync("MessageDelivered", new { id = messageId });
            }
            return Json(new { success = true });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMessage(int messageId, string newMessage)
        {
            var guestToken = Request.Cookies["GuestToken"];
            string currentUserId;
            
            if (!string.IsNullOrEmpty(guestToken))
            {
                var guest = await _context.GuestUsers
                    .FirstOrDefaultAsync(g => g.UniqueToken == guestToken && g.IsActive && g.ExpiryDate > DateTime.Now);
                if (guest != null)
                    currentUserId = guestToken;
                else
                    return Json(new { success = false, error = "احراز هویت نشده" });
            }
            else
            {
                currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
            
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
            
            await _hubContext.Clients.All.SendAsync("MessageEdited", new { 
                id = messageId, 
                message = sanitizedMessage,
                editedAt = message.EditedAt
            });
            
            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetForwardUsers()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var allowedUserIds = await _context.ChatAccesses
                .Where(c => c.UserId == currentUserId && !c.IsBlocked)
                .Select(c => c.AllowedUserId)
                .ToListAsync();
            
            var users = await _context.Users
                .Where(u => allowedUserIds.Contains(u.Id))
                .Select(u => new {
                    id = u.Id,
                    name = u.FirstName + " " + u.LastName,
                    image = string.IsNullOrEmpty(u.Image) ? "/UserImage/Male.png" : "/UserImage/" + u.Image
                })
                .ToListAsync();
            
            return Json(users);
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
                    ForwardedFromMessageId = messageId,
                    ReplyToMessageId = originalMessage.ReplyToMessageId,
                    ReplyToMessage = originalMessage.ReplyToMessage,
                    ReplyToSenderName = originalMessage.ReplyToSenderName
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
                    forwardedFromMessageId = messageId,
                    replyToMessageId = originalMessage.ReplyToMessageId,
                    replyToMessage = originalMessage.ReplyToMessage,
                    replyToSenderName = originalMessage.ReplyToSenderName
                };

                await _hubContext.Clients.All.SendAsync("ReceiveMessage", messageData);
                
                return Json(new { success = true, messageId = forwardedMessage.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "خطا در فوروارد پیام" });
            }
        }

        private async Task<List<ChatUser>> GetChatUsersInternal(string currentUserId)
        {
            var userIdsWithMessages = await _context.ChatMessages
                .Where(m => (m.SenderId == currentUserId || m.ReceiverId == currentUserId) &&
                           !m.IsDeletedBySender && !m.IsDeletedByReceiver)
                .Select(m => m.SenderId == currentUserId ? m.ReceiverId : m.SenderId)
                .Distinct()
                .ToListAsync();

            var allowedUserIds = await _context.ChatAccesses
                .Where(c => c.UserId == currentUserId && !c.IsBlocked)
                .Select(c => c.AllowedUserId)
                .ToListAsync();

            var filteredUserIds = userIdsWithMessages.Where(id => allowedUserIds.Contains(id) && id.Length <= 36).ToList();

            var users = await _context.Users
                .Where(u => filteredUserIds.Contains(u.Id))
                .Select(u => new ChatUser
                {
                    Id = u.Id,
                    Name = u.FirstName + " " + u.LastName,
                    Image = string.IsNullOrEmpty(u.Image) ? "/UserImage/Male.png" : "/UserImage/" + u.Image,
                    IsOnline = false,
                    UnreadCount = 0,
                    LastMessage = null,
                    LastMessageTime = null
                })
                .ToListAsync();

            var guestTokens = userIdsWithMessages.ToList();

            foreach (var token in guestTokens)
            {
                var guest = await _context.GuestUsers
                    .FirstOrDefaultAsync(g => g.UniqueToken == token && g.IsActive);

                if (guest != null)
                {
                    users.Add(new ChatUser
                    {
                        Id = token,
                        Name = (guest.FirstName + " " + guest.LastName) ?? "مهمان",
                        Image = string.IsNullOrEmpty(guest.Image) ? "/UserImage/Male.png" : "/UserImage/" + guest.Image,
                        IsOnline = false,
                        UnreadCount = 0,
                        LastMessage = null,
                        LastMessageTime = null
                    });
                }
            }

            foreach (var user in users)
            {
                var lastMsg = await _context.ChatMessages
                    .Where(m => (m.SenderId == currentUserId && m.ReceiverId == user.Id && !m.IsDeletedBySender) ||
                               (m.SenderId == user.Id && m.ReceiverId == currentUserId && !m.IsDeletedByReceiver))
                    .OrderByDescending(m => m.SentAt)
                    .FirstOrDefaultAsync();

                if (lastMsg != null)
                {
                    user.LastMessage = lastMsg.Message;
                    user.LastMessageTime = lastMsg.SentAt;
                }

                user.UnreadCount = await _context.ChatMessages
                    .CountAsync(m => m.SenderId == user.Id && m.ReceiverId == currentUserId && !m.IsRead && !m.IsDeletedByReceiver);
            }

            return users;
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
            
            var hasVisibleMessages = await _context.ChatMessages
                .AnyAsync(m => ((m.SenderId == currentUserId && m.ReceiverId == userId && !m.IsDeletedBySender) ||
                               (m.SenderId == userId && m.ReceiverId == currentUserId && !m.IsDeletedByReceiver)));
            
            return Json(new { success = true, shouldRemoveUser = !hasVisibleMessages });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            var guestToken = Request.Cookies["GuestToken"];
            string currentUserId;
            
            if (!string.IsNullOrEmpty(guestToken))
            {
                var guest = await _context.GuestUsers
                    .FirstOrDefaultAsync(g => g.UniqueToken == guestToken && g.IsActive && g.ExpiryDate > DateTime.Now);
                if (guest != null)
                    currentUserId = guestToken;
                else
                    return Json(new { success = false, error = "احراز هویت نشده" });
            }
            else
            {
                currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
            
            var message = await _context.ChatMessages.FindAsync(messageId);
            
            if (message == null || message.SenderId != currentUserId)
                return Json(new { success = false, error = "پیام یافت نشد" });
            
            message.IsDeletedBySender = true;
            message.DeletedAt = DateTime.Now;
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

        [HttpGet]
        public async Task<IActionResult> SearchUsers(string query)
        {
            if (string.IsNullOrEmpty(query)) return Json(new List<object>());
            
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var blockedUsers = await _context.ChatAccesses
                .Where(c => c.UserId == currentUserId && !c.IsBlocked)
                .Select(c => c.AllowedUserId)
                .ToListAsync();
            
            var users = await _context.Users
                .Where(u => u.Id != currentUserId && 
                       blockedUsers.Contains(u.Id) &&
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
            if (string.IsNullOrEmpty(userId)) 
                return Json(new { success = false, error = "شناسه کاربر خالی است" });
            
            var user = await _context.Users.FindAsync(userId);
            if (user == null) 
                return Json(new { success = false, error = "کاربر یافت نشد" });
            
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var hasAccess = await _context.ChatAccesses
                .AnyAsync(c => c.UserId == currentUserId && c.AllowedUserId == userId && !c.IsBlocked);
            
            return Json(new {
                success = true,
                id = user.Id,
                name = user.FirstName + " " + user.LastName,
                image = string.IsNullOrEmpty(user.Image) ? "/UserImage/Male.png" : "/UserImage/" + user.Image,
                isOnline = false,
                hasAccess = hasAccess
            });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllUsers()
        {
            var guestToken = Request.Cookies["GuestToken"];
            if (!string.IsNullOrEmpty(guestToken))
            {
                var guest = await _context.GuestUsers
                    .FirstOrDefaultAsync(g => g.UniqueToken == guestToken && g.IsActive && g.ExpiryDate > DateTime.Now);

                if (guest != null && guest.GroupId.HasValue)
                {
                    var userIdsInGroup = await _context.UserGroups
                        .Where(ug => ug.GroupID == guest.GroupId.Value)
                        .Select(ug => ug.UserID)
                        .ToListAsync();

                    var users = await _context.Users
                        .Where(u => userIdsInGroup.Contains(u.Id))
                        .Select(u => new {
                            id = u.Id,
                            name = u.FirstName + " " + u.LastName,
                            image = string.IsNullOrEmpty(u.Image) ? "/UserImage/Male.png" : "/UserImage/" + u.Image
                        })
                        .ToListAsync();

                    return Json(users);
                }
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var blockedUsers = await _context.ChatAccesses
                .Where(c => c.UserId == currentUserId && !c.IsBlocked)
                .Select(c => c.AllowedUserId)
                .ToListAsync();
            
            var allUsers = await _context.Users
                .Where(u => u.Id != currentUserId && blockedUsers.Contains(u.Id))
                .Select(u => new {
                    id = u.Id,
                    name = u.FirstName + " " + u.LastName,
                    image = string.IsNullOrEmpty(u.Image) ? "/UserImage/Male.png" : "/UserImage/" + u.Image
                })
                .ToListAsync();
            
            return Json(allUsers);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetChatUsers()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var users = await GetChatUsersInternal(currentUserId);
            return Json(users.OrderByDescending(u => u.LastMessageTime));
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddGuestChatAccess(int guestId, string allowedUserId)
        {
            var existing = await _context.GuestChatAccesses
                .FirstOrDefaultAsync(a => a.GuestUserId == guestId && a.AllowedUserId == allowedUserId);

            if (existing == null)
            {
                _context.GuestChatAccesses.Add(new GuestChatAccess
                {
                    GuestUserId = guestId,
                    AllowedUserId = allowedUserId,
                    GrantedDate = DateTime.Now
                });
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BlockUser(string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var existing = await _context.ChatAccesses
                .FirstOrDefaultAsync(c => c.UserId == currentUserId && c.AllowedUserId == userId);
            
            if (existing == null)
            {
                _context.ChatAccesses.Add(new ChatAccess 
                { 
                    UserId = currentUserId, 
                    AllowedUserId = userId, 
                    IsBlocked = true 
                });
            }
            else
            {
                existing.IsBlocked = true;
            }
            
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnblockUser(string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var access = await _context.ChatAccesses
                .FirstOrDefaultAsync(c => c.UserId == currentUserId && c.AllowedUserId == userId);
            
            if (access != null)
            {
                access.IsBlocked = false;
                await _context.SaveChangesAsync();
            }
            
            return Json(new { success = true });
        }

        private async Task<List<ChatUser>> GetChatUsers(string currentUserId)
        {
            var usersWithMessages = await _context.ChatMessages
                .Where(m => (m.SenderId == currentUserId && !m.IsDeletedBySender) ||
                           (m.ReceiverId == currentUserId && !m.IsDeletedByReceiver))
                .Select(m => m.SenderId == currentUserId ? m.ReceiverId : m.SenderId)
                .Distinct()
                .ToListAsync();

            var blockedUsers = await _context.ChatAccesses
                .Where(c => c.UserId == currentUserId && c.IsBlocked)
                .Select(c => c.AllowedUserId)
                .ToListAsync();

            var userMessages = await _context.Users
                .Where(u => usersWithMessages.Contains(u.Id) && !blockedUsers.Contains(u.Id))
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


        public IActionResult AccessControl(string userId = null)
        {
            ViewBag.UserId = userId;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult GuestLogout()
        {
            Response.Cookies.Delete("GuestToken");
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddChatAccess(string allowedUserId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var existing = await _context.ChatAccesses
                .FirstOrDefaultAsync(c => c.UserId == currentUserId && c.AllowedUserId == allowedUserId);
            
            if (existing == null)
            {
                _context.ChatAccesses.Add(new ChatAccess 
                { 
                    UserId = currentUserId, 
                    AllowedUserId = allowedUserId, 
                    IsBlocked = false 
                });
                await _context.SaveChangesAsync();
            }
            
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveChatAccess(string allowedUserId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var access = await _context.ChatAccesses
                .FirstOrDefaultAsync(c => c.UserId == currentUserId && c.AllowedUserId == allowedUserId);
            
            if (access != null)
            {
                _context.ChatAccesses.Remove(access);
                await _context.SaveChangesAsync();
            }
            
            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetUserAllowedUsers(string userId)
        {
            var allowedUserIds = await _context.ChatAccesses
                .Where(c => c.UserId == userId && !c.IsBlocked)
                .Select(c => c.AllowedUserId)
                .ToListAsync();
            
            var allowed = await _context.Users
                .Where(u => allowedUserIds.Contains(u.Id))
                .Select(u => new {
                    id = u.Id,
                    name = u.FirstName + " " + u.LastName,
                    image = string.IsNullOrEmpty(u.Image) ? "/UserImage/Male.png" : "/UserImage/" + u.Image
                })
                .ToListAsync();
            
            var available = await _context.Users
                .Where(u => u.Id != userId && !allowedUserIds.Contains(u.Id))
                .Select(u => new {
                    id = u.Id,
                    name = u.FirstName + " " + u.LastName,
                    image = string.IsNullOrEmpty(u.Image) ? "/UserImage/Male.png" : "/UserImage/" + u.Image
                })
                .ToListAsync();
            
            return Json(new { allowed, available });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddChatAccessForUser(string userId, string allowedUserId)
        {
            var existing = await _context.ChatAccesses
                .FirstOrDefaultAsync(c => c.UserId == userId && c.AllowedUserId == allowedUserId);
            
            if (existing == null)
            {
                _context.ChatAccesses.Add(new ChatAccess 
                { 
                    UserId = userId, 
                    AllowedUserId = allowedUserId, 
                    IsBlocked = false 
                });
                await _context.SaveChangesAsync();
            }
            
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveChatAccessForUser(string userId, string allowedUserId)
        {
            var access = await _context.ChatAccesses
                .FirstOrDefaultAsync(c => c.UserId == userId && c.AllowedUserId == allowedUserId);
            
            if (access != null)
            {
                _context.ChatAccesses.Remove(access);
                await _context.SaveChangesAsync();
            }
            
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMultipleChatAccess(string userId, List<string> allowedUserIds)
        {
            foreach (var allowedUserId in allowedUserIds)
            {
                var existing = await _context.ChatAccesses
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.AllowedUserId == allowedUserId);
                
                if (existing == null)
                {
                    _context.ChatAccesses.Add(new ChatAccess 
                    { 
                        UserId = userId, 
                        AllowedUserId = allowedUserId, 
                        IsBlocked = false 
                    });
                }
            }
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAllChatAccess(string userId)
        {
            var accesses = await _context.ChatAccesses
                .Where(c => c.UserId == userId)
                .ToListAsync();
            
            _context.ChatAccesses.RemoveRange(accesses);
            await _context.SaveChangesAsync();
            
            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllowedUsers()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var allowedUserIds = await _context.ChatAccesses
                .Where(c => c.UserId == currentUserId && !c.IsBlocked)
                .Select(c => c.AllowedUserId)
                .ToListAsync();
            
            var users = await _context.Users
                .Where(u => allowedUserIds.Contains(u.Id))
                .Select(u => new {
                    id = u.Id,
                    name = u.FirstName + " " + u.LastName,
                    image = string.IsNullOrEmpty(u.Image) ? "/UserImage/Male.png" : "/UserImage/" + u.Image
                })
                .ToListAsync();
            
            return Json(users);
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUnreadCount()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var count = await _context.ChatMessages
                .CountAsync(m => m.ReceiverId == currentUserId && !m.IsRead && !m.IsDeletedByReceiver);
            
            return Json(new { unreadCount = count });
        }
    }
}
