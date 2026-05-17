$(document).ready(function () {
    const notificationHub = $.connection.notificationHub;

    notificationHub.client.refreshNotificationList = refreshLatest;
    notificationHub.client.refreshAdminNotificationList = refreshLatest;
    notificationHub.client.refreshITNotificationList = refreshLatest;

    $.connection.hub.start().done(function () {
        console.log('SignalR connected');
    }).fail(function (error) {
        console.error('SignalR connection failed:', error);
    });

    function refreshLatest() {
        if (currentPage == 1) {
            $.get(getNotificationUrl, { pageSize: pageSize })
                .done(function (html) {
                    $('#notification_list').html(html);
                });
        }
    }
});