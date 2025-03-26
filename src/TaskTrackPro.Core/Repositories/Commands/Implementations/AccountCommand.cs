using System;
using System.Threading.Tasks;
using BCrypt.Net;
using Npgsql;
using TaskTrackPro.Core.Models;
using TaskTrackPro.Core.Repositories.Commands.Interfaces;

namespace TaskTrackPro.Core.Repositories.Commands.Implementations
{
    public class AccountCommand : IAccountCommand
    {
        private readonly NpgsqlConnection _conn;

        public AccountCommand(NpgsqlConnection connection)
        {
            _conn = connection;
        }

        public async Task<bool> ChangePassword(string userEmail, string newPassword)
        {
            try
            {
                await _conn.OpenAsync();

                // Hash the new password before storing it
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
                Console.WriteLine($"Hashed Password: {hashedPassword}"); // Debugging

                string query = "UPDATE t_user_task SET c_password = @c_password WHERE c_email = @c_email";
                using (var cmd = new NpgsqlCommand(query, _conn))
                {
                    cmd.Parameters.AddWithValue("@c_password", hashedPassword); // Store hashed password
                    cmd.Parameters.AddWithValue("@c_email", userEmail);

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating password: {ex.Message}");
                return false;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }



        public Task<t_User> GetUser(int userid)
        {
            throw new NotImplementedException();
        }

        public Task<t_User> GetUserProfile(int userId)
        {
            throw new NotImplementedException();
        }
    }
}
