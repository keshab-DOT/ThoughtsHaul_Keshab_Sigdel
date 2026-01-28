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
                "Thoughtsaul"
            );

            Directory.CreateDirectory(folder);
            _dbPath = Path.Combine(folder, "lifeink_app.db");

            Initialize();
        }

        private void Initialize()
        {
            using var connection = GetConnection();
            connection.Open();

            using var command = connection.CreateCommand();

            // 1️⃣ Create tables (fresh installs)
            command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT UNIQUE NOT NULL,
                PasswordHash TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS JournalEntries (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId INTEGER,
                Title TEXT,
                Content TEXT,
                Category TEXT,
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

            // 2️⃣ MIGRATION: Add Category if DB already exists
            TryAddColumn(connection, "JournalEntries", "Category", "TEXT");
        }

        private void TryAddColumn(
            SqliteConnection connection,
            string table,
            string column,
            string type
        )
        {
            try
            {
                using var cmd = connection.CreateCommand();
                cmd.CommandText = $"ALTER TABLE {table} ADD COLUMN {column} {type};";
                cmd.ExecuteNonQuery();
            }
            catch
            {
                // Column already exists → ignore
            }
        }

        public SqliteConnection GetConnection()
            => new SqliteConnection($"Data Source={_dbPath}");
    }
}
