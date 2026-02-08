function load_tab10() {
    $.ajax({
        url: '/GetDataPouya/Data_SaleTotalTargetProgress',
        type: 'GET',
        success: function (response) {
            function makeGauge(container, title, value, actual, target, color) {
                Highcharts.chart(container, {
                    chart: {
                        type: 'solidgauge',
                        backgroundColor: '#f8f9fa',
                        height: 250
                    },
                    title: {
                        text: title,
                        style: { fontSize: '16px', fontWeight: 'bold' }
                    },
                    tooltip: { enabled: false },
                    pane: {
                        startAngle: 0,
                        endAngle: 360,
                        background: [{
                            outerRadius: '112%',
                            innerRadius: '88%',
                            backgroundColor: '#eaeaea',
                            borderWidth: 0
                        }]
                    },
                    yAxis: {
                        min: 0,
                        max: 100,
                        lineWidth: 0,
                        tickPositions: []
                    },
                    plotOptions: {
                        solidgauge: {
                            dataLabels: {
                                borderWidth: 0,
                                useHTML: true,
                                formatter: function () {
                                    var a = this.point.actual || 0;
                                    var t = this.point.target || 0;
                                    return '<div style="text-align:center;">' +
                                        '<span style="font-size:22px;color:' + color + ';">' + Highcharts.numberFormat(this.y, 1) + '%</span><br/>' +
                                        '<span style="font-size:12px;color:#666;">' +
                                        'واقعی: <b>' + Highcharts.numberFormat(a, 0, '.', ',') + '</b>&nbsp;&nbsp;' +
                                        'تارگت: <b>' + Highcharts.numberFormat(t, 0, '.', ',') + '</b>' +
                                        '</span>' +
                                        '</div>';
                                }
                            },
                            linecap: 'round'
                        }
                    },
                    series: [{
                        name: title,
                        data: [{
                            color: color,
                            radius: '112%',
                            innerRadius: '88%',
                            y: value,
                            actual: actual,
                            target: target
                        }]
                    }],
                    credits: { enabled: false },
                    exporting: { enabled: false }
                });
            }
            makeGauge('sale_chart_mcb',
                'کلید مینیاتوری (MCB)',
                response.ProgressMcb,
                response.ActualMcb,
                response.TotalTargetMcb,
                '#007bff');
            makeGauge('sale_chart_rcbo',
                'کلید محافظ جان (RCBO)',
                response.ProgressRcbo,
                response.ActualRcbo,
                response.TotalTargetRcbo,
                '#dc3545');
            makeGauge('sale_chart_total',
                'کل',
                response.TotalProgress,
                (response.ActualMcb || 0) + (response.ActualRcbo || 0),
                (response.TotalTargetMcb || 0) + (response.TotalTargetRcbo || 0),
                '#28a745');
        },
        error: function () { alert("خطا در دریافت داده فروش!"); }
    });
    $.ajax({
        url: '/GetDataPouya/Data_SaleDelegateProgress',
        type: 'GET',
        async: true,
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (response) {
            delegateProgressChart(response);
            delegateHeatmapChart(response);
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.error("خطا در دریافت پیشرفت فروش نمایندگان:", textStatus, errorThrown);
        }
    });
    function delegateHeatmapChart(data) {
        var sortedData = data.sort(function (a, b) { return a.TotalProgress - b.TotalProgress; });
        Highcharts.chart('sale_heatmapChart', {
            chart: { type: 'heatmap', height: 1000 },
            title: { text: 'نقشه حرارتی پیشرفت گروه‌ها به تارگت ۱۴۰۴ (فروش)' },
            xAxis: { categories: ['کلید مینیاتوری', 'کلید محافظ جان', 'مجموع'] },
            yAxis: { categories: sortedData.map(function (d) { return d.GroupName; }), title: null },
            colorAxis: {
                stops: [
                    [0, '#DF5353'],
                    [0.5, '#DDDF0F'],
                    [1, '#55BF3B']
                ],
                min: 0, max: 100
            },
            series: [{
                name: 'پیشرفت',
                borderWidth: 1,
                data: sortedData.flatMap(function (group, y) {
                    return [
                        {
                            x: 0, y: y, value: group.ProgressMcb,
                            actual: group.ActualMcb, target: group.TargetMcb,
                            category: 'کلید مینیاتوری', groupName: group.GroupName
                        },
                        {
                            x: 1, y: y, value: group.ProgressRcbo,
                            actual: group.ActualRcbo, target: group.TargetRcbo,
                            category: 'کلید محافظ جان', groupName: group.GroupName
                        },
                        {
                            x: 2, y: y, value: group.TotalProgress,
                            actual: group.ActualMcb + group.ActualRcbo,
                            target: group.TargetMcb + group.TargetRcbo,
                            category: 'مجموع', groupName: group.GroupName
                        }
                    ];
                }),
                dataLabels: { enabled: true, format: '{point.value}%' }
            }],
            credits: { enabled: false },
            tooltip: {
                useHTML: true,
                formatter: function () {
                    return '<b>' + this.point.groupName + '</b><br/>' +
                        '<b>' + this.point.category + '</b><br/>' +
                        'پیشرفت: <b>' + this.point.value + '%</b><br/>' +
                        'واقعی: ' + this.point.actual + '<br/>' +
                        'تارگت: ' + this.point.target;
                }
            }
        });
    }
    function delegateProgressChart(data) {
        Highcharts.chart('sale_chartDelegateProgress', {
            chart: { type: 'bar', backgroundColor: '#f8f9fa', height: 1000 },
            title: { text: 'پیشرفت فروش نمایندگان به تارگت ۱۴۰۴', style: { fontWeight: 'bold' } },
            xAxis: {
                categories: data.map(d => d.GroupName),
                title: null,
                labels: { style: { fontSize: '13px' } }
            },
            yAxis: {
                min: 0, max: 120,
                title: { text: 'درصد پیشرفت' },
                gridLineColor: '#ddd'
            },
            legend: { enabled: true, reversed: true },
            tooltip: {
                useHTML: true,
                formatter: function () {
                    return '<b>' + this.series.name + '</b><br/>' + this.key + ': ' + this.y + '%<br/>' +
                        'واقعی: <b>' + this.point.actual + '</b><br/>تارگت: <b>' + this.point.target + '</b>';
                }
            },
            plotOptions: {
                series: {
                    stacking: 'normal',
                    dataLabels: { enabled: true, format: '{y}%' }
                }
            },
            credits: { enabled: false },
            series: [{
                name: 'کلید محافظ جان(RCBO)',
                data: data.map(d => ({ y: d.ProgressRcbo, actual: d.ActualRcbo, target: d.TargetRcbo })),
                color: '#dc3545'
            }, {
                name: 'کلید مینیاتوری (MCB)',
                data: data.map(d => ({ y: d.ProgressMcb, actual: d.ActualMcb, target: d.TargetMcb })),
                color: '#007bff'
            }]
        });
    }
    $.ajax({
        url: '/GetDataPouya/Data_SaleActiveDelegacy',
        type: 'GET',
        async: true,
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (response) {
            visitorData1(response);
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.error("خطا در دریافت داده‌های نمایندگان فعال (فروش):", textStatus, errorThrown);
        }
    });
    function visitorData1(a) {
        Highcharts.chart('sale_chart2', {
            chart: { type: 'column' },
            title: { text: '' },
            xAxis: {
                type: 'category',
                labels: { rotation: -45, style: { fontSize: '13px' } },
                reversed: true
            },
            credits: { enabled: false },
            yAxis: { min: 0, title: { text: 'تعداد فاکتور' }, allowDecimals: false },
            legend: { enabled: false },
            tooltip: { pointFormat: '<b>{point.y:.0f}</b>' },
            plotOptions: {
                column: {
                    cursor: 'pointer',
                    point: {
                        events: {
                            click: function () {
                                window.location = '/GetDataPouya/Index/' + this.name;
                            }
                        }
                    }
                }
            },
            series: [{
                name: 'Population',
                data: a,
                colorByPoint: true,
                groupPadding: 0,
                dataLabels: {
                    enabled: true,
                    rotation: 0,
                    color: '#FFFFFF',
                    align: 'center',
                    format: '{point.y:.0f}',
                    y: 0,
                    style: { fontSize: '13px', fontFamily: 'shabnam' }
                }
            }]
        });
    }
    $.ajax({
        url: '/GetDataPouya/Data_SaleActiveDelegacyByMiniatori',
        type: 'GET',
        async: true,
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (response) {
            visitorData3(response);
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.error("خطا در دریافت داده‌های نمایندگان فعال (مینیاتوری فروش):", textStatus, errorThrown);
        }
    });
    function visitorData3(a) {
        Highcharts.chart('sale_chart6', {
            chart: { type: 'column' },
            title: { text: '' },
            xAxis: {
                type: 'category',
                labels: { rotation: -45, style: { fontSize: '13px' } },
                reversed: true
            },
            credits: { enabled: false },
            yAxis: { min: 0, title: { text: 'تعداد فروش' }, allowDecimals: false },
            legend: { enabled: false },
            tooltip: { pointFormat: '<b>{point.y:.0f}</b>' },
            plotOptions: {
                column: {
                    cursor: 'pointer',
                    point: {
                        events: {
                            click: function () {
                                window.location = '/GetDataPouya/Index/' + this.name;
                            }
                        }
                    }
                }
            },
            series: [{
                name: 'Population',
                data: a,
                colorByPoint: true,
                groupPadding: 0,
                dataLabels: {
                    enabled: true,
                    rotation: 0,
                    color: '#FFFFFF',
                    align: 'center',
                    format: '{point.y:.0f}',
                    y: 0,
                    style: { fontSize: '13px', fontFamily: 'shabnam' }
                }
            }]
        });
    }
    $.ajax({
        url: '/GetDataPouya/Data_SaleActiveDelegacy_RCBO',
        type: 'GET',
        async: true,
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (response) {
            visitorData4(response);
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.error("خطا در دریافت داده‌های نمایندگان فعال (RCBO فروش):", textStatus, errorThrown);
        }
    });
    function visitorData4(a) {
        Highcharts.chart('sale_chart7', {
            chart: { type: 'column' },
            title: { text: '' },
            xAxis: {
                type: 'category',
                labels: { rotation: -45, style: { fontSize: '13px' } },
                reversed: true
            },
            credits: { enabled: false },
            yAxis: { min: 0, title: { text: 'تعداد فروش' }, allowDecimals: false },
            legend: { enabled: false },
            tooltip: { pointFormat: '<b>{point.y:.0f}</b>' },
            plotOptions: {
                column: {
                    cursor: 'pointer',
                    point: {
                        events: {
                            click: function () {
                                window.location = '/GetDataPouya/Index/' + this.name;
                            }
                        }
                    }
                }
            },
            series: [{
                name: 'Population',
                data: a,
                colorByPoint: true,
                groupPadding: 0,
                dataLabels: {
                    enabled: true,
                    rotation: 0,
                    color: '#FFFFFF',
                    align: 'center',
                    format: '{point.y:.0f}',
                    y: 0,
                    style: { fontSize: '13px', fontFamily: 'shabnam' }
                }
            }]
        });
    }
    $.ajax({
        url: '/GetDataPouya/Data_SaleDemandTrend_Miniature',
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
        Highcharts.chart('sale_chartTrend', {
            chart: { type: 'column' },
            title: { text: 'روند فروش کلید مینیاتوری در ماه' },
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
    $.ajax({
        url: '/GetDataPouya/Data_SaleDemandTrend_RCBO',
        type: 'GET',
        async: true,
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (response) {
            visitorDataTrendRCBO(response);
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.error("خطا در دریافت داده‌های روند فروش RCBO:", textStatus, errorThrown);
        }
    });
    function visitorDataTrendRCBO(a) {
        Highcharts.chart('sale_chartTrend-RCBO', {
            chart: { type: 'column' },
            title: { text: 'روند فروش کلید محافظ جان در ماه' },
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
                color: '#fc7777',
                borderColor: '#fc7777',
                borderWidth: 1
            }, {
                name: 'خط روند',
                type: 'line',
                data: a,
                color: '#ff0008',
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
    $.ajax({
        url: '/GetDataPouya/Data_SaleRepPerformanceByCategory',
        type: 'GET',
        async: true,
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            repPerformanceChart(data);
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.error("خطا در دریافت داده‌های عملکرد فروش نمایندگان:", textStatus, errorThrown);
        }
    });
    function repPerformanceChart(data) {
        Highcharts.chart('sale_chartRepPerformance', {
            chart: { type: 'column' },
            title: { text: 'عملکرد فروش نمایندگان بر اساس دسته محصولات (کلید مینیاتوری: میله،کلید محافظ جان: خط)' },
            xAxis: {
                categories: data.categories,
                labels: { rotation: -45, style: { fontSize: '13px' } }
            },
            credits: { enabled: false },
            yAxis: [{
                min: 0,
                title: { text: 'تعداد کلید مینیاتوری' },
                allowDecimals: false,
                opposite: false
            }, {
                min: 0,
                title: { text: 'تعداد کلید محافظ جان' },
                allowDecimals: false,
                opposite: true
            }],
            legend: { enabled: true },
            tooltip: {
                shared: true,
                pointFormat: '<b>{series.name}</b>: {point.y:.0f} واحد'
            },
            plotOptions: {
                column: {
                    pointPadding: 0,
                    groupPadding: 0.1,
                    pointWidth: 30
                },
                line: {
                    lineWidth: 2,
                    marker: { enabled: true, radius: 4 }
                },
                series: {
                    dataLabels: {
                        enabled: true,
                        format: '{y:.0f}',
                        style: { fontSize: '12px', fontFamily: 'shabnam', textOutline: 'none' }
                    }
                }
            },
            series: [{
                name: 'کلید مینیاتوری',
                type: 'column',
                yAxis: 0,
                data: data.series[0].data,
                color: '#007bff'
            }, {
                name: 'کلید محافظ جان',
                type: 'line',
                yAxis: 1,
                data: data.series[1].data,
                color: '#dc3545'
            }]
        });
    }
    $.ajax({
        url: '/GetDataPouya/Data_SaleRepRadarProfile',
        type: 'GET',
        async: true,
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            repRadarChart(data);
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.error("خطا در دریافت داده‌های پروفایل رادار فروش نمایندگان:", textStatus, errorThrown);
        }
    });
    function repRadarChart(data) {
        Highcharts.chart('sale_chartRepRadar', {
            chart: { polar: true, type: 'area' },
            title: { text: '10 نماینده برتر براساس امتیاز (فروش)' },
            xAxis: {
                categories: data.categories,
                tickmarkPlacement: 'on',
                lineWidth: 0
            },
            credits: { enabled: false },
            yAxis: {
                min: 0,
                max: 100,
                title: { text: 'امتیاز نرمال‌شده' },
                allowDecimals: false
            },
            legend: { enabled: true, align: 'right', verticalAlign: 'middle', layout: 'vertical' },
            tooltip: {
                shared: true,
                pointFormat: '<b>{series.name}</b>: <b>{point.y}</b> امتیاز در {point.category}'
            },
            plotOptions: {
                series: {
                    fillOpacity: 0.3,
                    lineWidth: 1,
                    marker: { enabled: true, radius: 3 }
                }
            },
            series: data.series
        });
    }
    $.ajax({
        url: '/GetDataPouya/Data_SalePopulateProduct',
        type: 'GET',
        async: true,
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            visitorData2(data);
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.error("خطا در دریافت داده‌های محصولات محبوب (فروش):", textStatus, errorThrown);
        }
    });
    function visitorData2(data) {
        Highcharts.chart('sale_chart3', {
            chart: { type: 'column' },
            title: { text: '' },
            xAxis: {
                type: 'category',
                labels: { rotation: -45, style: { fontSize: '13px' } },
                reversed: true
            },
            credits: { enabled: false },
            yAxis: { min: 0, title: { text: 'تعداد' }, allowDecimals: false },
            legend: { enabled: false },
            tooltip: { pointFormat: '<b>{point.y:.0f}</b>' },
            plotOptions: {
                column: {
                    cursor: 'pointer',
                    point: {
                        events: {
                            click: function () {
                                window.location = '/GetDataPouya/Index/' + this.name;
                            }
                        }
                    }
                }
            },
            series: [{
                name: 'Population',
                data: data,
                colorByPoint: true,
                groupPadding: 0,
                dataLabels: {
                    enabled: true,
                    rotation: 0,
                    color: '#FFFFFF',
                    align: 'center',
                    format: '{point.y:.0f}',
                    y: 0,
                    style: { fontSize: '13px', fontFamily: 'shabnam' }
                }
            }]
        });
    }
    $.ajax({
        url: '/GetDataPouya/Data_SaleProductCount',
        type: 'GET',
        async: true,
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (response) {
            Highcharts.chart('sale_chart4', {
                chart: {
                    type: 'bar',
                    height: 2000
                },
                title: { text: 'تعداد فروش هر محصول' },
                xAxis: {
                    type: 'category',
                    title: { text: 'نام محصول' },
                    labels: { rotation: -45, style: { fontSize: '13px' } },
                    reversed: true
                },
                yAxis: { min: 0, title: { text: 'تعداد کل' }, allowDecimals: false },
                legend: { enabled: false },
                tooltip: { pointFormat: '<b>{point.y} عدد</b>' },
                plotOptions: {
                    column: {
                        colorByPoint: true,
                        dataLabels: {
                            enabled: true,
                            rotation: 0,
                            color: '#FFFFFF',
                            align: 'center',
                            format: '{point.y:.0f}',
                            y: 0,
                            style: { fontSize: '13px', fontFamily: 'shabnam' }
                        }
                    }
                },
                series: [{
                    name: 'تعداد',
                    data: response.map(p => ({
                        name: p.name,
                        y: Number(p.y)
                    })),
                    dataLabels: {
                        enabled: true,
                        rotation: 0,
                        color: '#FFFFFF',
                        align: 'center',
                        format: '{point.y:.0f}',
                        y: 0,
                        style: { fontSize: '13px', fontFamily: 'shabnam' }
                    },
                    colorByPoint: true
                }],
                credits: { enabled: false }
            });
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.error("خطا در دریافت داده‌های فروش محصولات:", textStatus, errorThrown);
        }
    });
}