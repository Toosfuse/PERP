using System;
using System.Collections.Generic;

namespace ERP.Models
{
    public class Channel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Image { get; set; }
        public bool IsReadOnly { get; set; } = true;

        public virtual ICollection<ChannelMember> Members { get; set; }
        public virtual ICollection<ChannelMessage> Messages { get; set; }
    }

    public class ChannelMember
    {
        public int Id { get; set; }
        public int ChannelId { get; set; }
        public string UserId { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime JoinedAt { get; set; }

        public virtual Channel Channel { get; set; }
    }

    public class ChannelMessage
    {
        public int Id { get; set; }
        public int ChannelId { get; set; }
        public string SenderId { get; set; }
        public string Message { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsEdited { get; set; }
        public DateTime? EditedAt { get; set; }
        public string AttachmentPath { get; set; }
        public string AttachmentName { get; set; }

        public virtual Channel Channel { get; set; }
    }
}
