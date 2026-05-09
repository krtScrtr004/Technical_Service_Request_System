using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Models;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Core
{
    public class AppUserSession
    {
        // Cache values to avoid repeated session lookups
        private int _id;
        private string _firstName;
        private string _lastName;
        private string _middleName;
        private string _extensionName;
        private string _email;
        private string _contactNumber;
        private string _office;
        private string _position;
        private int _roleId;

        public int Id => _id;
        public string FirstName => _firstName;
        public string LastName => _lastName;
        public string MiddleName => _middleName;
        public string ExtensionName => _extensionName;
        public string Email => _email;
        public string ContactNumber => _contactNumber;
        public string Office => _office;
        public string Position => _position;
        public int RoleId => _roleId;

        public AppUserSession(
            int id,
            string firstName,
            string lastName,
            string middleName,
            string extensionName,
            string email,
            string contactNumber,
            string office,
            string position,
            int roleId
        )
        {
            if (HttpContext.Current?.Session == null)
            {
                throw new InvalidOperationException("HttpContext.Current.Session is not available.");
            }

            _id = id;
            _firstName = firstName;
            _lastName = lastName;
            _middleName = middleName;
            _extensionName = extensionName;
            _email = email;
            _contactNumber = contactNumber;
            _office = office;
            _position = position;
            _roleId = roleId;

            // Store in session for persistence across requests
            HttpContext.Current.Session["Id"] = id;
            HttpContext.Current.Session["firstName"] = firstName;
            HttpContext.Current.Session["lastName"] = lastName;
            HttpContext.Current.Session["middleName"] = middleName;
            HttpContext.Current.Session["extensionName"] = extensionName;
            HttpContext.Current.Session["email"] = email;
            HttpContext.Current.Session["contactNumber"] = contactNumber;
            HttpContext.Current.Session["office"] = office;
            HttpContext.Current.Session["position"] = position;
            HttpContext.Current.Session["roleId"] = roleId;
        }

        public AppUser ToAppUser()
        {

            return new AppUser
            {
                Id = this.Id,
                FirstName = this.FirstName,
                LastName = this.LastName,
                MiddleName = this.MiddleName,
                ExtensionName = this.ExtensionName,
                Email = this.Email,
                ContactNumber = this.ContactNumber,
                Office = this.Office,
                Position = this.Position,
                RoleId = this.RoleId
            };
        }

        public static AppUserSession LoadFromSession()
        {
            var session = HttpContext.Current?.Session;
            if (session == null)
            {
                return null;
            }

            if (session["Id"] == null)
            {
                return null;
            }

            return new AppUserSession(
                (int)session["Id"],
                (string)session["firstName"],
                (string)session["lastName"],
                (string)session["middleName"],
                (string)session["extensionName"],
                (string)session["email"],
                (string)session["contactNumber"],
                (string)session["office"],
                (string)session["position"],
                (int)session["roleId"]
            );
        }
    }

    public class UserSessionProvider
    {
        private readonly ApplicationDbContext _db;

        public UserSessionProvider(ApplicationDbContext db)
        {
            _db = db;
        }

        /// Load user session from session state, or from DB if session expired.
        public AppUserSession GetCurrentUserSession(string userEmail)
        {
            // Try session first
            var userSession = AppUserSession.LoadFromSession();
            if (userSession != null)
            {
                return userSession;
            }

            // Session expired/missing — load from DB
            var registration = _db.AppUsers
                .Where(r => r.Email == userEmail)
                .FirstOrDefault();
            if (registration == null)
            {
                return null;
            }

            // Create new session from DB record
            var newSession = new AppUserSession(
                id: registration.Id,
                firstName: registration.FirstName,
                lastName: registration.LastName,
                middleName: registration.MiddleName,
                extensionName: registration.ExtensionName,
                email: registration.Email,
                contactNumber: registration.ContactNumber,
                office: registration.Office,
                position: registration.Position,
                roleId: registration.RoleId
            );

            return newSession;
        }
    }
}