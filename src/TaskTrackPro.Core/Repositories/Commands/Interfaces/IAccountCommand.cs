using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskTrackPro.Core.Models;

namespace TaskTrackPro.Core.Repositories.Commands.Interfaces
{
    public interface IAccountCommand
    {
        Task<t_User> GetUser(int userid);
        Task<bool> ChangePassword(string userEmail, string NewPassword);
        Task<t_User> GetUserProfile(int userId);
    }
}