$(document).ready(function () {
    const token = $('input[name="__RequestVerificationToken"]').val();
    const modalId = "notification_detail_modal_dynamic";

    ensureDetailModal(modalId);

    // Use event delegation to handle clicks on dynamically generated elements
    $(document).on("click", ".js-open-notification-detail", function () {
        const $trigger = $(this);
        const $modal = $("#" + modalId);

        const id = $trigger.data("id");
        const isRead = $trigger.data("is-read");

        $modal.find("[data-field='title']").text($trigger.data("title") || "");
        $modal.find("[data-field='message']").text($trigger.data("message") || "");
        $modal.find("[data-field='created-at']").text($trigger.data("createdAt") || "");

        $modal.modal("show");

        if (id && !isRead) {
            // Mark as read
            $.ajax({
                url: markAsReadUrl + '/' + encodeURIComponent(id),
                type: 'POST',
                data: {
                    __RequestVerificationToken: token,
                },
                success: function (response) {
                    $trigger.find(".notification-box").addClass("bg-light");
                    console.log(response);
                },
                error: function (xhr, status, error) {
                    console.error(error);
                }
            });
        }

    });
});

function ensureDetailModal(modalId) {
    if ($("#" + modalId).length > 0) {
        return;
    }

    const modalHtml = `
        <section id="${modalId}" class="modal fade notification-detail-modal" role="dialog" tabindex="-1" aria-hidden="true">
            <section class="modal-dialog">
                <section class="modal-content">
                    <section class="content-panel-heading modal-header bg-primary">
                        <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                        <h4 class="flex-row flex-child-center-h text-light modal-title">
                            <i class="glyphicon glyphicon-bell"></i>
                            Notification Details
                        </h4>
                    </section>
                    <section class="content-panel-body modal-body flex-col">
                        <table class="table table-bordered table-striped">
                            <tbody>
                                <tr>
                                    <td class="active">Title</td>
                                    <td class="bold-text" data-field="title"></td>
                                </tr>
                                <tr class="message-row">
                                    <td class="active">Message</td>
                                    <td><div data-field="message"></div></td>
                                </tr>
                                <tr>
                                    <td class="active">Date</td>
                                    <td data-field="created-at"></td>
                                </tr>
                            </tbody>
                        </table>
                    </section>
                </section>
            </section>
        </section>`;

    $("body").append(modalHtml);
}