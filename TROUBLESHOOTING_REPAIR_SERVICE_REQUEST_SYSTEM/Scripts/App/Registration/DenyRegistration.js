$(document).ready(function () {
    const token = $('input[name="__RequestVerificationToken"]').val();

    $("#deny_registration_request_button").click(function () {
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
    });

});