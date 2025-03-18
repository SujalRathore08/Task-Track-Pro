using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskTrackPro.Core.Repositories.Queries.Interfaces
{
    public interface ITaskCount
    {
        public int GetPendingCount(int userId);

        public int GetCompletedCount(int userId);

        public int GetInProgressCount(int userId);
        public int GetAllMembers();
        public int GetApproveMembers();
        public int GetPendingTask();
    }
}