$(document).ready(function () {
    var $dashboardPage = $(".dashboard-page");

    // Disable animations for all charts
    Highcharts.setOptions({
        chart: { animation: false },
        plotOptions: {
            series: { animation: false }
        }
    });

    function setCardValue(selector, value) {
        $(selector).text(value != null ? value : 0);
    }

    function setExtraCard(index, label, value) {
        $("#extra_label_" + index).text(label || "-");
        $("#extra_value_" + index).text(value != null ? value : 0);
    }

    function buildTopRepairedEquipmentsTable(data) {
        var $tbody = $("#top_repaired_equipments_table_body");
        if ($tbody.length === 0) {
            return;
        }

        var rows = (data || []).map(function (item) {
            return [
                "<tr>",
                "<td>", item.AssetTag || "N/A", "</td>",
                "<td>", item.EquipmentModel || "N/A", "</td>",
                "<td>", item.EquipmentType || "N/A", "</td>",
                "<td>", item.EquipmentStatus || "N/A", "</td>",
                "<td class='text-right'>", item.RepairCount != null ? item.RepairCount : 0, "</td>",
                "</tr>"
            ].join("");
        }).join("");

        if (!rows) {
            rows = "<tr><td colspan='6' class='text-center text-muted'>No equipments found.</td></tr>";
        }

        $tbody.html(rows);
    }

    function buildStatusChart(data) {
        Highcharts.chart("request_status_chart", {
            chart: { type: "pie" },
            title: { text: null },
            credits: { enabled: false },
            tooltip: {
                pointFormat: "<b>{point.y}</b> ({point.percentage:.1f}%)"
            },
            series: [{
                name: "Requests",
                colorByPoint: true,
                data: (data || []).map(function (item) {
                    return {
                        name: item.Name,
                        y: item.Count
                    };
                }),
                animation: false
            }],

        });
    }

    function buildServiceTypeChart(data) {
        var categories = (data || []).map(function (item) { return item.Name; });
        var counts = (data || []).map(function (item) { return item.Count; });

        Highcharts.chart("request_type_chart", {
            chart: { type: "column" },
            title: { text: null },
            credits: { enabled: false },
            xAxis: {
                categories: categories,
                crosshair: true
            },
            yAxis: {
                min: 0,
                title: { text: "Requests" }
            },
            legend: { enabled: false },
            series: [{
                name: "Requests",
                data: counts,
                animation: false
            }]
        });
    }

    function buildTrendChart(submitted, resolved) {
        var categories = (submitted || []).map(function (item) { return item.Label; });
        var submittedCounts = (submitted || []).map(function (item) { return item.Count; });
        var resolvedCounts = (resolved || []).map(function (item) { return item.Count; });

        Highcharts.chart("request_trend_chart", {
            chart: { type: "line" },
            title: { text: null },
            credits: { enabled: false },
            xAxis: {
                categories: categories
            },
            yAxis: {
                min: 0,
                title: { text: "Requests" }
            },
            tooltip: {
                shared: true
            },
            series: [{
                name: "Submitted",
                data: submittedCounts,
                color: "#337ab7",
                animation: false
            }, {
                name: "Resolved/Closed",
                data: resolvedCounts,
                color: "#5cb85c",
                animation: false
            }]
        });
    }

    function loadDashboard() {
        $dashboardPage.addClass("dashboard-loading");

        $.ajax({
            url: dashboardDataUrl,
            type: "GET"
        }).done(function (response) {
            if (!response || response.success !== true) {
                return;
            }

            setCardValue("#card_total_requests", response.cards.totalRequests);
            setCardValue("#card_active_requests", response.cards.activeRequests);
            setCardValue("#card_resolved_requests", response.cards.resolvedRequests);
            setCardValue("#card_unread_notifications", response.cards.unreadNotifications);

            if (response.cards.extra) {
                setExtraCard(1, response.cards.extra.firstLabel, response.cards.extra.firstValue);
                setExtraCard(2, response.cards.extra.secondLabel, response.cards.extra.secondValue);
                setExtraCard(3, response.cards.extra.thirdLabel, response.cards.extra.thirdValue);
            }

            buildStatusChart(response.charts.requestByStatus);
            buildServiceTypeChart(response.charts.requestByType);
            buildTrendChart(response.charts.monthlySubmitted, response.charts.monthlyResolved);
            buildTopRepairedEquipmentsTable(response.tables && response.tables.topRepairedEquipments);
        }).fail(function () {
            $("#request_status_chart").html("<p class='text-danger'>Unable to load dashboard data.</p>");
            $("#request_type_chart").html("<p class='text-danger'>Unable to load dashboard data.</p>");
            $("#request_trend_chart").html("<p class='text-danger'>Unable to load dashboard data.</p>");
            $("#top_repaired_equipments_table_body").html("<tr><td colspan='6' class='text-center text-danger'>Unable to load dashboard data.</td></tr>");
        }).always(function () {
            $dashboardPage.removeClass("dashboard-loading");
        });
    }

    $(document).on("dashboard:refresh", function () {
        loadDashboard();
    });

    loadDashboard();
});