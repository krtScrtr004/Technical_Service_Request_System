$(document).ready(function () {
    const datepickerElement = $("#datetime_picker");
    if (datepickerElement.length === 0) {
        Swal.fire({
            title: "Error",
            text: "An error occured. Please try again.",
            icon: "error"
        });
        console.error("Datepicker element not found.");
        return;
    }

    // Fetch fully booked days for the initially selected service type
    const selectedServiceTypeId = $("#technical_service_type_select");
    if (selectedServiceTypeId.length === 0) {
        Swal.fire({
            title: "Error",
            text: "An error occured. Please try again.",
            icon: "error"
        });
        console.error("Service type select element not found.");
        return;
    }

    let fullyBookedDaysByLimit = [];
    let fullyBookedDaysByTimespan = [];

    // Initial fetch (timespan-based, e.g. 8AM-4PM)
    createGetRequest(fullyBookedDayByScheduleUrl, {}, function (dates) {
        fullyBookedDaysByTimespan = dates ?? [];
        initializeDateTimePicker();
    });

    // On service type change (limit-based)
    selectedServiceTypeId.on("change", function () {
        createGetRequest(fullyBookedDayByLimitUrl, {
            scheduleServiceTypeId: selectedServiceTypeId.val()
        }, function (dates) {
            fullyBookedDaysByLimit = dates ?? [];
            initializeDateTimePicker();
        });
    });

    function createGetRequest(url, payload, callback) {
        $.ajax({
            url: url,
            type: "GET",
            data: payload,
            success: function (response) {
                if (response?.success) {
                    callback(response.dates ?? []);
                }
            },
            error: function (xhr, status, error) {
                Swal.fire({
                    title: "Error",
                    text: "An error occured. Please try again.",
                    icon: "error"
                });
                console.error(error);
            }
        });
    }

    function initializeDateTimePicker() {
        // Destroy previous datepicker instance if it exists
        datepickerElement.datepicker('destroy');

        const MAX = new Date();
        MAX.setMonth(MAX.getMonth() + 1);
        MAX.setHours(0, 0, 0, 0);

        // Merge fully booked day arrays, remove duplicates
        const allFullyBooked = Array.from(new Set([
            ...fullyBookedDaysByLimit,
            ...fullyBookedDaysByTimespan]
        ));

        datepickerElement.datepicker({
            format: "yyyy-mm-dd",
            todayHighlight: true,
            autoclose: false,
            container: "#datetime_picker",
            startDate: "+8d",
            beforeShowDay: function (date) {
                const d = new Date(date);
                d.setHours(0, 0, 0, 0);

                const yyyy = date.getFullYear();
                const mm = String(date.getMonth() + 1).padStart(2, "0");
                const dd = String(date.getDate()).padStart(2, "0");
                const dateString = `${yyyy}-${mm}-${dd}`;

                if (allFullyBooked.includes(dateString)) {
                    return {
                        enabled: false,
                        classes: "disabled-date",
                        tooltip: "This date is already full"
                    }
                } else if (d > MAX) {
                    return {
                        enabled: false,
                        classes: "disabled-date",
                        tooltip: "Cannot schedule dates later than 30 days from now"
                    };
                }

                return true;
            }
        });

        const hiddenInput = $("#TechnicalServiceRequestScheduledDate");
        if (hiddenInput.length === 0) {
            Swal.fire({
                title: "Error",
                text: "An error occured. Please try again.",
                icon: "error"
            });
            console.error("Hidden schedule input is not found.");
            return;
        }

        // Update hidden schedule input with the one selected in the calendar
        datepickerElement.on("changeDate", function () {
            const formatted = datepickerElement.datepicker("getFormattedDate");
            hiddenInput.val(formatted);
        });
    }
});