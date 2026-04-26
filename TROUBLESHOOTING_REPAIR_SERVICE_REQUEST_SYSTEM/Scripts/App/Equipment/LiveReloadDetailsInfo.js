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
    const equipmentModelContainer = $(".equipment-model");
    if (assetTagContainer.length === 0 || equipmentModelContainer.length === 0) {
        console.warn("Asset tag or equipment model container not found");
    } else {
        hub.client.refreshEquipmentAssetTag = function (equipmentId, assetTag) {
            if (isCurrentEquipment(equipmentId)) {
                assetTagContainer.text(buildEquipmentLabel(assetTag));
            }
        };

        hub.client.refreshEquipmentModel = function (equipmentId, equipmentModel) {
            if (isCurrentEquipment(equipmentId)) {
                equipmentModelContainer.text(buildEquipmentLabel(equipmentModel));
            }
        };
    }

    // Type
    const equipmentTypeContainer = $(".equipment-type");
    if (equipmentTypeContainer.length === 0) {
        console.warn("Equipment type container not found");
    } else {
        hub.client.refreshEquipmentType = function (equipmentId, type) {
            if (isCurrentEquipment(equipmentId)) {
                equipmentTypeContainer.text(buildEquipmentLabel(type));
            }
        };
    }

    // Status
    const equipmentStatusContainer = $(".equipment-status");
    if (equipmentStatusContainer.length === 0) {
        console.warn("Equipment status container not found");
    } else {
        hub.client.refreshEquipmentStatus = function (equipmentId, status) {
            if (isCurrentEquipment(equipmentId)) {
                equipmentStatusContainer.text(buildEquipmentLabel(status));
            }
        };
    }

    // Building Number
    const buildingNumberContainer = $(".building-number");
    if (buildingNumberContainer.length === 0) {
        console.warn("Building number container not found");
    } else {
        hub.client.refreshEquipmentBuildingNumber = function (equipmentId, buildingNumber) {
            if (isCurrentEquipment(equipmentId)) {
                buildingNumberContainer.text(buildEquipmentLabel(buildingNumber));
            }
        };
    }

    // Floor Number
    const floorNumberContainer = $(".floor-number");
    if (floorNumberContainer.length === 0) {
        console.warn("Floor number container not found");
    } else {
        hub.client.refreshEquipmentFloorNumber = function (equipmentId, floorNumber) {
            if (isCurrentEquipment(equipmentId)) {
                floorNumberContainer.text(buildEquipmentLabel(floorNumber));
            }
        };
    }

    // Office
    const officeContainer = $(".office");
    if (officeContainer.length === 0) {
        console.warn("Office container not found");
    } else {
        hub.client.refreshEquipmentOffice = function (equipmentId, office) {
            if (isCurrentEquipment(equipmentId)) {
                officeContainer.text(buildEquipmentLabel(office));
            }
        };
    }

    // Repair Count
    const repairCountContainer = $(".repair-count");
    if (repairCountContainer.length === 0) {
        console.warn("Repair count container not found");
    } else {
        hub.client.refreshEquipmentRepairCount = function (equipmentId, repairCount) {
            if (isCurrentEquipment(equipmentId)) {
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
