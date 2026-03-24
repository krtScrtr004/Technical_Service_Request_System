$(document).ready(function () {
    const hub = $.connection.technicalServiceRequestHub;

    function buildRequestStatusLabel(status) {
        const normalizedStatus = (status || "").toString().trim().toUpperCase();
        let labelClass = "label-default";

        switch (normalizedStatus) {
            case "PENDING":
                labelClass = "label-warning";
                break;
            case "OPEN":
                labelClass = "label-primary";
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
            case "CLOSED":
                labelClass = "label-default";
                break;
        }

        return `<span class="label ${labelClass} status-label">${normalizedStatus || "N/A"}</span>`;
    }

    // Start the SignalR connection
    $.connection.hub.start().done(function () {
        console.log('SignalR connected');
    }).fail(function (error) {
        console.error('SignalR connection failed:', error);
    });

    // Listen for changes in request's severity
    const severityContainer = $("#severity_container")
    if (severityContainer.length === 0) {
        console.warn('Severity container not found');
    } else {
        hub.client.refreshTechnicalServiceRequestSeverity = function (severityName) {
            severityContainer.text(severityName);
        };
    }

    // Listen for changes in request's status
    const statusContainer = $("#status_container");
    if (statusContainer.length === 0) {
        console.warn('Status container not found');
    } else {
        // Update the status text when a request is cancelled
        hub.client.refreshTechnicalServiceRequestStatus = function (statusName) {
            statusContainer.html(buildRequestStatusLabel(statusName));

            const addActionHistoryButton = $("#add_action_history_button");
            if (statusName === "CANCELLED" && addActionHistoryButton.length > 0) {
                // Remove add action history button, if exists -> for ADMIN & IT only
                addActionHistoryButton.remove();
            }
        };
    }
})