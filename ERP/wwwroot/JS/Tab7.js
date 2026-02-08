function load_tab7() {
    // Chart 1: Pie Chart - توزیع نوع مشکل در کنتورهای برگشتی
    $.getJSON('/Home/Data_FaultyMeterIssueTypes', function (data) {
        if (!data || data.length === 0) {
            console.warn("داده‌ای برای کنتورهای برگشتی وجود ندارد");
            return;
        }
        Highcharts.chart('faulty-meter-issue-pie', {
            chart: { type: 'pie' },
            title: { text: 'توزیع نوع مشکل در کنتورها' },
            subtitle: { text: 'بر اساس IssueType' },
            tooltip: { pointFormat: '{series.name}: <b>{point.y} مورد</b> ({point.percentage:.1f}%)' },
            plotOptions: {
                pie: {
                    allowPointSelect: true,
                    cursor: 'pointer',
                    dataLabels: { enabled: true, format: '<b>{point.name}</b>: {point.y} مورد' }
                }
            },
            series: [{ name: 'تعداد', colorByPoint: true, data: data }]
        });
    }).fail(function (jqXHR, textStatus, errorThrown) {
        console.error("خطا در دریافت داده‌های IssueType کنتور:", textStatus, errorThrown);
    });

    // Chart 2: Bar Chart - تعداد فیوزهای برگشتی به تفکیک محصول
    $.getJSON('/Home/Data_FaultyFuseByProduct', function (data) {
        console.log("Raw data from server:", data);
        if (!data || data.length === 0) {
            console.warn("داده‌ای برای فیوزهای برگشتی وجود ندارد");
            data = [{ ProductName: "تست فیوز", Quantity: 10 }];
            console.log("Using dummy data for testing");
        }
        data.sort(function (a, b) {
            const qa = parseInt(a.Quantity) || 0;
            const qb = parseInt(b.Quantity) || 0;
            return qb - qa;
        });
        const names = data.map(x => x.ProductName || 'نامشخص');
        const quantities = data.map(x => parseInt(x.Quantity) || 0);
        console.log("Processed names:", names);
        console.log("Processed quantities:", quantities);
        Highcharts.chart('faulty-fuse-product-bar', {
            chart: { type: 'bar' },
            title: { text: 'تعداد فیوزهای برگشتی به تفکیک محصول' },
            xAxis: {
                categories: names,
                title: { text: 'نام محصول' },
                labels: { rotation: -45, style: { fontSize: '12px' } }
            },
            yAxis: { min: 0, title: { text: 'تعداد' }, allowDecimals: false },
            legend: { enabled: false },
            tooltip: { pointFormat: '<b>{point.y} عدد</b>' },
            plotOptions: {
                bar: {
                    dataLabels: { enabled: true, style: { fontWeight: 'bold', color: '#000' } }
                }
            },
            series: [{ name: 'تعداد فیوز', data: quantities, color: '#7cb5ec' }],
            credits: { enabled: false }
        });
    }).fail(function (jqXHR, textStatus, errorThrown) {
        console.error("خطا در دریافت داده‌های فیوز به تفکیک محصول:", textStatus, errorThrown);
        console.error("Response:", jqXHR.responseText);
        Highcharts.chart('faulty-fuse-product-bar', {
            chart: { type: 'bar' },
            title: { text: 'خطا در بارگذاری داده‌ها' },
            xAxis: { categories: ['خطا'] },
            series: [{ data: [0] }]
        });
    });

    // Chart 3: Line Chart - روند برگشتی کنتورها بر اساس تاریخ ورودی
    $.getJSON('/Home/Data_FaultyMeterByDate', function (data) {
        if (!data || data.length === 0) {
            console.warn("داده‌ای برای روند کنتورها وجود ندارد");
            return;
        }
        data.sort(function (a, b) { return new Date(a.EntryDate) - new Date(b.EntryDate); });
        const dates = data.map(x => x.EntryDate);
        const counts = data.map(x => x.Count || 1);
        Highcharts.chart('faulty-meter-date-line', {
            chart: { type: 'line' },
            title: { text: 'روند برگشتی کنتورها در طول زمان' },
            xAxis: {
                categories: dates,
                title: { text: 'تاریخ ورودی' },
                labels: { rotation: -45 }
            },
            yAxis: { title: { text: 'تعداد برگشتی' }, min: 0 },
            tooltip: { pointFormat: '<b>{point.y} مورد</b> در {point.x}' },
            plotOptions: {
                line: {
                    dataLabels: { enabled: true, style: { fontWeight: 'bold' } },
                    marker: { enabled: true }
                }
            },
            series: [{ name: 'تعداد', data: counts, color: '#2f7ed8' }],
            credits: { enabled: false }
        });
    }).fail(function (jqXHR, textStatus, errorThrown) {
        console.error("خطا در دریافت داده‌های روند کنتور:", textStatus, errorThrown);
    });

    // Chart 4: Column Chart - مقایسه تعداد برگشتی‌ها: کنتور vs فیوز
    Promise.all([
        $.getJSON('/Home/Data_FaultyMeterTotalCount'),
        $.getJSON('/Home/Data_FaultyFuseTotalCount')
    ]).then(([meterData, fuseData]) => {
        const meterCount = meterData.total || 0;
        const fuseCount = fuseData.total || 0;
        Highcharts.chart('faulty-comparison-column', {
            chart: { type: 'column' },
            title: { text: 'مقایسه تعداد کل برگشتی‌ها' },
            xAxis: { categories: ['کنتور', 'فیوز'], title: { text: 'نوع محصول' } },
            yAxis: { min: 0, title: { text: 'تعداد کل' }, allowDecimals: false },
            legend: { enabled: false },
            tooltip: { pointFormat: '<b>{point.y} مورد</b>' },
            plotOptions: {
                column: {
                    dataLabels: { enabled: true, style: { fontWeight: 'bold', color: '#000' } }
                }
            },
            series: [{
                name: 'تعداد برگشتی',
                data: [meterCount, fuseCount],
                colorByPoint: true
            }],
            credits: { enabled: false }
        });
    }).catch(function (error) {
        console.error("خطا در دریافت داده‌های مقایسه:", error);
    });

    // Chart 5: Pie Chart - توزیع نوع اشکال در فیوزهای برگشتی
    $.getJSON('/Home/Data_FaultyFuseIssueTypes', function (data) {
        if (!data || data.length === 0) {
            console.warn("داده‌ای برای اشکالات فیوز وجود ندارد");
            return;
        }
        Highcharts.chart('faulty-fuse-issue-pie', {
            chart: { type: 'pie' },
            title: { text: 'توزیع نوع اشکال در فیوزها' },
            subtitle: { text: 'بر اساس IssueType' },
            tooltip: { pointFormat: '{series.name}: <b>{point.y} مورد</b> ({point.percentage:.1f}%)' },
            plotOptions: {
                pie: {
                    allowPointSelect: true,
                    cursor: 'pointer',
                    dataLabels: { enabled: true, format: '<b>{point.name}</b>: {point.y} مورد' }
                }
            },
            series: [{ name: 'تعداد', colorByPoint: true, data: data }],
            credits: { enabled: false }
        });
    }).fail(function (jqXHR, textStatus, errorThrown) {
        console.error("خطا در دریافت داده‌های IssueType فیوز:", textStatus, errorThrown);
    });

    // Chart 6: Bar Chart - تعداد کنتورهای برگشتی به تفکیک نوع محصول
    $.getJSON('/Home/Data_FaultyMeterByProductType', function (data) {
        console.log("Raw data for meter by product:", data);
        if (!data || data.length === 0) {
            console.warn("داده‌ای برای کنتورهای برگشتی به تفکیک محصول وجود ندارد");
            data = [{ ProductType: "تست نوع A", Count: 15 }];
            console.log("Using dummy data for meter product chart");
        }
        data.sort(function (a, b) {
            const ca = parseInt(a.Count) || 0;
            const cb = parseInt(b.Count) || 0;
            return cb - ca;
        });
        const names = data.map(x => x.ProductType || 'نامشخص');
        const counts = data.map(x => parseInt(x.Count) || 0);
        console.log("Processed names for meter:", names);
        console.log("Processed counts for meter:", counts);
        Highcharts.chart('faulty-meter-product-bar', {
            chart: { type: 'bar' },
            title: { text: 'تعداد کنتورهای برگشتی به تفکیک نوع محصول' },
            xAxis: {
                categories: names,
                title: { text: 'نوع محصول' },
                labels: { rotation: -45, style: { fontSize: '12px' } }
            },
            yAxis: { min: 0, title: { text: 'تعداد' }, allowDecimals: false },
            legend: { enabled: false },
            tooltip: { pointFormat: '<b>{point.y} عدد</b>' },
            plotOptions: {
                bar: {
                    dataLabels: { enabled: true, style: { fontWeight: 'bold', color: '#000' } }
                }
            },
            series: [{ name: 'تعداد کنتور', data: counts, color: '#90EE90' }],
            credits: { enabled: false }
        });
    }).fail(function (jqXHR, textStatus, errorThrown) {
        console.error("خطا در دریافت داده‌های کنتور به تفکیک محصول:", textStatus, errorThrown);
        console.error("Response:", jqXHR.responseText);
        Highcharts.chart('faulty-meter-product-bar', {
            chart: { type: 'bar' },
            title: { text: 'خطا در بارگذاری داده‌ها' },
            xAxis: { categories: ['خطا'] },
            series: [{ data: [0] }]
        });
    });

    // Chart 7: Stacked Bar Chart - تعداد کنتورها بر اساس نوع محصول و خرابی
    $.getJSON('/Home/Data_FaultyMeterByProductAndIssue', function (data) {
        console.log("Raw data for meter product-issue:", data);
        if (!data || !data.categories || !data.series || data.series.length === 0) {
            console.warn("داده‌ای برای کنتورها به تفکیک محصول و خرابی وجود ندارد");
            data = {
                categories: ['تست نوع A'],
                series: [{ name: 'خرابی تست', data: [5] }]
            };
            console.log("Using dummy data for meter product-issue chart");
        }
        Highcharts.chart('meter-product-issue-stacked-bar', {
            chart: { type: 'bar' },
            title: { text: 'تعداد کنتورها بر اساس نوع محصول و خرابی' },
            xAxis: {
                categories: data.categories,
                title: { text: 'نوع محصول' },
                labels: { rotation: -45, style: { fontSize: '12px' } }
            },
            yAxis: { min: 0, title: { text: 'تعداد' }, allowDecimals: false },
            legend: { reversed: true },
            tooltip: { pointFormat: '<b>{point.y} عدد</b> ({series.name})' },
            plotOptions: {
                series: { stacking: 'normal' },
                bar: {
                    dataLabels: { enabled: true, style: { fontWeight: 'bold', color: '#000' } }
                }
            },
            series: data.series,
            credits: { enabled: false }
        });
    }).fail(function (jqXHR, textStatus, errorThrown) {
        console.error("خطا در دریافت داده‌های کنتور به تفکیک محصول و خرابی:", textStatus, errorThrown);
        console.error("Response:", jqXHR.responseText);
        Highcharts.chart('meter-product-issue-stacked-bar', {
            chart: { type: 'bar' },
            title: { text: 'خطا در بارگذاری داده‌ها' },
            xAxis: { categories: ['خطا'] },
            series: [{ data: [0] }]
        });
    });

    // Chart 8: Stacked Bar Chart - تعداد فیوزها بر اساس محصول و نوع خرابی
    $.getJSON('/Home/Data_FaultyFuseByProductAndIssue', function (data) {
        console.log("Raw data for fuse product-issue:", data);
        if (!data || !data.categories || !data.series || data.series.length === 0) {
            console.warn("داده‌ای برای فیوزها به تفکیک محصول و خرابی وجود ندارد");
            data = {
                categories: ['تست فیوز A'],
                series: [{ name: 'خرابی تست', data: [7] }]
            };
            console.log("Using dummy data for fuse product-issue chart");
        }
        Highcharts.chart('fuse-product-issue-stacked-bar', {
            chart: { type: 'bar' },
            title: { text: 'تعداد فیوزها بر اساس محصول و نوع خرابی' },
            xAxis: {
                categories: data.categories,
                title: { text: 'نام محصول' },
                labels: { rotation: -45, style: { fontSize: '12px' } }
            },
            yAxis: { min: 0, title: { text: 'تعداد' }, allowDecimals: false },
            legend: { reversed: true },
            tooltip: { pointFormat: '<b>{point.y} عدد</b> ({series.name})' },
            plotOptions: {
                series: { stacking: 'normal' },
                bar: {
                    dataLabels: { enabled: true, style: { fontWeight: 'bold', color: '#000' } }
                }
            },
            series: data.series,
            credits: { enabled: false }
        });
    }).fail(function (jqXHR, textStatus, errorThrown) {
        console.error("خطا در دریافت داده‌های فیوز به تفکیک محصول و خرابی:", textStatus, errorThrown);
        console.error("Response:", jqXHR.responseText);
        Highcharts.chart('fuse-product-issue-stacked-bar', {
            chart: { type: 'bar' },
            title: { text: 'خطا در بارگذاری داده‌ها' },
            xAxis: { categories: ['خطا'] },
            series: [{ data: [0] }]
        });
    });
}