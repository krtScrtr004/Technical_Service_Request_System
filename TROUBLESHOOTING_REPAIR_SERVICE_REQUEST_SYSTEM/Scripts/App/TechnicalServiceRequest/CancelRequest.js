$(document).ready(function () {
    const token = $('input[name="__RequestVerificationToken"]').val();

    var cancelButton = $("#cancel_request_button");
    if (cancelButton.length === 0) {
        console.warn('Cancel button not found');
        return;
    }

    cancelButton.click(function (e) {
        e.preventDefault();
        var requestId = $(this).data("request-id");

        // Confirmation Dialogue
        Swal.fire({
            title: 'Are you sure?',
            text: 'This action cannot be undone.',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Yes, proceed',
            cancelButtonText: 'Cancel'
        }).then((result) => {
            if (result.isConfirmed) {
                SendRequest(requestId)
            }
        });
    });

    function SendRequest(requestId) {
        $.ajax({
            url: cancelRequestUrl + '\\' + requestId,
            type: 'POST',
            headers: {
                'X-CSRF-TOKEN': token
            },
            data: {
                id: requestId,
                __RequestVerificationToken: token
            },
            success: function (response) {
                if (response.success) {
                    Swal.fire({
                        title: 'Success',
                        text: response.message,
                        icon: 'success'
                    }).then(() => {
                        window.location.href = response.redirectUrl;
                    });
                } else {
                    Swal.fire('Error!', response.message, 'error');
                }
            },
            error: function (xhr, status, error) {
                Swal.fire({
                    title: "Error",
                    text: "An error occured. Please try again.",
                    icon: "error",
                    confirmButtonText: 'Understood',
                });
                console.error(error);
            }
        });
    }
});