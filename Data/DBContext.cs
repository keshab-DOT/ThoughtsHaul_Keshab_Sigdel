using Microsoft.Data.Sqlite;
using System.IO;

namespace MauiApp1.Data
{
    public class DBContext
    {
        private readonly string _dbPath;

        public DBContext()
        {
            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ThoughsHaul"
            );

            Directory.CreateDirectory(folder);
            _dbPath = Path.Combine(folder, "ThoughsHaul_app.db");
            Initialize();
        }

        private void Initialize()
        {
            using var connection = GetConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT UNIQUE NOT NULL,
                PasswordHash TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS JournalEntries (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT,
                Content TEXT,
                EntryDate TEXT UNIQUE NOT NULL,
                PrimaryMood TEXT,
                SecondaryMoods TEXT,
                Tags TEXT,
                CreatedAt TEXT,
                UpdatedAt TEXT
            );

            CREATE TABLE IF NOT EXISTS CustomTags (
                TagName TEXT PRIMARY KEY
            );
            """;
            command.ExecuteNonQuery();
        }

        public SqliteConnection GetConnection() => new SqliteConnection($"Data Source={_dbPath}");
    }
}