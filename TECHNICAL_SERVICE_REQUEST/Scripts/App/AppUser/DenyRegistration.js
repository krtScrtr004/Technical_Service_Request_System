$(document).ready(function () {
    const token = $('input[name="__RequestVerificationToken"]').val();

    $("#deny_user_request_button").click(function () {
        Swal.fire({
            title: "Are you sure?",
            text: "This action cannot be undone.",
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: "Yes, deny request!",
            cancelButtonText: "Cancel"
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: denyUrl,
                    type: "POST",
                    data: {
                        id: registrationRequestId,
                        __RequestVerificationToken: token
                    },
                    success: function (response) {
                        Swal.fire({
                            title: "Success",
                            text: "Request has been denied successfully.",
                            icon: "info"
                        }).then(() => {
                            window.location.href = denyRedirectUrl
                        });
                    },
                    error: function (xhr, status, error) {
                        Swal.fire({
                            title: "Error",
                            text: "An error occured. Please try again.",
                            icon: "error",
                            confirmButtonText: "Understood"
                        });
                        console.error(error);
                    }
                });
            }
        })
    });

});