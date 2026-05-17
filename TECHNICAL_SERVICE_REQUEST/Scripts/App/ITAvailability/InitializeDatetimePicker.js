export const selectedDates = new Map();
export const deselectedDates = new Set();

$(document).ready(function() {
    const datepickerElement = $("#datetime_picker");
    if (datepickerElement.length === 0) {
        console.warn("Datepicker element not found.");
        return;
    }

    let badgeIdCounter = 1;

    const preloadedDates = new Map();
    let previousSelected = new Set();

    const TODAY = new Date().setHours(0, 0, 0, 0);
    const FUTURE = new Date();
    FUTURE.setMonth(FUTURE.getMonth() + 1);
    FUTURE.setHours(0, 0, 0, 0);

    datepickerElement.datepicker({
        format: "yyyy-mm-dd",
        multidate: true,
        multidateSeparator: ",",
        todayHighlight: true,
        autoclose: false,
        inline: true,
        container: "#datetime_picker",
        startDate: "+1d",
        yearRange: `${registrationYear}:2030`,
        beforeShowDay: function(date) {
            const d = new Date(date);
            d.setHours(0, 0, 0, 0); // Normalize time for comparison

            if (d < TODAY) {
                return {
                    enabled: false,
                    classes: "disabled-date",
                    tooltip: "Past dates are not selectable"
                }
            } else if (d > FUTURE) {
                return {
                    enabled: false,
                    classes: "disabled-date",
                    tootltip: "Cannot schedule dates later than 30 days from now"
                }
            }

            return true;
        }
    });

    const preloadedBlockedDates = $("#preloaded_blocked_dates");
    if (preloadedBlockedDates.length > 0 && preloadedBlockedDates.val().length > 0) {
        const blockedDatesArray = preloadedBlockedDates.val().split(",").map(d => d.trim());
        const validDates = blockedDatesArray.filter(function(stringDate) {
            const d = new Date(stringDate).setHours(0, 0, 0, 0);
            return d >= TODAY && d <= FUTURE
        });

        datepickerElement.datepicker("setDates", validDates);

        validDates.forEach(function(date) {
            preloadedDates.set(date, null); // Do not render badge for preloaded blocked dates
            previousSelected.add(date);
        });
    }

    datepickerElement.on("changeDate", function() {
        const currentSelected = new Set(
            datepickerElement.datepicker("getDates").map(formatDate)
        );

        // Added dates: current - previous
        currentSelected.forEach(function(date) {
            if (!previousSelected.has(date)) {
                const badgeId = preloadedDates.get(date);
                if (!badgeId) {
                    // Only add to selectedDates if it"s not a preloaded date
                    selectedDates.set(date, renderBadge(date));
                } else {
                    renderBadge(date, badgeId);
                }
                deselectedDates.delete(date);
            }
        });

        // Deselected dates: previous - current
        previousSelected.forEach(function(date) {
            if (!currentSelected.has(date)) {   // Handle all deselections
                const badgeId = selectedDates.get(date) ?? preloadedDates.get(date);
                if (badgeId) {
                    removeBadge(badgeId);
                }
                selectedDates.delete(date);

                // Only mark for DB delete if it was a preloaded date
                if (preloadedDates.has(date)) {
                    deselectedDates.add(date);
                }
            }
        });

        previousSelected = currentSelected;
    });

    function renderBadge(date, defaultId) {
        const container = $(".selected-date-badge-container > div");
        const id = defaultId ?? `selected-badge-${badgeIdCounter++}`;

        container.append(
            `<div id="${id}" class="selected-date-badge" data-formatted-date="${date}">
                <h3>${wordDate(new Date(date))}</h3>
            </div>`
        );

        const el = container.get(0);
        if (el) {
            container.scrollTop(el.scrollHeight);
        }

        return id;
    }

    function removeBadge(badgeId) {
        $(`#${badgeId}`).remove();
    }

    function formatDate(date) {
        const y = date.getFullYear();
        const m = String(date.getMonth() + 1).padStart(2, "0");
        const d = String(date.getDate()).padStart(2, "0");
        return `${y}-${m}-${d}`;
    }

    function wordDate(date) {
        return date.toLocaleDateString(undefined, {
            year: "numeric",
            month: "long",
            day: "numeric"
        });
    }

});
