$(document).ready(function () {
    const hub = $.connection.technicalServiceRequestHub;

    function buildRequestSeverityLabel(severity) {
        const normalizedSeverity = (severity || "").toString().trim().toUpperCase();
        let labelClass = "label-default"

        switch (normalizedSeverity) {
            case "LOW":
                labelClass = "label-success";
                break;
            case "MEDIM":
                labelClass = "label-warning";
                break;
            case "HIGH":
            case "CRITICAL":
                labelClass = "label-danger";
                break;
        }

        return `<span class="label ${labelClass}">${normalizedSeverity || "N/A"}</span>`
    }

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
                recalibrateUi();
                break;
            case "CLOSED":
                labelClass = "label-default";
                recalibrateUi();
                break;
        }

        return `<span class="label ${labelClass} status-label">${normalizedStatus || "N/A"}</span>`;
    }

    function recalibrateUi() {
        // Remove the update severity button and modal once cancelled
        const updateSeverityButton = $("#update_severity_button");
        if (updateSeverityButton.length > 0) {
            updateSeverityButton.remove();
        }
        const updateSeverityModal = $("#update_severity_modal");
        if (updateSeverityModal.length > 0) {
            updateSeverityModal.remove();
        }

        // Remove the action history button and modal once cancelled
        const addActionHistoryButton = $("#add_action_history_button");
        if (addActionHistoryButton.length > 0) {
            addActionHistoryButton.remove();
        }
        const addActionHistoryModal = $("#add_action_history_modal");
        if (addActionHistoryModal.length > 0) {
            addActionHistoryModal.remove();
        }
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
            severityContainer.html(buildRequestSeverityLabel(severityName));
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
            if ((statusName === "CANCELLED" || statusName === "CLOSED") && addActionHistoryButton.length > 0) {
                // Remove add action history button, if exists -> for ADMIN & IT only
                addActionHistoryButton.remove();
            }
        };
    }

})