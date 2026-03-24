/**
 * Toggle inputs' visibility for Zoom/Webex Link, Live Streaming Setup, & 
 * Audio/Visual Setup OR System Support Service Types
 */

$(document).ready(function () {
    const select = $("#technical_service_type_select");
    const otherServiceInput = $("#other_service_input");

    select.change(function () {
        const selectedOptionValue = parseInt($(this).val(), 10);
        // If nothing is selected, enable 'others' input
        if (isNaN(selectedOptionValue)) {
            otherServiceInput.prop("disabled", false);
        } else {
            otherServiceInput.prop("disabled", true);
        }

        const scheduleInputElements = $(".schedule-inputs");

        if (requiredScheduleTypeIds.indexOf(selectedOptionValue) !== -1) {
            scheduleInputElements.removeClass("no-display");
            scheduleInputElements.addClass("flex-child-center-h");
        } else {
            scheduleInputElements.addClass("no-display");
            scheduleInputElements.removeClass("flex-child-center-h");
        }
    })

    select.trigger("change");
})
