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

                var activeStatusIds = TechnicalServiceRequestStatusEnum.GetActiveStatusIds();
                var activeRequests = visibleRequests.Count(i =>
                    i.TechnicalServiceRequestStatusId.HasValue &&
                    activeStatusIds.Contains(i.TechnicalServiceRequestStatusId.Value)
                );

                var resolvedRequests = visibleRequests.Count(i =>
                    i.TechnicalServiceRequestStatusId == TechnicalServiceRequestStatusEnum.RESOLVED ||
                    i.TechnicalServiceRequestStatusId == TechnicalServiceRequestStatusEnum.CLOSED
                );

                var unreadNotifications = GetUnreadNotificationCount(currentUser);

                var statusData = visibleRequests
                    .Where(i => i.TechnicalServiceRequestStatusId.HasValue)
                    .GroupBy(i => i.TechnicalServiceRequestStatusId.Value)
                    .Select(g => new
                    {
                        StatusId = g.Key,
                        Count = g.Count()
                    })
                    .ToList()
                    .Select(g => new
                    {
                        Name = TechnicalServiceRequestStatusEnum.DisplayName(g.StatusId),
                        Count = g.Count
                    })
                    .ToList();

                var serviceTypeData = visibleRequests
                    .Select(i => new
                    {
                        Name = i.TechnicalServiceTypeId.HasValue
                            ? i.TechnicalServiceType.TechnicalServiceTypeName
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
                        Count = _db.TechnicalServiceRequestHistories.Count(h =>
                            h.TechnicalServiceRequestId.HasValue &&
                            requestIds.Contains(h.TechnicalServiceRequestId.Value) &&
                            h.TechnicalServiceRequestStatusId.HasValue &&
                            (h.TechnicalServiceRequestStatusId.Value == TechnicalServiceRequestStatusEnum.RESOLVED ||
                             h.TechnicalServiceRequestStatusId.Value == TechnicalServiceRequestStatusEnum.CLOSED) &&
                            h.DateAction.HasValue &&
                            h.DateAction.Value.Year == month.Year &&
                            h.DateAction.Value.Month == month.Month)
                    })
                    .ToList();

                var extraCards = BuildExtraCards(currentUser, visibleRequests);

                return Json(new
                {
                    success = true,
                    role = AccountTypeEnum.IsAdmin(currentUser.PrivilegeIds)
                        ? "ADMIN"
                        : AccountTypeEnum.IsIT(currentUser.PrivilegeIds)
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

        private IQueryable<Models.TechnicalServiceRequest> GetVisibleRequestQuery(UserSession currentUser)
        {
            if (AccountTypeEnum.IsAdmin(currentUser.PrivilegeIds))
            {
                return _db.TechnicalServiceRequests;
            }

            if (AccountTypeEnum.IsIT(currentUser.PrivilegeIds))
            {
                var nonAssistedServiceIds = TechnicalServiceTypeEnum.GetNonAssistedServiceIds();
                return _db.TechnicalServiceRequests.Where(i =>
                    i.TechnicalServiceRequestHistories.Any(h => h.ActionTakenByRegistrationId == currentUser.Id) ||
                    (i.TechnicalServiceTypeId.HasValue && nonAssistedServiceIds.Contains(i.TechnicalServiceTypeId.Value))
                );
            }

            return _db.TechnicalServiceRequests.Where(i => i.ClientRegistrationId == currentUser.Id);
        }

        private int GetUnreadNotificationCount(UserSession currentUser)
        {
            var query = _db.Notifications
                .Where(i => !i.IsRead)
                .AsQueryable();

            if (AccountTypeEnum.IsAdmin(currentUser.PrivilegeIds))
            {
                query = query.Where(i => i.ForAdmin || i.RecipientRegistrationId == currentUser.Id);
            }
            else if (AccountTypeEnum.IsIT(currentUser.PrivilegeIds))
            {
                query = query.Where(i => i.ForIT || i.RecipientRegistrationId == currentUser.Id);
            }
            else
            {
                query = query.Where(i => i.RecipientRegistrationId == currentUser.Id);
            }

            return query.Count();
        }

        private object BuildExtraCards(UserSession currentUser, IQueryable<Models.TechnicalServiceRequest> visibleRequests)
        {
            if (AccountTypeEnum.IsAdmin(currentUser.PrivilegeIds))
            {
                var itAccountDisplayName = AccountTypeEnum.DisplayName(AccountTypeEnum.IT);

                var pendingRegistrationRequests = _db.RegistrationRequests
                    .Count(i => !i.IsApproved && !i.IsDenied);
                var activeUsers = _db.Registrations.Count(i => i.IsActive);
                var itUsers = _db.Registrations.Count(i => 
                    i.IsActive && 
                    i.AccountType == itAccountDisplayName);

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

            if (AccountTypeEnum.IsIT(currentUser.PrivilegeIds))
            {
                var activeStatusIds = TechnicalServiceRequestStatusEnum.GetActiveStatusIds();
                var myAssignedActive = visibleRequests.Count(i =>
                    i.TechnicalServiceRequestStatusId.HasValue &&
                    activeStatusIds.Contains(i.TechnicalServiceRequestStatusId.Value) &&
                    i.TechnicalServiceRequestHistories.Any(h => 
                        h.ActionTakenByRegistrationId == currentUser.Id
                    )
                );

                var today = DateTime.Now.Date;
                var todayResolved = _db.TechnicalServiceRequestHistories.Count(i =>
                    i.ActionTakenByRegistrationId == currentUser.Id &&
                    i.TechnicalServiceRequestStatusId.HasValue &&
                    (i.TechnicalServiceRequestStatusId.Value == TechnicalServiceRequestStatusEnum.RESOLVED ||
                     i.TechnicalServiceRequestStatusId.Value == TechnicalServiceRequestStatusEnum.CLOSED) &&
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
                i.TechnicalServiceRequestStatusId == TechnicalServiceRequestStatusEnum.PENDING);
            var cancelledCount = visibleRequests.Count(i => 
                i.TechnicalServiceRequestStatusId == TechnicalServiceRequestStatusEnum.CANCELLED);
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
    }

    #endregion
}