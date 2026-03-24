using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Models;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Core
{
    public class UserSession
    {
        // Cache values to avoid repeated session lookups
        private int _id;
        private string _firstName;
        private string _lastName;
        private string _middleName;
        private string _extensionName;
        private string _userName;
        private string _email;
        private string _contactNumber;
        private int[] _privilegeIds;

        public int Id => _id;
        public string FirstName => _firstName;
        public string LastName => _lastName;
        public string MiddleName => _middleName;
        public string ExtensionName => _extensionName;
        public string UserName => _userName;
        public string Email => _email;
        public string ContactNumber => _contactNumber;
        public int[] PrivilegeIds => _privilegeIds;

        public UserSession(
            int id,
            string firstName,
            string lastName,
            string middleName,
            string extensionName,
            string userName,
            string email,
            string contactNumber,
            int[] privilegeIds
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
            _userName = userName;
            _email = email;
            _contactNumber = contactNumber;
            _privilegeIds = privilegeIds;

            // Store in session for persistence across requests
            HttpContext.Current.Session["Id"] = id;
            HttpContext.Current.Session["firstName"] = firstName;
            HttpContext.Current.Session["lastName"] = lastName;
            HttpContext.Current.Session["middleName"] = middleName;
            HttpContext.Current.Session["extensionName"] = extensionName;
            HttpContext.Current.Session["userName"] = userName;
            HttpContext.Current.Session["email"] = email;
            HttpContext.Current.Session["contactNumber"] = contactNumber;
            HttpContext.Current.Session["privilegeIds"] = privilegeIds;
        }

        public Registration ToRegistration()
        {
            var privileges = new List<UserPrivilege>();
            foreach (var privilegeId in this.PrivilegeIds)
            {
                privileges.Add(new UserPrivilege
                {
                    RegistrationId = this.Id,
                    PrivilegeId = privilegeId
                });
            }

            return new Registration
            {
                Id = this.Id,
                FirstName = this.FirstName,
                LastName = this.LastName,
                MiddleName = this.MiddleName,
                UserName = this.UserName,
                Email = this.Email,
                ContactNumber = this.ContactNumber,
                UserPrivileges = privileges
            };
        }

        public static UserSession LoadFromSession()
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

            return new UserSession(
                (int)session["Id"],
                (string)session["firstName"],
                (string)session["lastName"],
                (string)session["middleName"],
                (string)session["extensionName"],
                (string)session["userName"],
                (string)session["email"],
                (string)session["contactNumber"],
                (int[])session["privilegeIds"]
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
        public UserSession GetCurrentUserSession(string userEmail)
        {
            // Try session first
            var userSession = UserSession.LoadFromSession();
            if (userSession != null)
            {
                return userSession;
            }

            // Session expired/missing — load from DB
            var registration = _db.Registrations
                .Include(r => r.UserPrivileges)
                .Where(r => r.Email == userEmail)
                .FirstOrDefault();
            if (registration == null)
            {
                return null;
            }

            // Create new session from DB record
            var newSession = new UserSession(
                id: registration.Id,
                firstName: registration.FirstName,
                lastName: registration.LastName,
                middleName: registration.MiddleName,
                extensionName: string.Empty,
                userName: registration.UserName,
                email: registration.Email,
                contactNumber: registration.ContactNumber,
                privilegeIds: registration.UserPrivileges
                    .Where(p => p.PrivilegeId.HasValue)
                    .Select(p => p.PrivilegeId.Value)
                    .ToArray()
            );

            return newSession;
        }
    }
}