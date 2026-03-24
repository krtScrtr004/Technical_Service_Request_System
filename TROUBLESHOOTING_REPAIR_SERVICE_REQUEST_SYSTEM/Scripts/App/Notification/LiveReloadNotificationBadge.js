$('document').ready(function () {
    const notificationHub = $.connection.notificationHub;

    notificationHub.client.refreshNotificationBadge = refreshBadge;
    notificationHub.client.refreshAdminNotificationBadge = refreshBadge;
    notificationHub.client.refreshITNotificationBadge = refreshBadge;

    $.connection.hub.start().done(function () {
        console.log('SignalR connected');
    }).fail(function (error) {
        console.error('SignalR connection failed:', error);
    });

    function refreshBadge() {
        $.get(notificationBadgeUrl)
            .done(function (html) {
                const $notification = $('#notification_badge');
                const $parent = $notification.parent();

                $notification.remove();
                $parent.append(html);
            })
    };
});