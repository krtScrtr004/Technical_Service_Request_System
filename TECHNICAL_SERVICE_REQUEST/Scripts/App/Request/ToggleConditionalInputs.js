/**
 * Toggle inputs' visibility for Zoom/Webex Link, Live Streaming Setup, & 
 * Audio/Visual Setup OR System Support Service Types
 */

$(document).ready(function () {
    const select = $("#technical_service_type_select");
    const otherServiceInput = $("#other_service_input");

    const equipmentInputElements = $(".equipment-inputs");
    const scheduleInputElements = $(".schedule-inputs");

    select.change(function () {
        const selectedOptionValue = parseInt($(this).val(), 10);

        // If nothing is selected, enable 'others' input
        if (isNaN(selectedOptionValue) || selectedOptionValue < 0) {
            otherServiceInput.prop("disabled", false);
        } else {
            otherServiceInput.prop("disabled", true);
        }

        // Toggle visibility of equipment inputs based on selected service type
        if (selectedOptionValue === parseInt(equipmentRepairTroubleshootId, 10)) {
            equipmentInputElements.removeClass("no-display");
            equipmentInputElements.addClass("flex-wrap", "flex-row");
        } else {
            equipmentInputElements.addClass("no-display");
            equipmentInputElements.removeClass("flex-wrap", "flex-row");
        }

        // Toggle visibility of schedule inputs based on selected service type
        if (requiredScheduleTypeIds.indexOf(selectedOptionValue) !== -1) {
            scheduleInputElements.removeClass("no-display");
            scheduleInputElements.addClass("flex-row");
        } else {
            scheduleInputElements.addClass("no-display");
            scheduleInputElements.removeClass("flex-row");
        }
    })

    select.trigger("change");
})
