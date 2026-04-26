using System.Collections.Generic;
using System.Web.Mvc;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Enumerables
{
    public static class ValueConstants
    {
        public const string VALID_NAME_REGEX = "^[A-Za-z\\s\\-.,'']+$";
        public const string VALID_NAME_REGEX_MESSAGE = "Name must only contain letters, spaces, hyphens ( - ), dots ( . ), commas ( , ), and apostrophes ( ' )";

        public const string VALID_EMAIL_REGEX = "^[a-zA-Z0-9._%+\\-!#$&'*\\/=?^{|}~]+@[a-zA-Z0-9-]+(\\.[a-zA-Z0-9-]+)*\\.[a-zA-Z]{2,}$";
        public const string VALID_EMAIL_REGEX_MESSAGE = "Email must contain a valid local part, followed by @, and a valid domain with a 2-or-more letter extension (e.g., .com, .ph)";

        public const string VALID_CONTACT_NUMBER_REGEX = "^[+\\d\\s-]{7,20}$";
        public const string VALID_CONTACT_NUMBER_REGEX_MESSAGE = "Contact number must be between 7 and 20 characters and can include digits, spaces, dashes, and an optional leading plus sign";
    }


    public static class AccountTypeEnum
    {
        public const int ADMIN = 1;
        public const int IT = 2;
        public const int STANDARD = 3;

        public static string DisplayName(this int accountTypeId)
        {
            switch (accountTypeId)
            {
                case 1:
                    return "Administrator";
                case 2:
                    return "IT";
                case 3:
                    return "Standard User";
                default:
                    return "Unknown Account Type";
            }
        }

        public static bool IsAdmin(int accountTypeId)
        {
            return accountTypeId == ADMIN ? true : false;
        }

        public static bool IsAdmin(string accountTypeName)
        {
            return (accountTypeName == DisplayName(ADMIN)) ? true : false;
        }

        public static bool IsAdmin(int[] accountTypeIds)
        {
            foreach (var accountTypeId in accountTypeIds)
            {
                if (accountTypeId == ADMIN)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsIT(int accountTypeId)
        {
            return accountTypeId == IT ? true : false;
        }

        public static bool IsIT(string accountTypeName)
        {
            return (accountTypeName == DisplayName(IT)) ? true : false;
        }

        public static bool IsIT(int[] accountTypeIds)
        {
            foreach (var accountTypeId in accountTypeIds)
            {
                if (accountTypeId == IT)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsStandard(int accountTypeId)
        {
            return accountTypeId == STANDARD ? true : false;
        }

        public static bool IsStandard(string accountTypeName)
        {
            return (accountTypeName == DisplayName(STANDARD)) ? true : false;
        }

        public static bool IsStandard(int[] accountTypeIds)
        {
            foreach (var accountTypeId in accountTypeIds)
            {
                if (accountTypeId == STANDARD)
                {
                    return true;
                }
            }
            return false;
        }
    }


    public static class EquipmentCategoryEnum
    {
        public const int COMPUTING_DEVICES = 1;
        public const int INPUT_DEVICES = 2;
        public const int OUTPUT_DEVICES = 3;
        public const int NETWORK_CONNECTIVITY = 4;
        public const int STORAGE_DEVICES = 5;
        public const int POWER_ELECTRICAL = 6;
        public const int COMMUNICATION_DEVICES = 7;
        public const int SECURITY_EQUIPMENT = 8;
        public const int PERIPHERALS_ACCESSORIES = 9;
        public const int SERVER_ROOM_DATA_CENTER = 10;

        public static string DisplayName(this int categoryId)
        {
            switch (categoryId)
            {
                case 1:
                    return "Computing Devices";
                case 2:
                    return "Input Devices";
                case 3:
                    return "Output Devices";
                case 4:
                    return "Network Connectivity";
                case 5:
                    return "Storage Devices";
                case 6:
                    return "Power / Electrical";
                case 7:
                    return "Communication Devices";
                case 8:
                    return "Security Equipment";
                case 9:
                    return "Peripherals / Accessories";
                case 10:
                    return "Server Room / Data Center";
                default:
                    return "Unknown Category";
            }
        }
    }

    public static class EquipmentTypeEnum
    {
        public const int DESKTOP_COMPUTER = 1;
        public const int LAPTOP_NOTEBOOK = 2;
        public const int WORKSTATION = 3;
        public const int THIN_CLIENT = 4;
        public const int SERVER = 5;
        public const int TABLET_IPAD = 6;
        public const int KEYBOARD = 7;
        public const int MOUSE = 8;
        public const int TRACKPAD_TRACKBALL = 9;
        public const int BARCODE_SCANNER = 10;
        public const int FINGERPRINT_SCANNER_BIOMETRIC_READER = 11;
        public const int WEBCAM = 12;
        public const int MICROPHONE = 13;
        public const int DRAWING_TABLET_STYLUS = 14;
        public const int MONITOR_DISPLAY_SCREEN = 15;
        public const int PRINTER_INKJET_LASER_THERMAL = 16;
        public const int PLOTTER = 17;
        public const int LABEL_PRINTER = 18;
        public const int RECEIPT_PRINTER = 19;
        public const int PROJECTOR_LCD_PROJECTOR = 20;
        public const int INTERACTIVE_WHITEBOARD_SMART_BOARD = 21;
        public const int ROUTER = 22;
        public const int NETWORK_SWITCH = 23;
        public const int WIRELESS_ACCESS_POINT_WAP = 24;
        public const int MODEM = 25;
        public const int FIREWALL_APPLIANCE = 26;
        public const int NETWORK_ATTACHED_STORAGE_NAS = 27;
        public const int PATCH_PANEL = 28;
        public const int VOIP_PHONE_IP_PHONE = 29;
        public const int EXTERNAL_HARD_DRIVE = 30;
        public const int USB_FLASH_DRIVE = 31;
        public const int SOLID_STATE_DRIVE_SSD = 32;
        public const int NETWORK_STORAGE_DEVICE = 33;
        public const int TAPE_DRIVE_BACKUP_DEVICE = 34;
        public const int UPS_UNINTERRUPTIBLE_POWER_SUPPLY = 35;
        public const int POWER_STRIP_SURGE_PROTECTOR = 36;
        public const int PDU_POWER_DISTRIBUTION_UNIT = 37;
        public const int GENERATOR = 38;
        public const int TELEPHONE_DESK_PHONE = 39;
        public const int MOBILE_PHONE_SMARTPHONE = 40;
        public const int FAX_MACHINE = 41;
        public const int TWO_WAY_RADIO_WALKIE_TALKIE = 42;
        public const int VIDEO_CONFERENCING_EQUIPMENT = 43;
        public const int CCTV_IP_CAMERA = 44;
        public const int ACCESS_CONTROL_DEVICE = 45;
        public const int CARD_READER_KEY_FOB = 46;
        public const int ALARM_SYSTEM_PANEL = 47;
        public const int DOCKING_STATION = 48;
        public const int KVM_SWITCH = 49;
        public const int USB_HUB = 50;
        public const int CARD_READER_SD_CF = 51;
        public const int COOLING_PAD_FAN = 52;
        public const int RACK_CABINET = 53;
        public const int KVM_OVER_IP = 54;
        public const int SERVER_PHYSICAL_VIRTUAL_HOST = 55;
        public const int SAN_STORAGE_AREA_NETWORK = 56;
        public const int BLADE_SERVER = 57;

        public static string DisplayName(this int typeId)
        {
            switch (typeId)
            {
                case 1:
                    return "Desktop Computer";
                case 2:
                    return "Laptop / Notebook";
                case 3:
                    return "Workstation";
                case 4:
                    return "Thin Client";
                case 5:
                    return "Server";
                case 6:
                    return "Tablet / iPad";
                case 7:
                    return "Keyboard";
                case 8:
                    return "Mouse";
                case 9:
                    return "Trackpad / Trackball";
                case 10:
                    return "Barcode Scanner";
                case 11:
                    return "Fingerprint Scanner / Biometric Reader";
                case 12:
                    return "Webcam";
                case 13:
                    return "Microphone";
                case 14:
                    return "Drawing Tablet / Stylus";
                case 15:
                    return "Monitor / Display Screen";
                case 16:
                    return "Printer (Inkjet, Laser, Thermal)";
                case 17:
                    return "Plotter";
                case 18:
                    return "Label Printer";
                case 19:
                    return "Receipt Printer";
                case 20:
                    return "Projector / LCD Projector";
                case 21:
                    return "Interactive Whiteboard / Smart Board";
                case 22:
                    return "Router";

                // ── Network & Connectivity (cont.) ──────────────────────────────────
                case 23:
                    return "Network Switch";
                case 24:
                    return "Wireless Access Point (WAP)";
                case 25:
                    return "Modem";
                case 26:
                    return "Firewall Appliance";
                case 27:
                    return "Network Attached Storage (NAS)";
                case 28:
                    return "Patch Panel";
                case 29:
                    return "VoIP Phone / IP Phone";

                // ── Storage Devices ─────────────────────────────────────────────────
                case 30:
                    return "External Hard Drive";
                case 31:
                    return "USB Flash Drive";
                case 32:
                    return "Solid State Drive (SSD)";
                case 33:
                    return "Network Storage Device";
                case 34:
                    return "Tape Drive / Backup Device";

                // ── Power & Electrical ──────────────────────────────────────────────
                case 35:
                    return "UPS (Uninterruptible Power Supply)";
                case 36:
                    return "Power Strip / Surge Protector";
                case 37:
                    return "PDU (Power Distribution Unit)";
                case 38:
                    return "Generator";

                // ── Communication Devices ───────────────────────────────────────────
                case 39:
                    return "Telephone / Desk Phone";
                case 40:
                    return "Mobile Phone / Smartphone";
                case 41:
                    return "Fax Machine";
                case 42:
                    return "Two-Way Radio / Walkie-Talkie";
                case 43:
                    return "Video Conferencing Equipment";

                // ── Security Equipment ──────────────────────────────────────────────
                case 44:
                    return "CCTV / IP Camera";
                case 45:
                    return "Access Control Device";
                case 46:
                    return "Card Reader / Key Fob";
                case 47:
                    return "Alarm System Panel";

                // ── Peripherals & Accessories ───────────────────────────────────────
                case 48:
                    return "Docking Station";
                case 49:
                    return "KVM Switch";
                case 50:
                    return "USB Hub";
                case 51:
                    return "Card Reader (SD / CF)";
                case 52:
                    return "Cooling Pad / Fan";

                // ── Server Room / Data Center ───────────────────────────────────────
                case 53:
                    return "Rack Cabinet";
                case 54:
                    return "KVM over IP";
                case 55:
                    return "Blade Server";
                case 56:
                    return "SAN (Storage Area Network)";

                // ── Speakers & Audio ────────────────────────────────────────────────
                case 57:
                    return "Speakers / Headset";

                default:
                    return "Unknown Equipment Type";
            }
        }

        public static List<int> GetComputingDeviceIds()
        {
            return new List<int>
            {
                DESKTOP_COMPUTER,
                LAPTOP_NOTEBOOK,
                WORKSTATION,
                THIN_CLIENT,
                SERVER,
                TABLET_IPAD
            };
        }

        public static List<int> GetInputDeviceIds()
        {
            return new List<int>
            {
                KEYBOARD,
                MOUSE,
                TRACKPAD_TRACKBALL,
                BARCODE_SCANNER,
                FINGERPRINT_SCANNER_BIOMETRIC_READER,
                WEBCAM,
                MICROPHONE,
                DRAWING_TABLET_STYLUS
            };
        }

        public static List<int> GetOutputDeviceIds()
        {
            return new List<int>
            {
                MONITOR_DISPLAY_SCREEN,
                PRINTER_INKJET_LASER_THERMAL,
                PLOTTER,
                LABEL_PRINTER,
                RECEIPT_PRINTER,
                PROJECTOR_LCD_PROJECTOR,
                INTERACTIVE_WHITEBOARD_SMART_BOARD
            };
        }

        public static List<int> GetNetworkConnectivityIds()
        {
            return new List<int>
            {
                ROUTER,
                NETWORK_SWITCH,
                WIRELESS_ACCESS_POINT_WAP,
                MODEM,
                FIREWALL_APPLIANCE,
                NETWORK_ATTACHED_STORAGE_NAS,
                PATCH_PANEL,
                VOIP_PHONE_IP_PHONE
            };
        }

        public static List<int> GetStorageDeviceIds()
        {
            return new List<int>
            {
                EXTERNAL_HARD_DRIVE,
                USB_FLASH_DRIVE,
                SOLID_STATE_DRIVE_SSD,
                NETWORK_STORAGE_DEVICE,
                TAPE_DRIVE_BACKUP_DEVICE
            };
        }

        public static List<int> GetPowerElectricalIds()
        {
            return new List<int>
            {
                UPS_UNINTERRUPTIBLE_POWER_SUPPLY,
                POWER_STRIP_SURGE_PROTECTOR,
                PDU_POWER_DISTRIBUTION_UNIT,
                GENERATOR
            };
        }

        public static List<int> GetCommunicationDeviceIds()
        {
            return new List<int>
            {
                TELEPHONE_DESK_PHONE,
                MOBILE_PHONE_SMARTPHONE,
                FAX_MACHINE,
                TWO_WAY_RADIO_WALKIE_TALKIE,
                VIDEO_CONFERENCING_EQUIPMENT
            };
        }

        public static List<int> GetSecurityEquipmentIds()
        {
            return new List<int>
            {
                CCTV_IP_CAMERA,
                ACCESS_CONTROL_DEVICE,
                CARD_READER_KEY_FOB,
                ALARM_SYSTEM_PANEL
            };
        }

        public static List<int> GetPeripheralsAccessoryIds()
        {
            return new List<int>
            {
                DOCKING_STATION,
                KVM_SWITCH,
                USB_HUB,
                CARD_READER_SD_CF,
                COOLING_PAD_FAN
            };
        }

        public static List<int> GetServerRoomDataCenterIds()
        {
            return new List<int>
            {
                RACK_CABINET,
                KVM_OVER_IP,
                SERVER_PHYSICAL_VIRTUAL_HOST,
                SAN_STORAGE_AREA_NETWORK,
                BLADE_SERVER
            };
        }

        public static List<SelectListItem> GetSelectListItems()
        {
            var computingGroup = new SelectListGroup { Name = EquipmentCategoryEnum.DisplayName(EquipmentCategoryEnum.COMPUTING_DEVICES) };
            var inputGroup = new SelectListGroup { Name = EquipmentCategoryEnum.DisplayName(EquipmentCategoryEnum.INPUT_DEVICES) };
            var outputGroup = new SelectListGroup { Name = EquipmentCategoryEnum.DisplayName(EquipmentCategoryEnum.OUTPUT_DEVICES) };
            var networkGroup = new SelectListGroup { Name = EquipmentCategoryEnum.DisplayName(EquipmentCategoryEnum.NETWORK_CONNECTIVITY) };
            var storageGroup = new SelectListGroup { Name = EquipmentCategoryEnum.DisplayName(EquipmentCategoryEnum.STORAGE_DEVICES) };
            var powerGroup = new SelectListGroup { Name = EquipmentCategoryEnum.DisplayName(EquipmentCategoryEnum.POWER_ELECTRICAL) };
            var communicationGroup = new SelectListGroup { Name = EquipmentCategoryEnum.DisplayName(EquipmentCategoryEnum.COMMUNICATION_DEVICES) };
            var securityGroup = new SelectListGroup { Name = EquipmentCategoryEnum.DisplayName(EquipmentCategoryEnum.SECURITY_EQUIPMENT) };
            var peripheralGroup = new SelectListGroup { Name = EquipmentCategoryEnum.DisplayName(EquipmentCategoryEnum.PERIPHERALS_ACCESSORIES) };
            var serverGroup = new SelectListGroup { Name = EquipmentCategoryEnum.DisplayName(EquipmentCategoryEnum.SERVER_ROOM_DATA_CENTER) };

            var equipmentTypeOptions = new List<SelectListItem>();

            // Computing Devices
            foreach (var id in EquipmentTypeEnum.GetComputingDeviceIds())
            {
                equipmentTypeOptions.Add(new SelectListItem
                {
                    Value = id.ToString(),
                    Text = EquipmentTypeEnum.DisplayName(id),
                    Group = computingGroup
                });
            }

            // Input Devices
            foreach (var id in EquipmentTypeEnum.GetInputDeviceIds())
            {
                equipmentTypeOptions.Add(new SelectListItem
                {
                    Value = id.ToString(),
                    Text = EquipmentTypeEnum.DisplayName(id),
                    Group = inputGroup
                });
            }

            // Output Devices
            foreach (var id in EquipmentTypeEnum.GetOutputDeviceIds())
            {
                equipmentTypeOptions.Add(new SelectListItem
                {
                    Value = id.ToString(),
                    Text = EquipmentTypeEnum.DisplayName(id),
                    Group = outputGroup
                });
            }

            // Network & Connectivity
            foreach (var id in EquipmentTypeEnum.GetNetworkConnectivityIds())
            {
                equipmentTypeOptions.Add(new SelectListItem
                {
                    Value = id.ToString(),
                    Text = EquipmentTypeEnum.DisplayName(id),
                    Group = networkGroup
                });
            }

            // Storage Devices
            foreach (var id in EquipmentTypeEnum.GetStorageDeviceIds())
            {
                equipmentTypeOptions.Add(new SelectListItem
                {
                    Value = id.ToString(),
                    Text = EquipmentTypeEnum.DisplayName(id),
                    Group = storageGroup
                });
            }

            // Power / Electrical
            foreach (var id in EquipmentTypeEnum.GetPowerElectricalIds())
            {
                equipmentTypeOptions.Add(new SelectListItem
                {
                    Value = id.ToString(),
                    Text = EquipmentTypeEnum.DisplayName(id),
                    Group = powerGroup
                });
            }

            // Communication Devices
            foreach (var id in EquipmentTypeEnum.GetCommunicationDeviceIds())
            {
                equipmentTypeOptions.Add(new SelectListItem
                {
                    Value = id.ToString(),
                    Text = EquipmentTypeEnum.DisplayName(id),
                    Group = communicationGroup
                });
            }

            // Security Equipment
            foreach (var id in EquipmentTypeEnum.GetSecurityEquipmentIds())
            {
                equipmentTypeOptions.Add(new SelectListItem
                {
                    Value = id.ToString(),
                    Text = EquipmentTypeEnum.DisplayName(id),
                    Group = securityGroup
                });
            }

            // Peripherals & Accessories
            foreach (var id in EquipmentTypeEnum.GetPeripheralsAccessoryIds())
            {
                equipmentTypeOptions.Add(new SelectListItem
                {
                    Value = id.ToString(),
                    Text = EquipmentTypeEnum.DisplayName(id),
                    Group = peripheralGroup
                });
            }

            // Server Room / Data Center
            foreach (var id in EquipmentTypeEnum.GetServerRoomDataCenterIds())
            {
                equipmentTypeOptions.Add(new SelectListItem
                {
                    Value = id.ToString(),
                    Text = EquipmentTypeEnum.DisplayName(id),
                    Group = serverGroup
                });
            }

            return equipmentTypeOptions;
        }
    }

    public static class EquipmentStatusEnum 
    {
        public const int OPERATIONAL = 1;
        public const int UNDER_REPAIR = 2;
        public const int INACTIVE = 3;
        public const int FOR_DISPOSAL = 4;

        public static string DisplayName(this int statusId)
        {
            switch (statusId)
            {
                case 1:
                    return "Operational";
                case 2:
                    return "Under Repair";
                case 3:
                    return "Inactive";
                case 4:
                    return "For Disposal";
                default:
                    return "Unknown Status";
            }
        }

        public static List<int> GetActiveIds()
        {
            return new List<int>
            {
                OPERATIONAL, UNDER_REPAIR
            };
        }

        public static List<int> GetInActiveIds()
        {
            return new List<int> 
            {
                INACTIVE, FOR_DISPOSAL
            };
        }

        public static List<SelectListItem> GetSelectListItems()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = OPERATIONAL.ToString(), Text = DisplayName(OPERATIONAL) },
                new SelectListItem { Value = UNDER_REPAIR.ToString(), Text = DisplayName(UNDER_REPAIR) },
                new SelectListItem { Value = INACTIVE.ToString(), Text = DisplayName(INACTIVE) },
                new SelectListItem { Value = FOR_DISPOSAL.ToString(), Text = DisplayName(FOR_DISPOSAL) }
            };
        }
    }


    public static class TechnicalServiceTypeEnum
    {
        public const int EQUIPMENT_REPAIR_TROUBLESHOOTING = 1;
        public const int ZOOM_WEBEX_LINK = 2;
        public const int GOVERNMENT_EMAIL_ACCOUNT = 3;
        public const int AUDIO_VISUAL_SETUP = 4;
        public const int INTERNET_CONNECTIVITY = 5;
        public const int LIVESTREAM_SETUP = 6;
        public const int ACCOUNT_CREATION = 7;
        public const int PASSWORD_ACCESS_ISSUE = 8;
        public const int SYSTEM_SUPPORT = 9;
        public const int DATA_CORRECTION = 10;
        public const int TECHNICAL_GUIDANCE = 11;
        public const int PRODUCTION_MATERIAL_PRINTING = 12;

        public static string DisplayName(this int typeId)
        {
            switch (typeId)
            {
                case 1:
                    return "Equipment Repair / Troubleshooting";
                case 2:
                    return "Zoom / Webex Link";
                case 3:
                    return "Government Email Account";
                case 4:
                    return "Audio Visual Setup";
                case 5:
                    return "Internet Connectivity";
                case 6:
                    return "Livestream Setup";
                case 7:
                    return "Account Creation";
                case 8:
                    return "Password / Access Issue";
                case 9:
                    return "System Support";
                case 10:
                    return "Data Correction";
                case 11:
                    return "Technical Guidance";
                case 12:
                    return "Production Material Printing";
                default:
                    return "Unknown Service Type";
            }
        }

        public static List<int> GetScheduledServiceIds()
        {
            return new List<int>
            {
                AUDIO_VISUAL_SETUP,
                LIVESTREAM_SETUP,
                ZOOM_WEBEX_LINK
            };
        }

        public static List<int> GetNonAssistedServiceIds()
        {
            return new List<int>
            {
                GOVERNMENT_EMAIL_ACCOUNT,
                ACCOUNT_CREATION,
                PASSWORD_ACCESS_ISSUE,
                SYSTEM_SUPPORT,
                DATA_CORRECTION,
                TECHNICAL_GUIDANCE,
                PRODUCTION_MATERIAL_PRINTING
            };
        }

        public static bool IsRepairTroubleshootingRequest(this int id)
        {
            if (id == EQUIPMENT_REPAIR_TROUBLESHOOTING ||
                id == INTERNET_CONNECTIVITY)
            {
                return true;
            }
            return false;
        }

        public static bool IsScheduleControlProcessRequest(this int id)
        {
            if (id == AUDIO_VISUAL_SETUP ||
                id == LIVESTREAM_SETUP ||
                id == ZOOM_WEBEX_LINK)
            {
                return true;
            }
            return false;
        }

        public static bool IsNonAssistedRequest(this int id)
        {
            if (id == GOVERNMENT_EMAIL_ACCOUNT ||
                id == ACCOUNT_CREATION ||
                id == PASSWORD_ACCESS_ISSUE ||
                id == SYSTEM_SUPPORT ||
                id == DATA_CORRECTION ||
                id == TECHNICAL_GUIDANCE ||
                id == PRODUCTION_MATERIAL_PRINTING)
            {
                return true;
            }
            return false;
        }

        public static List<SelectListItem> GetSelectListItems()
        {
            var assistedGroup = new SelectListGroup { Name = "Assisted" };
            var scheduledGroup = new SelectListGroup { Name = "Scheduled Control Process" };
            var nonAssistedGroup = new SelectListGroup { Name = "Non-Assisted" };

            return new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = TechnicalServiceTypeEnum.EQUIPMENT_REPAIR_TROUBLESHOOTING.ToString(),
                    Text = TechnicalServiceTypeEnum.DisplayName(TechnicalServiceTypeEnum.EQUIPMENT_REPAIR_TROUBLESHOOTING),
                    Group = assistedGroup
                },
                new SelectListItem
                {
                    Value = TechnicalServiceTypeEnum.INTERNET_CONNECTIVITY.ToString(),
                    Text = TechnicalServiceTypeEnum.DisplayName(TechnicalServiceTypeEnum.INTERNET_CONNECTIVITY),
                    Group = assistedGroup
                },

                new SelectListItem
                {
                    Value = TechnicalServiceTypeEnum.AUDIO_VISUAL_SETUP.ToString(),
                    Text = TechnicalServiceTypeEnum.DisplayName(TechnicalServiceTypeEnum.AUDIO_VISUAL_SETUP),
                    Group = scheduledGroup
                },
                new SelectListItem
                {
                    Value = TechnicalServiceTypeEnum.LIVESTREAM_SETUP.ToString(),
                    Text = TechnicalServiceTypeEnum.DisplayName(TechnicalServiceTypeEnum.LIVESTREAM_SETUP),
                    Group = scheduledGroup
                },
                new SelectListItem
                {
                    Value = TechnicalServiceTypeEnum.ZOOM_WEBEX_LINK.ToString(),
                    Text = TechnicalServiceTypeEnum.DisplayName(TechnicalServiceTypeEnum.ZOOM_WEBEX_LINK),
                    Group = scheduledGroup
                },

                new SelectListItem
                {
                    Value = TechnicalServiceTypeEnum.ACCOUNT_CREATION.ToString(),
                    Text = TechnicalServiceTypeEnum.DisplayName(TechnicalServiceTypeEnum.ACCOUNT_CREATION),
                    Group = nonAssistedGroup
                },
                new SelectListItem
                {
                    Value = TechnicalServiceTypeEnum.DATA_CORRECTION.ToString(),
                    Text = TechnicalServiceTypeEnum.DisplayName(TechnicalServiceTypeEnum.DATA_CORRECTION),
                    Group = nonAssistedGroup
                },
                new SelectListItem
                {
                    Value = TechnicalServiceTypeEnum.GOVERNMENT_EMAIL_ACCOUNT.ToString(),
                    Text = TechnicalServiceTypeEnum.DisplayName(TechnicalServiceTypeEnum.GOVERNMENT_EMAIL_ACCOUNT),
                    Group = nonAssistedGroup
                },
                new SelectListItem
                {
                    Value = TechnicalServiceTypeEnum.PRODUCTION_MATERIAL_PRINTING.ToString(),
                    Text = TechnicalServiceTypeEnum.DisplayName(TechnicalServiceTypeEnum.PRODUCTION_MATERIAL_PRINTING),
                    Group = nonAssistedGroup
                },
                new SelectListItem
                {
                    Value = TechnicalServiceTypeEnum.SYSTEM_SUPPORT.ToString(),
                    Text = TechnicalServiceTypeEnum.DisplayName(TechnicalServiceTypeEnum.SYSTEM_SUPPORT),
                    Group = nonAssistedGroup
                },
                new SelectListItem
                {
                    Value = TechnicalServiceTypeEnum.TECHNICAL_GUIDANCE.ToString(),
                    Text = TechnicalServiceTypeEnum.DisplayName(TechnicalServiceTypeEnum.TECHNICAL_GUIDANCE),
                    Group = nonAssistedGroup
                },

                // Keep "Others" as -1 for your existing JS logic
                new SelectListItem { Value = "-1", Text = "Others" }
            };
        }
    }

    public static class TechnicalServiceRequestStatusEnum
    {
        public const int PENDING = 1;
        public const int ONGOING = 2;
        public const int RESOLVED = 3;
        public const int CANCELLED = 4;
        public const int OPEN = 5;
        public const int CLOSED = 6;

        public static string DisplayName(this int statusId)
        {
            switch (statusId)
            {
                case 1:
                    return "Pending";
                case 2:
                    return "On Going";
                case 3:
                    return "Resolved";
                case 4:
                    return "Cancelled";
                case 5:
                    return "Open";
                case 6:
                    return "Closed";
                default:
                    return "Unknown Status";
            }
        }

        public static List<int> GetActiveStatusIds()
        {
            return new List<int>() { PENDING, ONGOING, OPEN };
        }

        public static List<int> GetCancellableStatusIds()
        {
            return new List<int> { PENDING, OPEN };
        }

        public static List<int> GetCompletedStatusIds()
        {
            return new List<int> { RESOLVED, CLOSED };
        }

        public static List<SelectListItem> GetSelectListItems()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = PENDING.ToString(), Text = DisplayName(PENDING) },
                new SelectListItem { Value = ONGOING.ToString(), Text = DisplayName(ONGOING) },
                new SelectListItem { Value = RESOLVED.ToString(), Text = DisplayName(RESOLVED) },
                new SelectListItem { Value = CANCELLED.ToString(), Text = DisplayName(CANCELLED) },
                new SelectListItem { Value = OPEN.ToString(), Text = DisplayName(OPEN) },
                new SelectListItem { Value = CLOSED.ToString(), Text = DisplayName(CLOSED) }
            };
        }
    }

    public static class TechnicalServicRequestSeverityEnum
    {
        public const int LOW = 1;
        public const int MEDIUM = 2;
        public const int HIGH = 3;
        public const int CRITICAL = 4;

        public static string DisplayName(this int severityId)
        {
            switch (severityId)
            {
                case 1:
                    return "Low";
                case 2:
                    return "Medium";
                case 3:
                    return "High";
                case 4:
                    return "Critical";
                default:
                    return "Unknown Severity";
            }
        }

        public static List<SelectListItem> GetSelectListItems()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "Not Applicable"},
                new SelectListItem {
                    Value = TechnicalServicRequestSeverityEnum.LOW.ToString(),
                    Text = TechnicalServicRequestSeverityEnum.DisplayName(TechnicalServicRequestSeverityEnum.LOW)
                },
                new SelectListItem {
                    Value = TechnicalServicRequestSeverityEnum.MEDIUM.ToString(),
                    Text = TechnicalServicRequestSeverityEnum.DisplayName(TechnicalServicRequestSeverityEnum.MEDIUM)
                },
                new SelectListItem
                {
                    Value = TechnicalServicRequestSeverityEnum.HIGH.ToString(),
                    Text = TechnicalServicRequestSeverityEnum.DisplayName(TechnicalServicRequestSeverityEnum.HIGH)
                },
                new SelectListItem
                {
                    Value = TechnicalServicRequestSeverityEnum.CRITICAL.ToString(),
                    Text = TechnicalServicRequestSeverityEnum.DisplayName(TechnicalServicRequestSeverityEnum.CRITICAL)
                }
            };
        }
    }

    public static class TechnicalServiceRequestScheduleLimitEnum
    {
        public const int AUDIO_VISUAL_SETUP = 3;
        public const int LIVESTREAM_SETUP = 2;
        public const int ZOOM_WEBEX_LINK = 4;
    }
}