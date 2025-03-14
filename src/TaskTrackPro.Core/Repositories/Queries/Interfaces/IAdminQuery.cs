using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskTrackPro.Core.Models;

namespace TaskTrackPro.Core.Repositories.Queries.Interfaces
{
    public interface IAdminQuery
    {
        Task<List<t_User>> GetAllUsers(); 
    }
}