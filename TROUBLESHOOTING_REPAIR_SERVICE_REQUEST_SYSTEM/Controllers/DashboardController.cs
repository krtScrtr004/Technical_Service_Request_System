using Serilog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Attributes;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Core;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Enumerables;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Controllers
{
    [Authorize2]
    [AuthenticateUserPrivilege(new int[] { AccountTypeEnum.STANDARD, AccountTypeEnum.IT, AccountTypeEnum.ADMIN })]
    public class DashboardController : BaseController
    {
        public ActionResult Index()
        {
            var currentUser = GetUserSession();
            if (currentUser == null)
            {
                return RedirectToAction("Unauthorized", "Error");
            }

            ViewBag.CurrentUser = currentUser;
            return View();
        }

        #region API

        [HttpGet]
        public ActionResult GetDashboardData()
        {
            try
            {
                var currentUser = GetUserSession();
                if (currentUser == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "You are not authorized to view dashboard data."
                    }, JsonRequestBehavior.AllowGet);
                }

                var visibleRequests = GetVisibleRequestQuery(currentUser).AsNoTracking();
                var totalRequests = visibleRequests.Count();

                var activeStatusIds = RequestStatusEnum.GetActiveStatusIds();
                var activeRequests = visibleRequests.Count(i =>
                    i.StatusId.HasValue &&
                    activeStatusIds.Contains(i.StatusId.Value)
                );

                var resolvedRequests = visibleRequests.Count(i =>
                    i.StatusId == RequestStatusEnum.RESOLVED ||
                    i.StatusId == RequestStatusEnum.CLOSED
                );

                var unreadNotifications = GetUnreadNotificationCount(currentUser);

                var statusData = visibleRequests
                    .Where(i => i.StatusId.HasValue)
                    .GroupBy(i => i.StatusId.Value)
                    .Select(g => new
                    {
                        StatusId = g.Key,
                        Count = g.Count()
                    })
                    .ToList()
                    .Select(g => new
                    {
                        Name = RequestStatusEnum.DisplayName(g.StatusId),
                        Count = g.Count
                    })
                    .ToList();

                var serviceTypeData = visibleRequests
                    .Select(i => new
                    {
                        Name = i.TypeId.HasValue
                            ? i.Type.Name
                            : "Others"
                    })
                    .GroupBy(i => i.Name)
                    .Select(g => new
                    {
                        Name = g.Key,
                        Count = g.Count()
                    })
                    .OrderByDescending(i => i.Count)
                    .Take(8)
                    .ToList();

                var monthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-5);
                var monthBuckets = Enumerable.Range(0, 6)
                    .Select(offset => monthStart.AddMonths(offset))
                    .ToList();

                var monthlySubmitted = monthBuckets
                    .Select(month => new
                    {
                        Label = month.ToString("MMM yyyy"),
                        Count = visibleRequests.Count(i =>
                            i.DateRequest.HasValue &&
                            i.DateRequest.Value.Year == month.Year &&
                            i.DateRequest.Value.Month == month.Month)
                    })
                    .ToList();

                var requestIds = visibleRequests.Select(i => i.Id);
                var monthlyResolved = monthBuckets
                    .Select(month => new
                    {
                        Label = month.ToString("MMM yyyy"),
                        Count = _db.RequestHistories.Count(h =>
                            h.RequestId.HasValue &&
                            requestIds.Contains(h.RequestId.Value) &&
                            h.StatusId.HasValue &&
                            (h.StatusId.Value == RequestStatusEnum.RESOLVED ||
                             h.StatusId.Value == RequestStatusEnum.CLOSED) &&
                            h.DateAction.HasValue &&
                            h.DateAction.Value.Year == month.Year &&
                            h.DateAction.Value.Month == month.Month)
                    })
                    .ToList();

                var topRepairedEquipments = BuildTopRepairedEquipment(currentUser)
                    .Take(10)
                    .ToList();

                var extraCards = BuildExtraCards(currentUser, visibleRequests);

                return Json(new
                {
                    success = true,
                    role = AccountTypeEnum.IsAdmin(currentUser.RoleId)
                        ? "ADMIN"
                        : AccountTypeEnum.IsIT(currentUser.RoleId)
                            ? "IT"
                            : "STANDARD",
                    cards = new
                    {
                        totalRequests,
                        activeRequests,
                        resolvedRequests,
                        unreadNotifications,
                        extra = extraCards
                    },
                    charts = new
                    {
                        requestByStatus = statusData,
                        requestByType = serviceTypeData,
                        monthlySubmitted,
                        monthlyResolved
                    },
                    tables = new
                    {
                        topRepairedEquipments
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error while fetching dashboard data for user {GetUserSession()?.Id.ToString() ?? "Unknown"}");
                return Json(new
                {
                    success = false,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Helper

        private IQueryable<Models.Request> GetVisibleRequestQuery(UserSession currentUser)
        {
            if (AccountTypeEnum.IsAdmin(currentUser.RoleId))
            {
                return _db.Requests;
            }

            if (AccountTypeEnum.IsIT(currentUser.RoleId))
            {
                var nonAssistedServiceIds = RequestTypeEnum.GetNonAssistedServiceIds();
                return _db.Requests.Where(i =>
                    i.Histories.Any(h => h.ActionTakenById == currentUser.Id) ||
                    (i.TypeId.HasValue && nonAssistedServiceIds.Contains(i.TypeId.Value))
                );
            }

            return _db.Requests.Where(i => i.ClientId == currentUser.Id);
        }

        private int GetUnreadNotificationCount(UserSession currentUser)
        {
            var query = _db.Notifications
                .Where(i => !i.IsRead)
                .AsQueryable();

            if (AccountTypeEnum.IsAdmin(currentUser.RoleId))
            {
                query = query.Where(i => i.ForAdmin || i.RecipientRegistrationId == currentUser.Id);
            }
            else if (AccountTypeEnum.IsIT(currentUser.RoleId))
            {
                query = query.Where(i => i.ForIT || i.RecipientRegistrationId == currentUser.Id);
            }
            else
            {
                query = query.Where(i => i.RecipientRegistrationId == currentUser.Id);
            }

            return query.Count();
        }

        private object BuildExtraCards(UserSession currentUser, IQueryable<Models.Request> visibleRequests)
        {
            if (AccountTypeEnum.IsAdmin(currentUser.RoleId))
            {
                var pendingRegistrationRequests = _db.RegistrationRequests
                    .Count(i => !i.IsApproved && !i.IsDenied);
                var activeUsers = _db.Registrations.Count(i => i.IsActive);
                var itUsers = _db.Registrations.Count(i => 
                    i.IsActive && 
                    i.RoleId == AccountTypeEnum.IT);

                return new
                {
                    firstLabel = "Pending Registration Requests",
                    firstValue = pendingRegistrationRequests,
                    secondLabel = "Active Users",
                    secondValue = activeUsers,
                    thirdLabel = "Active IT Users",
                    thirdValue = itUsers
                };
            }

            if (AccountTypeEnum.IsIT(currentUser.RoleId))
            {
                var activeStatusIds = RequestStatusEnum.GetActiveStatusIds();
                var myAssignedActive = visibleRequests.Count(i =>
                    i.StatusId.HasValue &&
                    activeStatusIds.Contains(i.StatusId.Value) &&
                    i.Histories.Any(h => h.ActionTakenById == currentUser.Id)
                );

                var today = DateTime.Now.Date;
                var todayResolved = _db.RequestHistories.Count(i =>
                    i.ActionTakenById == currentUser.Id &&
                    i.StatusId.HasValue &&
                    (i.StatusId.Value == RequestStatusEnum.RESOLVED ||
                     i.StatusId.Value == RequestStatusEnum.CLOSED) &&
                     i.DateAction.HasValue && DbFunctions.TruncateTime(i.DateAction.Value) == DbFunctions.TruncateTime(today)
                );

                var blockedDays = _db.ITAvailabilities.Count(i => 
                    i.RegistrationId == currentUser.Id && 
                    i.BlockDate >= today
                );

                return new
                {
                    firstLabel = "My Active Assigned",
                    firstValue = myAssignedActive,
                    secondLabel = "Resolved Today",
                    secondValue = todayResolved,
                    thirdLabel = "Blocked Days (Upcoming)",
                    thirdValue = blockedDays
                };
            }

            var pendingCount = visibleRequests.Count(i => 
                i.StatusId == RequestStatusEnum.PENDING);
            var cancelledCount = visibleRequests.Count(i => 
                i.StatusId == RequestStatusEnum.CANCELLED);
            var monthStartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var submittedThisMonth = visibleRequests.Count(i => 
                i.DateRequest.HasValue && 
                i.DateRequest.Value >= monthStartDate
            );

            return new
            {
                firstLabel = "Pending Requests",
                firstValue = pendingCount,
                secondLabel = "Cancelled Requests",
                secondValue = cancelledCount,
                thirdLabel = "Submitted This Month",
                thirdValue = submittedThisMonth
            };
        }

        private IEnumerable<object> BuildTopRepairedEquipment(UserSession currentUser)
        {
            IQueryable<Models.Equipment> query;

            if (AccountTypeEnum.IsAdmin(currentUser.RoleId) || AccountTypeEnum.IsIT(currentUser.RoleId))
            {
                query = _db.Equipments.AsNoTracking();
            }
            else
            {
                query = _db.Requests
                    .AsNoTracking()
                    .Where(r => r.ClientId == currentUser.Id && r.EquipmentId.HasValue)
                    .Select(r => r.Equipment)
                    .Where(e => e != null);
            }

            var allowedStatuses = new[]
            {
                EquipmentStatusEnum.OPERATIONAL,
                EquipmentStatusEnum.UNDER_REPAIR
            };

            return query
                .Where(e => e.StatusId.HasValue && allowedStatuses.Contains(e.StatusId.Value))
                .GroupBy(e => e.Id)
                .Select(g => g.FirstOrDefault())
                .OrderByDescending(e => e.RepairCount)
                .AsEnumerable()
                .Select(e => new
                {
                    e.Id,
                    e.AssetTag,
                    e.Model,
                    Type = e.TypeId.HasValue
                        ? EquipmentTypeEnum.DisplayName(e.TypeId.Value)
                        : "N/A",
                    Status = e.StatusId.HasValue
                        ? EquipmentStatusEnum.DisplayName(e.StatusId.Value)
                        : "N/A",
                    e.RepairCount
                });
        }
    }

    #endregion
}