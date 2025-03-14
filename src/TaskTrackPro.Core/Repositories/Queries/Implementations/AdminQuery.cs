using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using TaskTrackPro.Core.Models;
using TaskTrackPro.Core.Repositories.Queries.Interfaces;

namespace TaskTrackPro.Core.Repositories.Queries.Implementations
{
    public class AdminQuery : IAdminQuery
    {
        private readonly string _connectionString;
        public AdminQuery(NpgsqlConnection connection)
        {
            _connectionString = connection.ConnectionString;
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
                        c_profilepicture = reader.IsDBNull(5) ? "default.png" : reader.GetString(5),
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return users;
        }
    }
}