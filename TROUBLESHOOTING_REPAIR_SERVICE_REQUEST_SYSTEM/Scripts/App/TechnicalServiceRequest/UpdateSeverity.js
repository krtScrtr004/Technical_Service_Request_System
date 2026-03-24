$(document).ready(function () {
    const token = $('input[name="__RequestVerificationToken"]').val();

    var confirmUpdateSeverityButton = $("#confirm_update_severity_button");
    if (confirmUpdateSeverityButton.length === 0) {
        console.warn('Update severity button not found.');
        return;
    }

    confirmUpdateSeverityButton.click(function (e) {
        e.preventDefault();
        var requestId = $(this).data("request-id");

        // Confirmation Dialogue
        Swal.fire({
            title: 'Are you sure?',
            text: 'Confirm the changes.',
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
        const severityDropdown = $("#severity_dropdown");
        if (!severityDropdown) {
            Swal.fire({
                title: "Error",
                text: "An error occured. Please try again later.",
                icon: "Error"
            })
            console.warn("Severity dropdown not found.")
            return;
        }

        const selectedSeverityId = severityDropdown.val();

        $.ajax({
            url: updateSeverityUrl + '\\' + requestId,
            type: 'POST',
            headers: {
                'X-CSRF-TOKEN': token
            },
            data: {
                id: requestId,
                severityId: selectedSeverityId,
                __RequestVerificationToken: token
            },
            success: function (response) {
                if (response.success) {
                    Swal.fire('Success', response.message, 'success');
                    window.location.href = response.redirectUrl;
                } else {
                    Swal.fire('Error!', response.message, 'error');
                }
            },
            error: function (xhr, status, error) {
                Swal.fire({
                    title: "Error",
                    text: "An error occured. Please try again.",
                    icon: "error",
                });
                console.error(error)
            }
        });
    }
});