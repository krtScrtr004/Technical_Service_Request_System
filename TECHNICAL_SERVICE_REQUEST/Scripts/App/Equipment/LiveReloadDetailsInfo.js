$(document).ready(function () {
    const hub = $.connection.equipmentHub;
    if (!hub) {
        return;
    }

    function buildEquipmentLabel(value) {
        const normalizedValue = (value || "").toString().trim();
        return normalizedValue || "N/A";
    }

    function isCurrentEquipment(equipmentId) {
        return currentEquipmentId !== null && parseInt(equipmentId, 10) === currentEquipmentId;
    }

    // Asset Tag / Model
    const assetTagContainer = $(".asset-tag");
    const modelContainer = $(".model");
    if (assetTagContainer.length === 0 || equipmentModelContainer.length === 0) {
        console.warn("Asset tag or equipment model container not found");
    } else {
        hub.client.refreshEquipmentAssetTag = function (id, assetTag) {
            if (isCurrentEquipment(id)) {
                assetTagContainer.text(buildEquipmentLabel(assetTag));
            }
        };

        hub.client.refreshEquipmentModel = function (id, model) {
            if (isCurrentEquipment(id)) {
                modelContainer.text(buildEquipmentLabel(model));
            }
        };
    }

    // Type
    const equipmentTypeContainer = $(".equipment-type > .label");
    if (equipmentTypeContainer.length === 0) {
        console.warn("Equipment type container not found");
    } else {
        hub.client.refreshEquipmentType = function (id, type) {
            if (isCurrentEquipment(id)) {
                equipmentTypeContainer.text(buildEquipmentLabel(type));
            }
        };
    }

    // Status
    const equipmentStatusContainer = $(".equipment-status > .label");
    if (equipmentStatusContainer.length === 0) {
        console.warn("Equipment status container not found");
    } else {
        hub.client.refreshEquipmentStatus = function (id, status) {
            if (isCurrentEquipment(id)) {
                equipmentStatusContainer.text(buildEquipmentLabel(status));
            }
        };
    }

    // Building Number
    const buildingNumberContainer = $(".building-number");
    if (buildingNumberContainer.length === 0) {
        console.warn("Building number container not found");
    } else {
        hub.client.refreshEquipmentBuildingNumber = function (id, buildingNumber) {
            if (isCurrentEquipment(id)) {
                buildingNumberContainer.text(buildEquipmentLabel(buildingNumber));
            }
        };
    }

    // Floor Number
    const floorNumberContainer = $(".floor-number");
    if (floorNumberContainer.length === 0) {
        console.warn("Floor number container not found");
    } else {
        hub.client.refreshEquipmentFloorNumber = function (id, floorNumber) {
            if (isCurrentEquipment(id)) {
                floorNumberContainer.text(buildEquipmentLabel(floorNumber));
            }
        };
    }

    // Office
    const officeContainer = $(".office");
    if (officeContainer.length === 0) {
        console.warn("Office container not found");
    } else {
        hub.client.refreshEquipmentOffice = function (id, office) {
            if (isCurrentEquipment(id)) {
                officeContainer.text(buildEquipmentLabel(office));
            }
        };
    }

    // Repair Count
    const repairCountContainer = $(".repair-count");
    if (repairCountContainer.length === 0) {
        console.warn("Repair count container not found");
    } else {
        hub.client.refreshEquipmentRepairCount = function (id, repairCount) {
            if (isCurrentEquipment(id)) {
                repairCountContainer.text(buildEquipmentLabel(repairCount));
            }
        };
    }

    // Start the SignalR connection
    $.connection.hub.start().done(function () {
        console.log('SignalR connected');
    }).fail(function (error) {
        console.error('SignalR connection failed:', error);
    });
});
