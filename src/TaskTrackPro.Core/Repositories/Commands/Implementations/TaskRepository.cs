using System;
using System.Threading.Tasks;
using Npgsql;
using TaskTrackPro.Core.Models;
using TaskTrackPro.Core.Repositories.Commands.Interfaces;

namespace TaskTrackPro.Core.Repositories.Commands.Implementations
{
    public class TaskRepository : ITaskInterface
    {
        private readonly string _connectionString;

        public TaskRepository(NpgsqlConnection connection)
        {
            _connectionString = connection.ConnectionString;
        }

        public async Task<int> Add(t_task data)
        {
            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                await using var cm = new NpgsqlCommand(@"
                    INSERT INTO t_task (c_uid, c_task_title, c_description, c_start_date, c_end_date, c_task_status) 
                    VALUES (@c_uid, @c_task_title, @c_description, @c_start_date, @c_end_date, @c_task_status)
                    RETURNING c_tid;", conn);

                cm.Parameters.AddWithValue("@c_uid", data.c_uid);
                cm.Parameters.AddWithValue("@c_task_title", data.c_task_title);
                cm.Parameters.AddWithValue("@c_description", data.c_description);
                cm.Parameters.AddWithValue("@c_start_date", data.c_start_date);
                cm.Parameters.AddWithValue("@c_end_date", data.c_end_date);
                cm.Parameters.AddWithValue("@c_task_status", data.c_task_status);

                var insertedId = await cm.ExecuteScalarAsync();

                return insertedId != null ? Convert.ToInt32(insertedId) : 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return 0;
            }
        }

        public async Task<List<t_User>> GetAllUsers()
        {
            var users = new List<t_User>();

            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                await using var cmd = new NpgsqlCommand("SELECT * FROM t_user_task", conn);
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    users.Add(new t_User
                    {
                        c_uid = reader.GetInt32(0),
                        c_uname = reader.GetString(1),
                        c_email = reader.GetString(2),
                        c_gender = reader.GetString(4),
                        // c_profilepicture = reader.GetString(6)
                        // c_profilepicture = reader.IsDBNull(5) ? "default.png" : reader.GetString(5),
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return users;
        }


        public async Task<List<string>> GetApprovedUsernames()
        {
            var usernames = new List<string>();

            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                await using var cmd = new NpgsqlCommand("SELECT c_uname FROM t_user_task WHERE c_approve_status = TRUE", conn);
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    usernames.Add(reader.GetString(0)); // Only storing usernames
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return usernames;
        }


        public async Task<bool> ApproveUser(int userId)
        {
            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                string query = "UPDATE t_user_task SET c_approve_status = TRUE WHERE c_uid = @userId";

                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@userId", userId);

                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                return rowsAffected > 0; // ✅ Returns true if update was successful
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }


        public async Task<int> Update(t_task taskData)
        {
            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                string query = @"
            UPDATE t_task 
            SET 
                c_uid = @c_uid, 
                c_task_title = @c_task_title, 
                c_description = @c_description, 
                c_start_date = @c_start_date, 
                c_end_date = @c_end_date, 
                c_task_status = @c_task_status
            WHERE c_tid = @c_tid";

                await using var cm = new NpgsqlCommand(query, conn);

                cm.Parameters.AddWithValue("@c_tid", taskData.c_tid); // ✅ Identify by Task ID
                cm.Parameters.AddWithValue("@c_uid", taskData.c_uid);
                cm.Parameters.AddWithValue("@c_task_title", taskData.c_task_title);
                cm.Parameters.AddWithValue("@c_description", taskData.c_description);
                cm.Parameters.AddWithValue("@c_start_date", taskData.c_start_date);
                cm.Parameters.AddWithValue("@c_end_date", taskData.c_end_date);
                cm.Parameters.AddWithValue("@c_task_status", taskData.c_task_status);

                int rowsAffected = await cm.ExecuteNonQueryAsync();
                return rowsAffected; // ✅ Returns number of affected rows
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return 0;
            }
        }


        public async Task<int> Delete(int c_TaskID)
        {
            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                string query = "DELETE FROM t_task WHERE c_tid = @c_TaskID";

                await using var cm = new NpgsqlCommand(query, conn);
                cm.Parameters.AddWithValue("@c_TaskID", c_TaskID);

                int rowsAffected = await cm.ExecuteNonQueryAsync();
                return rowsAffected; // ✅ Returns number of affected rows
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return 0;
            }
        }

        public async Task<List<t_task>> GetAllTask()
        {
            var tasks = new List<t_task>();

            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                await using var cmd = new NpgsqlCommand("SELECT * FROM t_task", conn);
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    tasks.Add(new t_task
                    {
                        c_tid = reader.GetInt32(0),
                        c_uid = reader.GetInt32(1),
                        c_task_title = reader.GetString(2),
                        c_description = reader.GetString(3),
                        c_start_date = reader.GetDateTime(4),
                        c_end_date = reader.GetDateTime(5),
                        c_task_status = reader.GetString(6)
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return tasks;
        }

        public async Task<List<t_task>> GetTaskById(int userId)
        {
            var tasks = new List<t_task>();

            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                await using var cmd = new NpgsqlCommand("SELECT * FROM t_task where c_uid = @c_uid", conn);
                cmd.Parameters.AddWithValue("@c_uid", userId);
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    tasks.Add(new t_task
                    {
                        c_tid = reader.GetInt32(0),
                        c_uid = reader.GetInt32(1),
                        c_task_title = reader.GetString(2),
                        c_description = reader.GetString(3),
                        c_start_date = reader.GetDateTime(4),
                        c_end_date = reader.GetDateTime(5),
                        c_task_status = reader.GetString(6)
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return tasks;
        }

    }
}
