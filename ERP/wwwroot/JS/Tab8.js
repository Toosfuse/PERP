function load_tab8() {
    $.getJSON('/Home/Data_TenderWinners', function (result) {
        Highcharts.chart('tender-winner-pie', {
            chart: { type: 'pie' },
            title: { text: 'سهم برنده‌های مناقصات' },
            subtitle: { text: 'تعداد کل مناقصات: ' + result.total },
            tooltip: { pointFormat: '{series.name}: <b>{point.y} بار</b> ({point.percentage:.1f}%)' },
            plotOptions: {
                pie: {
                    allowPointSelect: true,
                    cursor: 'pointer',
                    dataLabels: { enabled: true, format: '<b>{point.name}</b>: {point.y} بار' }
                }
            },
            series: [{ name: 'تعداد برد', colorByPoint: true, data: result.data }]
        });
    });

    $.getJSON('/Home/Data_InqueryWinners', function (result) {
        Highcharts.chart('inquery-winner-pie', {
            chart: { type: 'pie' },
            title: { text: 'سهم برنده‌های استعلامات' },
            subtitle: { text: 'تعداد کل استعلامات: ' + result.total },
            tooltip: { pointFormat: '{series.name}: <b>{point.y} بار</b> ({point.percentage:.1f}%)' },
            plotOptions: {
                pie: {
                    allowPointSelect: true,
                    cursor: 'pointer',
                    dataLabels: { enabled: true, format: '<b>{point.name}</b>: {point.y} بار' }
                }
            },
            series: [{ name: 'تعداد برد', colorByPoint: true, data: result.data }]
        });
    });
}