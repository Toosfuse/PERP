using System;
using System.Collections.Generic;

namespace ERP.Models
{
    public class ChatGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Image { get; set; }
        public List<GroupMember> Members { get; set; } = new List<GroupMember>();
        public List<GroupMessage> Messages { get; set; } = new List<GroupMessage>();
    }

    public class GroupMember
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public string UserId { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime JoinedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? LeftAt { get; set; }
        public ChatGroup Group { get; set; }
    }

    public class GroupMessage
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public string SenderId { get; set; }
        public string Message { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsEdited { get; set; }
        public DateTime? EditedAt { get; set; }
        public string AttachmentPath { get; set; }
        public string AttachmentName { get; set; }
        public int? ReplyToMessageId { get; set; }
        public string ReplyToMessage { get; set; }
        public string ReplyToSenderName { get; set; }
        public int? ForwardedFromMessageId { get; set; }
        public ChatGroup Group { get; set; }
    }
}
