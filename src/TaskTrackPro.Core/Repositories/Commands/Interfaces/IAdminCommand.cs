using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskTrackPro.Core.Repositories.Commands.Interfaces
{
    public interface IAdminCommand
    {
        Task<bool> DeleteUser(int userId);
    }
}