function load_tab6() {


    jalaliDatepicker.startWatch();

    // محتوای کامل تب
    const tabContent = `
        <div class="container-fluid py-4">
            <!-- فیلتر تاریخ -->
            <div class="row mb-4 align-items-end">
                <div class="col-md-5">
                    <label class="form-label fw-bold">از تاریخ:</label>
                    <input type="text" id="voipFromDate" class="form-control form-control-lg" data-jdp autocomplete="off"/>
                </div>
                <div class="col-md-5">
                    <label class="form-label fw-bold">تا تاریخ:</label>
                    <input type="text" id="voipToDate" class="form-control form-control-lg" data-jdp autocomplete="off" />
                </div>
                <div class="col-md-2">
                    <button id="voipRefresh" class="btn btn-success btn-lg w-100 shadow">
                        <i class="fas fa-sync-alt"></i> اعمال فیلتر
                    </button>
                </div>
            </div>

            <!-- لیست داخلی‌ها -->
            <div class="card shadow-lg mb-5 border-0">
                <div class="card-header bg-gradient-primary text-white">
                    <h4 class="mb-0">لیست داخلی‌ها و آمار تماس</h4>
                </div>
                <div class="card-body p-0">
                    <div id="voipGrid"></div>
                </div>
            </div>

            <!-- داشبورد چارت‌ها -->
            <div class="row g-4">
                <div class="col-lg-8">
                    <div class="card shadow-lg border-0">
                        <div class="card-header bg-primary text-white">
                            <h5 class="mb-0">پیک ساعتی تماس‌ها (تمام سازمان)</h5>
                        </div>
                        <div class="card-body">
                            <div id="voipHourlyChart" style="height:420px;"></div>
                        </div>
                    </div>
                </div>
                <div class="col-lg-4">
                    <div class="card shadow-lg border-0">
                        <div class="card-header bg-warning text-dark">
                            <h5 class="mb-0">۱۰ داخلی با بیشترین تماس از دست رفته</h5>
                        </div>
                        <div class="card-body">
                            <div id="voipMissedChart" style="height:420px;"></div>
                        </div>
                    </div>
                </div>
            </div>

            <div class="row mt-4">
                <div class="col-12">
                    <div class="card shadow-lg border-0">
                        <div class="card-header bg-success text-white">
                            <h5 class="mb-0">۱۰ داخلی پرتماس (ورودی + خروجی)</h5>
                        </div>
                        <div class="card-body">
                            <div id="voipTop10Chart" style="height:480px;"></div>
                        </div>
                    </div>
                </div>
            </div>

            <!-- پنجره جزئیات -->
            <div id="voipDetailsWindow"></div>
        </div>
    `;

    $("#tab6").html(tabContent);


    // ساخت گرید
    $("#voipGrid").kendoGrid({
        dataSource: {
            transport: {
                read: {
                    url: "/Issable/GetExtensions",
                    data: () => ({ from: $("#voipFromDate").val(), to: $("#voipToDate").val() })
                }
            },
            schema: { data: "Data", total: "Total" },
            pageSize: 10
        },
        height: 600,
        sortable: true,
        pageable: { pageSizes: [10, 25, 50, 100], refresh: true },
        scrollable: true,
        columns: [
            { field: "Extension", title: "داخلی", width: 130, attributes: { class: "text-center fw-bold text-center" } },
            { field: "Name", title: "نام داخلی", width: 220, attributes: { class: "text-center" } },
            { field: "TotalCalls", title: "تعداد تماس", width: 150, attributes: { class: "text-center" } },
            { field: "TotalDuration", title: "مجموع مدت", format: "{0:hh\\:mm\\:ss}", width: 160, attributes: { class: "text-center" } },
            { template: "<button class='btn btn-info btn-sm shadow'><i class='fas fa-chart-bar'></i> جزئیات</button>", width: 140, attributes: { class: "text-center" } }
        ]
    });

    // کلیک روی دکمه جزئیات
    $("#voipGrid").on("click", "button", function () {
        const grid = $("#voipGrid").data("kendoGrid");
        const dataItem = grid.dataItem($(this).closest("tr"));
        showVoipDetails(dataItem.Extension, dataItem.Name || "نامشخص");
    });

    // دکمه فیلتر
    $("#voipRefresh").click(() => {
        $("#voipGrid").data("kendoGrid").dataSource.read();
        loadVoipCharts();
    });

    // لود اولیه چارت‌ها
    loadVoipCharts();

    function loadVoipCharts() {
        const from = $("#voipFromDate").val();
        const to = $("#voipToDate").val();

        // پیک ساعتی
        $.get("/Issable/HourlyPeak", { from, to }, data => {
            const hours = data.map(x => x.Hour + ":00");
            const counts = data.map(x => x.Count);
            Highcharts.chart('voipHourlyChart', {
                chart: { type: 'column', backgroundColor: '#f8f9fa' },
                title: { text: 'توزیع تماس‌ها در ساعات روز' },
                xAxis: { categories: hours, crosshair: true },
                yAxis: { min: 0, title: { text: 'تعداد تماس' } },
                tooltip: { headerFormat: '<span style="font-size:10px">{point.key}</span><table>', pointFormat: '<tr><td style="color:{series.color}">{series.name}: </td><td style="text-align:right"><b>{point.y}</b></td></tr>', footerFormat: '</table>' },
                plotOptions: { column: { pointPadding: 0.2, borderWidth: 0, dataLabels: { enabled: true, format: '{y}' } } },
                credits: { enabled: false },
                series: [{ name: 'تعداد تماس', data: counts, color: '#007bff' }]
            });
        });

        // تماس‌های از دست رفته
        $.get("/Issable/MissedCalls", { from, to }, data => {
            Highcharts.chart('voipMissedChart', {
                chart: { type: 'bar', backgroundColor: '#fff3cd' },
                title: { text: 'تماس‌های از دست رفته' },
                xAxis: { categories: data.map(x => x.Extension), title: { text: null } },
                yAxis: { min: 0, title: { text: 'تعداد' } },
                plotOptions: { bar: { dataLabels: { enabled: true } } },
                credits: { enabled: false },
                series: [{ name: 'از دست رفته', data: data.map(x => x.Missed), color: '#ffc107' }]
            });
        });

        // Top 10 پرتماس
        $.get("/Issable/Top10Overall", { from, to }, data => {
            Highcharts.chart('voipTop10Chart', {
                chart: { type: 'bar', backgroundColor: '#d4edda' },
                title: { text: '۱۰ داخلی پرتماس (ورودی + خروجی)' },
                xAxis: { categories: data.map(x => x.Extension), title: { text: null } },
                yAxis: { min: 0, title: { text: 'تعداد کل تماس' } },
                plotOptions: { bar: { dataLabels: { enabled: true } } },
                credits: { enabled: false },
                series: [{ name: 'کل تماس', data: data.map(x => x.Total), color: '#28a745' }]
            });
        });
    }

    let detailsWindow = null; // یه بار تعریف می‌کنیم

    function showVoipDetails(ext, name) {
        // اگر پنجره وجود نداره، بسازش
        if (!detailsWindow) {
            $("#voipDetailsWindow").kendoWindow({
                width: 1200,
                height: 720,
                title: "در حال بارگذاری...",
                modal: true,
                visible: false,
                close: function () {
                    // وقتی بسته شد، محتوا رو پاک کن تا دفعه بعد درست کار کنه
                    this.wrapper.find(".k-window-content").empty();
                }
            });
            detailsWindow = $("#voipDetailsWindow").data("kendoWindow");
        }

        // عنوان رو آپدیت کن
        detailsWindow.title(`آمار تماس‌های ${name} — داخلی ${ext}`);

        // محتوای جدید با id منحصر به فرد
        const content = `
        <div class="p-4">
            <div class="row">
                <div class="col-md-12">
                    <h4 class="text-center text-primary fw-bold mb-4">۱۰ نفر اول که به داخلی ${ext} زنگ زدن (ورودی)</h4>
                    <div id="incomingChart_${ext}" style="height:500px;"></div>
                </div>
                <div class="col-md-12">
                    <h4 class="text-center text-danger fw-bold mb-4">۱۰ نفر اول که داخلی ${ext} بهشون زنگ زده (خروجی)</h4>
                    <div id="outgoingChart_${ext}" style="height:500px;"></div>
                </div>
            </div>
        </div>
    `;

        detailsWindow.content(content);
        detailsWindow.center();
        detailsWindow.open();

        // پاک کردن چارت‌های قبلی (مهم!)
        $(`#incomingChart_${ext}, #outgoingChart_${ext}`).empty();

        // گرفتن داده و کشیدن چارت
        $.get("/Issable/GetCallStats", {
            ext: ext,
            from: $("#voipFromDate").val(),
            to: $("#voipToDate").val()
        }, function (data) {
            Highcharts.chart(`incomingChart_${ext}`, {
                chart: { type: 'bar' },
                title: { text: '' },
                xAxis: { categories: data.incoming.map(x => x.Number + " (" + x.Count + ")") },
                yAxis: { min: 0, title: { text: 'تعداد تماس' } },
                tooltip: { pointFormat: '<b>{point.y}</b> تماس' },
                plotOptions: { bar: { dataLabels: { enabled: true, format: '{y}' } } },
                legend: { enabled: false },
                credits: { enabled: false },
                series: [{ name: 'ورودی', data: data.incoming.map(x => x.Count), color: '#3498db' }]
            });

            Highcharts.chart(`outgoingChart_${ext}`, {
                chart: { type: 'bar' },
                title: { text: '' },
                xAxis: { categories: data.outgoing.map(x => x.Number + " (" + x.Count + ")") },
                yAxis: { min: 0, title: { text: 'تعداد تماس' } },
                tooltip: { pointFormat: '<b>{point.y}</b> تماس' },
                plotOptions: { bar: { dataLabels: { enabled: true, format: '{y}' } } },
                legend: { enabled: false },
                credits: { enabled: false },
                series: [{ name: 'خروجی', data: data.outgoing.map(x => x.Count), color: '#e74c3c' }]
            });
        }).fail(() => {
            detailsWindow.content("<h3 class='text-danger text-center p-5'>خطا در دریافت آمار</h3>");
        });
    }
}