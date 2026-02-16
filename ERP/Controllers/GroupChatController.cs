using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ERP.Data;
using ERP.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using ERP.Hubs;
using ERP.Services;

namespace ERP.Controllers
{
    [Authorize]
    public class GroupChatController : Controller
    {
        private readonly ERPContext _context;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IServices _services;

        public GroupChatController(ERPContext context, IHubContext<ChatHub> hubContext, IServices services)
        {
            _context = context;
            _hubContext = hubContext;
            _services = services;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGroup(string name, List<string> memberIds)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var group = new ChatGroup
            {
                Name = name,
                CreatedBy = currentUserId,
                CreatedAt = DateTime.Now,
                Image = "/UserImage/Male.png"
            };

            _context.ChatGroups.Add(group);
            await _context.SaveChangesAsync();

            var creator = new GroupMember
            {
                GroupId = group.Id,
                UserId = currentUserId,
                IsAdmin = true,
                JoinedAt = DateTime.Now
            };
            _context.GroupMembers.Add(creator);

            if (memberIds != null && memberIds.Count > 0)
            {
                foreach (var memberId in memberIds)
                {
                    if (memberId != currentUserId)
                    {
                        var member = new GroupMember
                        {
                            GroupId = group.Id,
                            UserId = memberId,
                            IsAdmin = false,
                            JoinedAt = DateTime.Now
                        };
                        _context.GroupMembers.Add(member);
                    }
                }
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, groupId = group.Id, groupName = name });
        }

        [HttpGet]
        public async Task<IActionResult> GetUserGroups()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var groups = await _context.ChatGroups
                .Where(g => g.Members.Any(m => m.UserId == currentUserId && m.IsActive))
                .Select(g => new
                {
                    id = g.Id,
                    name = g.Name,
                    image = g.Image,
                    createdBy = g.CreatedBy,
                    memberCount = g.Members.Count(m => m.IsActive),
                    lastMessage = g.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault().Message
                })
                .ToListAsync();

            return Json(groups);
        }

        [HttpGet]
        public async Task<IActionResult> GetGroupMembers(int groupId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isMember = await _context.GroupMembers.AnyAsync(m => m.GroupId == groupId && m.UserId == currentUserId && m.IsActive);
            
            if (!isMember)
                return Json(new { success = false, error = "دسترسی رد شد" });

            var members = await _context.GroupMembers
                .Where(m => m.GroupId == groupId && m.IsActive)
                .Join(_context.Users,
                    gm => gm.UserId,
                    u => u.Id,
                    (gm, u) => new
                    {
                        userId = gm.UserId,
                        isAdmin = gm.IsAdmin,
                        userName = u.FirstName + " " + u.LastName,
                        userImage = string.IsNullOrEmpty(u.Image) ? "/UserImage/Male.png" : "/UserImage/" + u.Image
                    })
                .ToListAsync();

            return Json(members);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(int groupId, string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var isAdmin = await _context.GroupMembers
                .AnyAsync(m => m.GroupId == groupId && m.UserId == currentUserId && m.IsAdmin && m.IsActive);

            if (!isAdmin)
                return Json(new { success = false, error = "فقط ادمین میتواند عضو اضافه کند" });

            var existingMember = await _context.GroupMembers
                .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId);

            if (existingMember != null)
            {
                if (existingMember.IsActive)
                    return Json(new { success = false, error = "کاربر قبلاً عضو است" });
                
                existingMember.IsActive = true;
                existingMember.LeftAt = null;
                existingMember.JoinedAt = DateTime.Now;
            }
            else
            {
                var member = new GroupMember
                {
                    GroupId = groupId,
                    UserId = userId,
                    IsAdmin = false,
                    JoinedAt = DateTime.Now,
                    IsActive = true
                };
                _context.GroupMembers.Add(member);
            }

            await _context.SaveChangesAsync();

            var memberCount = await _context.GroupMembers.CountAsync(m => m.GroupId == groupId && m.IsActive);
            await _hubContext.Clients.All.SendAsync("GroupMemberCountUpdated", groupId, memberCount);

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember(int groupId, string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var group = await _context.ChatGroups.FindAsync(groupId);
            if (group == null)
                return Json(new { success = false, error = "گروه یافت نشد" });

            if (group.CreatedBy == userId)
                return Json(new { success = false, error = "مدیر گروه نمیتواند از گروه خارج شود" });
            
            var isAdmin = await _context.GroupMembers
                .AnyAsync(m => m.GroupId == groupId && m.UserId == currentUserId && m.IsAdmin);

            if (!isAdmin && userId != currentUserId)
                return Json(new { success = false, error = "فقط ادمین یا خود شخص میتواند حذف کند" });

            var member = await _context.GroupMembers
                .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId && m.IsActive);

            if (member == null)
                return Json(new { success = false, error = "عضو یافت نشد" });

            member.IsActive = false;
            member.LeftAt = DateTime.Now;
            await _context.SaveChangesAsync();

            var memberCount = await _context.GroupMembers.CountAsync(m => m.GroupId == groupId && m.IsActive);
            await _hubContext.Clients.All.SendAsync("GroupMemberCountUpdated", groupId, memberCount);

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetGroupMessages(int groupId, int page = 1, int pageSize = 50)
        {
            try
            {
                var messages = await _context.GroupMessages
                    .Where(m => m.GroupId == groupId)
                    .OrderByDescending(m => m.SentAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .OrderBy(m => m.SentAt)
                    .ToListAsync();

                var result = new List<object>();
                foreach (var m in messages)
                {
                    var sender = await _context.Users.FindAsync(m.SenderId);
                    result.Add(new
                    {
                        id = m.Id,
                        senderId = m.SenderId,
                        senderName = sender?.FirstName + " " + sender?.LastName,
                        senderImage = string.IsNullOrEmpty(sender?.Image) ? "/UserImage/Male.png" : "/UserImage/" + sender.Image,
                        message = m.Message,
                        sentAt = m.SentAt.ToString("HH:mm"),
                        dateAt = _services.iGregorianToPersian(m.SentAt),
                        isEdited = m.IsEdited,
                        attachmentPath = m.AttachmentPath,
                        attachmentName = m.AttachmentName,
                        replyToMessageId = m.ReplyToMessageId,
                        replyToMessage = m.ReplyToMessage,
                        replyToSenderName = m.ReplyToSenderName
                    });
                }

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendGroupMessage(int groupId, string message, int? replyToMessageId = null)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var group = await _context.ChatGroups.FindAsync(groupId);

                if (group == null)
                    return Json(new { success = false, error = "گروه یافت نشد" });

                if (string.IsNullOrEmpty(message))
                    return Json(new { success = false, error = "پیام نمیتواند خالی باشد" });

                string replyToMessage = "";
                string replyToSenderName = "";
                
                if (replyToMessageId.HasValue)
                {
                    var repliedMessage = await _context.GroupMessages.FindAsync(replyToMessageId.Value);
                    if (repliedMessage != null)
                    {
                        replyToMessage = repliedMessage.Message;
                        var sender = await _context.Users.FindAsync(repliedMessage.SenderId);
                        replyToSenderName = sender?.FirstName + " " + sender?.LastName;
                    }
                }

                var groupMessage = new GroupMessage
                {
                    GroupId = groupId,
                    SenderId = currentUserId,
                    Message = message,
                    SentAt = DateTime.Now,
                    IsEdited = false,
                    AttachmentPath = string.Empty,
                    AttachmentName = string.Empty,
                    ReplyToMessageId = replyToMessageId,
                    ReplyToMessage = replyToMessage,
                    ReplyToSenderName = replyToSenderName
                };

                _context.GroupMessages.Add(groupMessage);
                await _context.SaveChangesAsync();

                var senderUser = await _context.Users.FindAsync(currentUserId);

                await _hubContext.Clients.Group($"group-{groupId}").SendAsync("ReceiveGroupMessage", new
                {
                    id = groupMessage.Id,
                    groupId = groupId,
                    senderId = currentUserId,
                    senderName = senderUser?.FirstName + " " + senderUser?.LastName,
                    senderImage = string.IsNullOrEmpty(senderUser?.Image) ? "/UserImage/Male.png" : "/UserImage/" + senderUser.Image,
                    message = message,
                    sentAt = groupMessage.SentAt.ToString("HH:mm"),
                    dateAt = _services.iGregorianToPersian(groupMessage.SentAt),
                    replyToMessageId = replyToMessageId,
                    replyToMessage = replyToMessage,
                    replyToSenderName = replyToSenderName
                });

                return Json(new { success = true, messageId = groupMessage.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "خطا در ارسال پیام گروهی" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateGroupName(int groupId, string newName)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var group = await _context.ChatGroups.FindAsync(groupId);

            if (group == null || group.CreatedBy != currentUserId)
                return Json(new { success = false, error = "دسترسی رد شد" });

            if (string.IsNullOrEmpty(newName))
                return Json(new { success = false, error = "نام گروه نمیتواند خالی باشد" });

            group.Name = newName;
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGroup(int groupId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var group = await _context.ChatGroups.FindAsync(groupId);

            if (group == null || group.CreatedBy != currentUserId)
                return Json(new { success = false, error = "دسترسی رد شد" });

            _context.ChatGroups.Remove(group);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGroupMessage(int messageId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var message = await _context.GroupMessages.FindAsync(messageId);
            
            if (message == null || message.SenderId != currentUserId)
                return Json(new { success = false, error = "پیام یافت نشد" });
            
            _context.GroupMessages.Remove(message);
            await _context.SaveChangesAsync();
            
            await _hubContext.Clients.Group($"group-{message.GroupId}").SendAsync("GroupMessageDeleted", new { id = messageId });
            
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditGroupMessage(int messageId, string newMessage)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var message = await _context.GroupMessages.FindAsync(messageId);
            
            if (message == null || message.SenderId != currentUserId)
                return Json(new { success = false, error = "پیام یافت نشد" });
            
            message.Message = newMessage;
            message.IsEdited = true;
            message.EditedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            
            await _hubContext.Clients.Group($"group-{message.GroupId}").SendAsync("GroupMessageEdited", new { 
                id = messageId, 
                message = newMessage,
                editedAt = message.EditedAt
            });
            
            return Json(new { success = true });
        }
    }
}
