//using ImageResizer;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Attributes;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Core;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Enumerables;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Models;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Services;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Utilities;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Controllers
{

    public class RegistrationController : BaseController
    {
        public UserManager<ApplicationUser> UserManager { get; private set; }

        public RegistrationController()
            : this(
                  new UserManager<ApplicationUser>(
                      new UserStore<ApplicationUser>(new ApplicationDbContext())
                  )
              )
        { }

        public RegistrationController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }

        // GET: /Registration/
        [Authorize2]
        [AuthenticateUserPrivilege(new int[] { AccountTypeEnum.ADMIN })]
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

        // GET: /Registration/Details/5
        [Authorize2]
        [AuthenticateUserPrivilege(new int[] { AccountTypeEnum.STANDARD, AccountTypeEnum.IT, AccountTypeEnum.ADMIN })]
        public ActionResult Details(RegistrationDetailsViewModel model, int id, string status)
        {
            var currentUser = GetUserSession();
            if (currentUser == null)
            {
                throw new HttpException(403, "Forbidden");
            }

            /**
             * If a standard or an IT user tries to access the details of another user, 
             * redirect them to their own details page instead.
             */
            if (id != currentUser.Id &&
                (AccountTypeEnum.IsStandard(currentUser.PrivilegeIds) ||
                 AccountTypeEnum.IsIT(currentUser.PrivilegeIds)))
            {
                return RedirectToAction("Details", new { id = currentUser.Id });
            }

            var user = _db.Registrations.Find(id);
            if (user == null)
            {
                throw new HttpException(404, "Not found");
            }
            var userRegistrationRequest = _db.RegistrationRequests
                .FirstOrDefault(i => i.Id == user.RegistrationRequestId);
            var userPrivileges = _db.UserPrivileges
                .Where(i => i.RegistrationId == user.Id)
                .ToList();

            // Populate model with foreign properties
            model.User = user;
            model.UserRegistrationRequest = userRegistrationRequest;
            model.UserPrivileges = userPrivileges;

            ViewBag.CurrentUser = currentUser;
            ViewBag.CurrentUserPrivileges = currentUser.PrivilegeIds;
            return View(model);
        }

        // GET: /Registration/Create
        [Authorize2]

        public ActionResult Create(int id)
        {
            var registrationRequest = _db.RegistrationRequests
                .FirstOrDefault(i => i.Id == id);
            if (registrationRequest == null)
            {
                throw new HttpException(404, "Not found");
            }

            return View(new RegistrationCreateViewModel()
            {
                RegistrationRequest = registrationRequest
            });
        }

        // POST: /Registration/Create
        [Authorize2]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthenticateUserPrivilege(new int[] { AccountTypeEnum.ADMIN })]
        public ActionResult Create(RegistrationCreateViewModel registrationCreateViewModel, int id)
        {
            var currentUser = GetUserSession();
            if (currentUser == null)
            {
                throw new HttpException(403, "Forbidden");
            }
            ViewBag.CurrentUser = currentUser;

            var registrationRequestFiller = new RegistrationRequest()
            {
                Id = id,
                FirstName = registrationCreateViewModel.FirstName,
                MiddleName = registrationCreateViewModel.MiddleName,
                LastName = registrationCreateViewModel.LastName,
                Email = registrationCreateViewModel.Email,
                ContactNumber = registrationCreateViewModel.ContactNumber
            };

            if (id < 1)
            {
                TempData["alertModal"] = new AlertModalUtility()
                {
                    Title = "Error",
                    Message = "Invalid registration request.",
                    Status = AlertModalStatus.Error
                };
                return View(new RegistrationCreateViewModel()
                {
                    RegistrationRequest = registrationRequestFiller
                });
            }

            var registrationRequest = _db.RegistrationRequests
                .FirstOrDefault(i => i.Id == id);
            if (registrationRequest == null)
            {
                TempData["alertModal"] = new AlertModalUtility()
                {
                    Title = "Error",
                    Message = "Registration request not found.",
                    Status = AlertModalStatus.Error
                };
                return View(new RegistrationCreateViewModel()
                {
                    RegistrationRequest = registrationRequestFiller
                });
            }
            registrationCreateViewModel.RegistrationRequest = registrationRequest;

            if (!ModelState.IsValid)
            {
                var errors = GetModelStateErrors();
                return View(registrationCreateViewModel);
            }

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var isEmailTaken = _db.Registrations
                        .Count(I => I.Email == registrationCreateViewModel.Email);
                    if (isEmailTaken > 0)
                    {
                        TempData["alertModal"] = new AlertModalUtility()
                        {
                            Title = "Error",
                            Message = "Email has already been taken.",
                            Status = AlertModalStatus.Error
                        };
                        return View(registrationCreateViewModel);
                    }

                    var registration = new Registration
                    {
                        Email = registrationCreateViewModel.Email.Trim(),
                        FirstName = registrationCreateViewModel.FirstName.Trim().ToUpper(),
                        MiddleName = registrationCreateViewModel.MiddleName?.Trim().ToUpper(),
                        LastName = registrationCreateViewModel.LastName.Trim().ToUpper(),
                        RegistrationDate = DateTime.Now,
                        ExpiryDate = registrationCreateViewModel.ExpiryDate,
                        AccountType = registrationCreateViewModel.AccountType,
                        RegistrationRequestId = registrationRequest.Id,
                        ContactNumber = registrationCreateViewModel.ContactNumber.Trim(),
                        IsActive = true,
                    };

                    _db.Registrations.Add(registration);
                    registrationRequest.IsApproved = true; // Mark user registration request - approved 

                    // Generate code for password
                    string getcode = registration.RegistrationDate.Value.ToString("yyMMddHHmm");
                    // Convert the generated code to hexadecimal format to make it more complex and less predictable
                    string code = String.Format("{0:X}", Convert.ToInt64(getcode));

                    // Replace "Ñ" with "N" in the last name to avoid issues with usernames
                    registration.LastName.Replace("Ñ", "N");

                    // Update the registration with the generated username and code
                    //registration.UserName = registrationCreateViewModel.Email;
                    registration.Code = code;

                    this.CreateApplicationUser(ref registrationCreateViewModel, registration, code);

                    // this.SendEmailUponCreation(registration, code);

                    var userprivilege = new UserPrivilege
                    {
                        RegistrationId = registration.Id
                    };

                    if (AccountTypeEnum.IsAdmin(registrationCreateViewModel.AccountType))
                    {
                        userprivilege.PrivilegeId = AccountTypeEnum.ADMIN;
                    }
                    else if (AccountTypeEnum.IsIT(registrationCreateViewModel.AccountType))
                    {
                        userprivilege.PrivilegeId = AccountTypeEnum.IT;
                    }
                    else if (AccountTypeEnum.IsStandard(registrationCreateViewModel.AccountType))
                    {
                        userprivilege.PrivilegeId = AccountTypeEnum.STANDARD;
                    }
                    else
                    {
                        throw new Exception("Invalid account type.");
                    }
                    _db.UserPrivileges.Add(userprivilege);

                    // registration.SessionPrivilegeId = userprivilege.PrivilegeId;

                    // Update the registration request to indicate that the account information has been provided
                    registrationRequest.AccountInformation = true;
                    _db.Entry(registrationRequest).State = EntityState.Modified;
                    _db.SaveChanges();

                    transaction.Commit();

                    var enc = Custom.Controllers.EncryptionHelper.Encrypt(registration.Id.ToString());
                    return RedirectToAction(
                        "Success",
                        "Registration",
                        new { registrationId = enc }
                    );

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
                    ModelState.AddModelError("", "An error occurred while making a request: " + ex.Message);
                    return View(registrationCreateViewModel);
                }
            }
        }

        [Authorize2]
        [AuthenticateUserPrivilege(new int[] { AccountTypeEnum.STANDARD, AccountTypeEnum.IT, AccountTypeEnum.ADMIN })]
        public ActionResult Success(string registrationId)
        {
            try
            {
                // Decrypt the registration ID from the query string
                var dec = Custom.Controllers.EncryptionHelper.Decrypt(registrationId);
                int? id = Int32.Parse(dec);
                if (!id.HasValue)
                {
                    throw new HttpException(403, "Forbidden");
                }

                // Find the registration request by the decrypted id
                var registration = _db.Registrations
                    .FirstOrDefault(i => i.Id == id);
                if (registration == null)
                {
                    throw new HttpException(404, "Not found");
                }

                return View(new RegistrationCreateViewModel
                {
                    Registration = registration
                });
            }
            catch (Exception ex)
            {
                return RedirectToAction("NotFound", "Error");
            }
        }

        [Authorize2]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthenticateUserPrivilege(new int[] { AccountTypeEnum.ADMIN })]
        public ActionResult ManagePrivilege(int id, int update_privilege) // update_privilege -> name attribute of the select element
        {
            if (id < 1)
            {
                throw new HttpException(403, "Forbidden");
            }

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var validPrivilegeIds = new List<int>
                    {
                        AccountTypeEnum.ADMIN,
                        AccountTypeEnum.IT,
                        AccountTypeEnum.STANDARD
                    };
                    if (!validPrivilegeIds.Contains(update_privilege))
                    {
                        throw new Exception("Invalid privilege Id.");
                    }

                    var registration = _db.Registrations
                        .Include(r => r.UserPrivileges)
                        .FirstOrDefault(r => r.Id == id);
                    if (registration == null)
                    {
                        throw new HttpException(404, "Not found");
                    }

                    var currentPrivileges = registration.UserPrivileges.ToArray();
                    // Check if user does not have the privilege that is being updated
                    if (!currentPrivileges
                        .Where(p => p.PrivilegeId.HasValue)
                        .Select(r => r.PrivilegeId.Value)
                        .ToArray()
                        .Contains(update_privilege))
                    {
                        foreach (var privilege in currentPrivileges)
                        {
                            // Modify all existing privilege
                            if (privilege.PrivilegeId.HasValue)
                            {
                                privilege.PrivilegeId = update_privilege;
                                _db.Entry(privilege).State = EntityState.Modified;
                            }
                        }

                        var privilegeName = AccountTypeEnum.DisplayName(update_privilege);
                        registration.AccountType = privilegeName;
                        _db.Entry(registration).State = EntityState.Modified;

                        _db.Notifications.Add(new Notification()
                        {
                            Title = "Privilege Update",
                            Message = "Your privilege has been updated to " + privilegeName + ". Please log in again in order for this change to take effect.",
                            RecipientRegistrationId = registration.Id,
                            IsRead = false,
                            IsActive = true,
                            ForAdmin = false,
                            ForIT = false,
                            CreatedAt = DateTime.Now
                        });

                        // Update user roles based on the new privilege
                        UpdateUserRoles(registration, privilegeName);

                        _db.SaveChanges();
                        transaction.Commit();

                        // Notify user 
                        (new NotificationService()).RefreshUserUi(registration.Id);

                        TempData["AlertModal"] = new AlertModalUtility
                        {
                            Title = "Success",
                            Message = "User privilege updated successfully.",
                            Status = AlertModalStatus.Success
                        };
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    TempData["AlertModal"] = new AlertModalUtility
                    {
                        Title = "Error",
                        Message = "An error occured. Please try again.",
                        Status = AlertModalStatus.Error
                    };
                    ModelState.AddModelError("", "An error occurred while making a request: " + ex.Message);
                }
            }

            return RedirectToAction("Details", new { id = id });
        }

        [Authorize2]
        [AuthenticateUserPrivilege(new int[] { AccountTypeEnum.ADMIN })]
        public ActionResult Deactivate(int id)
        {
            if (id < 1)
            {
                throw new HttpException(403, "Forbidden");
            }

            var registration = _db.Registrations.Find(id);
            if (registration == null)
            {
                throw new HttpException(404, "Not found");
            }

            return View(new Deactivation
            {
                Registration = registration
            });
        }

        [Authorize2]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthenticateUserPrivilege(new int[] { AccountTypeEnum.ADMIN })]
        public ActionResult Deactivate(Deactivation deactivation, int id)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    if (id < 1)
                    {
                        throw new Exception("Invalid registration ID.");
                    }

                    var currentUser = _db.Registrations
                        .FirstOrDefault(x => x.Email == User.Identity.Name);
                    if (currentUser == null || !AccountTypeEnum.IsAdmin(currentUser.AccountType))
                    {
                        throw new Exception("You do not have permission to perform this action.");
                    }

                    var registration = _db.Registrations
                        .FirstOrDefault(i => i.Id == id);
                    if (registration == null)
                    {
                        TempData["alertModal"] = new AlertModalUtility()
                        {
                            Title = "Error",
                            Message = "Registration not found.",
                            Status = AlertModalStatus.Error
                        };
                        return View(id);
                    }

                    registration.IsActive = false;
                    registration.DeactivatedRemarks = "Deactivated by " + currentUser.FirstName + " " + currentUser.LastName;
                    registration.DeactivatedByRegistrationId = currentUser.Id;

                    _db.Entry(registration).State = EntityState.Modified;
                    _db.SaveChanges();

                    transaction.Commit();

                    TempData["alertModal"] = new AlertModalUtility()
                    {
                        Title = "Success",
                        Message = "You have successfully deactivated this account.",
                        Status = AlertModalStatus.Success
                    };
                    return RedirectToAction("Index", "Registration");
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
                    ModelState.AddModelError("", "An error occurred while making a request: " + ex.Message);
                    return View(id);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }

        #region API

        [Authorize2]
        [HttpGet]
        [AuthenticateUserPrivilege(new int[] { AccountTypeEnum.ADMIN })]
        public ActionResult GetRegistrations()
        {
            try
            {
                // Authenticate user
                int.TryParse(Request["userId"], out int Id);
                var associatedUser = _db.Registrations
                    .Where(i => i.Id == Id)
                    .Select(i => new
                    {
                        i.Id,
                        i.AccountType,
                        i.ContactNumber,
                        i.IsActive,
                        UserPrivileges = i.UserPrivileges
                            .Select(j => j.PrivilegeId)
                    })
                    .FirstOrDefault();
                if (associatedUser == null || associatedUser?.IsActive == false)
                {
                    throw new Exception("User not found.");
                }

                if (!associatedUser.UserPrivileges.Contains(AccountTypeEnum.ADMIN))
                {
                    throw new Exception("You do not have permission to access this resource.");
                }

                // Get DataTables parameters from request
                var draw = Request["draw"];
                var start = Request["start"];
                var length = Request["length"];
                var searchValue = Request["search[value]"];
                var sortColumn = Request["order[0][column]"];
                var sortDirection = Request["order[0][dir]"];

                var accountTypeFilter = Request["accountTypeFilter"];

                var query = _db.Registrations.Where(i => i.IsActive == true);

                // Apply account type filter
                if (int.TryParse(accountTypeFilter, out var accountTypeIntValue) && accountTypeIntValue > 0)
                {
                    query = query.Where(i =>
                        i.UserPrivileges
                            .Any(j =>
                                j.PrivilegeId.HasValue &&
                                j.PrivilegeId.Value == accountTypeIntValue
                            )
                    );
                }

                var recordsTotal = query.Count();

                // Apply search filter
                if (!string.IsNullOrEmpty(searchValue))
                {
                    query = query.Where(i =>
                        i.FirstName.Contains(searchValue) ||
                        i.MiddleName.Contains(searchValue) ||
                        i.LastName.Contains(searchValue) ||
                        i.Email.Contains(searchValue) ||
                        i.ContactNumber.Contains(searchValue) ||
                        i.AccountType.Contains(searchValue)
                    );
                }
                var recordsFiltered = query.Count();

                // Apply sorting
                if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDirection))
                {
                    int columnIndex = int.Parse(sortColumn);
                    switch (columnIndex)
                    {
                        case 1:
                            query = sortDirection == "asc"
                                ? query.OrderBy(i => i.LastName)
                                : query.OrderByDescending(i => i.LastName);
                            break;
                        case 2:
                            query = sortDirection == "asc"
                                ? query.OrderBy(i => i.Email)
                                : query.OrderByDescending(i => i.Email);
                            break;
                        case 3:
                            query = sortDirection == "asc"
                                ? query.OrderBy(i => i.ContactNumber)
                                : query.OrderByDescending(i => i.ContactNumber);
                            break;
                        case 4:
                            query = sortDirection == "asc"
                                ? query.OrderBy(i => i.AccountType)
                                : query.OrderByDescending(i => i.AccountType);
                            break;
                        default:
                            query = query.OrderBy(i => i.LastName); // Default sorting
                            break;
                    }
                }
                else
                {
                    query = query.OrderBy(i => i.LastName); // Default sorting
                }

                var data = query
                    .Skip(int.Parse(start))
                    .Take(int.Parse(length))
                    .ToList()
                    .Select(i => new
                    {
                        i.Id,
                        i.FirstName,
                        i.MiddleName,
                        i.LastName,
                        i.Email,
                        i.ContactNumber,
                        i.AccountType,
                        i.IsActive
                    })
                    .ToList();

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
                return Json(
                    new { success = false, message = ex.Message },
                    JsonRequestBehavior.AllowGet
                );
            }
        }

        [Authorize2]
        [AuthenticateUserPrivilege(new int[] { AccountTypeEnum.ADMIN })]
        public ActionResult DenyRegistration(int id)
        {
            try
            {
                if (id < 1)
                {
                    throw new Exception("Invalid regstration request Id.");
                }

                var registrationRequest = _db.RegistrationRequests
                    .FirstOrDefault(r => r.Id == id);
                if (registrationRequest == null)
                {
                    throw new Exception("Registration request not found.");
                }

                registrationRequest.IsDenied = true;
                registrationRequest.IsApproved = false;
                _db.Entry(registrationRequest).State = EntityState.Modified;
                _db.SaveChanges();

                return Json(new
                {
                    success = true,
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    succes = false,
                    error = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Helper

        private void CreateApplicationUser(ref RegistrationCreateViewModel model, Registration registration, string code)
        {

            // Generate username using email
            var username = model.Email;
            var pass = code; // Default password is the generated code, which will be sent to the user via email

            // Create a new user in the ASP.NET Identity system with the generated username and password
            var user = new ApplicationUser() { UserName = username, Email = model.Email };

            // Allow special characters in Username
            UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };
            var result = UserManager.Create(user, pass);
            if (!result.Succeeded)
            {
                throw new Exception("Failed to create user: " + string.Join(", ", result.Errors));
            }

            // If the user creation is successful, proceed to assign roles and privileges
            var officerNew = _db.Registrations.Find(registration.Id);
            //officerNew.UserName = username;

            var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(_db));
            // Check if the role for the officer's account type exists, and if not, create it
            if (!RoleManager.RoleExists(officerNew.AccountType))
            {
                var role = new IdentityRole(officerNew.AccountType);
                RoleManager.Create(role);
            }

            // Assign the officer to the appropriate role based on their account type
            var temp = _db.Users.Single(i => i.UserName == user.UserName);
            UserManager.AddToRole(temp.Id, officerNew.AccountType);
        }

        private void UpdateUserRoles(Registration registration, string privilegeName)
        {
            var appUser = UserManager.FindByEmail(registration.Email);
            if (appUser != null)
            {
                // ensure role exists
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(_db));
                if (!roleManager.RoleExists(privilegeName))
                {
                    var createRoleResult = roleManager.Create(new IdentityRole(privilegeName));
                    if (!createRoleResult.Succeeded)
                    {
                        throw new Exception("Failed to create role: " + string.Join(", ", createRoleResult.Errors));
                    }
                }

                // remove all current roles (or remove specific ones)
                var currentRoles = UserManager.GetRoles(appUser.Id).ToArray();
                if (currentRoles.Any())
                {
                    var removeResult = UserManager.RemoveFromRoles(appUser.Id, currentRoles);
                    if (!removeResult.Succeeded)
                    {
                        throw new Exception("Failed to remove user roles: " + string.Join(", ", removeResult.Errors));
                    }
                }

                // add new role
                var addResult = UserManager.AddToRole(appUser.Id, privilegeName);
                if (!addResult.Succeeded)
                {
                    throw new Exception("Failed to add role: " + string.Join(", ", addResult.Errors));
                }

                // Update security stamp to invalidate existing cookies (forces re-login on all devices)
                UserManager.UpdateSecurityStamp(appUser.Id);
            }
        }

        #endregion
    }
}
