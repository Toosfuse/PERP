using System.ComponentModel.DataAnnotations;

namespace ERP.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public string Message { get; set; }
        public DateTime SentAt { get; set; } = DateTime.Now;
        public bool IsDelivered { get; set; } = false;
        public DateTime? DeliveredAt { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }
        public string? AttachmentPath { get; set; }
        public string? AttachmentName { get; set; }
        public bool IsDeletedBySender { get; set; } = false;
        public bool IsDeletedByReceiver { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public bool IsEdited { get; set; } = false;
        public DateTime? EditedAt { get; set; }
        public int? ReplyToMessageId { get; set; }
        public string? ReplyToMessage { get; set; }
        public string? ReplyToSenderName { get; set; }
        public int? ForwardedFromMessageId { get; set; }
        
        public Users Sender { get; set; }
        public Users Receiver { get; set; }
    }

    public class ChatUser
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public bool IsOnline { get; set; }
        public int UnreadCount { get; set; }
        public DateTime? LastMessageTime { get; set; }
        public string? LastMessage { get; set; }
    }
}
