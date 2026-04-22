$(document).ready(function () {
    const dashboardHub = $.connection.dashboardHub;

    dashboardHub.client.refreshDashboard = function () {
        $(document).trigger("dashboard:refresh");
    };

    $.connection.hub.start().done(function () {
        console.log("SignalR connected");
    }).fail(function (error) {
        console.error("SignalR connection failed:", error);
    });
});