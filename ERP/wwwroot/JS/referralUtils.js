function loadReferralData(guid, containerId, callback) {
    $.ajax({
        type: "POST",
        url: "/Referral/ListReferral",
        data: { Guid: guid },
        contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
        success: function (r) {
            $(`#${containerId}`).empty();
            if (r.data && r.data.length > 0) {
                for (var i = 0; i < r.data.length; i++) {
                    var z = r.data.length;
                    var y = z - i;
                    var markup = `
                        
                                <div class='row'>
                                    <div class='col-lg-12'>
                                        <div class='bs-component'>
                                            <div class='card' style='margin-bottom:5px'>
                                                <h5 class='card-header'>
                                                    <img class='image-rounded' src='/../userimage/${r.data[i].vahid[0].Image}'/>
                                                    فرستنده: ${r.data[i].vahid[0].FullName}
                                                </h5>
                                                <div class='card-body'>
                                                    <h5 class='card-title'>
                                                        <span id='num'>${y}</span>
                                                        گیرنده: <div style='margin-top:5px' id='listreciver${y}'></div>
                                                    </h5>
                                                </div>
                                                <div class='card-body2'>${r.data[i].vahid[0].Description || ''}</div>
                                                <hr style="border: none; border-top: 1px solid #f3e6e6;margin-bottom:0;">
                                                <div class='text-muted' style='text-align: center; margin: 5px 0;color:#644e4e !important;'>ارسال شده: ${r.data[i].vahid[0].CreateON}</div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                           `;
                    $(`#${containerId}`).append(markup);

                    for (var v = 0; v < r.data[i].vahid.length; v++) {
                        var receiverMarkup = r.data[i].vahid[v].FirstView == null
                            ? `<div style='margin-top:5px !important;'>
                                <img class='image-rounded' src='/../userimage/${r.data[i].vahid[v].ReceiverImage}'/>
                                <span>${r.data[i].vahid[v].ReceiverFullName2} <small style='font-size: 14px; color: #666;'>${r.data[i].vahid[v].ReceiverPost}</small></span>
                                <img style='float: left;' class='image-rounded1' src='/../icon/letterUnread.png'/>
                               </div>`
                            : `<div style='margin-top:5px !important;'>
                                <img class='image-rounded' src='/../userimage/${r.data[i].vahid[v].ReceiverImage}'/>
                               <span>${r.data[i].vahid[v].ReceiverFullName2} <small style='font-size: 14px; color: #666;'>${r.data[i].vahid[v].ReceiverPost}</small></span>
                                <img style='float: left;' class='image-rounded1 titleimage' src='/../icon/letterread.png' title='${moment(r.data[i].vahid[v].FirstView).format('jYYYY/jMM/jDD HH:mm:ss')}'/>
                               </div>`;
                        $(`#listreciver${y}`).append(receiverMarkup);
                    }
                }
            } else {
                $(`#${containerId}`).append("<div>داده‌ای یافت نشد</div>");
            }

            if (callback && typeof callback === "function") {
                callback(r);
            }
        },
        error: function (response) {
            console.error("Error loading referral data:", response.responseText || "Unknown error");
            $(`#${containerId}`).append("<div>خطا در بارگذاری داده‌ها</div>");
        }
    });
}


const REFERRAL_CONFIG = {
    controllerUrl: '/Referral',  // Centralized controller path
    redirectUrl: '/Home/SendCartable',  // After successful referral
    emptyBadge: function () { $('#numnotifications').text(''); }
};


$(document).ready(function () {
    // بلوک اول رو اضافه کن
    addReferralBlock();

    // دکمه اضافه کردن بلوک جدید
    $('#add-new-block').click(function () {
        addReferralBlock();
    });

    // Event listener برای dropdown (event delegation برای همه بلوک‌ها)
    $(document).on('click', '.referral-block .dropdown-item', function (e) {
        e.preventDefault();
        const selectedType = $(this).data('type');
        const $button = $(this).closest('.dropdown').find('.selected-btn');
        $button.data('selected-type', selectedType);

        if (selectedType && selectedType !== "") {
            $button.html(`<span class="fa fa-check"></span> ${$(this).text()}`)
                .addClass('selected');
        } else {
            $button.html(`<span class="fa fa-list"></span> انتخاب نوع ارجاع`)
                .removeClass('selected')
                .removeAttr('data-selected-type');
        }
    });

    // بقیه modal shown (init Editor/MultiSelect بعد از shown)
    $('#exampleModal').on('shown.bs.modal', function () {
        $('.referral-editor:not(.initialized)').each(function () {
            var descriptionId = $(this).attr('id');
            initKendoEditor(descriptionId);
            $(this).addClass('initialized');
        });
        $('.referral-multiselect:not(.initialized)').each(function () {
            var receiverId = $(this).attr('id');
        /*    initKendoMultiSelect(receiverId);*/
            $(this).addClass('initialized');
        });
        $(document).off('focusin.modal');
    });
});

// تابع برای اضافه کردن بلوک جدید
let blockCounter = 0;
function addReferralBlock() {
    blockCounter++;
    const blockId = `block-${blockCounter}`;
    const receiverId = `ReceiverID-${blockCounter}`;
    const descriptionId = `Description-${blockCounter}`;
    const typeId = `type-${blockCounter}`;

    const blockHtml = `
    <div class="referral-block" id="${blockId}" style="border: 1px solid #ccc; padding: 10px; margin-bottom: 15px; position: relative;">
        <button type="button" class="btn btn-danger btn-sm delete-block" style="position: absolute; top: 5px; left: 5px;z-index: 999;">حذف</button> 
        <div class="k-d-flex k-justify-content-center">
            <div class="k-w-300 col-md-12">
                <label for="Users">گیرندگان ارجاع</label>
                <select id="${receiverId}" name="${receiverId}" class="referral-multiselect"></select> 
                <p class="demo-hint">گیرنده یا گیرندگان ارجاع را مشخص نمایید.</p>
            </div>
        </div>
        <div class="form-group">
            <label for="message-text" class="col-form-label">متن ارجاع:</label>
            <div class="col-md-12">
                <div class="k-rtl">
                    <textarea id="${descriptionId}" class="referral-editor"></textarea> 
                </div>
            </div>
        </div>
        <div class="form-group mb-3 col-md-3">
            <label for="${typeId}">نوع ارجاع:</label>
            <div class="dropdown">
                <button class="btn btn-info dropdown-toggle w-100 selected-btn" type="button" id="${typeId}" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false" data-selected-type="">
                    <span class="fa fa-list"></span> انتخاب نوع ارجاع
                </button>
                <div class="dropdown-menu w-100" aria-labelledby="${typeId}">
                    <a class="dropdown-item" href="#" data-type="">بدون نوع ارجاع</a>
                    <a class="dropdown-item" href="#" data-type="جهت اقدام">جهت اقدام</a>
                    <a class="dropdown-item" href="#" data-type="جهت پیگیری">جهت پیگیری</a>
                    <a class="dropdown-item" href="#" data-type="جهت استحضار">جهت استحضار</a>
                    <a class="dropdown-item" href="#" data-type="جهت اطلاع">جهت اطلاع</a>
                    <a class="dropdown-item" href="#" data-type="جهت بررسی">جهت بررسی</a>
                </div>
            </div>
        </div>
    </div>
`;

    $('#referral-blocks').append(blockHtml);

    // initialize Kendo MultiSelect برای گیرندگان
    $(`#${receiverId}`).kendoMultiSelect({
        filter: "contains",
        dataTextField: "LastName",
        dataValueField: "Id",
        dataSource: {
            transport: {
                read: {
                    url: "/Referral/GetUsers",
                    type: "GET"
                }
            }
        },
        height: 300,
        itemTemplate: '<span class="k-state-default" style="background-image: url(' + '/userimage/' + '#:data.Image#' + ');"></span>' +
            '<span class="k-state-default"><h3>#: data.LastName #&nbsp#: data.Post #</h3></span>',
        tagTemplate: '<span class="selected-value" style="background-image: url(' + '/userimage/' + '#:data.Image#' + ');"></span>' +
            '<span>#: data.LastName #&nbsp#: data.Post #</span>'
    });

    // مقداردهی Kendo Editor
    initKendoEditor(descriptionId);

    // دکمه حذف بلوک
    $(`#${blockId} .delete-block`).click(function () {
        $(`#${blockId}`).remove();
    });
}




function initKendoEditor(descriptionId) {
    const el = document.getElementById(descriptionId);
    if (!el) return;

    // جلوگیری از دوباره ساختن ادیتور
    if ($(`#${descriptionId}`).data("kendoEditor")) return;

    el.style.height = "200px";
    el.style.direction = "rtl";
    el.style.textAlign = "right";
    el.classList.add("rtl-editor");

    $(`#${descriptionId}`).kendoEditor({
        resizable: false,
        encoded: false,
        tools: [
            "bold", "italic", "underline",
            "justifyLeft", "justifyCenter", "justifyRight", "justifyFull",
            "insertUnorderedList", "insertOrderedList",
            "foreColor", "backColor",
            "createLink", "unlink", "cleanFormatting"
        ],
        stylesheets: [
            "data:text/css,body{font-size:16px !important; direction:rtl !important; text-align:right !important; line-height:1; margin:8px; font-family:'Yekan'}"
        ]
    });

    const editor = $(`#${descriptionId}`).data("kendoEditor");
    if (editor && editor.body) {
        editor.body.dir = "rtl";
        editor.body.style.height = "auto";
        editor.body.style.textAlign = "right";
        editor.body.style.fontFamily = "Vazirmatn, IRANSans, Tahoma";
        editor.body.style.fontSize = "16px";
        editor.body.style.lineHeight = "1";
    }
}



/*Insert Referral (AJAX call)*/
function insertReferral(blocks, Guid, username, Type, Title) {  // blocks یک آرایه از {ReceiverIDs, Description} است
    if (!blocks || blocks.length === 0) {
        alert('لطفاً حداقل یک بلوک ارجاع اضافه کنید.');
        return false;
    }

    return $.ajax({
        type: 'POST',
        url: REFERRAL_CONFIG.controllerUrl + '/InsertReferral',
        data: {
            Blocks: JSON.stringify(blocks),
            Guid: Guid,
            username: username,
            Type: Type,
            Title: Title
        },
        success: function (r) {
            if (r.status === 'OK') {
                Swal.fire({
                    toast: true,
                    position: 'top-start',
                    icon: 'success',
                    text: 'ارجاع با موفقیت ارسال شد.',
                    iconColor: 'white',
                    customClass: {
                        popup: 'colored-toast',
                        title: 'colored-toast-title',
                        content: 'colored-toast-content'
                    },
                    showConfirmButton: false,
                    timer: 2500,
                    timerProgressBar: true,
                    willClose: function () {
                        setTimeout(function () {
                            window.location.href = REFERRAL_CONFIG.redirectUrl;
                        }, 200);
                    }
                });
                loadReferralData(Guid, 'referralrow', function (response) { });
                $('#exampleModal').modal('hide');
            } else {
                Swal.fire({
                    toast: true,
                    position: 'top-start',
                    icon: 'error',
                    text: r.message || 'مشکل در ارسال.',
                    showConfirmButton: false,
                    timer: 3000
                });
            }
        },
        error: function (response) {
            alert(response.responseText || 'خطا در ارتباط با سرور.');
            return false;
        }
    });
}

// List Files
function loadFileList(Guid, callback) {
    return $.ajax({
        type: 'POST',
        url: REFERRAL_CONFIG.controllerUrl + '/ListFile',
        data: { Guid: Guid },
        contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
        success: function (r) {
            if (r.status === 'OK' && callback) {
                callback(r.data);
            }
        },
        error: function (response) {
            console.error('Error loading files:', response);
        }
    });
}

// File List Display (populate table)
function displayFileList(files) {
    $('#tableFile').empty();
    if (files && files.length > 0) {
        files.forEach(function (file) {
            var markup = `<tr>
                <td>${file.Name}${file.Extension}</td>
                <td>
                    <a class='fa fa-download asd' href='/FileUpload/DownloadFileDatabase/${file.FileID}'></a>
                    <a class='fa fa-trash-o asd' onclick='deleteFile(${file.FileID})'></a>
                </td>
            </tr>`;
            $('#tableFile').append(markup);
        });
        $('#ali').removeClass('HideTable').addClass('ShowTable');
    }
    // Reset upload UI
    $('.k-upload-files').remove();
    $('.k-upload-status').remove();
    $('.k-upload.k-header').addClass('k-upload-empty');
    $('.k-upload-button').removeClass('k-state-focused');
}

// Upload Files (commit temp to permanent)
function commitFileUpload(Guid) {
    return $.ajax({
        type: 'POST',
        url: REFERRAL_CONFIG.controllerUrl + '/UploadFile',
        data: { Guid: Guid },
        contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
        success: function (r) {
            if (r.status === 'OK') {
                Swal.fire({
                    toast: true,
                    position: 'top-start',
                    icon: 'success',
                    text: 'فایل‌ها با موفقیت آپلود و ثبت شدند.',
                    iconColor: 'white',
                    customClass: {
                        popup: 'colored-toast',
                        title: 'colored-toast-title',
                        content: 'colored-toast-content'
                    },
                    showConfirmButton: false,
                    timer: 2500,
                    timerProgressBar: true
                });
                $('#AttachModal').modal('hide');
                loadFileList(Guid, displayFileList);
                updateFileNotification(Guid);
                return true;
            }
            return false;
        },
        error: function (response) {
            Swal.fire({
                toast: true,
                position: 'top-start',
                icon: 'error',
                text: response.responseText || 'خطا در آپلود فایل.',
                showConfirmButton: false,
                timer: 3000
            });
            return false;
        }
    });
}

// Delete File
function deleteFile(fileId, Guid) {
    swal({
        title: 'هشدار',
        text: 'آیا از حذف مطمئن هستید؟',
        type: 'error',
        showCancelButton: true,
        confirmButtonColor: '#DD6B55',
        confirmButtonText: 'بله',
        cancelButtonText: 'خیر'
    }).then(function (isConfirm) {
        if (isConfirm) {
            $.ajax({
                type: 'POST',
                url: '/FileUpload/Async_RemovebyID',
                data: { id: fileId },
                contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
                success: function (r) {
                    if (r.status === 'OK') {
                        swal({
                            title: '',
                            text: 'با موفقیت حذف شد.',
                            type: 'success',
                            confirmButtonText: 'اوکی'
                        }).then(function () {
                            loadFileList(guid, displayFileList);
                            updateFileNotification(guid);
                        });
                    }
                },
                error: function (response) {
                    alert(response.responseText || 'خطا در حذف.');
                }
            });
        }
    });
}

// Update File Notification Badge
function updateFileNotification(Guid) {
    $.ajax({
        type: 'POST',
        url: REFERRAL_CONFIG.controllerUrl + '/ListFileNotif',
        data: { Guid: Guid },
        contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
        success: function (r) {
            if (r.status === 'OK') {
                $('#numnotifications').text(r.data);
            }
        },
        error: function () {
            console.error('Error updating notification');
        }
    });
}



// Global variables for Type and Title (from View)
var REFERRAL_TYPE = '';  // از View پاس داده می‌شه
var REFERRAL_TITLE = '';  // از View پاس داده می‌شه

// Auto-init files and referrals
$(document).ready(function () {
    var guid = $('#Guid').val();
    if (guid) {
        window.ReferralUtils.updateFileNotification(guid);
    }

    // Bind #addreferral click
    $("#addreferral").click(handleReferralSubmit);

    // File modal open
    $('#AttachModal').on('shown.bs.modal', function () {
        FList();
    });
    $('#uploadBtn').click(function () {
        FUpload();
    });
});

// Function for #addreferral click (uses global Type and Title)
function handleReferralSubmit() {
    const blocks = [];
    $('.referral-block').each(function () {
        const block = $(this);
        const receiverId = block.find('select').attr('id');
        const descriptionId = block.find('textarea.referral-editor').attr('id');
        const typeButton = block.find('.selected-btn');

        const multi = $(`#${receiverId}`).data("kendoMultiSelect");
        const editor = $(`#${descriptionId}`).data("kendoEditor");

        if (!multi) return; // گیرنده باید حتما وجود داشته باشه

        const ReceiverIDs = multi.value();
        let Description = editor ? editor.value() : "";

        const selectedType = typeButton.data('selected-type');
        if (selectedType && selectedType !== "") {
            Description += "<br><span style='color: red'>نوع ارجاع: &nbsp" + selectedType + "</span>";
        }

        // فقط اگر گیرنده انتخاب شده باشد
        if (ReceiverIDs.length > 0) {
            blocks.push({ ReceiverIDs, Description });
        }
    });

    if (blocks.length === 0) {
        Swal.fire({
            toast: true,
            position: 'top-start',
            icon: 'error',
            text: 'لطفاً حداقل یک گیرنده انتخاب کنید.',
            showConfirmButton: false,
            timer: 3000
        });
        return;
    }

    const Guid = $('#Guid').val();
    const username = $('#username').val();

    // استفاده از global Type و Title
    window.ReferralUtils.insertReferral(blocks, Guid, username, REFERRAL_TYPE, REFERRAL_TITLE);
}

function FList() {
    var Guid = $('#Guid').val();
    window.ReferralUtils.loadFileList(Guid, window.ReferralUtils.displayFileList);
}

function FUpload() {
    var Guid = $('#Guid').val();
    window.ReferralUtils.commitFileUpload(Guid);
}

function deleteFile(id) {
    var Guid = $('#Guid').val();
    window.ReferralUtils.deleteFile(id, Guid);
}

function listfilenotif() {
    var Guid = $('#Guid').val();
    window.ReferralUtils.updateFileNotification(Guid);
}

// Upload events
function onUpload(e) {
    e.data = { guid: $('#Guid').val() };
}
function onRemove(e) {
    e.data = { guid: $('#Guid').val() };
}


// Export functions for global use
window.ReferralUtils = {
    insertReferral: insertReferral,
    loadFileList: loadFileList,
    displayFileList: displayFileList,
    commitFileUpload: commitFileUpload,
    deleteFile: deleteFile,
    updateFileNotification: updateFileNotification
};