let connection;
let selectedConversation = null;
let currentFilters = {};

$(document).ready(function() {
    initSignalR();
    loadStatistics();
    loadConversations();
    
    $('#refreshBtn').click(function() {
        loadConversations();
        loadStatistics();
    });
    
    $('#applyFilters').click(function() {
        const fromDateVal = $('#fromDate').val();
        const toDateVal = $('#toDate').val();
        
        currentFilters = {
            fromDate: fromDateVal || null,
            toDate: toDateVal || null,
            isOnline: $('#onlineStatus').val() === 'online' ? true : ($('#onlineStatus').val() === 'offline' ? false : null),
            isRead: $('#readStatus').val() === 'read' ? true : ($('#readStatus').val() === 'unread' ? false : null)
        };
        
        console.log('Filters applied:', currentFilters);
        loadConversations();
        
        if (selectedConversation) {
            loadMessages(selectedConversation.user1Id, selectedConversation.user2Id);
        }
    });
    
    $('#clearFilters').click(function() {
        $('#fromDate').val('');
        $('#toDate').val('');
        $('#onlineStatus').val('');
        $('#readStatus').val('');
        currentFilters = {};
        loadConversations();
        if (selectedConversation) {
            loadMessages(selectedConversation.user1Id, selectedConversation.user2Id);
        }
    });
    
    $('#searchConversations').on('input', function() {
        const query = $(this).val().toLowerCase();
        filterConversations(query);
    });
    
    $('#searchMessages').on('input', function() {
        const query = $(this).val().toLowerCase();
        filterMessages(query);
    });
    
    $(document).on('click', '.conversation-card', function() {
        const user1Id = $(this).data('user1-id');
        const user2Id = $(this).data('user2-id');
        const user1Name = $(this).data('user1-name');
        const user2Name = $(this).data('user2-name');
        
        selectConversation(user1Id, user2Id, user1Name, user2Name);
    });
    
    $(document).on('click', '.delete-message-btn', function() {
        const messageId = $(this).data('message-id');
        if (confirm('آیا مطمئن هستید که میخواهید این پیام را حذف کنید؟')) {
            deleteMessage(messageId);
        }
    });
});

function initSignalR() {
    connection = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub")
        .withAutomaticReconnect()
        .build();

    connection.start()
        .then(() => console.log('SignalR متصل شد'))
        .catch(err => console.error('خطا در اتصال SignalR:', err));

    connection.on("ReceiveMessage", function (msg) {
        if (selectedConversation) {
            const { user1Id, user2Id } = selectedConversation;
            if ((msg.senderId === user1Id && msg.receiverId === user2Id) ||
                (msg.senderId === user2Id && msg.receiverId === user1Id)) {
                addMessageToList(msg);
            }
        }
        loadConversations();
        loadStatistics();
    });
}

function loadStatistics() {
    $.get('/ChatMonitoring/GetStatistics', function(stats) {
        console.log('Statistics loaded:', stats);
        $('#totalMessages').text(stats.totalMessages || 0);
        $('#activeConversations').text(stats.activeConversations || 0);
        $('#messagesLast24h').text(stats.messagesLast24h || 0);
        $('#totalUsers').text(stats.totalUsers || 0);
    }).fail(function(error) {
        console.error('Error loading statistics:', error);
    });
}

function loadConversations() {
    $('#conversationsList').html('<div class="loading"><i class="fa fa-spinner fa-spin"></i><p>در حال بارگذاری...</p></div>');
    
    const params = {};
    if (currentFilters.fromDate) params.fromDate = currentFilters.fromDate;
    if (currentFilters.toDate) params.toDate = currentFilters.toDate;
    if (currentFilters.isOnline !== null && currentFilters.isOnline !== undefined) params.isOnline = currentFilters.isOnline;
    if (currentFilters.isRead !== null && currentFilters.isRead !== undefined) params.isRead = currentFilters.isRead;
    
    console.log('Loading conversations with params:', params);
    
    $.get('/ChatMonitoring/GetActiveConversations', params, function(conversations) {
        console.log('Conversations loaded:', conversations);
        if (conversations.error) {
            console.error('Server error:', conversations.error);
            $('#conversationsList').html('<div class="empty-state"><i class="fa fa-exclamation-triangle"></i><p>خطا: ' + conversations.error + '</p></div>');
        } else {
            renderConversations(conversations);
        }
    }).fail(function(xhr, status, error) {
        console.error('AJAX Error:', status, error);
        console.error('Response:', xhr.responseText);
        $('#conversationsList').html('<div class="empty-state"><i class="fa fa-exclamation-triangle"></i><p>خطا در بارگذاری: ' + error + '</p></div>');
    });
}

function renderConversations(conversations) {
    let html = '';
    conversations.forEach(function(conv) {
        const user1Status = conv.user1.isOnline ? '<span style="color:#48bb78;">●</span>' : '<span style="color:#cbd5e0;">●</span>';
        const user2Status = conv.user2.isOnline ? '<span style="color:#48bb78;">●</span>' : '<span style="color:#cbd5e0;">●</span>';
        html += `
      
            <div class="conversation-card" 
                 data-user1-id="${conv.user1.id}" 
                 data-user2-id="${conv.user2.id}"
                 data-user1-name="${conv.user1.name}"
                 data-user2-name="${conv.user2.name}">
                <div class="conversation-users">
                    <div style="position:relative;">
                        <img src="${conv.user1.image}" class="user-avatar" alt="${conv.user1.name}" />
                        <span style="position:absolute;bottom:0;left:0;font-size:12px;">${user1Status}</span>
                    </div>
                    <i class="fa fa-exchange" style="color:#667eea;"></i>
                    <div style="position:relative;">
                        <img src="${conv.user2.image}" class="user-avatar" alt="${conv.user2.name}" />
                        <span style="position:absolute;bottom:0;left:0;font-size:12px;">${user2Status}</span>
                    </div>
                    <div class="user-names">${conv.user1.name} ↔ ${conv.user2.name}</div>
                </div>
                <div class="conversation-meta">
                    <span>${formatDate(conv.lastMessageTime)}</span>
                    <span class="message-count">${conv.messageCount} پیام</span>
                    ${conv.unreadCount > 0 ? `<span style="background:#f56565;color:white;padding:2px 8px;border-radius:10px;font-size:11px;">${conv.unreadCount} خوانده نشده</span>` : ''}
                </div>
            </div>
        `;
    });
    $('#conversationsList').html(html || '<div class="empty-state"><i class="fa fa-comments-o"></i><p>مکالمهای یافت نشد</p></div>');
    
    // بازگردانی مکالمه انتخاب شده بعد از فیلتر
    if (selectedConversation) {
        $(`.conversation-card[data-user1-id="${selectedConversation.user1Id}"][data-user2-id="${selectedConversation.user2Id}"]`).addClass('active');
    }
}

function selectConversation(user1Id, user2Id, user1Name, user2Name) {
    selectedConversation = { user1Id, user2Id, user1Name, user2Name };
    
    $('.conversation-card').removeClass('active');
    $(`.conversation-card[data-user1-id="${user1Id}"][data-user2-id="${user2Id}"]`).addClass('active');
    
    $('#conversationTitle').html(`<span style="color:#667eea;font-weight:600;">${user1Name}</span><span style="color:#718096;margin:0 5px;">(فرستنده)</span> <span style="color:#667eea;margin:0 5px;">↔</span> <span style="color:#f59e0b;font-weight:600;">${user2Name}</span><span style="color:#718096;margin:0 5px;">(گیرنده)</span>`);
    $('#conversationSubtitle').text('مشاهده پیامها');
    
    loadMessages(user1Id, user2Id);
}

function loadMessages(user1Id, user2Id) {
    $('#messagesList').html('<div class="loading"><i class="fa fa-spinner fa-spin"></i><p>در حال بارگذاری پیامها...</p></div>');
    
    const params = { 
        user1Id, 
        user2Id
    };
    
    // اضافه کردن فیلترها به پارامترها
    if (currentFilters.fromDate) params.fromDate = currentFilters.fromDate;
    if (currentFilters.toDate) params.toDate = currentFilters.toDate;
    if (currentFilters.isRead !== null && currentFilters.isRead !== undefined) params.isRead = currentFilters.isRead;
    
    console.log('Loading messages with params:', params);
    
    $.get('/ChatMonitoring/GetConversationMessages', params, function(messages) {
        console.log('Messages loaded:', messages);
        renderMessages(messages);
    }).fail(function(error) {
        console.error('Error loading messages:', error);
        $('#messagesList').html('<div class="empty-state"><i class="fa fa-exclamation-triangle"></i><p>خطا در بارگذاری پیامها</p></div>');
    });
}

function renderMessages(messages) {
    let html = '';
    messages.forEach(function(msg) {
        const readStatus = msg.isRead ? '<span style="color:#48bb78;"><i class="fa fa-check-circle"></i> خوانده شده</span>' : '<span style="color:#f56565;"><i class="fa fa-circle"></i> خوانده نشده</span>';
        const isSender = msg.senderId === selectedConversation.user1Id;
        const senderName = isSender ? selectedConversation.user1Name : selectedConversation.user2Name;
        const isDeleted = msg.isDeletedBySender || msg.isDeletedByReceiver;
        const messageBg = isDeleted ? 'linear-gradient(135deg, rgba(239, 68, 68, 0.15) 0%, rgba(220, 38, 38, 0.2) 100%)' : (isSender ? 'linear-gradient(135deg, rgba(102, 126, 234, 0.08) 0%, rgba(102, 126, 234, 0.12) 100%)' : 'linear-gradient(135deg, rgba(245, 158, 11, 0.08) 0%, rgba(245, 158, 11, 0.12) 100%)');
        const borderColor = isDeleted ? '#ef4444' : (isSender ? '#667eea' : '#f59e0b');
        const messageAlign = isSender ? 'margin-right:auto;margin-left:60%;' : 'margin-left:auto;margin-right:60%;';
        
        let replyHtml = '';
        if (msg.replyToMessageId && msg.replyToMessage) {
            replyHtml = `
                <div class="reply-box" style="background:rgba(102,126,234,0.1);border-right:3px solid ${borderColor};padding:8px 10px;margin-bottom:8px;border-radius:8px;max-width:85%;">
                    <div style="font-size:11px;color:${borderColor};font-weight:600;margin-bottom:4px;">${escapeHtml(msg.replyToSenderName || '')}</div>
                    <div style="font-size:13px;color:#4a5568;white-space:nowrap;overflow:hidden;text-overflow:ellipsis;">${escapeHtml(msg.replyToMessage)}</div>
                </div>
            `;
        }
        
        const deletedLabel = isDeleted ? '<span style="color:#ef4444;font-size:11px;font-weight:600;margin-right:8px;"><i class="fa fa-trash"></i> حذف شده</span>' : '';
        
        html += `
            <div class="message-item" data-message-id="${msg.id}" style="background:${messageBg};border-right-color:${borderColor};max-width:40%;${messageAlign}border-radius:12px;">
                <div class="message-header">
                    <span class="message-sender" style="color:${borderColor};font-weight:600;">
                        <i class="fa fa-user" style="margin-left:5px;"></i>${senderName}
                    </span>
                    <div>
                        ${deletedLabel}
                        <span class="message-time">${formatDateTime(msg.sentAt)}</span>
                        <span style="margin:0 10px;font-size:12px;">${readStatus}</span>
                    </div>
                </div>
                ${replyHtml}
                <div class="message-text">${escapeHtml(msg.message)}</div>
                ${msg.attachmentPath ? `
                    <div class="message-attachment">
                        <i class="fa fa-file"></i>
                        <a href="${msg.attachmentPath}" target="_blank">${msg.attachmentName}</a>
                    </div>
                ` : ''}
                ${msg.isEdited ? '<span style="font-size:11px;color:#718096;margin-top:5px;display:block;">(ویرایش شده)</span>' : ''}
            </div>
        `;
    });
    $('#messagesList').html(html || '<div class="empty-state"><i class="fa fa-comments-o"></i><p>پیامی وجود ندارد</p></div>');
    scrollToBottom();
}

function addMessageToList(msg) {
    const isSender = msg.senderId === selectedConversation.user1Id;
    const senderName = isSender ? selectedConversation.user1Name : selectedConversation.user2Name;
    const messageBg = isSender ? 'linear-gradient(135deg, rgba(102, 126, 234, 0.08) 0%, rgba(102, 126, 234, 0.12) 100%)' : 'linear-gradient(135deg, rgba(245, 158, 11, 0.08) 0%, rgba(245, 158, 11, 0.12) 100%)';
    const borderColor = isSender ? '#667eea' : '#f59e0b';
    const messageAlign = isSender ? 'margin-right:auto;margin-left:60%;' : 'margin-left:auto;margin-right:60%;';
    const html = `
        <div class="message-item" data-message-id="${msg.id}" style="background:${messageBg};border-right-color:${borderColor};max-width:40%;${messageAlign}border-radius:12px;">
            <div class="message-header">
                <span class="message-sender" style="color:${borderColor};font-weight:600;">
                    <i class="fa fa-user" style="margin-left:5px;"></i>${senderName}
                </span>
                <div>
                    <span class="message-time">${formatDateTime(new Date())}</span>
                    
                </div>
            </div>
            <div class="message-text">${escapeHtml(msg.message)}</div>
        </div>
    `;
    $('#messagesList').append(html);
    scrollToBottom();
}

function deleteMessage(messageId) {
    $.post('/ChatMonitoring/DeleteMessage', { messageId }, function(result) {
        if (result.success) {
            $(`.message-item[data-message-id="${messageId}"]`).fadeOut(300, function() {
                $(this).remove();
            });
        } else {
            alert('خطا در حذف پیام');
        }
    });
}

function filterConversations(query) {
    $('.conversation-card').each(function() {
        const text = $(this).text().toLowerCase();
        if (text.includes(query)) {
            $(this).show();
        } else {
            $(this).hide();
        }
    });
}

function filterMessages(query) {
    $('.message-item').each(function() {
        const text = $(this).find('.message-text').text().toLowerCase();
        if (text.includes(query)) {
            $(this).show();
        } else {
            $(this).hide();
        }
    });
}

function formatDate(date) {
    if (!date) return '';
    const d = new Date(date);
    const now = new Date();
    const diff = now - d;
    const minutes = Math.floor(diff / 60000);
    const hours = Math.floor(diff / 3600000);
    const days = Math.floor(diff / 86400000);
    
    if (minutes < 1) return 'الان';
    if (minutes < 60) return `${minutes} دقیقه پیش`;
    if (hours < 24) return `${hours} ساعت پیش`;
    if (days < 7) return `${days} روز پیش`;
    return d.toLocaleDateString('fa-IR');
}

function formatDateTime(date) {
    if (!date) return '';
    const d = new Date(date);
    return d.toLocaleString('fa-IR', { 
        year: 'numeric', 
        month: '2-digit', 
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit'
    });
}

function escapeHtml(text) {
    if (!text) return '';
    const map = {
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#039;'
    };
    return text.replace(/[&<>"']/g, m => map[m]);
}

function scrollToBottom() {
    const messagesList = $('#messagesList')[0];
    if (messagesList) {
        messagesList.scrollTop = messagesList.scrollHeight;
    }
}
