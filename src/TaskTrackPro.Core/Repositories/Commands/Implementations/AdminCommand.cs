using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using TaskTrackPro.Core.Repositories.Commands.Interfaces;

namespace TaskTrackPro.Core.Repositories.Commands.Implementations
{
    public class AdminCommand : IAdminCommand
    {
        private readonly string _connectionString;
        public AdminCommand(NpgsqlConnection connection)
        {
            _connectionString = connection.ConnectionString;
        }
        public async Task<bool> DeleteUser(int userId)
        {
            bool isDeleted = false;

            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                await using var cmd = new NpgsqlCommand("DELETE FROM t_user_task WHERE c_uid = @UserId", conn);
                cmd.Parameters.AddWithValue("@UserId", userId);

                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                isDeleted = rowsAffected > 0; // Returns true if at least one row is deleted
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return isDeleted;
        }
    }
}