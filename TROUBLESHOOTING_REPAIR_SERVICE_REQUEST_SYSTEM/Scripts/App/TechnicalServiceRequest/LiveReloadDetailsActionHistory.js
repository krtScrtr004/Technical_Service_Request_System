$(document).ready(function () {
    const actionHistoryTable = $("#action_history_table")
    if (actionHistoryTable.length === 0) {
        console.warn("Action history table not found");
        return;
    }

    const hub = $.connection.technicalServiceRequestHub;

    // Update the status text when a request is cancelled
    hub.client.refreshTechnicalServiceRequestActionHistory = function (technicalServiceRequestHistoryId) {
        if (!technicalServiceRequestHistoryId ||
             technicalServiceRequestHistoryId < 1) {
            return;
        }

        const url = actionHistoryRowUrl;
        $.ajax({
            url: url,
            method: "GET",
            data: {
                id: technicalServiceRequestHistoryId,
            },
            success: function (response) {
                if (!response?.html) {
                    console.warn("No HTML content returned for action history row.");
                }
                actionHistoryTable.find("tbody").append(response.html);
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    }

    // Start the SignalR connection
    $.connection.hub.start().done(function () {
        console.log('SignalR connected');
    }).fail(function (error) {
        console.error('SignalR connection failed:', error);
    });
})