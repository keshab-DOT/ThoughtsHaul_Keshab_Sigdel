//using SQLite;
//using BCrypt.Net;

//namespace MauiApp1.Models
//{
//    public class User
//    {
//        [PrimaryKey, AutoIncrement]
//        public int Id { get; set; }

//        [Unique, Indexed]
//        public string Username { get; set; }

//        public string PasswordHash { get; set; }

//        public string Salt { get; set; }
//    }

//    public static class PasswordHelper
//    {
//        // Hashes a plain-text password
//        public static string HashPassword(string password)
//        {
//            return BCrypt.Net.BCrypt.HashPassword(password);
//        }

//        // Verifies a plain-text password against a stored hash
//        public static bool VerifyPassword(string password, string hash)
//        {
//            return BCrypt.Net.BCrypt.Verify(password, hash);
//        }
//    }
//}