$(document).ready(function () {
    $.fn.dataTable.ext.pager.numbers_length = 5;

    var table = $("#registration_table").DataTable({
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
            type: 'GET',
            data: function (d) {
                d.userId = userId,
                d.statusFilter = $("#status_filter").val(),
                d.requestDateFilter = $("#request_date_filter").val()
            },
            error: function (xhr, error, thrown) {
                Swal.fire({
                    title: "Error",
                    text: "An error occured. Please try again.",
                    icon: "error",
                    confirmButtonText: 'Understood',
                });
                console.error('Error loading data:', error);
            }
        },
        columns: [
              {
                data: null,
                name: "",
                orderable: false,
                className: "dtr-control all",
                render: function(data, type, row) {
                    return "";
                }
            },
            {
                data: null,
                name: "LastName",
                render: function (data, type, row) {
                    return row.LastName + ", " + row.FirstName + " " + (row.MiddleName ? row.MiddleName : "");
                }
            },
            {
                data: "Email",
                name: "Email"
            },
            {
                data: "RequestDate",
                name: "RequestDate",
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
                className: "all",
                render: function (data, type, row) {
                    const parse = (val) => val === true || val === 1 || val?.toString().toLowerCase().trim() === "true";

                    if (parse(row.IsApproved)) {
                        return `<span class="label label-success">APPROVED</span>`;
                    } else if (parse(row.IsDenied)) {
                        return `<span class="label label-danger">DENIED</span>`;
                    }

                    return `<span class="label label-warning">PENDING</span>`;
                }
            },
            {
                data: null,
                orderable: false,
                render: function (data, type, row) {
                    const parse = (val) => val === true || val === 1 || val?.toString().toLowerCase().trim() === "true";

                    if (!parse(row.IsApproved) && !parse(row.IsDenied)) {
                        return `<a class="btn btn-sm btn-primary" href="/User/Create/${row.Id}">
                                    <i class="glyphicon glyphicon-flash"></i>
                                </a>`;
                    }

                    return null;
                }
            }
        ],
        columnDefs: [
            { responsivePriority: 1, targets: 1 },
            { responsivePriority: 2, targets: 4 },
            { responsivePriority: 100, targets: [2, 3, 5] }
        ],
        order: [[3, 'desc']], // Sort by date requested descending
        pageLength: 10,
        lengthMenu: [[5, 10, 25, 50], [5, 10, 25, 50]],
        ordering: true,
        searching: true
    });

    table.on("draw.dt", function () {
        var $rows = $("#registration_table tbody tr");
        var maxH = 0;

        $rows.each(function () {
            var h = $(this).outerHeight();
            if (h > maxH) maxH = h;
        });

        $rows.height(maxH);
    });

    $("#status_filter").on("change", function () {
        table.ajax.reload();
    });

    $("#request_date_filter").on("change", function () {
        table.ajax.reload();
    });

    // Setup SignalR connection
    var hub = $.connection.appUserRegistrationHub;

    // Listen for new request notifications
    hub.client.refreshAppUserRequestList = function () {
        table.ajax.reload(null, false); // false keeps current page
    };

    // Start the SignalR connection
    $.connection.hub.start().done(function () {
        console.log('SignalR connected');
    }).fail(function (error) {
        console.error('SignalR connection failed:', error);
    });
});