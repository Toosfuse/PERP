let connection;
let currentUserId = null;
let currentUserName = null;
let myUserId = window.myUserId || '';
let typingTimer;
let isTyping = false;
let editingMessageId = null;
let replyingToMessageId = null;
let currentPage = 1;
let isLoadingMessages = false;
window.currentGroupId = null;

$(document).ready(function() {
    if ('Notification' in window) {
        if (Notification.permission === 'granted') {
            console.log('Notification فعال است');
        } else if (Notification.permission !== 'denied') {
            Notification.requestPermission().then(function(permission) {
                console.log('Notification اجازه:', permission);
            });
        }
    }
    
    connection = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub", {
            transport: signalR.HttpTransportType.LongPolling
        })
        .withAutomaticReconnect()
        .build();

    connection.start()
        .then(() => console.log('SignalR متصل شد'))
        .catch(err => console.error('خطا:', err));

    connection.on("UserOnline", function (userId) {
        var selector = '.user-item[data-user-id="' + userId + '"] .online-status';
        $(selector).addClass('online');
    });

    connection.on("UserOffline", function (userId) {
        var selector = '.user-item[data-user-id="' + userId + '"] .online-status';
        $(selector).removeClass('online');
    });

    connection.on("LoadOnlineUsers", function (onlineUsers) {
        onlineUsers.forEach(function(userId) {
            var selector = '.user-item[data-user-id="' + userId + '"] .online-status';
            $(selector).addClass('online');
        });
    });

    connection.on("ReceiveMessage", function (msg) {
        if (currentUserId && (msg.senderId === currentUserId || msg.receiverId === currentUserId)) {
            const isMine = msg.senderId === myUserId;
            addMessageToChat(msg, isMine);
            scrollToBottom();
            
            if (!isMine) {
                markMessageDelivered(msg.id);
                markAsRead(msg.senderId);
                showNotification(msg);
            }
        }
        
        const otherUserId = msg.senderId === myUserId ? msg.receiverId : msg.senderId;
        const userItem = $(`.user-item[data-user-id="${otherUserId}"]`);
        
        if (userItem.length === 0) {
            $.get('/Chat/GetUserInfo', { userId: otherUserId }, function(user) {
                const newUserHtml = `
                    <div class="user-item" data-user-id="${user.id}" data-user-name="${user.name}">
                        <div class="user-avatar">
                            <img src="${user.image}" alt="${user.name}" />
                            <span class="online-status ${user.isOnline ? 'online' : ''}"></span>
                        </div>
                        <div class="user-info">
                            <div class="user-name">${user.name}</div>
                            <div class="last-message">${msg.message}</div>
                        </div>
                    </div>
                `;
                $('.users-list').prepend(newUserHtml);
            });
        } else {
            userItem.prependTo('.users-list');
            userItem.find('.last-message').text(msg.message);
        }
    });

    connection.on("UserTyping", function (userId) {
        showTypingIndicator(userId);
    });

    connection.on("UserStoppedTyping", function (userId) {
        hideTypingIndicator(userId);
    });

    connection.on("MessageDelivered", function (msg) {
        $(`.message[data-message-id="${msg.id}"] .message-status`)
            .removeClass('sent')
            .addClass('delivered')
            .html('<i class="fa fa-check"></i><i class="fa fa-check" style="margin-right:-8px"></i>');
    });

    connection.on("MessagesRead", function (userId) {
        $(`.message.mine .message-status`)
            .removeClass('delivered')
            .addClass('read')
            .html('<i class="fa fa-check"></i><i class="fa fa-check" style="margin-right:-8px"></i>');
        $(`.message.mine[data-message-id]`).removeClass('editable');
        $(`.message.mine .edit-btn`).remove();
    });

    connection.on("ReceiveGroupMessage", function (msg) {
        if (window.currentGroupId) {
            addGroupMessageToChat(msg);
            scrollToBottom();
        }
    });

    connection.on("GroupMemberCountUpdated", function (groupId, memberCount) {
        $(`.group-item[data-group-id="${groupId}"] .last-message`).text(memberCount + ' عضو');
    });

    connection.on("GroupMessageDeleted", function (data) {
        $(`.message[data-message-id="${data.id}"]`).remove();
    });

    connection.on("GroupMessageEdited", function (data) {
        $(`.message[data-message-id="${data.id}"] .message-text`).text(data.message);
    });

    $('#searchUser').on('input', function() {
        const searchText = $(this).val().trim();
        
        if (searchText.length < 2) {
            $('#searchResults').empty().hide();
            $('.user-item').show();
            return;
        }
        
        $('.user-item').hide();
        
        $.get('/Chat/SearchUsers', { query: searchText }, function(users) {
            $('#searchResults').empty();
            
            if (users.length > 0) {
                users.forEach(function(user) {
                    const html = `
                        <div class="search-result-item" data-user-id="${user.id}" data-user-name="${user.name}">
                            <img src="${user.image}" alt="${user.name}" />
                            <span>${user.name}</span>
                        </div>
                    `;
                    $('#searchResults').append(html);
                });
                $('#searchResults').show();
            } else {
                $('#searchResults').hide();
            }
        });
    });

    $(document).on('click', '.search-result-item', function() {
        selectUserFromSearch($(this));
    });

    $(document).on('click', '.all-user-item', function() {
        selectUserFromAllUsers($(this));
    });

    $(document).on('click', '.user-item:not(.group-item)', function() {
        selectUser($(this));
    });

    $(document).on('keydown', function(e) {
        if (e.key === 'ArrowDown' && e.ctrlKey && currentUserId) {
            const currentItem = $('.user-item.active');
            const nextItem = currentItem.next('.user-item:not(.group-item)');
            if (nextItem.length) {
                selectUser(nextItem);
            }
        }
    });

    $('#sendBtn').click(function() {
        if (editingMessageId) {
            if (window.currentGroupId) {
                editGroupMessage();
            } else {
                editMessage();
            }
        } else if (window.currentGroupId) {
            sendGroupMessage();
        } else {
            sendMessage();
        }
    });

    $('#messageInput').keypress(function(e) {
        if (e.which === 13) {
            if (editingMessageId) {
                editMessage();
            } else if (window.currentGroupId) {
                console.log(window.currentGroupId);

                sendGroupMessage();
            } else {
                console.log("1");

                sendMessage();
            }
        } else {
            if (!editingMessageId) {
                handleTyping();
            }
        }
    });

    $('#messageInput').on('input', handleTyping);

    $('#attachBtn').click(function() {
        $('#fileInput').click();
    });

    $('#fileInput').change(function() {
        if (this.files[0]) {
            uploadFile(this.files[0]);
        }
    });

    $('#emojiBtn').click(function(e) {
        e.stopPropagation();
        $('#emojiPicker').toggle();
    });

    $('.emoji-item').click(function() {
        const emoji = $(this).text();
        const input = $('#messageInput');
        input.val(input.val() + emoji);
        input.focus();
    });

    $(document).click(function(e) {
        if (!$(e.target).closest('.emoji-picker, #emojiBtn').length) {
            $('#emojiPicker').hide();
        }
        if (!$(e.target).closest('.search-box').length) {
            $('#searchResults').hide();
        }
    });

    $('#deleteChatBtn').click(function() {
        if (confirm('چت فقط برای شما پاک میشود. ادامه میدهید؟')) {
            deleteChat();
        }
    });

    $('#restoreChatBtn').click(function() {
        restoreChat();
    });

    $('#darkModeBtn').click(function() {
        $('body').toggleClass('dark-mode');
        var isDarkMode = $('body').hasClass('dark-mode');
        localStorage.setItem('darkMode', isDarkMode);
        $(this).find('i').toggleClass('fa-moon-o fa-sun-o');
    });

    if (localStorage.getItem('darkMode') === 'true') {
        $('body').addClass('dark-mode');
        $('#darkModeBtn').find('i').removeClass('fa-moon-o').addClass('fa-sun-o');
    }

    $('#searchMessages').on('input', function() {
        var searchText = $(this).val().toLowerCase().trim();
        
        if (searchText.length === 0) {
            $('.message').show();
            return;
        }
        
        $('.message').each(function() {
            var messageText = $(this).find('.message-text').text().toLowerCase();
            if (messageText.includes(searchText)) {
                $(this).show();
            } else {
                $(this).hide();
            }
        });
    });

    $('#logoutBtn').click(function() {
        if (confirm('آیا میخواهید خارج شوید؟')) {
            window.location.href = '/Home/Cartable';
        }
    });

    $('#newChatBtn').click(function() {
        loadAllUsers();
        $('#allUsersModal').show();
        $('#newChatBtn').hide();
        $('#closeModalBtn').show();
    });

    $('#closeModalBtn').click(function() {
        $('#allUsersModal').hide();
        $('#closeModalBtn').hide();
        $('#newChatBtn').show();
    });

    $(document).on('click', '.message', function(e) {
        if ($(e.target).closest('.edit-btn, .reply-btn').length) return;
        
        if (editingMessageId) {
            cancelEdit();
            return;
        }
        
        $('.message .message-actions').removeClass('show');
        $(this).find('.message-actions').addClass('show');
    });

    $(document).on('click', '.reply-btn', function(e) {
        e.stopPropagation();
        const messageEl = $(this).closest('.message');
        const messageId = messageEl.data('message-id');
        const messageText = messageEl.find('.message-text').text();
        
        replyingToMessageId = messageId;
        $('#messageInput').focus();
        $('#messageInput').attr('placeholder', 'پاسخ به: ' + messageText.substring(0, 30) + '...');
    });

    $(document).on('click', '.edit-btn', function(e) {
        e.stopPropagation();
        const messageEl = $(this).closest('.message');
        const messageId = messageEl.data('message-id');
        const messageText = messageEl.find('.message-text').text();
        
        editingMessageId = messageId;
        $('#messageInput').val(messageText).focus();
        $('#sendBtn').html('<i class="fa fa-check"></i>').addClass('edit-mode');
        messageEl.addClass('editing');
    });

    $(document).on('click', '.forward-btn', function(e) {
        e.stopPropagation();
        const messageId = $(this).data('message-id');
        showForwardModal(messageId);
    });

    $(document).on('click', '.forward-user-item', function() {
        const userId = $(this).data('user-id');
        const messageId = $(this).data('message-id');
        forwardMessage(messageId, userId);
    });

    $(document).on('click', function(e) {
        if (!$(e.target).closest('.forward-modal-content').length) {
            $('#forwardModal').removeClass('show');
        }
    });

    $(document).on('input', '#forwardSearch', function() {
        const searchText = $(this).val().toLowerCase();
        if (searchText.length === 0) {
            $('.forward-user-item').show();
        } else {
            $('.forward-user-item').each(function() {
                const userName = $(this).find('.user-name').text().toLowerCase();
                if (userName.includes(searchText)) {
                    $(this).show();
                } else {
                    $(this).hide();
                }
            });
        }
    });

    $('#chatMessages').on('scroll', function() {
        if ($(this).scrollTop() === 0 && !isLoadingMessages && currentUserId) {
            loadMoreMessages();
        }
    });

    $(document).on('click', '.next-user-btn', function() {
        const currentItem = $('.user-item.active');
        const nextItem = currentItem.next('.user-item:not(.group-item)');
        if (nextItem.length) {
            selectUser(nextItem);
        }
    });
});

function selectUserFromSearch(element) {
    const userId = element.data('user-id');
    const userName = element.data('user-name');
    const userImage = element.find('img').attr('src');
    
    currentUserId = userId;
    currentUserName = userName;
    window.currentGroupId = null;
    
    $('#selectedUserName').text(userName);
    $('#selectedUserAvatar').attr('src', userImage);
    
    $('#chatHeader').show();
    $('#chatInput').show();
    $('.no-chat-selected').hide();
    $('#searchResults').empty().hide();
    $('#searchUser').val('');
    
    $('.user-item').removeClass('active');
    
    if ($(`.user-item[data-user-id="${userId}"]`).length === 0) {
        const newUserHtml = `
            <div class="user-item active" data-user-id="${userId}" data-user-name="${userName}">
                <div class="user-avatar">
                    <img src="${userImage}" alt="${userName}" />
                    <span class="online-status"></span>
                </div>
                <div class="user-info">
                    <div class="user-name">${userName}</div>
                    <div class="last-message">پیامی وجود ندارد</div>
                </div>
            </div>
        `;
        $('.users-list').prepend(newUserHtml);
    } else {
        $(`.user-item[data-user-id="${userId}"]`).addClass('active').prependTo('.users-list');
    }
    
    loadMessages(userId);
    markAsRead(userId);
}

function selectUser(element) {
    const userId = element.data('user-id');
    const userName = element.data('user-name');
    
    currentUserId = userId;
    currentUserName = userName;
    window.currentGroupId = null;
    
    $('.user-item').removeClass('active');
    element.addClass('active');
    
    $('#selectedUserName').text(currentUserName);
    $('#selectedUserAvatar').attr('src', element.find('img').attr('src'));
    
    $('#chatHeader').show();
    $('#chatInput').show();
    $('.no-chat-selected').hide();
    $('#groupMembersBtn').hide();
    
    element.find('.unread-count').remove();
    
    loadMessages(currentUserId);
    markAsRead(currentUserId);
}

function selectUserFromAllUsers(element) {
    window.currentGroupId = null;
    const userId = element.data('user-id');
    const userName = element.data('user-name');
    const userImage = element.find('img').attr('src');
    
    currentUserId = userId;
    currentUserName = userName;
    
    $('#selectedUserName').text(userName);
    $('#selectedUserAvatar').attr('src', userImage);
    
    $('#chatHeader').show();
    $('#chatInput').show();
    $('.no-chat-selected').hide();
    $('#allUsersModal').hide();
    $('#closeModalBtn').hide();
    $('#newChatBtn').show();
    
    $('.user-item').removeClass('active');
    
    const userItem = $(`.user-item[data-user-id="${userId}"]`);
    if (userItem.length === 0) {
        const newUserHtml = `
            <div class="user-item active" data-user-id="${userId}" data-user-name="${userName}">
                <div class="user-avatar">
                    <img src="${userImage}" alt="${userName}" />
                    <span class="online-status"></span>
                </div>
                <div class="user-info">
                    <div class="user-name">${userName}</div>
                    <div class="last-message">پیامی وجود ندارد</div>
                </div>
            </div>
        `;
        $('.users-list').prepend(newUserHtml);
    } else {
        userItem.addClass('active').prependTo('.users-list');
    }
    
    loadMessages(userId);
}

function loadAllUsers() {
    $.get('/Chat/GetAllUsers', function(users) {
        let usersHtml = '';
        users.forEach(function(user) {
            usersHtml += `
                <div class="all-user-item" data-user-id="${user.id}" data-user-name="${user.name}">
                    <img src="${user.image}" alt="${user.name}" />
                    <div class="user-name">${user.name}</div>
                </div>
            `;
        });
        $('#allUsersList').html(usersHtml);
    }).fail(function() {
        $('#allUsersList').html('<p style="text-align:center;color:#666;">خطا در بارگذاری کاربران</p>');
    });
}

function loadMessages(userId) {
    currentPage = 1;
    isLoadingMessages = false;
    $.get('/Chat/GetMessages', { userId: userId, page: currentPage, pageSize: 50 }, function(messages) {
        $('#chatMessages').empty();
        messages.forEach(function(message) {
            const isMine = message.senderId === myUserId;
            addMessageToChat(message, isMine);
        });
        scrollToBottom();
    });
}

function loadMoreMessages() {
    if (isLoadingMessages || !currentUserId) return;
    
    isLoadingMessages = true;
    currentPage++;
    
    $.get('/Chat/GetMessages', { userId: currentUserId, page: currentPage, pageSize: 50 }, function(messages) {
        if (messages.length > 0) {
            const firstMessageId = $('.message').first().data('message-id');
            messages.forEach(function(message) {
                const isMine = message.senderId === myUserId;
                addMessageToChat(message, isMine);
            });
        }
        isLoadingMessages = false;
    });
}

function sendMessage() {
    const message = $('#messageInput').val().trim();
    if (!message || !currentUserId) return;
    
    stopTyping();
    
    $.post('/Chat/SendMessage', {
        receiverId: currentUserId,
        message: message,
        attachmentPath: null,
        attachmentName: null,
        replyToMessageId: replyingToMessageId,
        __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
    }, function(result) {
        if (result.success) {
            $('#messageInput').val('');
            $('#messageInput').attr('placeholder', 'پیام خود را بنویسید...');
            replyingToMessageId = null;
            const userItem = $(`.user-item[data-user-id="${currentUserId}"]`);
            if (userItem.length === 0) {
                $.get('/Chat/GetUserInfo', { userId: currentUserId }, function(user) {
                    const newUserHtml = `
                        <div class="user-item active" data-user-id="${user.id}" data-user-name="${user.name}">
                            <div class="user-avatar">
                                <img src="${user.image}" alt="${user.name}" />
                                <span class="online-status"></span>
                            </div>
                            <div class="user-info">
                                <div class="user-name">${user.name}</div>
                                <div class="last-message">${message}</div>
                            </div>
                        </div>
                    `;
                    $('.users-list').prepend(newUserHtml);
                });
            } else {
                userItem.prependTo('.users-list');
                updateUserLastMessage(currentUserId, message);
            }
        } else {
            console.error(result.error);
        }
    }).fail(function(err) {
        console.error('خطا در ارسال پیام', err);
    });
}

function editMessage() {
    const newMessage = $('#messageInput').val().trim();
    if (!newMessage || !editingMessageId) return;
    
    $.post('/Chat/EditMessage', {
        messageId: editingMessageId,
        newMessage: newMessage
    }, function(result) {
        if (result.success) {
            const messageEl = $(`.message[data-message-id="${editingMessageId}"]`);
            messageEl.find('.message-text').text(newMessage);
            if (!messageEl.find('.edited-label').length) {
                messageEl.find('.message-content').append('<span class="edited-label"> (ویرایش شده)</span>');
            }
            cancelEdit();
        } else {
            alert(result.error || 'خطا در ویرایش پیام');
        }
    }).fail(function() {
        alert('خطا در ویرایش پیام');
    });
}

function cancelEdit() {
    editingMessageId = null;
    $('#messageInput').val('');
    $('#sendBtn').html('<i class="fa fa-paper-plane"></i>').removeClass('edit-mode');
    $('.message').removeClass('editing');
}

function editGroupMessage() {
    const newMessage = $('#messageInput').val().trim();
    if (!newMessage || !editingMessageId) return;
    
    $.post('/GroupChat/EditGroupMessage', {
        messageId: editingMessageId,
        newMessage: newMessage
    }, function(result) {
        if (result.success) {
            const messageEl = $(`.message[data-message-id="${editingMessageId}"]`);
            messageEl.find('.message-text').text(newMessage);
            if (!messageEl.find('.edited-label').length) {
                messageEl.find('.message-content').append('<span class="edited-label"> (ویرایش شده)</span>');
            }
            cancelEdit();
        } else {
            alert(result.error || 'خطا در ویرایش پیام');
        }
    }).fail(function() {
        alert('خطا در ویرایش پیام');
    });
}

function addMessageToChat(message, isMine) {
    let statusIcon = '';
    let editableClass = '';
    let editBtn = '';
    let replyBox = '';
    
    if (isMine) {
        if (message.isRead) {
            statusIcon = '<span class="message-status read"><i class="fa fa-check"></i><i class="fa fa-check" style="margin-right:-8px"></i></span>';
        } else if (message.isDelivered) {
            statusIcon = '<span class="message-status delivered"><i class="fa fa-check"></i><i class="fa fa-check" style="margin-right:-8px"></i></span>';
            editableClass = 'editable';
            editBtn = '<button class="edit-btn" title="ویرایش"><i class="fa fa-pencil"></i></button>';
        } else {
            statusIcon = '<span class="message-status sent"><i class="fa fa-check"></i></span>';
            editableClass = 'editable';
            editBtn = '<button class="edit-btn" title="ویرایش"><i class="fa fa-pencil"></i></button>';
        }
    }
    
    if (message.replyToMessage) {
        replyBox = `
            <div class="reply-box">
                <div class="reply-sender">${escapeHtml(message.replyToSenderName)}</div>
                <div class="reply-text">${escapeHtml(message.replyToMessage)}</div>
            </div>
        `;
    }
    
    const editedLabel = message.isEdited ? '<span class="edited-label"> (ویرایش شده)</span>' : '';
    const replyBtn = `<button class="reply-btn" data-message-id="${message.id}" title="پاسخ"><i class="fa fa-reply"></i></button>`;
    const forwardBtn = `<button class="forward-btn" data-message-id="${message.id}" title="انتقال"><i class="fa fa-share"></i></button>`;
    const copyBtn = `<button class="copy-btn" data-message-id="${message.id}" title="کپی"><i class="fa fa-copy"></i></button>`;
    const deleteBtn = isMine ? `<button class="delete-message-btn" data-message-id="${message.id}" title="حذف"><i class="fa fa-trash"></i></button>` : '';
    
    const messageHtml = `
        <div class="message ${isMine ? 'mine' : ''} ${editableClass}" data-message-id="${message.id}">
            <div class="message-content">
                ${replyBox}
                ${message.attachmentPath ? `
                    <div class="attachment">
                        <i class="fa fa-file"></i>
                        <a href="${escapeHtml(message.attachmentPath)}" target="_blank">${escapeHtml(message.attachmentName)}</a>
                    </div>
                ` : ''}
                <span class="message-text">${escapeHtml(message.message || '')}</span>
                ${editedLabel}
                <div class="message-time">${statusIcon} ${message.sentAt} - ${message.dateAt || ''}</div>
            </div>
            <div class="message-actions">
                ${replyBtn}
                ${forwardBtn}
                ${copyBtn}
                ${editBtn}
                ${deleteBtn}
            </div>
        </div>
    `;
    $('#chatMessages').append(messageHtml);
}

function uploadFile(file) {
    const formData = new FormData();
    formData.append('file', file);
    
    $.ajax({
        url: '/Chat/UploadFile',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function(result) {
            if (result.success && currentUserId) {
                $.post('/Chat/SendMessage', {
                    receiverId: currentUserId,
                    message: "📎 فایل پیوستی",
                    attachmentPath: result.path,
                    attachmentName: result.name
                });
            } else {
                alert(result.error || 'خطا در آپلود فایل');
            }
        },
        error: function() {
            alert('خطا در آپلود فایل');
        }
    });
}

function handleTyping() {
    if (!currentUserId) return;
    
    if (!isTyping) {
        isTyping = true;
        connection.invoke("SendTyping", currentUserId);
    }
    
    clearTimeout(typingTimer);
    typingTimer = setTimeout(stopTyping, 2000);
}

function stopTyping() {
    if (isTyping && currentUserId) {
        isTyping = false;
        connection.invoke("StopTyping", currentUserId);
    }
    clearTimeout(typingTimer);
}

function showTypingIndicator(userId) {
    if (!$(`.typing-indicator[data-user-id="${userId}"]`).length) {
        const typingHtml = `
            <div class="typing-indicator" data-user-id="${userId}">
                <div class="typing-dots">
                    <span></span><span></span><span></span>
                </div>
                <span class="typing-text">در حال تایپ...</span>
            </div>
        `;
        $('#chatMessages').append(typingHtml);
        scrollToBottom();
    }
}

function hideTypingIndicator(userId) {
    $(`.typing-indicator[data-user-id="${userId}"]`).remove();
}

function markAsRead(userId) {
    $.post('/Chat/MarkAsRead', { userId: userId });
}

function markMessageDelivered(messageId) {
    $.post('/Chat/MarkAsDelivered', { messageId: messageId });
}

function deleteChat() {
    if (!currentUserId) return;
    
    $.post('/Chat/DeleteChat', { userId: currentUserId, permanent: false }, function(result) {
        if (result.success) {
            $('#chatMessages').empty();
            $('.no-chat-selected').show();
            $('#chatHeader').hide();
            $('#chatInput').hide();
            $('#restoreChatBtn').show();
            alert('چت پاک شد. میتوانید بازیابی کنید.');
        }
    }).fail(function() {
        alert('خطا در پاک کردن چت');
    });
}

function restoreChat() {
    if (!currentUserId) return;
    
    $.post('/Chat/RestoreChat', { userId: currentUserId }, function(result) {
        if (result.success) {
            loadMessages(currentUserId);
            $('#restoreChatBtn').hide();
            alert('چت بازیابی شد.');
        }
    }).fail(function() {
        alert('خطا در بازیابی چت');
    });
}

function updateUserLastMessage(userId, message) {
    $(`.user-item[data-user-id="${userId}"] .last-message`).text(escapeHtml(message));
}

function scrollToBottom() {
    const chatMessages = $('#chatMessages')[0];
    if (chatMessages) {
        chatMessages.scrollTop = chatMessages.scrollHeight;
    }
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

function showForwardModal(messageId) {
    $.get('/Chat/GetAllUsers', function(users) {
        let usersHtml = '';
        users.forEach(function(user) {
            usersHtml += `
                <div class="forward-user-item" data-user-id="${user.id}" data-message-id="${messageId}">
                    <img src="${user.image}" alt="${user.name}" />
                    <div class="user-name">${user.name}</div>
                </div>
            `;
        });
        $('#forwardUsersList').html(usersHtml);
        $('#forwardModal').addClass('show');
    });
}

function forwardMessage(messageId, userId) {
    $.post('/Chat/ForwardMessage', {
        messageId: messageId,
        receiverId: userId
    }, function(result) {
        if (result.success) {
            $('#forwardModal').removeClass('show');
            alert('پیام با موفقیت انتقال یافت');
        } else {
            alert(result.error || 'خطا در انتقال پیام');
        }
    }).fail(function() {
        alert('خطا در انتقال پیام');
    });
}

function showNotification(msg) {
    if (window.notificationsEnabled && 'Notification' in window && Notification.permission === 'granted') {
        const senderName = currentUserName || 'کاربر';
        const notification = new Notification('پیام جدید از ' + senderName, {
            body: msg.message.substring(0, 100),
            icon: '/UserImage/Male.png',
            tag: 'chat-' + msg.senderId,
            requireInteraction: false
        });
        
        notification.onclick = function() {
            window.focus();
            notification.close();
        };
    }
}

function sendGroupMessage() {
    const message = $('#messageInput').val().trim();
    if (!message || !window.currentGroupId) return;
    
    $.post('/GroupChat/SendGroupMessage', {
        groupId: window.currentGroupId,
        message: message,
        replyToMessageId: replyingToMessageId,
        __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
    }, function(result) {
        if (result.success) {
            $('#messageInput').val('');
            $('#messageInput').attr('placeholder', 'پیام خود را بنویسید...');
            replyingToMessageId = null;
            scrollToBottom();
        } else {
            console.error(result.error);
        }
    }).fail(function(err) {
        console.error('خطا در ارسال پیام گروهی', err);
    });
}

function selectGroup(groupId) {
    window.currentGroupId = groupId;
    currentUserId = null;
    $('#chatHeader').show();
    $('#chatInput').show();
    $('#chatMessages').empty();
    $('.no-chat-selected').hide();
    $('#groupMembersBtn').show();
    $('#deleteGroupBtn').show();
    $('#deleteChatBtn').hide();
    $('.user-item').removeClass('active');
    
    $.get('/GroupChat/GetUserGroups', function(groups) {
        const group = groups.find(g => g.id === groupId);
        if (group) {
            $('#selectedUserName').text(group.name);
            $('#selectedUserAvatar').attr('src', group.image);
        }
    });
    
    connection.invoke('JoinGroup', groupId);
    loadGroupMessages(groupId);
}

function loadGroupMessages(groupId) {
    $.get('/GroupChat/GetGroupMessages', { groupId: groupId }, function(messages) {
        $('#chatMessages').empty();
        if (Array.isArray(messages)) {
            messages.forEach(function(msg) {
                addGroupMessageToChat(msg);
            });
        }
        scrollToBottom();
    });
}

function addGroupMessageToChat(msg) {
    const isMine = msg.senderId === window.myUserId;
    let replyBox = '';
    
    if (msg.replyToMessage) {
        replyBox = `
            <div class="reply-box">
                <div class="reply-sender">${escapeHtml(msg.replyToSenderName)}</div>
                <div class="reply-text">${escapeHtml(msg.replyToMessage)}</div>
            </div>
        `;
    }
    
    const editedLabel = msg.isEdited ? '<span class="edited-label"> (ویرایش شده)</span>' : '';
    const replyBtn = `<button class="reply-btn" data-message-id="${msg.id}" title="پاسخ"><i class="fa fa-reply"></i></button>`;
    const editBtn = isMine ? `<button class="edit-btn" data-message-id="${msg.id}" title="ویرایش"><i class="fa fa-pencil"></i></button>` : '';
    const copyBtn = `<button class="copy-btn" data-message-id="${msg.id}" title="کپی"><i class="fa fa-copy"></i></button>`;
    const deleteBtn = isMine ? `<button class="delete-message-btn" data-message-id="${msg.id}" data-group="true" title="حذف"><i class="fa fa-trash"></i></button>` : '';
    
    const html = `
        <div class="message ${isMine ? 'mine' : ''}" data-message-id="${msg.id}">
            <div class="message-content">
                ${replyBox}
                <span class="message-text">${escapeHtml(msg.message)}</span>
                ${editedLabel}
                <div class="message-time">${msg.sentAt}</div>
            </div>
            <div class="message-actions">
                ${replyBtn}
                ${copyBtn}
                ${editBtn}
                ${deleteBtn}
            </div>
        </div>
    `;
    $('#chatMessages').append(html);
}

$(document).on('click', '.copy-btn', function(e) {
    e.stopPropagation();
    const messageEl = $(this).closest('.message');
    const messageText = messageEl.find('.message-text').text();
    navigator.clipboard.writeText(messageText).then(() => {
        alert('پیام کپی شد');
    });
});

$(document).on('click', '.delete-message-btn:not([data-group])', function(e) {
    e.stopPropagation();
    const messageId = $(this).data('message-id');
    if (confirm('آیا مطمئن هستید پیام پاک شود؟')) {
        $.post('/Chat/DeleteMessage', { messageId: messageId }, function(result) {
            if (result.success) {
                $(`.message[data-message-id="${messageId}"]`).remove();
            } else {
                alert(result.error || 'خطا در حذف پیام');
            }
        });
    }
});

$(document).on('click', '#deleteGroupBtn', function() {
    if (confirm('آیا مطمئن هستیدگروه پاک شود؟')) {
        $.post('/GroupChat/DeleteGroup', { groupId: window.currentGroupId }, function(result) {
            if (result.success) {
                $('#chatMessages').empty();
                $('.no-chat-selected').show();
                $('#chatHeader').hide();
                $('#chatInput').hide();
                window.currentGroupId = null;
                loadGroups();
            }
        });
    }
});

$(document).on('click', '.delete-message-btn[data-group]', function(e) {
    e.stopPropagation();
    const messageId = $(this).data('message-id');
    if (confirm('آیا مطمئن هستید پیام گروه پاک شود؟')) {
        $.post('/GroupChat/DeleteGroupMessage', { messageId: messageId }, function(result) {
            if (result.success) {
                $(`.message[data-message-id="${messageId}"]`).remove();
            }
        });
    }
});

$(document).on('contextmenu', '.user-item', function(e) {
    e.preventDefault();
    const userId = $(this).data('user-id');
    const userName = $(this).data('user-name');
    
    const menu = `
        <div class="context-menu" style="position:fixed; top:${e.pageY}px; left:${e.pageX}px; background:white; border:1px solid #ddd; border-radius:4px; box-shadow:0 2px 8px rgba(0,0,0,0.15); z-index:10000;">
            <button class="block-user-btn" data-user-id="${userId}" style="display:block; width:100%; padding:8px 12px; border:none; background:none; text-align:right; cursor:pointer; font-size:14px;">مسدود کردن</button>
        </div>
    `;
    
    $('body').append(menu);
    
    $(document).one('click', function() {
        $('.context-menu').remove();
    });
});

$(document).on('click', '.block-user-btn', function() {
    const userId = $(this).data('user-id');
    $.post('/Chat/BlockUser', { userId: userId, __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val() }, function(result) {
        if (result.success) {
            $(`.user-item[data-user-id="${userId}"]`).remove();
            alert('کاربر مسدود شد');
        }
    });
    $('.context-menu').remove();
});
