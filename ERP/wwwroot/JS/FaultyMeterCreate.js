
document.addEventListener('DOMContentLoaded', function () {
    var barcodes = JSON.parse(localStorage.getItem('barcodes')) || [];

    var complaintInput = document.getElementById('ComplaintNumber');
    if (complaintInput && !complaintInput.value) {
        var storedComplaint = localStorage.getItem('complaintNumber');
        if (storedComplaint) complaintInput.value = storedComplaint;
    }

    var entryDateInput = document.getElementById('EntryDate');
    if (entryDateInput && !entryDateInput.value) {
        var storedDate = localStorage.getItem('entryDate');
        if (storedDate) entryDateInput.value = storedDate;
    }

    if (entryDateInput) {
        entryDateInput.addEventListener('input', () => {
            localStorage.setItem('entryDate', entryDateInput.value.trim());
        });
    }

    if (complaintInput) {
        complaintInput.addEventListener('input', () => {
            localStorage.setItem('complaintNumber', complaintInput.value.trim());
        });
    }

    function saveToLocalStorage() {
        localStorage.setItem('barcodes', JSON.stringify(barcodes));
    }

    function updateBarcodeList(searchTerm = '') {
        var list = document.getElementById('BarcodeList');
        if (!list) return;
        list.innerHTML = '';

        barcodes.forEach((barcode, index) => {
            if (barcode.includes(searchTerm)) {
                var li = document.createElement('li');
                li.className = 'list-group-item d-flex justify-content-between align-items-center';
                li.innerHTML = `
    ${barcode}
    <button class="btn btn-sm btn-danger" onclick="removeBarcode(${index})">🗑️</button>`;
                list.appendChild(li);
            }
        });

        var countSpan = document.getElementById('BarcodeCount');
        if (countSpan) countSpan.innerText = barcodes.length;
    }

    window.removeBarcode = function (index) {
        Swal.fire({
            title: 'حذف بارکد',
            text: 'آیا مطمئن هستید؟',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'بله',
            cancelButtonText: 'خیر'
        }).then(result => {
            if (result.isConfirmed) {
                barcodes.splice(index, 1);
                saveToLocalStorage();
                updateBarcodeList(document.getElementById('SearchBarcode').value.trim());
                Swal.fire('حذف شد!', '', 'success');
            }
        });
    };

    updateBarcodeList();

    var serialInput = document.getElementById('SerialNumber');
    if (serialInput) {
        serialInput.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                var val = serialInput.value.trim();
                if (val && !barcodes.includes(val)) {
                    barcodes.push(val);
                    saveToLocalStorage();
                    updateBarcodeList();
                } else {
                    Swal.fire('تکراری', 'این بارکد قبلاً اسکن شده است.', 'warning');
                }
                serialInput.value = '';
            }
        });
    }

    var clearAllBtn = document.getElementById('clearAll');
    if (clearAllBtn) {
        clearAllBtn.addEventListener('click', () => {
            if (barcodes.length === 0) {
                Swal.fire('خالی است', 'هیچ بارکدی برای حذف وجود ندارد.', 'info');
                return;
            }

            Swal.fire({
                title: 'پاکسازی همه',
                text: 'همه بارکدها حذف شوند؟',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'بله',
                cancelButtonText: 'خیر'
            }).then(result => {
                if (result.isConfirmed) {
                    barcodes = [];
                    saveToLocalStorage();
                    updateBarcodeList();
                    Swal.fire('پاک شد!', '', 'success');
                }
            });
        });
    }

    var searchInput = document.getElementById('SearchBarcode');
    if (searchInput) {
        searchInput.addEventListener('input', function () {
            updateBarcodeList(this.value.trim());
        });
    }

    var processExcelBtn = document.getElementById('ProcessExcel');
    if (processExcelBtn) {
        processExcelBtn.addEventListener('click', function () {
            var fileInput = document.getElementById('ExcelFileUpload');
            if (!fileInput.files.length) {
                Swal.fire('خطا', 'فایلی انتخاب نشده است.', 'error');
                return;
            }

            var file = fileInput.files[0];
            var reader = new FileReader();

            reader.onload = function (e) {
                var data = new Uint8Array(e.target.result);
                var workbook = XLSX.read(data, { type: 'array' });

                var firstSheet = workbook.Sheets[workbook.SheetNames[0]];
                var rows = XLSX.utils.sheet_to_json(firstSheet, { header: 1 });

                var newBarcodes = [];
                rows.forEach((row, index) => {
                    if (index > 0 && row[0]) {
                        var barcode = row[0].toString().trim();
                        if (barcode && !barcodes.includes(barcode)) {
                            newBarcodes.push(barcode);
                        }
                    }
                });

                if (newBarcodes.length === 0) {
                    Swal.fire('خالی', 'هیچ بارکد جدیدی در فایل یافت نشد.', 'info');
                    return;
                }

                barcodes = barcodes.concat(newBarcodes);
                saveToLocalStorage();
                updateBarcodeList();
                fileInput.value = '';
                Swal.fire('موفقیت', `${newBarcodes.length} بارکد جدید اضافه شد.`, 'success');
            };

            reader.onerror = function () {
                Swal.fire('خطا', 'خطا در خواندن فایل.', 'error');
            };

            reader.readAsArrayBuffer(file);
        });
    }

    var sendBtn = document.getElementById('sendBarcodes');
    if (sendBtn) {
        sendBtn.addEventListener('click', function () {
            var complaintNumber = document.getElementById('ComplaintNumber').value.trim();
            var entryDate = document.getElementById('EntryDate').value.trim();
            if (barcodes.length === 0 || !complaintNumber || !entryDate) {
                Swal.fire('خطا!', 'همه فیلدها را پر کنید و بارکدها را وارد کنید.', 'error');
                return;
            }
            sendBtn.disabled = true;
            sendBtn.innerText = 'در حال ارسال...';
            Swal.fire({
                title: 'در حال ارسال بارکدها...',
                html: `<div class="progress" style="height: 20px;">
                       <div id="progressBar" class="progress-bar bg-info" role="progressbar" style="width: 0%;" aria-valuenow="0" aria-valuemin="0" aria-valuemax="100">0%</div>
                   </div>
                   <p id="progressText">در حال بررسی اطلاعات...</p>`,
                allowOutsideClick: false,
                showConfirmButton: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });
            var dataToCheck = {
                ComplaintNumber: complaintNumber,
                EntryDate: entryDate,
                Barcodes: barcodes
            };
            let progress = 0;
            const progressInterval = setInterval(() => {
                progress = Math.min(progress + 10, 50);
                document.getElementById('progressBar').style.width = `${progress}%`;
                document.getElementById('progressBar').innerText = `${progress}%`;
                document.getElementById('progressText').innerText = progress < 50 ? 'در حال بررسی اطلاعات...' : 'در حال ارسال به سرور...';
            }, 500);
            fetch('/FaultyMeter/CheckComplaintOrSerial', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': getAntiForgeryToken(),
                    'Accept': 'application/json'
                },
                body: JSON.stringify(dataToCheck)
            })
                .then(res => res.text())
                .then(text => {
                    console.log('Check Response Text:', text);
                    try {
                        var result = JSON.parse(text);
                        clearInterval(progressInterval);
                        if (result.complaintExists && result.serialExists && result.allSerialsExist) {
                            Swal.fire('شماره شکایت تکراری!', 'تمام بارکدها قبلاً با این شکایت ثبت شده‌اند.', 'error');
                            sendBtn.disabled = false;
                            sendBtn.innerText = '🚀 ارسال بارکدها به سرور';
                            return;
                        }
                        if (result.serialExists && result.existingSerials.length > 0) {
                            let msg = result.existingSerials.join('<br>');
                            Swal.fire('بارکدهای تکراری یافت شد!', `بارکدهای زیر قبلاً ثبت شده‌اند:<br>${msg}`, 'error');
                            sendBtn.disabled = false;
                            sendBtn.innerText = '🚀 ارسال بارکدها به سرور';
                            return;
                        }
                        // Proceed to Submit
                        const finalProgressInterval = setInterval(() => {
                            progress = Math.min(progress + 10, 100);
                            document.getElementById('progressBar').style.width = `${progress}%`;
                            document.getElementById('progressBar').innerText = `${progress}%`;
                            document.getElementById('progressText').innerText = 'در حال ارسال به سرور...';
                            if (progress >= 100) clearInterval(finalProgressInterval);
                        }, 500);
                        fetch('/FaultyMeter/SubmitBarcodes', {
                            method: 'POST',
                            headers: {
                                'Content-Type': 'application/json',
                                'RequestVerificationToken': getAntiForgeryToken(),
                                'Accept': 'application/json'
                            },
                            body: JSON.stringify(dataToCheck)
                        })
                            .then(res2 => {
                                console.log('Submit Response Status:', res2.status);  // لاگ status
                                clearInterval(finalProgressInterval);
                                document.getElementById('progressBar').style.width = '100%';
                                document.getElementById('progressBar').innerText = '100%';
                                document.getElementById('progressText').innerText = 'ارسال تکمیل شد!';
                                if (!res2.ok) {
                                    return res2.text().then(text2 => {
                                        console.error('Submit Error Status:', res2.status, 'Body:', text2);
                                        Swal.fire('خطا!', `کد: ${res2.status} - جزئیات: ${text2.substring(0, 300)}...`, 'error');
                                    });
                                }
                                return res2.text().then(text2 => {
                                    console.log('Submit Success Text:', text2);
                                    try {
                                        var result2 = JSON.parse(text2);
                                        Swal.fire('موفقیت', 'بارکدها ثبت شدند.', 'success');
                                        barcodes = [];
                                        saveToLocalStorage();
                                        updateBarcodeList();
                                        document.getElementById('ComplaintNumber').value = '';
                                        document.getElementById('EntryDate').value = '';
                                    } catch (e) {
                                        console.error('Parse Error in Submit:', e, text2);
                                        Swal.fire('هشدار', 'پاسخ سرور معتبر نیست، اما ثبت شد: ' + text2.substring(0, 200), 'warning');
                                    }
                                });
                            })
                            .catch(err2 => {
                                console.error('Submit Fetch Error:', err2);
                                Swal.fire('خطا', 'مشکل در ارسال: ' + err2.message, 'error');
                            });
                    } catch (e) {
                        console.error('Parse Error in Check:', e, text);
                        clearInterval(progressInterval);
                        Swal.fire('خطا', 'پاسخ بررسی نامعتبر: ' + text.substring(0, 200), 'error');
                    }
                })
                .catch(err => {
                    clearInterval(progressInterval);
                    console.error('Check Fetch Error:', err);
                    Swal.fire('خطا', 'مشکل در بررسی: ' + err.message, 'error');
                })
                .finally(() => {
                    sendBtn.disabled = false;
                    sendBtn.innerText = '🚀 ارسال بارکدها به سرور';
                });
        });
    }
});
