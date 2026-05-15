//using ImageResizer;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Serilog;
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
using TECHNICAL_SERVICE_REQUEST.Attributes;
using TECHNICAL_SERVICE_REQUEST.Core;
using TECHNICAL_SERVICE_REQUEST.Enumerables;
using TECHNICAL_SERVICE_REQUEST.Models;
using TECHNICAL_SERVICE_REQUEST.Services;
using TECHNICAL_SERVICE_REQUEST.Utilities;

namespace TECHNICAL_SERVICE_REQUEST.Controllers
{
    [RoutePrefix("User")]
    public class AppUserController : BaseController
    {
        public UserManager<ApplicationUser> UserManager { get; private set; }

        public AppUserController()
            : this(
                  new UserManager<ApplicationUser>(
                      new UserStore<ApplicationUser>(new ApplicationDbContext())
                  )
              )
        { }

        public AppUserController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }

        [Route("Index")]
        [Authorize2]
        [AuthenticateUserPrivilege(new int[] { AppUserRoleEnum.ADMIN })]
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
                Log.Error(ex, $"An error occured while loading accounts list page: {ex.Message}");
                return View("Error", "Error");
            }
        }

        [Route("Details/{id}")]
        [Authorize2]
        [AuthenticateUserPrivilege(new int[] { AppUserRoleEnum.STANDARD, AppUserRoleEnum.IT, AppUserRoleEnum.ADMIN })]
        public ActionResult Details(AppUserDetailsViewModel model, int id)
        {
            var currentUser = GetAppUserSession();
            if (currentUser == null)
            {
                throw new HttpException(403, "Forbidden");
            }

            /**
             * If a standard or an IT user tries to access the details of another user, 
             * redirect them to their own details page instead.
             */
            if (id != currentUser.Id &&
                (AppUserRoleEnum.IsStandard(currentUser.RoleId) ||
                 AppUserRoleEnum.IsIT(currentUser.RoleId)))
            {
                return RedirectToAction("Details", new { id = currentUser.Id });
            }

            var user = _db.AppUsers
                .Include(i => i.Role)
                .FirstOrDefault(i => i.Id == id);
            if (user == null)
            {
                throw new HttpException(404, "Not found");
            }

            try
            {
                var registration = _db.AppUserRegistrations
                    .FirstOrDefault(i => i.Id == user.RegistrationId);

                // Populate model with foreign properties
                model.User = user;
                model.Registration = registration;

                ViewBag.CurrentUser = currentUser;
                return View(model);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"An error occured while loading account details page: {ex.Message}");
                return View("Error", "Error");
            }
        }

        [Route("Create/{id}")]
        [Authorize2]
        [AuthenticateUserPrivilege(new int[] { AppUserRoleEnum.ADMIN })]
        public ActionResult Create(int id)
        {
            var registration = _db.AppUserRegistrations
                .FirstOrDefault(i => i.Id == id);
            if (registration == null)
            {
                throw new HttpException(404, "Not found");
            }

            try
            {
                return View(new AppUserCreateViewModel
                {
                    Registration = registration
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"An error occured while loading account create page: {ex.Message}");
                return View("Error", "Error");
            }
            
        }

        [Route("Create/{id}")]
        [Authorize2]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthenticateUserPrivilege(new int[] { AppUserRoleEnum.ADMIN })]
        public ActionResult Create(AppUserCreateViewModel appUserCreateViewModel, int id)
        {
            var currentUser = GetAppUserSession();
            if (currentUser == null)
            {
                throw new HttpException(403, "Forbidden");
            }
            ViewBag.CurrentUser = currentUser;

            var registrationFiller = new AppUserRegistration()
            {
                Id = id,
                FirstName = appUserCreateViewModel.FirstName,
                MiddleName = appUserCreateViewModel.MiddleName,
                LastName = appUserCreateViewModel.LastName,
                ExtensionName = appUserCreateViewModel.ExtensionName,
                Email = appUserCreateViewModel.Email,
                ContactNumber = appUserCreateViewModel.ContactNumber,
                Office = appUserCreateViewModel.Office,
                Position = appUserCreateViewModel.Position
            };

            if (id < 1)
            {
                TempData["alertModal"] = new AlertModalUtility()
                {
                    Title = "Error",
                    Message = "Invalid registration request.",
                    Status = AlertModalStatus.Error
                };
                return View(new AppUserCreateViewModel()
                {
                    Registration = registrationFiller
                });
            }

            var registration = _db.AppUserRegistrations
                .FirstOrDefault(i => i.Id == id);
            if (registration == null)
            {
                TempData["alertModal"] = new AlertModalUtility()
                {
                    Title = "Error",
                    Message = "Registration request not found.",
                    Status = AlertModalStatus.Error
                };
                return View(new AppUserCreateViewModel()
                {
                    Registration = registrationFiller
                });
            }            
            appUserCreateViewModel.Registration = registration;

            if (!ModelState.IsValid)
            {
                var errors = GetModelStateErrors();
                Log.Warning($"Model state is invalid: {errors}");
                return View(appUserCreateViewModel);
            }

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var isEmailTaken = _db.AppUsers
                        .Count(I => I.Email == appUserCreateViewModel.Email);
                    if (isEmailTaken > 0)
                    {
                        TempData["alertModal"] = new AlertModalUtility()
                        {
                            Title = "Error",
                            Message = "Email has already been taken.",
                            Status = AlertModalStatus.Error
                        };
                        return View(appUserCreateViewModel);
                    }

                    var user = new AppUser
                    {
                        Email = appUserCreateViewModel.Email.Trim(),
                        FirstName = appUserCreateViewModel.FirstName.Trim().ToUpper(),
                        MiddleName = appUserCreateViewModel.MiddleName?.Trim().ToUpper(),
                        LastName = appUserCreateViewModel.LastName.Trim().ToUpper(),
                        ExtensionName = appUserCreateViewModel.ExtensionName?.Trim().ToUpper(),
                        Office = appUserCreateViewModel.Office?.Trim().ToUpper(),
                        Position = appUserCreateViewModel.Position?.Trim().ToUpper(),
                        RegistrationDate = DateTime.Now,
                        ExpiryDate = appUserCreateViewModel.ExpiryDate,
                        RegistrationId = registration.Id,
                        ContactNumber = appUserCreateViewModel.ContactNumber.Trim(),
                        IsActive = true,
                        RoleId = appUserCreateViewModel.RoleId
                    };

                    _db.AppUsers.Add(user);
                    registration.IsApproved = true; // Mark user registration - approved 

                    // Generate code for password
                    string getcode = user.RegistrationDate.Value.ToString("yyMMddHHmm");
                    // Convert the generated code to hexadecimal (form)at to make it more complex and less predictable
                    string code = String.Format("{0:X}", Convert.ToInt64(getcode));

                    // Replace "Ñ" with "N" in the last name to avoid issues with usernames
                    user.LastName = user.LastName.Replace("Ñ", "N");

                    // Update the registration with the generated username and code
                    user.Code = code;

                    this.CreateApplicationUser(ref appUserCreateViewModel, user, code);

                    // this.SendEmailUponCreation(registration, code);

                    _db.Entry(registration).State = EntityState.Modified;
                    _db.SaveChanges();

                    transaction.Commit();

                    var enc = Custom.Controllers.EncryptionHelper.Encrypt(user.Id.ToString());

                    Log.Information($"Registration created successfully for registration ID {user.Id} by admin ID {currentUser.Id}.");
                    return RedirectToAction(
                        "Success",
                        "User",
                        new { userId = enc }
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

                    Log.Error($"An error occurred while creating registration for registration request ID {id} by admin ID {currentUser.Id}: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while making a request: " + ex.Message);

                    return View(appUserCreateViewModel);
                }
            }
        }

        [Route("Success/")]
        [Authorize2]
        [AuthenticateUserPrivilege(new int[] { AppUserRoleEnum.STANDARD, AppUserRoleEnum.IT, AppUserRoleEnum.ADMIN })]
        public ActionResult Success(string userId)
        {
            try
            {
                // Decrypt the user ID from the query string
                var dec = Custom.Controllers.EncryptionHelper.Decrypt(userId);
                int? id = Int32.Parse(dec);
                if (!id.HasValue)
                {
                    throw new HttpException(403, "Forbidden");
                }

                // Find the registration by the decrypted id
                var user = _db.AppUsers
                    .FirstOrDefault(i => i.Id == id);
                if (user == null)
                {
                    throw new HttpException(404, "Not found");
                }

                return View(new AppUserCreateViewModel
                {
                    User = user
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"An error occurred while loading account creation success page: {ex.Message}");
                return RedirectToAction("NotFound", "Error");
            }
        }

        [Route("ManagePrivilege/{id}")]
        [Authorize2]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthenticateUserPrivilege(new int[] { AppUserRoleEnum.ADMIN })]
        public ActionResult ManagePrivilege(int id, int update_privilege) // update_privilege -> name attribute of the select element
        {
            if (id < 1)
            {
                throw new HttpException(403, "Forbidden");
            }

            using (var transaction = _db.Database.BeginTransaction())
            {
                var validPrivilegeIds = new List<int>
                    {
                        AppUserRoleEnum.ADMIN,
                        AppUserRoleEnum.IT,
                        AppUserRoleEnum.STANDARD
                    };
                if (!validPrivilegeIds.Contains(update_privilege))
                {
                    throw new Exception("Invalid privilege Id.");
                }

                var user = _db.AppUsers
                    .FirstOrDefault(r => r.Id == id);
                if (user == null)
                {
                    throw new HttpException(404, "Not found");
                }

                try
                {
                    // Check for active service when user is an IT
                    if (AppUserRoleEnum.IsIT(user.RoleId) && IsTechnicianBusy(user.Id))
                    {
                        TempData["AlertModal"] = new AlertModalUtility
                        {
                            Title = "Error",
                            Message = "Cannot modify user privilege with an active service.",
                            Status = AlertModalStatus.Error
                        };
                        return RedirectToAction("Details", new { id = id });
                    }

                    // Check if user does not have the privilege that is being updated
                    if (user.RoleId != update_privilege)
                    {
                        user.RoleId = update_privilege;

                        _db.Entry(user).State = EntityState.Modified;
                        var roleName = AppUserRoleEnum.DisplayName(update_privilege);
                        _db.Notifications.Add(new Notification()
                        {
                            Title = "Privilege Update",
                            Message = "Your privilege has been updated to " + roleName + ". Please log in again in order for this change to take effect.",
                            RecipientId = user.Id,
                            IsRead = false,
                            IsActive = true,
                            ForAdmin = false,
                            ForIT = false,
                            CreatedAt = DateTime.Now
                        });

                        // Update user roles based on the new privilege
                        UpdateUserRoles(user, roleName);

                        _db.SaveChanges();
                        transaction.Commit();

                        // Notify user 
                        (new NotificationService()).RefreshUserUi(user.Id);

                        TempData["AlertModal"] = new AlertModalUtility
                        {
                            Title = "Success",
                            Message = "User privilege updated successfully.",
                            Status = AlertModalStatus.Success
                        };
                        Log.Information($"User privilege updated successfully for registration ID {user.Id} by admin ID {GetAppUserSession()?.Id.ToString() ?? "Unknown"}. New privilege: {roleName}");
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Log.Error(ex, $"An error occurred while updating user privilege for registration ID {user.Id} by admin ID {GetAppUserSession()?.Id.ToString() ?? "Unknown"}.");
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

        [Route("Deactivate/{id}")]
        [Authorize2]
        [AuthenticateUserPrivilege(new int[] { AppUserRoleEnum.ADMIN })]
        public ActionResult Deactivate(int id)
        {
            if (id < 1)
            {
                throw new HttpException(403, "Forbidden");
            }

            var user = _db.AppUsers.Find(id);
            if (user == null)
            {
                throw new HttpException(404, "Not found");
            }

            try
            {
                return View(new AppUserDeactivationViewModel
                {
                    User = user
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"An error occured while loading account deactivation page: {ex.Message}");
                return View("Error", "Error");
            }
        }

        [Route("Deactivate/{id}")]
        [Authorize2]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthenticateUserPrivilege(new int[] { AppUserRoleEnum.ADMIN })]
        public ActionResult Deactivate(AppUserDeactivationViewModel deactivation, int id)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    if (id < 1)
                    {
                        throw new Exception("Invalid registration ID.");
                    }

                    var currentUser = _db.AppUsers
                        .FirstOrDefault(x => x.Email == User.Identity.Name);
                    if (currentUser == null)
                    {
                        throw new Exception("You do not have permission to perform this action.");
                    }

                    var user = _db.AppUsers
                        .FirstOrDefault(i => i.Id == id);
                    if (user == null)
                    {
                        Log.Warning($"Attempt to deactivate non-existent user with ID {id} by admin ID {currentUser.Id}.");
                        TempData["alertModal"] = new AlertModalUtility()
                        {
                            Title = "Error",
                            Message = "Registration not found.",
                            Status = AlertModalStatus.Error
                        };
                        return View(id);
                    }

                    // Check for active service when user is an IT
                    if (AppUserRoleEnum.IsIT(user.RoleId) && IsTechnicianBusy(user.Id))
                    {
                        TempData["AlertModal"] = new AlertModalUtility
                        {
                            Title = "Error",
                            Message = "Cannot deactivate a user with an active service.",
                            Status = AlertModalStatus.Error
                        };
                        return RedirectToAction("Details", new { id = id });
                    }

                    user.IsActive = false;
                    user.DeactivatedRemarks = "Deactivated by " + currentUser.FirstName + " " + currentUser.LastName;
                    user.DeactivatedById = currentUser.Id;

                    _db.Entry(user).State = EntityState.Modified;
                    _db.SaveChanges();

                    transaction.Commit();

                    TempData["alertModal"] = new AlertModalUtility()
                    {
                        Title = "Success",
                        Message = "You have successfully deactivated this account.",
                        Status = AlertModalStatus.Success
                    };
                    Log.Information($"User with ID {user.Id} deactivated successfully by admin ID {currentUser.Id}.");
                    return RedirectToAction("Index", "User");
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
                    Log.Error(ex, $"An error occurred while deactivating registration with ID {id} by admin ID {GetAppUserSession()?.Id.ToString() ?? "Unknown"}.");
                    ModelState.AddModelError("", "An error occurred while making a request: " + ex.Message);
                    return View(id);
                }
            }
        }

        #region API

        [Route("GetUsers/")]
        [Authorize2]
        [HttpGet]
        [AuthenticateUserPrivilege(new int[] { AppUserRoleEnum.ADMIN })]
        public ActionResult GetUsers()
        {
            try
            {
                // Authenticate user
                int.TryParse(Request["userId"], out int Id);
                var associatedUser = _db.AppUsers
                    .Where(i => i.Id == Id)
                    .Select(i => new
                    {
                        i.Id,
                        i.RoleId,
                        i.ContactNumber,
                        i.IsActive,
                    })
                    .FirstOrDefault();
                if (associatedUser == null || associatedUser?.IsActive == false)
                {
                    throw new Exception("User not found.");
                }

                if (!AppUserRoleEnum.IsAdmin(associatedUser.RoleId))
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

                var query = _db.AppUsers
                    .Include(i => i.Role)
                    .Where(i => i.IsActive == true);

                // Apply account type filter
                if (int.TryParse(accountTypeFilter, out var accountTypeIntValue) && accountTypeIntValue > 0)
                {
                    query = query.Where(i => i.RoleId == accountTypeIntValue);
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
                        i.Role.Name.Contains(searchValue)
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
                                ? query.OrderBy(i => i.Role.Name)
                                : query.OrderByDescending(i => i.Role.Name);
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
                        Role = i.Role.Name,
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
                Log.Error(ex, $"An error occurred while fetching user data by admin ID {GetAppUserSession()?.Id.ToString() ?? "Unknown"}.");
                return Json(
                    new { success = false, message = ex.Message },
                    JsonRequestBehavior.AllowGet
                );
            }
        }

        [Route("DenyRegistration/{id}")]
        [Authorize2]
        [AuthenticateUserPrivilege(new int[] { AppUserRoleEnum.ADMIN })]
        public ActionResult DenyRegistration(int id)
        {
            try
            {
                if (id < 1)
                {
                    throw new Exception("Invalid user request Id.");
                }

                var registration = _db.AppUserRegistrations
                    .FirstOrDefault(r => r.Id == id);
                if (registration == null)
                {
                    throw new Exception("User request not found.");
                }

                registration.IsDenied = true;
                registration.IsApproved = false;
                _db.Entry(registration).State = EntityState.Modified;
                _db.SaveChanges();

                Log.Information($"User request with ID {id} denied successfully by admin ID {GetAppUserSession()?.Id.ToString() ?? "Unknown"}.");
                return Json(new
                {
                    success = true,
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"An error occurred while denying user request with ID {id} by admin ID {GetAppUserSession()?.Id.ToString() ?? "Unknown"}.");
                return Json(new
                {
                    succes = false,
                    error = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Helper

        private void CreateApplicationUser(ref AppUserCreateViewModel model, AppUser appUser, string code)
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

            var roleName = AppUserRoleEnum.DisplayName(appUser.RoleId);
            var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(_db));
            // Check if the role for the officer's account type exists, and if not, create it
            if (!RoleManager.RoleExists(roleName))
            {
                var role = new IdentityRole(roleName);
                RoleManager.Create(role);
            }

            // Assign the officer to the appropriate role based on their account type
            var temp = _db.Users.Single(i => i.UserName == user.UserName);
            UserManager.AddToRole(temp.Id, roleName);
        }

        private void UpdateUserRoles(AppUser appUser, string privilegeName)
        {
            var user = UserManager.FindByEmail(appUser.Email);
            if (user != null)
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
                var currentRoles = UserManager.GetRoles(user.Id).ToArray();
                if (currentRoles.Any())
                {
                    var removeResult = UserManager.RemoveFromRoles(user.Id, currentRoles);
                    if (!removeResult.Succeeded)
                    {
                        throw new Exception("Failed to remove user roles: " + string.Join(", ", removeResult.Errors));
                    }
                }

                // add new role
                var addResult = UserManager.AddToRole(user.Id, privilegeName);
                if (!addResult.Succeeded)
                {
                    throw new Exception("Failed to add role: " + string.Join(", ", addResult.Errors));
                }

                // Update security stamp to invalidate existing cookies (forces re-login on all devices)
                UserManager.UpdateSecurityStamp(user.Id);
            }
        }

        private bool IsTechnicianBusy(int technicianId)
        {
            var activeStatusIds = RequestStatusEnum.GetActiveStatusIds();
            var nonAssistedRequestIds = RequestTypeEnum.GetNonAssistedServiceIds();

            return _db.Requests.Any(r =>
                r.StatusId.HasValue &&
                activeStatusIds.Contains(r.StatusId.Value) &&
                r.TypeId.HasValue &&
                !nonAssistedRequestIds.Contains(r.TypeId.Value) &&
                r.Histories
                    .OrderByDescending(h => h.UpdatedAt)
                    .Select(h => h.ActionTakenById)
                    .FirstOrDefault() == technicianId
            );
        }

        #endregion
    }
}
