using SQLite;
using BCrypt.Net;
using ServiceStack.DataAnnotations;
using PrimaryKeyAttribute = SQLite.PrimaryKeyAttribute;

namespace MauiApp1.Models
{
    public class User
    {
        [PrimaryKey, ServiceStack.DataAnnotations.AutoIncrement]
        public int Id { get; set; }

        [SQLite.Unique, Indexed]
        public string Username { get; set; }

        public string PasswordHash { get; set; }

        public string Salt { get; set; }
    }

    public static class PasswordHelper
    {
        
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

      
        public static bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}