using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskTrackPro.Core.Models;

namespace TaskTrackPro.Core.Repositories.Commands.Interfaces
{
    public interface IAccountCommand
    {
        // Task<int> Register(t_User userData);
        // Task<t_User> Login(vm_Login userData);
        Task<t_User> GetUser(int userid);
        Task<bool> ChangePassword(int userId, string newPasswordHash);
        Task<t_User> GetUserProfile(int userId);
    }
}