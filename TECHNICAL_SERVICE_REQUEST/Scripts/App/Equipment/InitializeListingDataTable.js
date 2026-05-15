$(document).ready(function () {
    $.fn.dataTable.ext.pager.numbers_length = 5;

    function buildStatusLabel(status) {
        const normalizedStatus = (status.replaceAll(" ", "_") || "").toString().trim().toUpperCase();
        var labelClass = "label-default";

        switch (normalizedStatus) {
            case "UNDER_REPAIR":
                labelClass = "label-warning";
                break;
            case "OPERATIONAL":
                labelClass = "label-info";
                break;
            case "INACTIVE":
                labelClass = "label-danger";
                break;
            case "FOR_DISPOSAL":
            default:
                labelClass = "label-default";
                break;
        }

        return `<span class="label ${labelClass}">${normalizedStatus.replaceAll("_", " ") || "N/A"}</span>`;
    }

    var table = $("#equipment_table").DataTable({
        processing: true,
        serverSide: true,
        responsive: {
            details: {
                type: "column",
                target: "td.dtr-control"
            }
        },
        autoWidth: false,
        ajax: {
            url: requestUrl,
            type: "GET",
            data: function (d) {
                d.userId = userId,
                    d.statusFilter = $("#status_filter").val(),
                    d.typeFilter = $("#type_filter").val()
            },
            error: function (xhr, error, thrown) {
                console.error("Error loading data:", error);
            }
        },
        columns: [
            {
                data: null,
                name: "",
                orderable: false,
                className: "dtr-control all",
                render: function (data, type, row) {
                    return "";
                }
            },
            {
                data: "AssetTag",
                name: "AssetTag",
                className: "asset-tag"
            },
            {
                data: "Model",
                name: "Model",
                className: "model"
            },
            {
                data: "Type",
                name: "Type",
                className: "type"
            },
            {
                data: null,
                name: null,
                className: "status",
                render: function (data, type, row) {
                    return buildStatusLabel(row["Status"])
                }
            },
            {
                data: "RepairCount",
                name: "RepairCount",
                className: "repair-count"
            },
            {
                data: null,
                orderable: false,
                render: function (data, type, row) {
                    let buttons = `<a class="btn btn-sm btn-primary" href="/Equipment/Details/${row.Id}">
                                        <i class="glyphicon glyphicon-eye-open"></i>
                                    </a>`;

                    return `<div class="action-buttons flex-row">${buttons}</div>`;
                }
            }
        ],
        columnDefs: [
            { responsivePriority: 1, targets: [1, 2] },

        ],
        order: [[1, "desc"]], // Sort by date requested descending
        pageLength: 10,
        lengthMenu: [[5, 10, 25, 50], [5, 10, 25, 50]],
        ordering: true,
        searching: true
    });

    $("#status_filter").on("change", function () {
        table.ajax.reload();
    });

    $("#type_filter").on("change", function () {
        table.ajax.reload();
    });

    // Setup SignalR connection
    var hub = $.connection.equipmentHub;

    // Listen for changes
    hub.client.refreshEquipmentList = function () {
        table.ajax.reload(null, false); // false keeps current page
    };

    // Start the SignalR connection
    $.connection.hub.start().done(function () {
        console.log("SignalR connected");
    }).fail(function (error) {
        console.error("SignalR connection failed:", error);
    });
});