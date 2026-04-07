$(document).ready(function () {
    function buildRequestStatusLabel(status) {
        var normalizedStatus = (status || "").toString().trim().toUpperCase();
        var labelClass = "label-default";

        switch (normalizedStatus) {
            case "PENDING":
                labelClass = "label-warning";
                break;
            case "ONGOING":
                labelClass = "label-info";
                break;
            case "RESOLVED":
                labelClass = "label-success";
                break;
            case "CANCELLED":
                labelClass = "label-danger";
                break;
            case "OPEN":
                labelClass = "label-primary";
                break;
            case "CLOSED":
                labelClass = "label-default";
                break;
        }

        return `<span class="label ${labelClass}">${normalizedStatus || "N/A"}</span>`;
    }

    var table = $("#technical_request_table").DataTable({
        processing: true,
        serverSide: true,
        responsive: true,
        autoWidth: false,
        ajax: {
            url: requestUrl,
            type: 'GET',
            data: function (d) {
                d.userId = userId,
                d.typeFilter = $("#type_filter").val(),
                d.statusFilter = $("#status_filter").val(),
                d.dateRequestFilter = $("#date_request_filter").val()
            },
            error: function (xhr, error, thrown) {
                console.error('Error loading data:', error);
            }
        },
        columns: [
            {
                data: 'ReferenceCode',
                name: 'ReferenceCode'
            },
            {
                data: null,
                orderable: false,
                render: function (data, type, row) {
                    var fullName = row.ClientFirstName + ' ';
                    if (row.ClientMiddleName) {
                        fullName += row.ClientMiddleName.charAt(0) + '. ';
                    }
                    fullName += row.ClientLastName;

                    return `<div class="flex-col">
                                <section><strong>${fullName}</strong></section>
                                <section class="client-sub-info flex-row">
                                    <p class="contact-info single-line-ellipsis" title="${row.ClientEmailAddress}">${row.ClientEmailAddress}</p>
                                    <span class="separator">|</span>
                                    <p class="contact-info single-line-ellipsis" title="${row.ClientContactNumber}">${row.ClientContactNumber}</p>
                                    <span class="separator">|</span>
                                    <p class="single-line-ellipsis" title="${row.ClientOffice}">${row.ClientOffice}</p>
                                </section>
                            </div>`;
                }
            },
            {
                data: null,
                orderable: false,
                render: function (data, type, row) {
                    return row.TechnicalServiceTypeName !== null
                        ? row.TechnicalServiceTypeName
                        : row.Others;
                }
            },
            {
                data: 'TechnicalServiceRequestStatusName',
                name: 'TechnicalServiceRequestStatusName',
                render: function (data) {
                    return buildRequestStatusLabel(data);
                }
            },
            {
                data: 'DateRequest',
                name: 'DateRequest',
                render: function (data, type, row) {
                    if (data) {
                        var date = new Date(data);
                        var options = { year: 'numeric', month: 'long', day: 'numeric' };
                        return date.toLocaleDateString('en-US', options);
                    }
                    return 'Unknown Date';
                }
            },
            {
                data: null,
                orderable: false,
                render: function (data, type, row) {
                    let buttons = `<a class="btn btn-sm btn-primary" href="/TechnicalServiceRequests/Details/${row.Id}">
                                        <i class="glyphicon glyphicon-eye-open"></i>
                                    </a>`;

                    return `<div class="action-buttons flex-row">${buttons}</div>`;
                }
            }
        ],
        order: [[4, 'desc']], // Sort by date requested descending
        pageLength: 10,
        lengthMenu: [[5, 10, 25, 50], [5, 10, 25, 50]],
        ordering: true,
        searching: true
    });

    $("#type_filter").on("change", function () {
        table.ajax.reload();
    });

    $("#status_filter").on("change", function () {
        table.ajax.reload();
    });

    $("#date_request_filter").on("change", function () {
        table.ajax.reload();
    });

    // Setup SignalR connection
    var hub = $.connection.technicalServiceRequestHub;

    // Listen for new request notifications
    hub.client.refreshTechnicalServiceRequestList = function () {
        table.ajax.reload(null, false); // false keeps current page
    };

    // Start the SignalR connection
    $.connection.hub.start().done(function () {
        console.log('SignalR connected');
    }).fail(function (error) {
        console.error('SignalR connection failed:', error);
    });
});