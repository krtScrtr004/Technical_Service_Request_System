using Serilog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TECHNICAL_SERVICE_REQUEST.Attributes;
using TECHNICAL_SERVICE_REQUEST.Core;
using TECHNICAL_SERVICE_REQUEST.Enumerables;
using TECHNICAL_SERVICE_REQUEST.Models;
using TECHNICAL_SERVICE_REQUEST;
using TECHNICAL_SERVICE_REQUEST.Services;
using TECHNICAL_SERVICE_REQUEST.Utilities;

namespace TECHNICAL_SERVICE_REQUEST.Controllers
{
    [Authorize2]
    public class EquipmentController : BaseController
    {
        // GET: Equipment
        public ActionResult Index()
        {
            var currentUser = GetAppUserSession();
            if (currentUser == null)
            {
                throw new HttpException(403, "Forbidden");
            }

            try
            {
                ViewBag.CurrentUser = currentUser;
                return View();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"An error occured while loading equipments list page: {ex.Message}");
                return View("Error", "Error");
            }
        }

        [AuthenticateUserPrivilege(new int[] { AppUserRoleEnum.IT, AppUserRoleEnum.ADMIN })]
        public ActionResult Create()
        {
            var currentUser = GetAppUserSession();
            if (currentUser == null)
            {
                throw new HttpException(403, "Forbidden");
            }

            try
            {
                ViewBag.CurrentUser = currentUser;
                return View(new EquipmentFormViewModel());
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"An error occured while loading equipment creation page: {ex.Message}");
                return View("Error", "Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthenticateUserPrivilege(new int[] { AppUserRoleEnum.IT, AppUserRoleEnum.ADMIN })]
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
                    var currentUser = GetAppUserSession();
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
                        Model = equipmentFormViewModel.Model.ToUpperInvariant(),
                        AssetTag = normalizedAssetTag,
                        TypeId = equipmentFormViewModel.TypeId,
                        StatusId = equipmentFormViewModel.StatusId,
                        CreatedById = currentUser.Id,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                    };

                    if (locationRecordId > 0)
                    {
                        // Refer to existing location entry if it already exists
                        equipment.LocationId = locationRecordId;
                    }
                    else
                    {
                        // Create a new equipment location entry
                        equipment.Location = equipmentLocation;
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

                    Log.Error(ex, $"Error creating equipment entry for user ID {GetAppUserSession()?.Id}");
                    ModelState.AddModelError("", "An error occurred while making a request: " + ex.Message);

                    return View(equipmentFormViewModel);
                }
            }
        }

        [AuthenticateUserPrivilege(new int[] { AppUserRoleEnum.IT, AppUserRoleEnum.ADMIN })]
        public ActionResult Edit(int id)
        {
            if (id < 0)
            {
                throw new HttpException(404, "Not Found");
            }

            var currentUser = GetAppUserSession();
            if (currentUser == null)
            {
                throw new HttpException(403, "Forbidden");
            }

            var equipment = _db.Equipments
                .Include(e => e.Location)
                .Where(e => e.Id == id)
                .Select(e => new
                {
                    e.Id,
                    e.AssetTag,
                    e.Model,
                    e.TypeId,
                    e.LocationId,
                    e.StatusId,
                    BuildingNumber = (int?)e.Location.BuildingNumber,
                    FloorNumber = (int?)e.Location.FloorNumber,
                    e.Location.Office
                })
                .FirstOrDefault();
            if (equipment == null)
            {
                throw new HttpException(404, "Not Found");
            }

            try
            {
                ViewBag.CurrentUser = currentUser;
                return View(new EquipmentFormViewModel
                {
                    Id = equipment.Id,
                    AssetTag = equipment.AssetTag,
                    Model = equipment.Model,
                    TypeId = (int)equipment.TypeId,
                    StatusId = (int)equipment.StatusId,

                    LocationId = equipment.LocationId,
                    BuildingNumber = equipment.BuildingNumber.HasValue
                        ? equipment.BuildingNumber.Value
                        : (int?)null,
                    FloorNumber = equipment.FloorNumber.HasValue
                        ? equipment.FloorNumber.Value
                        : (int?)null,
                    Office = equipment.Office
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"An error occured while loading equipment edit page: {ex.Message}");
                return View("Error", "Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthenticateUserPrivilege(new int[] { AppUserRoleEnum.IT, AppUserRoleEnum.ADMIN })]
        public ActionResult Edit(EquipmentFormViewModel equipmentFormViewModel, int id)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                if (id < 0)
                {
                    throw new HttpException(404, "Not Found");
                }

                var currentUser = GetAppUserSession();
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
                        .Include(e => e.Location)
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

                    var normalizedEquipmentModel = equipmentFormViewModel.Model.Trim().ToUpperInvariant();
                    if (!string.Equals(equipment.Model, normalizedEquipmentModel, StringComparison.Ordinal))
                    {
                        equipment.Model = normalizedEquipmentModel;
                        updatedDetails.Add("Model");
                    }

                    if (equipment.TypeId != equipmentFormViewModel.TypeId)
                    {
                        equipment.TypeId = equipmentFormViewModel.TypeId;
                        updatedDetails.Add("Type");
                    }

                    if (equipment.StatusId != equipmentFormViewModel.StatusId)
                    {
                        equipment.StatusId = equipmentFormViewModel.StatusId;
                        updatedDetails.Add("Status");

                        var inactiveStatusIds = EquipmentStatusEnum.GetInActiveIds();
                        if (inactiveStatusIds.Contains(equipmentFormViewModel.StatusId))
                        {
                            // If status is being changed to an inactive status, check if it's currently under repair. If it is, prevent the update and show an error message.
                            var isUnderRepair = equipment.StatusId.HasValue && 
                                equipment.StatusId.Value == (int)EquipmentStatusEnum.UNDER_REPAIR;
                            if (isUnderRepair)
                            {
                                TempData["alertModal"] = new AlertModalUtility()
                                {
                                    Title = "Error",
                                    Message = "Equipment is currently under repair. Status cannot be updated to " + EquipmentStatusEnum.DisplayName(equipmentFormViewModel.StatusId),
                                    Status = AlertModalStatus.Success
                                };
                                return View(equipmentFormViewModel);
                            }
                        }
                    }

                    var currentLocation = equipment.Location;

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
                        equipment.LocationId = locationRecordId;
                    }
                    else
                    {
                        // Create a new equipment location entry
                        equipment.Location = equipmentLocation;
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
                        EquipmentHub.RefreshEquipmentModel(equipment.Id, equipment.Model);
                    }

                    if (updatedDetails.Contains("Type"))
                    {
                        EquipmentHub.RefreshEquipmentType(
                            equipment.Id,
                            equipment.TypeId.HasValue
                                ? EquipmentTypeEnum.DisplayName(equipment.TypeId.Value)
                                : "N/A"
                        );
                    }

                    if (updatedDetails.Contains("Status"))
                    {
                        EquipmentHub.RefreshEquipmentStatus(
                            equipment.Id,
                            equipment.StatusId.HasValue
                                ? EquipmentStatusEnum.DisplayName(equipment.StatusId.Value)
                                : "N/A"
                        );
                    }

                    if (updatedDetails.Contains("Location"))
                    {
                        if (equipment.LocationId.HasValue || equipment.Location != null)
                        {
                            var refreshedLocation = equipment.Location ?? _db.EquipmentLocations.Find(equipment.LocationId);
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

                    Log.Error(ex, $"An error occurred while editing equipment entry for user ID {GetAppUserSession()?.Id}");
                    ModelState.AddModelError("", "An error occurred while making a request: " + ex.Message);

                    return View(equipmentFormViewModel);
                }
            }
        }


        [Authorize2]
        [AuthenticateUserPrivilege(new int[] { AppUserRoleEnum.STANDARD, AppUserRoleEnum.IT, AppUserRoleEnum.ADMIN })]
        public ActionResult Details(int id)
        {
            if (id < 1)
            {
                throw new HttpException(404, "Not Found");
            }

            var currentUser = GetAppUserSession();
            if (currentUser == null)
            {
                throw new HttpException(403, "Forbidden");
            }

            var equipment = _db.Equipments
                .Include(e => e.Location)
                .Include(e => e.CreatedBy)
                .FirstOrDefault(e => e.Id == id);

            if (equipment == null)
            {
                throw new HttpException(404, "Not Found");
            }

            try
            {
                ViewBag.CurrentUser = currentUser;
                return View(new EquipmentDetailsViewModel
                {
                    Id = equipment.Id,
                    AssetTag = equipment.AssetTag,
                    Model = equipment.Model,
                    TypeId = equipment.TypeId ?? 0,
                    BuildingNumber = equipment.Location?.BuildingNumber,
                    FloorNumber = equipment.Location?.FloorNumber,
                    Office = equipment.Location?.Office,
                    StatusId = equipment.StatusId ?? 0,
                    RepairCount = equipment.RepairCount,
                    CreatedByFirstName = equipment.CreatedBy?.FirstName,
                    CreatedByMiddleName = equipment.CreatedBy?.MiddleName,
                    CreatedByLastName = equipment.CreatedBy?.LastName,
                    CreatedByExtensionName = equipment.CreatedBy?.ExtensionName
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"An error occured while loading equipment details page: {ex.Message}");
                return View("Error", "Error");
            }
        }

        #region API

        [HttpGet]
        [AuthenticateUserPrivilege(new int[] { AppUserRoleEnum.STANDARD, AppUserRoleEnum.IT, AppUserRoleEnum.ADMIN })]
        public JsonResult GetEquipment()
        {
            try
            {
                // Authenticate user
                int.TryParse(Request["userId"], out int Id);
                var associatedUser = _db.AppUsers
                    .Where(i => i.Id == Id)
                    .FirstOrDefault();
                if (associatedUser == null)
                {
                    throw new Exception("User not found.");
                }

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

                var isStandardUser = AppUserRoleEnum.IsStandard(associatedUser.RoleId);
                if (isStandardUser)
                {
                    // Fetch only equipments associated with user requests
                    query = _db.Requests
                        .Include(r => r.Equipment)
                        .Include(r => r.Equipment.Type)
                        .Include(r => r.Equipment.Status)
                        .Where(r =>
                            r.ClientId == associatedUser.Id &&
                            r.Equipment != null
                        )
                        .Select(r => r.Equipment)
                        .GroupBy(e => e.AssetTag)
                        .Select(g => g.FirstOrDefault())
                        .AsQueryable();
                }
                else
                {
                    query = _db.Equipments
                        .Include(e => e.Type)
                        .Include(e => e.Status)
                        .AsQueryable();
                }

                // Apply status filter
                if (!string.IsNullOrEmpty(statusFilter))
                {
                    if (int.TryParse(statusFilter, out int statusIntValue) && statusIntValue > 0)
                    {
                        query = query.Where(e =>
                                e.StatusId.HasValue &&
                                e.StatusId.Value == statusIntValue
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
                                e.TypeId.HasValue &&
                                e.TypeId.Value == typeIntValue
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
                        e.Model.Contains(searchValue) ||
                        e.Type.Name.Contains(searchValue) ||
                        e.Status.Name.Contains(searchValue)
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
                                ? query.OrderBy(e => e.Model)
                                : query.OrderByDescending(e => e.Model);
                            break;
                        case 3:
                            query = sortDirection == "asc"
                                ? query.OrderBy(e => e.Type.Name)
                                : query.OrderByDescending(e => e.Type.Name);
                            break;
                        case 4:
                            query = sortDirection == "asc"
                                ? query.OrderBy(e => e.Status.Name)
                                : query.OrderByDescending(e => e.Status.Name);
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
                        e.Model,
                        Type = e.Type != null
                            ? e.Type.Name
                            : "N/A",
                        Status = e.Status != null
                            ? e.Status.Name
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