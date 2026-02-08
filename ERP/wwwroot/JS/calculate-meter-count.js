// محاسبه خودکار تعداد سفارش هنگام تغییر سریال‌ها
function calculateMeterCount() {
    const startSerial = document.querySelector("input[name='StartSerial']")?.value.trim() || "";
    const endSerial = document.querySelector("input[name='EndSerial']")?.value.trim() || "";
    const display = document.getElementById("MeterCountDisplay");

    if (!display) return;

    if (startSerial && endSerial) {
        const start = parseInt(startSerial, 10);
        const end = parseInt(endSerial, 10);

        if (!isNaN(start) && !isNaN(end) && end >= start) {
            const count = end - start + 1;
            // فرمت کردن عدد به صورت separated
            display.value = count.toLocaleString('fa-IR');
            display.style.color = "#27F554";
        } else if (!isNaN(start) && !isNaN(end) && start > end) {
            display.value = "Eroor";
            display.style.color = "#F52727";
        } else {
            display.value = "0";
            display.style.color = "#27F554";
        }
    } else {
        display.value = "0";
        display.style.color = "#27F554";
    }
}

// Event listeners
document.addEventListener("DOMContentLoaded", function() {
    const startInput = document.querySelector("input[name='StartSerial']");
    const endInput = document.querySelector("input[name='EndSerial']");

    if (startInput) startInput.addEventListener("input", calculateMeterCount);
    if (endInput) endInput.addEventListener("input", calculateMeterCount);

    // محاسبه اولیه
    setTimeout(calculateMeterCount, 300);
});
