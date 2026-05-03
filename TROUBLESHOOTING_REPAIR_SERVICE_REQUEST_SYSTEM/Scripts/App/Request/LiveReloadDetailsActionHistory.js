$(document).ready(function () {
    const actionHistoryTable = $("#action_history_table")
    if (actionHistoryTable.length === 0) {
        console.warn("Action history table not found");
        return;
    }

    const hub = $.connection.requestHub;

    function refreshFormGenerationButton() {
        const generateFormButton = $("#generate_tsrf_button");
        if (generateFormButton.length === 0 || !formGenerationStateUrl) {
            return;
        }

        $.ajax({
            url: formGenerationStateUrl,
            method: "GET",
            data: { id: currentTechnicalServiceRequestId },
            success: function (response) {
                if (!response || response.success !== true) {
                    return;
                }

                if (response.isFormGeneratable) {
                    generateFormButton.attr("href", response.formLink || "#");
                    generateFormButton.removeClass("no-display").addClass("block");
                } else {
                    generateFormButton.attr("href", "#");
                    generateFormButton.removeClass("block").addClass("no-display");
                }
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    }

    // Update the status text when a request is cancelled
    hub.client.refreshRequestActionHistory = function (technicalServiceRequestHistoryId, technicalServiceRequestId) {
        if (!technicalServiceRequestHistoryId ||
             technicalServiceRequestHistoryId < 1) {
            return;
        }

        if (parseInt(technicalServiceRequestId, 10) !== parseInt(currentTechnicalServiceRequestId, 10)) {
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

    hub.client.refreshRequestFormGeneration = function (technicalServiceRequestId) {
        if (parseInt(technicalServiceRequestId, 10) !== parseInt(currentTechnicalServiceRequestId, 10)) {
            return;
        }

        refreshFormGenerationButton();
    }

    // Start the SignalR connection
    $.connection.hub.start().done(function () {
        console.log('SignalR connected');
    }).fail(function (error) {
        console.error('SignalR connection failed:', error);
    });
})