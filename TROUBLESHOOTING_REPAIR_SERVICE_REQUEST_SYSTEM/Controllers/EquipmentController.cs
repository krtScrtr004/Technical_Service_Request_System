using Serilog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Attributes;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Core;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Enumerables;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Models;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Services;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Utilities;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Controllers
{
    [Authorize2]
    public class EquipmentController : BaseController
    {
        // GET: Equipment
        public ActionResult Index()
        {
            var currentUser = GetUserSession();
            if (currentUser == null)
            {
                throw new HttpException(403, "Forbidden");
            }

            ViewBag.CurrentUser = currentUser;
            return View();
        }

        [AuthenticateUserPrivilege(new int[] { AccountTypeEnum.IT, AccountTypeEnum.ADMIN })]
        public ActionResult Create()
        {
            var currentUser = GetUserSession();
            if (currentUser == null)
            {
                throw new HttpException(403, "Forbidden");
            }

            ViewBag.CurrentUser = currentUser;
            return View(new EquipmentFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthenticateUserPrivilege(new int[] { AccountTypeEnum.IT, AccountTypeEnum.ADMIN })]
        public ActionResult Create(EquipmentFormViewModel equipmentFormViewModel)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                if (!ModelState.IsValid)
                {
                    var errors = GetModelStateErrors();
                    Log.Warning($"Model state is invalid: {errors}");
                    return View(equipmentFormViewModel);
                }

                try
                {
                    var currentUser = GetUserSession();
                    if (currentUser == null)
                    {
                        throw new HttpException(403, "Forbidden");
                    }

                    var normalizedAssetTag = (new EquipmentService()).NormalizeAssetTag(equipmentFormViewModel.AssetTag);

                    var isAssetTagTaken = _db.Equipments
                        .Any(e => e.AssetTag == normalizedAssetTag);
                    if (isAssetTagTaken)
                    {
                        TempData["alertModal"] = new AlertModalUtility()
                        {
                            Title = "Error",
                            Message = "The provided asset tag is already associated with another equipment entry. Please use a different asset tag.",
                            Status = AlertModalStatus.Error
                        };
                        return View(equipmentFormViewModel);
                    }

                    EquipmentLocation equipmentLocation = null;

                    var hasCompleteLocationInfo = equipmentFormViewModel.BuildingNumber.HasValue &&
                        equipmentFormViewModel.FloorNumber.HasValue &&
                        !string.IsNullOrEmpty(equipmentFormViewModel.Office);
                    if (hasCompleteLocationInfo)
                    {
                        equipmentLocation = new EquipmentLocation
                        {
                            BuildingNumber = equipmentFormViewModel.BuildingNumber.Value,
                            FloorNumber = equipmentFormViewModel.FloorNumber.Value,
                            Office = equipmentFormViewModel.Office.ToUpperInvariant(),
                            IsActive = true
                        };
                    }

                    int locationRecordId = 0;
                    if (equipmentLocation != null)
                    {
                        locationRecordId = _db.EquipmentLocations
                       .Where(l =>
                           l.BuildingNumber == equipmentLocation.BuildingNumber &&
                           l.FloorNumber == equipmentLocation.FloorNumber &&
                           l.Office == equipmentLocation.Office
                       )
                       .Select(l => l.Id)
                       .FirstOrDefault();
                    }

                    var equipment = new Equipment
                    {
                        EquipmentModel = equipmentFormViewModel.EquipmentModel.ToUpperInvariant(),
                        AssetTag = normalizedAssetTag,
                        EquipmentTypeId = equipmentFormViewModel.EquipmentTypeId,
                        EquipmentStatusId = equipmentFormViewModel.EquipmentStatusId,
                        CreatedByRegistrationId = currentUser.Id,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    if (locationRecordId > 0)
                    {
                        // Refer to existing location entry if it already exists
                        equipment.EquipmentLocationId = locationRecordId;
                    }
                    else
                    {
                        // Create a new equipment location entry
                        equipment.EquipmentLocation = equipmentLocation;
                    }

                    _db.Equipments.Add(equipment);
                    _db.SaveChanges();

                    // Notify Admins and Its about new equipoment entry
                    (new NotificationService()).NotifyNewEquipmentEntry(equipment.AssetTag, currentUser.FirstName);

                    transaction.Commit();

                    // Refresh list 
                    EquipmentHub.RefreshEquipmentList();

                    ViewBag.CurrentUser = currentUser;

                    Log.Information($"Equipment entry created successfully with ID {equipment.Id} by user ID {currentUser.Id}");
                    TempData["alertModal"] = new AlertModalUtility()
                    {
                        Title = "Success",
                        Message = "Equipment entry successfully created.",
                        Status = AlertModalStatus.Success
                    };
                    return View("Index");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    TempData["alertModal"] = new AlertModalUtility()
                    {
                        Title = "Error",
                        Message = "An error occurred while making a request. Please try again.",
                        Status = AlertModalStatus.Error
                    };

                    Log.Error(ex, $"Error creating equipment entry for user ID {GetUserSession()?.Id}");
                    ModelState.AddModelError("", "An error occurred while making a request: " + ex.Message);

                    return View(equipmentFormViewModel);
                }
            }
        }

        public ActionResult Edit(int id)
        {
            if (id < 0)
            {
                throw new HttpException(404, "Not Found");
            }

            var currentUser = GetUserSession();
            if (currentUser == null)
            {
                throw new HttpException(403, "Forbidden");
            }

            var equipment = _db.Equipments
                .Include(e => e.EquipmentLocation)
                .Where(e => e.Id == id)
                .Select(e => new
                {
                    e.Id,
                    e.AssetTag,
                    e.EquipmentModel,
                    e.EquipmentTypeId,
                    e.EquipmentLocationId,
                    e.EquipmentStatusId,
                    BuildingNumber = (int?)e.EquipmentLocation.BuildingNumber,
                    FloorNumber = (int?)e.EquipmentLocation.FloorNumber,
                    e.EquipmentLocation.Office
                })
                .FirstOrDefault();
            if (equipment == null)
            {
                throw new HttpException(404, "Not Found");
            }

            ViewBag.CurrentUser = currentUser;
            return View(new EquipmentFormViewModel
            {
                Id = equipment.Id,
                AssetTag = equipment.AssetTag,
                EquipmentModel = equipment.EquipmentModel,
                EquipmentTypeId = (int)equipment.EquipmentTypeId,
                EquipmentStatusId = (int)equipment.EquipmentStatusId,

                EquipmentLocationId = equipment.EquipmentLocationId,
                BuildingNumber = equipment.BuildingNumber.HasValue
                    ? equipment.BuildingNumber.Value
                    : (int?)null,
                FloorNumber = equipment.FloorNumber.HasValue
                    ? equipment.FloorNumber.Value
                    : (int?)null,
                Office = equipment.Office
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthenticateUserPrivilege(new int[] { AccountTypeEnum.IT, AccountTypeEnum.ADMIN })]
        public ActionResult Edit(EquipmentFormViewModel equipmentFormViewModel, int id)
        {

            using (var transaction = _db.Database.BeginTransaction())
            {
                if (id < 0)
                {
                    throw new HttpException(404, "Not Found");
                }

                var currentUser = GetUserSession();
                if (currentUser == null)
                {
                    throw new HttpException(403, "Forbidden");
                }

                if (!ModelState.IsValid)
                {
                    var errors = GetModelStateErrors();
                    Log.Warning($"Model state is invalid: {errors}");
                    return View(equipmentFormViewModel);
                }

                try
                {
                    var normalizedAssetTag = (new EquipmentService()).NormalizeAssetTag(equipmentFormViewModel.AssetTag);

                    var isAssetTagTaken = _db.Equipments
                        .Any(e => e.AssetTag == normalizedAssetTag && e.Id != id);
                    if (isAssetTagTaken)
                    {
                        TempData["alertModal"] = new AlertModalUtility()
                        {
                            Title = "Error",
                            Message = "The provided asset tag is already associated with another equipment entry. Please use a different asset tag.",
                            Status = AlertModalStatus.Error
                        };
                        return View(equipmentFormViewModel);
                    }
                    var equipment = _db.Equipments
                        .Include(e => e.EquipmentLocation)
                        .Where(e => e.Id == id)
                        .FirstOrDefault();
                    if (equipment == null)
                    {
                        throw new HttpException(404, "Not Found");
                    }

                    var updatedDetails = new List<string>();

                    if (!string.Equals(equipment.AssetTag, normalizedAssetTag, StringComparison.OrdinalIgnoreCase))
                    {
                        equipment.AssetTag = normalizedAssetTag;
                        updatedDetails.Add("Asset Tag");
                    }

                    var normalizedEquipmentModel = equipmentFormViewModel.EquipmentModel.Trim().ToUpperInvariant();
                    if (!string.Equals(equipment.EquipmentModel, normalizedEquipmentModel, StringComparison.Ordinal))
                    {
                        equipment.EquipmentModel = normalizedEquipmentModel;
                        updatedDetails.Add("Model");
                    }

                    if (equipment.EquipmentTypeId != equipmentFormViewModel.EquipmentTypeId)
                    {
                        equipment.EquipmentTypeId = equipmentFormViewModel.EquipmentTypeId;
                        updatedDetails.Add("Type");
                    }

                    if (equipment.EquipmentStatusId != equipmentFormViewModel.EquipmentStatusId)
                    {
                        equipment.EquipmentStatusId = equipmentFormViewModel.EquipmentStatusId;
                        updatedDetails.Add("Status");

                        var inactiveStatusIds = EquipmentStatusEnum.GetInActiveIds();
                        if (inactiveStatusIds.Contains(equipmentFormViewModel.EquipmentStatusId))
                        {
                            // If status is being changed to an inactive status, check if it's currently under repair. If it is, prevent the update and show an error message.
                            var isUnderRepair = equipment.EquipmentStatusId.HasValue &&
                                equipment.EquipmentStatusId.Value == (int)EquipmentStatusEnum.UNDER_REPAIR;
                            if (isUnderRepair)
                            {
                                TempData["alertModal"] = new AlertModalUtility()
                                {
                                    Title = "Error",
                                    Message = "Equipment is currently under repair. Status cannot be updated to " + EquipmentStatusEnum.DisplayName(equipmentFormViewModel.EquipmentStatusId),
                                    Status = AlertModalStatus.Success
                                };
                                return View(equipmentFormViewModel);
                            }

                            equipment.IsActive = false;
                        }
                    }

                    var currentLocation = equipment.EquipmentLocation;

                    EquipmentLocation equipmentLocation = null;
                    var previousBuildingNumber = currentLocation?.BuildingNumber;
                    var previousFloorNumber = currentLocation?.FloorNumber;
                    var previousOffice = currentLocation?.Office;

                    var hasCompleteLocationInfo = equipmentFormViewModel.BuildingNumber.HasValue &&
                        equipmentFormViewModel.FloorNumber.HasValue &&
                        !string.IsNullOrEmpty(equipmentFormViewModel.Office);
                    if (hasCompleteLocationInfo)
                    {
                        equipmentLocation = new EquipmentLocation
                        {
                            BuildingNumber = equipmentFormViewModel.BuildingNumber.Value,
                            FloorNumber = equipmentFormViewModel.FloorNumber.Value,
                            Office = equipmentFormViewModel.Office.ToUpperInvariant(),
                            IsActive = true
                        };
                    }

                    var locationChanged = false;

                    // Check if there's an existing location entry with the same details to prevent duplicates
                    int locationRecordId = 0;
                    if (equipmentLocation != null)
                    {
                        locationChanged =
                            previousBuildingNumber != equipmentLocation.BuildingNumber ||
                            previousFloorNumber != equipmentLocation.FloorNumber ||
                            !string.Equals(previousOffice, equipmentLocation.Office, StringComparison.OrdinalIgnoreCase);

                        locationRecordId = _db.EquipmentLocations
                       .Where(l =>
                           l.BuildingNumber == equipmentLocation.BuildingNumber &&
                           l.FloorNumber == equipmentLocation.FloorNumber &&
                           l.Office == equipmentLocation.Office
                       )
                       .Select(l => l.Id)
                       .FirstOrDefault();
                    }

                    if (locationRecordId > 0)
                    {
                        // Refer to existing location entry if it already exists
                        equipment.EquipmentLocationId = locationRecordId;
                    }
                    else
                    {
                        // Create a new equipment location entry
                        equipment.EquipmentLocation = equipmentLocation;
                    }

                    if (locationChanged)
                    {
                        updatedDetails.Add("Location");
                    }

                    _db.Entry(equipment).State = EntityState.Modified;
                    _db.SaveChanges();

                    if (updatedDetails.Contains("Asset Tag"))
                    {
                        EquipmentHub.RefreshEquipmentAssetTag(equipment.Id, equipment.AssetTag);
                    }

                    if (updatedDetails.Contains("Model"))
                    {
                        EquipmentHub.RefreshEquipmentModel(equipment.Id, equipment.EquipmentModel);
                    }

                    if (updatedDetails.Contains("Type"))
                    {
                        EquipmentHub.RefreshEquipmentType(
                            equipment.Id,
                            equipment.EquipmentTypeId.HasValue
                                ? EquipmentTypeEnum.DisplayName(equipment.EquipmentTypeId.Value)
                                : "N/A"
                        );
                    }

                    if (updatedDetails.Contains("Status"))
                    {
                        EquipmentHub.RefreshEquipmentStatus(
                            equipment.Id,
                            equipment.EquipmentStatusId.HasValue
                                ? EquipmentStatusEnum.DisplayName(equipment.EquipmentStatusId.Value)
                                : "N/A"
                        );
                    }

                    if (updatedDetails.Contains("Location"))
                    {
                        if (equipment.EquipmentLocationId.HasValue || equipment.EquipmentLocation != null)
                        {
                            var refreshedLocation = equipment.EquipmentLocation ?? _db.EquipmentLocations.Find(equipment.EquipmentLocationId);
                            if (refreshedLocation != null)
                            {
                                EquipmentHub.RefreshEquipmentBuildingNumber(equipment.Id, refreshedLocation.BuildingNumber);
                                EquipmentHub.RefreshEquipmentFloorNumber(equipment.Id, refreshedLocation.FloorNumber);
                                EquipmentHub.RefreshEquipmentOffice(equipment.Id, refreshedLocation.Office);
                            }
                        }
                    }

                    transaction.Commit();

                    // Refresh list 
                    EquipmentHub.RefreshEquipmentList();

                    ViewBag.CurrentUser = currentUser;

                    Log.Information($"Equipment entry with ID {equipment.Id} updated successfully by user ID {currentUser.Id}. Updated details: {string.Join(", ", updatedDetails)}");
                    TempData["alertModal"] = new AlertModalUtility()
                    {
                        Title = "Success",
                        Message = "Equipment entry successfully updated.",
                        Status = AlertModalStatus.Success
                    };
                    return RedirectToAction("Details", new { id = equipment.Id });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    TempData["alertModal"] = new AlertModalUtility()
                    {
                        Title = "Error",
                        Message = "An error occurred while making a request. Please try again.",
                        Status = AlertModalStatus.Error
                    };

                    Log.Error(ex, $"An error occurred while editing equipment entry for user ID {GetUserSession()?.Id}");
                    ModelState.AddModelError("", "An error occurred while making a request: " + ex.Message);

                    return View(equipmentFormViewModel);
                }
            }
        }


        [Authorize2]
        [AuthenticateUserPrivilege(new int[] { AccountTypeEnum.STANDARD, AccountTypeEnum.IT, AccountTypeEnum.ADMIN })]
        public ActionResult Details(int id)
        {
            if (id < 1)
            {
                throw new HttpException(404, "Not Found");
            }

            var currentUser = GetUserSession();
            if (currentUser == null)
            {
                throw new HttpException(403, "Forbidden");
            }

            var equipment = _db.Equipments
                .Include(e => e.EquipmentLocation)
                .Include(e => e.CreatedByRegistration)
                .FirstOrDefault(e => e.Id == id);

            if (equipment == null)
            {
                throw new HttpException(404, "Not Found");
            }

            ViewBag.CurrentUser = currentUser;

            return View(new EquipmentDetailsViewModel
            {
                Id = equipment.Id,
                AssetTag = equipment.AssetTag,
                EquipmentModel = equipment.EquipmentModel,
                EquipmentTypeId = equipment.EquipmentTypeId ?? 0,
                BuildingNumber = equipment.EquipmentLocation?.BuildingNumber,
                FloorNumber = equipment.EquipmentLocation?.FloorNumber,
                Office = equipment.EquipmentLocation?.Office,
                EquipmentStatusId = equipment.EquipmentStatusId ?? 0,
                RepairCount = equipment.RepairCount,
                CreatedByRegistrationFirstName = equipment.CreatedByRegistration?.FirstName,
                CreatedByRegistrationMiddleName = equipment.CreatedByRegistration?.MiddleName,
                CreatedByRegistrationLastName = equipment.CreatedByRegistration?.LastName,
                CreatedByRegistrationExtensionName = equipment.CreatedByRegistration?.ExtensionName
            });
        }

        #region API

        [HttpGet]
        [AuthenticateUserPrivilege(new int[] { AccountTypeEnum.STANDARD, AccountTypeEnum.IT, AccountTypeEnum.ADMIN })]
        public JsonResult GetEquipment()
        {
            try
            {
                // Authenticate user
                int.TryParse(Request["userId"], out int Id);
                var associatedUser = _db.Registrations
                    .Where(i => i.Id == Id)
                    .FirstOrDefault();
                if (associatedUser == null)
                {
                    throw new Exception("User not found.");
                }

                // Get user's privilege
                var associatedUserPrivilege = _db.UserPrivileges
                    .Where(j => j.RegistrationId == associatedUser.Id && j.PrivilegeId.HasValue)
                    .Select(i => i.PrivilegeId.Value)
                    .ToArray();

                // Get DataTables parameters from request
                var draw = Request["draw"];
                var start = Request["start"];
                var length = Request["length"];
                var searchValue = Request["search[value]"];
                var sortColumn = Request["order[0][column]"];
                var sortDirection = Request["order[0][dir]"];

                // Parse parameters
                int pageSize = length != null ? Convert.ToInt32(length) : 10;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;

                var statusFilter = Request["statusFilter"];
                var typeFilter = Request["typeFilter"];


                IQueryable<Equipment> query = null;

                var isStandardUser = AccountTypeEnum.IsStandard(associatedUserPrivilege);
                if (isStandardUser)
                {
                    // Fetch only equipments associated with user requests
                    query = _db.TechnicalServiceRequests
                        .Include(r => r.TechnicalServiceRequestEquipment)
                        .Include(r => r.TechnicalServiceRequestEquipment.EquipmentType)
                        .Include(r => r.TechnicalServiceRequestEquipment.EquipmentStatus)
                        .Where(r =>
                            r.ClientRegistrationId == associatedUser.Id &&
                            r.TechnicalServiceRequestEquipment != null
                        )
                        .Select(r => r.TechnicalServiceRequestEquipment)
                        .GroupBy(e => e.AssetTag)
                        .Select(g => g.FirstOrDefault())
                        .AsQueryable();
                }
                else
                {
                    query = _db.Equipments
                        .Include(e => e.EquipmentType)
                        .Include(e => e.EquipmentStatus)
                        .AsQueryable();
                }

                if (!string.IsNullOrEmpty(statusFilter))
                {
                    if (int.TryParse(statusFilter, out int statusIntValue) && statusIntValue > 0)
                    {
                        query = query.Where(e =>
                                e.EquipmentStatusId.HasValue &&
                                e.EquipmentStatusId.Value == statusIntValue
                            )
                            .AsQueryable();
                    }
                }

                // Apply type filter
                if (!string.IsNullOrEmpty(typeFilter))
                {
                    if (int.TryParse(typeFilter, out int typeIntValue) && typeIntValue > 0)
                    {
                        query = query.Where(e =>
                                e.EquipmentTypeId.HasValue &&
                                e.EquipmentTypeId.Value == typeIntValue
                            )
                            .AsQueryable();
                    }
                }

                // Get filtered count
                int recordsFiltered = query.Count();

                // Apply search filter
                if (!string.IsNullOrEmpty(searchValue))
                {
                    query = query.Where(e =>
                        e.AssetTag.Contains(searchValue) ||
                        e.EquipmentModel.Contains(searchValue) ||
                        e.EquipmentType.EquipmentTypeName.Contains(searchValue) ||
                        e.EquipmentStatus.EquipmentStatusName.Contains(searchValue)
                    );
                }

                // Apply sorting
                if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDirection))
                {
                    int columnIndex = Convert.ToInt32(sortColumn);
                    switch (columnIndex)
                    {
                        case 1:
                            query = sortDirection == "asc"
                                ? query.OrderBy(e => e.AssetTag)
                                : query.OrderByDescending(e => e.AssetTag);
                            break;
                        case 2:
                            query = sortDirection == "asc"
                                ? query.OrderBy(e => e.EquipmentModel)
                                : query.OrderByDescending(e => e.EquipmentModel);
                            break;
                        case 3:
                            query = sortDirection == "asc"
                                ? query.OrderBy(e => e.EquipmentType.EquipmentTypeName)
                                : query.OrderByDescending(e => e.EquipmentType.EquipmentTypeName);
                            break;
                        case 4:
                            query = sortDirection == "asc"
                                ? query.OrderBy(e => e.EquipmentStatus.EquipmentStatusName)
                                : query.OrderByDescending(e => e.EquipmentStatus.EquipmentStatusName);
                            break;
                        case 5:
                            query = sortDirection == "asc"
                                ? query.OrderBy(e => e.RepairCount)
                                : query.OrderByDescending(e => e.RepairCount);
                            break;
                        default:
                            query = query.OrderBy(e => e.Id);
                            break;
                    }
                }
                else
                {
                    query = query.OrderBy(e => e.Id);
                }

                recordsTotal = query.Count();

                // Apply pagination
                var data = query
                    .Skip(skip)
                    .Take(recordsTotal)
                    .ToList()
                    .Select(e => new
                    {
                        e.Id,
                        e.AssetTag,
                        e.EquipmentModel,
                        EquipmentType = e.EquipmentType != null
                            ? e.EquipmentType.EquipmentTypeName
                            : "N/A",
                        EquipmentStatus = e.EquipmentStatus != null
                            ? e.EquipmentStatus.EquipmentStatusName
                            : "N/A",
                        e.RepairCount
                    })
                    .ToList();

                // Return data in DataTables format
                return Json(new
                {
                    draw,
                    recordsTotal,
                    recordsFiltered,
                    data
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching equipment data for user ID {UserId}", Request["userId"]);
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}