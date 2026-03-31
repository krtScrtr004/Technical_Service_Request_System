$(document).ready(function () {
    const yearSelect = $("#history_year_select");
    const monthSelect = $("#history_month_select");
    const calendarContainer = $("#history_calendar_container");

    const MONTHS = [
        "January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December"
    ];

    const currentYear = new Date().getFullYear();
    const currentMonth = new Date().getMonth();
    const yearsToShow = currentYear - (registrationYear - 1); // Show only years from registration year to current year

    let selectedYear = currentYear;
    let selectedMonth = currentMonth;

    // Render year select options
    function renderYearSelect() {
        yearSelect.empty();
        for (let y = currentYear; y > currentYear - yearsToShow; y--) {
            yearSelect.append($('<option>', {
                value: y,
                text: y,
                selected: y === selectedYear
            }));
        }
    }

    // Render month select options
    function renderMonthSelect() {
        monthSelect.empty();
        for (let m = 0; m < 12; m++) {
            monthSelect.append($('<option>', {
                value: m,
                text: MONTHS[m],
                selected: m === selectedMonth
            }));
        }
    }

    function fetchAndRenderCalendar() {
        if (!getBlockedDatesUrl) {
            console.error("Url for fetching blocked dates is not defined.");
            return;
        }

        $.ajax({
            url: getBlockedDatesUrl,
            type: "GET",
            data: {
                month: selectedMonth + 1, // Adjust for 1-based month indexing in backend
                year: selectedYear
            },
            success: function (response) {
                if (response?.success) {
                    renderCalendar(
                        selectedYear,
                        selectedMonth,
                        response?.dates ?? []
                    );
                }
            }, 
            error: function (xhr, status, error) {
                console.error(error)
            }
        });

    }

    function renderCalendar(year, month, blockedDates) {
        calendarContainer.empty();

        const firstDay = new Date(year, month, 1);
        const lastDay = new Date(year, month + 1, 0);
        const startDay = firstDay.getDay();
        const daysInMonth = lastDay.getDate();

        let html = '<table class="table calendar-table table-striped"><thead><tr>';
        ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"]
            .forEach(function (d) {
                html += `<th>${d}</th>`;
            });

        html += '</tr></thead><tbody><tr>';

        // Fill initial empty cells
        let dayOfWeek = 0;
        for (let i = 0; i < startDay; i++) {
            html += '<td></td>';
            dayOfWeek++;
        }

        // Fill days of the month
        for (let day = 1; day <= daysInMonth; day++) {
            const dateStr = `${year}-${String(month + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`;

            // Check if the date is blocked and apply the appropriate class
            const isBlockedClass = blockedDates.includes(dateStr) ? "blocked-date" : "";
            const isBlockedTitle = blockedDates.includes(dateStr) ? "You have blocked this date" : "";
            html += `<td class="${isBlockedClass} text-center" title="${isBlockedTitle}">
                        ${day}
                    </td>`;

            dayOfWeek++;
            // Start a new row after Saturday
            if (dayOfWeek === 7) {
                html += '</tr><tr>';
                dayOfWeek = 0;
            }
        }

        // Fill remaining empty cells at the end of the month
        while (dayOfWeek > 0 && dayOfWeek < 7) {
            html += '<td></td>';
            dayOfWeek++;
        }
        html += '</tr></tbody></table>';

        calendarContainer.html(html);
    }

    // Year select handler
    yearSelect.on('change', function () {
        selectedYear = parseInt($(this).val());
        fetchAndRenderCalendar();
    });

    // Month select handler
    monthSelect.on('change', function () {
        selectedMonth = parseInt($(this).val(), 10);
        fetchAndRenderCalendar();
    });

    // Initialize
    if (yearSelect.length && monthSelect.length && calendarContainer.length) {
        renderYearSelect();
        renderMonthSelect();
        fetchAndRenderCalendar();
    }
});