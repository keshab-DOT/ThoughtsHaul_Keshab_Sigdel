using MauiApp1.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp1.Services
{
    internal class AuthServices
    {
        private readonly DBContext _dbService;

        public bool IsLoggedIn { get; private set; } = false;
        public string? CurrentUser { get; private set; }

        public AuthServices(DBContext dbService) => _dbService = dbService;

        public async Task<bool> Register(string username, string password)
        {
            using var connection = _dbService.GetConnection();
            await connection.OpenAsync();

            var hash = BCrypt.Net.BCrypt.HashPassword(password);
            var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Users (Username, PasswordHash) VALUES ($user, $hash)";
            command.Parameters.AddWithValue("$user", username);
            command.Parameters.AddWithValue("$hash", hash);

            try
            {
                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> Login(string username, string password)
        {
            using var connection = _dbService.GetConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT PasswordHash FROM Users WHERE Username = $user";
            command.Parameters.AddWithValue("$user", username);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                string storedHash = reader.GetString(0);
                bool isValid = BCrypt.Net.BCrypt.Verify(password, storedHash);

                if (isValid)
                {
                    IsLoggedIn = true;
                    CurrentUser = username;
                    return true;
                }
            }

            IsLoggedIn = false;
            return false;
        }

        public void Logout()
        {
            IsLoggedIn = false;
            CurrentUser = null;
        }
    }
}
