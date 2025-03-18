using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using TaskTrackPro.Core.Repositories.Queries.Interfaces;

namespace TaskTrackPro.Core.Repositories.Queries.Implementations
{
    public class TaskCountRepository : ITaskCount
    {
        private readonly NpgsqlConnection _conn;

        public TaskCountRepository(NpgsqlConnection connection)
        {
            _conn = connection;
        }

        public int GetAllMembers()
        {
            NpgsqlCommand cmd = new NpgsqlCommand("select count(*) from t_user_task", _conn);
            _conn.Close();
            _conn.Open();
            
            var value = (int)(long)cmd.ExecuteScalar();
            _conn.Close();
            return value;
        }

        public int GetApproveMembers()
        {
            NpgsqlCommand cmd = new NpgsqlCommand("select count(*) from t_user_task Where c_approve_status = 'true'", _conn);
            _conn.Close();
            _conn.Open();
            
            var value = (int)(long)cmd.ExecuteScalar();
            _conn.Close();
            return value;
        }

        public int GetCompletedCount(int userId)
        {
            NpgsqlCommand cmd = new NpgsqlCommand("select count(*) from t_user_task where c_task_status=@Status AND c_uid=@Id", _conn);
            _conn.Close();
            cmd.Parameters.AddWithValue("@Status", "COMPLETE");
            cmd.Parameters.AddWithValue("@Id", userId);
            _conn.Open();

            return (int)(long)cmd.ExecuteScalar();
        }

        public int GetInProgressCount(int userId)
        {
            NpgsqlCommand cmd = new NpgsqlCommand("select count(*) from t_user_task where c_task_status=@Status AND c_uid=@Id", _conn);
            _conn.Close();
            cmd.Parameters.AddWithValue("@Status", "IN-PROGRESS");
            cmd.Parameters.AddWithValue("@Id", userId);
            _conn.Open();

            return (int)(long)cmd.ExecuteScalar();
        }

        public int GetPendingCount(int userId)
        {
             NpgsqlCommand cmd = new NpgsqlCommand("select count(*) from t_user_task where c_task_status=@Status AND c_uid=@Id", _conn);
            _conn.Close();
            cmd.Parameters.AddWithValue("@Status", "PENDING");
            cmd.Parameters.AddWithValue("@Id", userId);
            _conn.Open();

            return (int)(long)cmd.ExecuteScalar();
        }

        public int GetPendingTask()
        {
            NpgsqlCommand cmd = new NpgsqlCommand("select count(*) from t_task where c_task_status=@Status", _conn);
            _conn.Close();
            cmd.Parameters.AddWithValue("@Status", "PENDING");
            _conn.Open();

            return (int)(long)cmd.ExecuteScalar();
        }
    }
}