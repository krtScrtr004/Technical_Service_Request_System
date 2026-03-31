$(document).ready(function () {
    var table = $("#registration_table").DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: requestUrl,
            type: 'GET',
            data: function (d) {
                d.userId = userId,
                d.accountTypeFilter = $("#account_type_filter").val()
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
                name: "LastName",
                render: function (data, type, row) {
                    return row.LastName + ", " + row.FirstName + " " + (row.MiddleName ? row.MiddleName : "");
                },
            },
            {
                data: 'Email',
                name: 'Email'
            },
            {
                data: 'ContactNumber',
                name: 'ContactNumber'
            },
            {
                data: null,
                name: 'AccountType',
                render: function (data, type, row) {
                    const userPrivilegeName = row.AccountType.toUpperCase();

                    let htmlClass = "label-default";
                    if (userPrivilegeName === "ADMINISTRATOR" || userPrivilegeName === "ADMIN") {
                        htmlClass = 'label-success'
                    } else if (userPrivilegeName === "IT") {
                        htmlClass = 'label-warning'
                    }
                    return `<span class="label ${htmlClass}">${userPrivilegeName}</span>`;
                }
            },
            {
                data: null,
                orderable: false,
                render: function (data, type, row) {
                    return `<a class="btn btn-sm btn-primary" href="/Registration/Details/${row.Id}">
                                <i class="glyphicon glyphicon-eye-open"></i>
                            </a>`;
                }
            }
        ],
        order: [[0, 'asc']], // Sort by last name asc
        pageLength: 10,
        lengthMenu: [[5, 10, 25, 50], [5, 10, 25, 50]],
        ordering: true,
        searching: true
    });

    $("#account_type_filter").on("change", function () {
        table.ajax.reload();
    })

    // Setup SignalR connection
    var hub = $.connection.registrationHub;

    // Listen for new request notifications
    hub.client.refreshRegistrationList = function () {
        table.ajax.reload(null, false); // false keeps current page
    };

    // Start the SignalR connection
    $.connection.hub.start()
        .done(function () {
            console.log('SignalR connected');
        }).fail(function (error) {
            console.error('SignalR connection failed:', error);
        });
});