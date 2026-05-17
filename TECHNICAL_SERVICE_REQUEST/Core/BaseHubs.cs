using Microsoft.AspNet.SignalR;
using System.Linq;
using System.Threading.Tasks;
using TECHNICAL_SERVICE_REQUEST.Attributes;
using TECHNICAL_SERVICE_REQUEST.Enumerables;
using TECHNICAL_SERVICE_REQUEST.Models;

namespace TECHNICAL_SERVICE_REQUEST.Core
{
    public abstract class BaseHub : Hub
    {
        protected const string AdminGroupName = "ADMIN";
        protected const string ITGroupName = "IT";

        public override async Task OnConnected()
        {
            var email = Context.User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(email))
            {
                // If the email is not available, we cannot determine the recipient, so we won't add the connection to any group.
                await base.OnConnected();
                return;
            }

            using (var _db = new ApplicationDbContext())
            {
                // Look up the registration ID associated with the email.
                var userPermission = _db.AppUsers
                    .Where(r => r.Email == email)
                    .Select(r => r.Role.Name)
                    .FirstOrDefault();
                if (!string.IsNullOrEmpty(userPermission))
                {
                    if (AppUserRoleEnum.IsAdmin(userPermission))
                    {
                        // Add the connection to a group named "ADMIN" so that we can target notifications to admins
                        await Groups.Add(Context.ConnectionId, AdminGroupName);
                    }
                    else if (AppUserRoleEnum.IsIT(userPermission))
                    {
                        // Add the connection to a group named "IT" so that we can target notifications to IT staff
                        await Groups.Add(Context.ConnectionId, ITGroupName);
                    }
                }

                await base.OnConnected();
            }
        }

        private async Task JoinGroup(string groupName)
        {
            await Groups.Add(Context.ConnectionId, groupName);
        }

        private async Task LeaveGroup(string groupName)
        {
            await Groups.Remove(Context.ConnectionId, groupName);
        }
    }
}