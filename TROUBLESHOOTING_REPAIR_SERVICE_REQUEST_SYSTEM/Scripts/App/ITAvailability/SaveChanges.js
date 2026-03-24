import { selectedDates, deselectedDates } from './InitializeDatetimePicker.js';

$('document').ready(function () {
    const modifyAvailabilityButton = $('#modify_availability_button');
    if (modifyAvailabilityButton.length === 0) {
        console.warn('Modify Availability button not found');
        return;
    }

    const token = $('input[name="__RequestVerificationToken"]').val();

    modifyAvailabilityButton.click(function (e) {
        e.preventDefault();

        const payload = BuildPayload();
        $.ajax({
            url: submitUrl,
            type: 'POST',
            headers: {
                'X-CSRF-TOKEN': token
            },
            data: {
                toAdd: payload.toAdd,
                toRemove: payload.toRemove,
                __RequestVerificationToken: token
            },
            success: function (response) {
                if (response.success) {
                    Swal.fire({
                        title: 'Success',
                        text: response.message,
                        icon: 'success',
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
                console.error(error)
            }
        });
    })

    function BuildPayload() {
        return {
            toAdd: Array.from(selectedDates.keys()),
            toRemove: Array.from(deselectedDates)
        };
    }
});