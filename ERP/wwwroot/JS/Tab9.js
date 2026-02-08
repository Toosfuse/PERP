function load_tab9() {

    $.ajax({
        url: '/GetDataPouya/Data_SaleTotalTargetProgress_Meter',
        type: 'GET',
        success: function (response) {

            var percent = parseFloat(response.TotalProgress) || 0;
            var actual = parseInt(response.ActualMeter) || 0;
            var target = parseInt(response.TotalTargetMeter) || 0;

            Highcharts.chart('sale_chart_total_meter', {

                chart: {
                    type: 'solidgauge',
                    height: 350,
                    backgroundColor: '#f8f9fa'
                },

                title: {
                    text: 'درصد تحقق فروش کنتور',
                    style: { fontSize: '18px', fontWeight: 'bold' }
                },

                pane: {
                    startAngle: -90,
                    endAngle: 90,
                    background: [{
                        outerRadius: '100%',
                        innerRadius: '60%',
                        backgroundColor: '#eee',
                        borderWidth: 0
                    }]
                },

                tooltip: { enabled: false },

                yAxis: {
                    min: 0,
                    max: 100,
                    stops: [
                        [0.4, '#dc3545'],   // قرمز
                        [0.7, '#ffc107'],   // زرد
                        [1.0, '#28a745']    // سبز
                    ],
                    lineWidth: 0,
                    tickWidth: 0,
                    minorTickInterval: null,
                    labels: { enabled: false },
                },

                plotOptions: {
                    solidgauge: {
                        dataLabels: {
                            y: -20,
                            borderWidth: 0,
                            useHTML: true,
                            format: `
                                <div style="text-align:center">
                                    <span style="font-size:28px; font-weight:bold; color:#000">{y:.1f}%</span><br/>
                                    <span style="font-size:14px; color:#555">واقعی: ${actual.toLocaleString()}</span><br/>
                                    <span style="font-size:14px; color:#555">تارگت: ${target.toLocaleString()}</span>
                                </div>
                            `
                        },
                        linecap: 'round',
                        stickyTracking: false
                    }
                },

                series: [{
                    name: 'درصد تحقق',
                    data: [percent],
                    radius: '100%',
                    innerRadius: '60%',
                    color: '#28a745'
                }],

                credits: { enabled: false },
                exporting: { enabled: false }
            });
        },
        error: function () {
            alert("خطا در دریافت داده فروش!");
        }
    });




    $.ajax({
        url: '/GetDataPouya/GetChartData',
        type: 'GET',
        async: true,
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (response) {
            visitorData1(response);
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.error("خطا در دریافت داده‌های فروش:", textStatus, errorThrown);
            $('#sync-result').html('<p class="alert alert-danger">خطا در لود چارت.</p>');
        }
    });
    function visitorData1(a) {  // a = response
        if (a.Success && a.Data && a.Data.length > 0) {
            // data رو به array [name, y] تبدیل کن (مثل کد کاربر)
            var chartData = a.Data.map(d => [d.name, d.y]);

            Highcharts.chart('sync-result', {
                chart: { type: 'bar' },
                title: { text: 'فروش به تفکیک محصول' },
                xAxis: {
                    type: 'category',
                    labels: { style: { fontSize: '13px' } },
                    reversed: true
                },
                credits: { enabled: false },
                yAxis: {
                    min: 0,
                    title: { text: 'مجموع مقدار فروش' },
                    allowDecimals: false
                },
                legend: { enabled: false },
                tooltip: { pointFormat: '<b>{point.y:.0f}</b>' },
                plotOptions: {
                    column: {
                        cursor: 'pointer',
                        point: {
                            events: {
                                click: function () {
                                    window.location = '/SaleInvoiceHeaders/Details/' + this.name;  // redirect به جزئیات کالا
                                }
                            }
                        }
                    }
                },
                series: [{
                    name: 'مجموع فروش',
                    data: chartData,  // array [name, y]
                    colorByPoint: true,
                    groupPadding: 0,
                    dataLabels: {
                        enabled: true,
                        rotation: 0,
                        color: '#000000',
                        align: 'center',
                        format: '{point.y:.0f}',
                        y: 0,
                        style: { fontSize: '13px' }
                    }
                }]
            });
        } else {
            var errorMsg = a.Error || 'داده‌ای موجود نیست.';
            $('#chart-container').html('<p class="alert alert-danger">' + errorMsg + '</p>');
        }
    }

    $.ajax({
        url: '/GetDataPouya/Data_SaleDemandTrend_Meter',
        type: 'GET',
        async: true,
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (response) {
            visitorDataTrend(response);
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.error("خطا در دریافت داده‌های روند فروش:", textStatus, errorThrown);
        }
    });
    function visitorDataTrend(a) {
        Highcharts.chart('sale_chartTrend_Meter', {
            chart: { type: 'column' },
            title: { text: 'روند فروش کنتور در ماه' },
            xAxis: {
                type: 'category',
                labels: { rotation: -45, style: { fontSize: '13px' } }
            },
            credits: { enabled: false },
            yAxis: { min: 0, title: { text: 'مجموع تعداد' }, allowDecimals: false },
            legend: { enabled: false },
            tooltip: { pointFormat: '<b>{point.y:.0f}</b> واحد در {point.name}' },
            plotOptions: {
                column: {
                    grouping: false,
                    pointPadding: 0.1
                },
                series: {
                    dataLabels: {
                        enabled: true,
                        format: '{y:.0f}',
                        y: -10,
                        style: { fontSize: '12px', fontFamily: 'shabnam', textOutline: 'none' },
                        align: 'center'
                    }
                }
            },
            series: [{
                name: 'میله‌ها (مساحت پایه)',
                type: 'column',
                data: a,
                color: '#e3f2fd',
                borderColor: '#007bff',
                borderWidth: 1
            }, {
                name: 'خط روند',
                type: 'line',
                data: a,
                color: '#007bff',
                marker: { enabled: true },
                lineWidth: 2,
                dataLabels: {
                    enabled: true,
                    y: -15,
                    format: '{y:.0f}'
                }
            }]
        });
    }
}