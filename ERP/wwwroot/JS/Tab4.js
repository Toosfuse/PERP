function load_tab4() {
    fetch('/Home/Data_ProductSales')
        .then(response => response.json())
        .then(result => {
            setTimeout(() => {
                Highcharts.chart('product-sales-chart', {
                    chart: { type: 'bar' },
                    title: { text: 'فروش به تفکیک محصول' },
                    xAxis: { type: 'category', title: { text: 'نام محصول' } },
                    yAxis: { title: { text: 'تعداد فروش' } },
                    legend: { enabled: false },
                    tooltip: {
                        useHTML: true,
                        formatter: function () {
                            return '<div style="direction:ltr;text-align:left;">تعداد فروش: <b>' + this.y + '</b></div>';
                        }
                    },
                    plotOptions: {
                        series: {
                            dataLabels: {
                                enabled: true,
                                style: { direction: 'ltr', fontWeight: 'bold' },
                                formatter: function () {
                                    return Highcharts.numberFormat(this.y, 0);
                                }
                            }
                        }
                    },
                    series: [{
                        name: 'فروش',
                        colorByPoint: true,
                        data: result.data.map(p => ({
                            name: p.name,
                            y: Number(p.y)
                        }))
                    }]
                });
            }, 300);
        });

    $.getJSON('/Home/GetCustomerOrderStats', function (data) {
        if (!data || data.length === 0) {
            console.warn("داده‌ای برای نمایش وجود ندارد");
            return;
        }
        data.sort(function (a, b) {
            return b.OrderCount - a.OrderCount;
        });
        const names = data.map(x => x.CustomerName);
        const counts = data.map(x => x.OrderCount);
        Highcharts.chart('customerOrderChart', {
            chart: { type: 'column' },
            title: { text: 'تعداد سفارش‌ها به تفکیک مشتری' },
            xAxis: {
                categories: names,
                reversed: true,
                title: { text: 'مشتری' },
                labels: { style: { fontSize: '12px' }, rotation: -45 }
            },
            yAxis: { min: 0, title: { text: 'تعداد سفارش', align: 'high' } },
            tooltip: { valueSuffix: ' سفارش' },
            plotOptions: {
                column: {
                    colorByPoint: true,
                    dataLabels: { enabled: true, inside: false, style: { fontWeight: 'bold', color: '#000000' } }
                }
            },
            credits: { enabled: false },
            series: [{ name: 'سفارش‌ها', data: counts }]
        });
    }).fail(function (jqXHR, textStatus, errorThrown) {
        console.error("خطا در دریافت داده:", textStatus, errorThrown);
    });

    fetch('/Home/GetTopProductPerCustomer')
        .then(response => response.json())
        .then(data => {
            const customers = [...new Set(data.map(item => item.CustomerName))];
            const seriesData = customers.map(customer => {
                const rec = data.find(d => d.CustomerName === customer);
                return rec ? {
                    y: rec.TotalQuantity,
                    product: rec.ProductName,
                    customer: rec.CustomerName
                } : { y: 0, product: '', customer: customer };
            });
            Highcharts.chart('line-chart-container', {
                chart: { type: 'line' },
                title: { text: 'پرمصرف‌ترین محصول هر مشتری' },
                xAxis: {
                    categories: customers,
                    reversed: true,
                    title: { text: 'مشتری' },
                    labels: { rotation: -45, style: { fontSize: '12px' } }
                },
                yAxis: { min: 0, title: { text: 'تعداد کل خریداری‌شده' } },
                tooltip: {
                    formatter: function () {
                        return '<b>مشتری:</b> ' + this.point.customer + '<br/>' +
                            '<b>محصول:</b> ' + this.point.product + '<br/>' +
                            '<b>تعداد:</b> ' + this.y + ' عدد';
                    }
                },
                plotOptions: {
                    line: {
                        dataLabels: { enabled: true, style: { fontWeight: 'bold', color: '#000', textOutline: 'none' } },
                        enabledMouseTracking: true
                    }
                },
                series: [{ name: 'تعداد', data: seriesData, color: '#0071A7' }],
                credits: { enabled: false }
            });
        });
}
