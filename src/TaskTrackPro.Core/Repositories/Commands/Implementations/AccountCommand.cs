using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using RabbitMQ.Client.Exceptions;
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

        // public async Task<int> Register(t_User userData)
        // {
        //     var qry = "Insert into t_user_task (c_uname, c_email,c_gender,c_password) values (@c_uname, @c_email,@c_gender,@c_password)";

        //     try
        //     {
        //         using (NpgsqlCommand cmd = new NpgsqlCommand(qry, _conn))
        //         {
        //             cmd.Parameters.AddWithValue("@c_uname", userData.c_uname);
        //             cmd.Parameters.AddWithValue("@c_email", userData.c_email);
        //             cmd.Parameters.AddWithValue("@c_password", userData.c_password);
        //             cmd.Parameters.AddWithValue("@c_gender", userData.c_gender);
        //             // cmd.Parameters.AddWithValue("@c_profilepicture", userData.c_profilepicture ?? (object)DBNull.Value);

        //             _conn.Close();
        //             _conn.Open();
        //             cmd.ExecuteNonQuery();
        //             _conn.Close();

        //             return 1;
        //         }
        //     }
        //     catch (Exception e)
        //     {
        //         Console.WriteLine(e.Message);
        //         return -1;

        //     }
        //     finally
        //     {
        //         await _conn.CloseAsync();
        //     }
        // }


        public async Task<int> Register(t_User user){
            try{
                await _conn.CloseAsync();
                await _conn.OpenAsync();

                string checkQuery = "SELECT COUNT(*) FROM t_user_task WHERE c_email = @c_email";
                using (NpgsqlCommand cmd = new NpgsqlCommand(checkQuery, _conn))
                {
                    cmd.Parameters.AddWithValue("@c_email", user.c_email);
                    int existingCount = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    if(existingCount > 0){
                        await _conn.CloseAsync();
                        return 0;
                    }
                }
                string insertQuery = @"
                INSERT INTO t_user_task(c_uname, c_email,c_gender,c_password,c_profilepicture) values (@c_uname, @c_email,@c_gender,@c_password, @c_profilepicture) RETURNING c_uid";
                using (NpgsqlCommand comInsert = new NpgsqlCommand(insertQuery, _conn))
                {
                    comInsert.Parameters.AddWithValue("@c_uname", user.c_uname);
                    comInsert.Parameters.AddWithValue("@c_email", user.c_email);
                    comInsert.Parameters.AddWithValue("@c_password", user.c_password);
                    comInsert.Parameters.AddWithValue("@c_gender", user.c_gender);
                    comInsert.Parameters.AddWithValue("@c_profilepicture", (object?)user.c_profilepicture ?? DBNull.Value);

                    int userId = Convert.ToInt32(await comInsert.ExecuteScalarAsync());
                    await _conn.CloseAsync();
                    return userId;
            }
        }catch (Exception ex){
            await _conn.CloseAsync();
            throw new Exception("An error occurred while registering the user: " + ex.Message);
        }
        }

        public async Task<t_User> Login(vm_Login userData)
        {
            var qry = "SELECT c_uid, c_uname, c_email, c_gender FROM t_user_task WHERE c_email = @c_email AND c_password = @c_password";

            try
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand(qry, _conn))
                {
                    cmd.Parameters.AddWithValue("@c_email", userData.c_email);
                    cmd.Parameters.AddWithValue("@c_password", userData.c_password);

                    _conn.Close();
                    _conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var user = new t_User
                            {
                                c_uid = reader.GetInt32(0),
                                c_uname = reader.GetString(1),
                                c_email = reader.GetString(2),
                                c_gender = reader.GetString(3)
                            };

                            return user; // Return user object
                        }
                    }
                    _conn.Close();
                }
                return null; // No user found
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public Task<bool> ChangePassword(int userId, string newPasswordHash)
        {
            throw new NotImplementedException();
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