$(document).ready(function () {
    const assetTagInput = $("#asset-tag-input");
    const equipmentModelInput = $("#TechnicalServiceRequestEquipmentModel");
    const equipmentTypeSelect = $("#technical_service_request_equipment_type_select");
    const technicalServiceTypeSelect = $("#technical_service_type_select");
    const suggestionContainer = $("#asset-tag-suggestions");

    if (assetTagInput.length === 0 || suggestionContainer.length === 0) {
        return;
    }

    let debounceTimer = null;
    let requestSequence = 0;

    function isEquipmentServiceSelected() {
        const selectedType = parseInt(technicalServiceTypeSelect.val(), 10);
        return selectedType === parseInt(equipmentRepairTroubleshootId, 10);
    }

    function closeSuggestions() {
        suggestionContainer.empty();
        suggestionContainer.removeClass("open");
        suggestionContainer.attr("aria-hidden", "true");
    }

    function openSuggestions() {
        suggestionContainer.addClass("open");
        suggestionContainer.attr("aria-hidden", "false");
    }

    function renderSuggestions(items) {
        suggestionContainer.empty();

        if (!Array.isArray(items) || items.length === 0) {
            closeSuggestions();
            return;
        }

        // Render each suggestion item
        items.forEach(function (item) {
            const label = `${item.AssetTag} - ${item.EquipmentModel || "N/A"}`;
            const suggestionItem = $("<div></div>")
                .addClass("suggestion-item")
                .attr("data-asset-tag", item.AssetTag || "")
                .attr("data-model", item.EquipmentModel || "")
                .attr("data-equipment-type-id", item.EquipmentTypeId || "")
                .text(label);

            suggestionContainer.append(suggestionItem);
        });

        openSuggestions();
    }

    function fetchSuggestions(assetTag) {
        requestSequence += 1;
        const currentRequest = requestSequence;

        $.ajax({
            url: equipmentSuggestionUrl,
            type: "GET",
            data: {
                assetTag: assetTag
            },
            success: function (response) {
                if (currentRequest !== requestSequence) {
                    return;
                }

                if (!response || !response.success) {
                    closeSuggestions();
                    return;
                }

                renderSuggestions(response.data || []);
            },
            error: function () {
                if (currentRequest !== requestSequence) {
                    return;
                }

                closeSuggestions();
            }
        });
    }

    // Fetch suggestions when the user types in the asset tag input
    assetTagInput.on("input", function () {
        const rawValue = assetTagInput.val();
        const normalizedQuery = (rawValue || "").toString().trim();

        if (!isEquipmentServiceSelected()) {
            closeSuggestions();
            return;
        }

        if (normalizedQuery.length < 2) {
            closeSuggestions();
            return;
        }

        if (debounceTimer) {
            clearTimeout(debounceTimer);
        }

        debounceTimer = setTimeout(function () {
            fetchSuggestions(normalizedQuery);
        }, 350);
    });

    // When a suggestion is selected, populate the input fields and close the suggestions
    suggestionContainer.on("click", ".suggestion-item", function () {
        const selectedSuggestion = $(this);
        const selectedAssetTag = selectedSuggestion.attr("data-asset-tag") || "";
        const selectedModel = selectedSuggestion.attr("data-model") || "";
        const selectedEquipmentTypeId = selectedSuggestion.attr("data-equipment-type-id") || "";

        assetTagInput.val(selectedAssetTag).trigger("change");
        equipmentModelInput.val(selectedModel).trigger("change");

        if (selectedEquipmentTypeId !== "") {
            equipmentTypeSelect.val(selectedEquipmentTypeId).trigger("change");
        }

        closeSuggestions();
    });

    // Close suggestions when the technical service type changes and it's not equipment service
    technicalServiceTypeSelect.on("change", function () {
        if (!isEquipmentServiceSelected()) {
            closeSuggestions();
        }
    });

    // Close suggestions when clicking outside the suggestion container
    $(document).on("click", function (event) {
        const target = $(event.target);
        if (!target.closest(".asset-tag-wrapper").length) {
            closeSuggestions();
        }
    });
});