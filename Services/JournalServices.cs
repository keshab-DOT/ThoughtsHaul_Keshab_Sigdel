using MauiApp1.Data;
using MauiApp1.Models;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Text.RegularExpressions;

namespace MauiApp1.Services
{
    public class DashboardStats
    {
        public int TotalEntries { get; set; } = 0;
        public int CurrentStreak { get; set; } = 0;
        public int AverageWords { get; set; } = 0;
        public int LongestStreak { get; set; } = 0;
        public int[] ChartData { get; set; } = Array.Empty<int>();
        public string[] ChartLabels { get; set; } = Array.Empty<string>();
    }

    public class JournalService
    {
        private readonly DBContext _dbService;

        public JournalService(DBContext dbService)
        {
            _dbService = dbService;
        }

        // --- TAG METHODS ---

        public async Task<List<string>> GetAvailableTags()
        {
            using var connection = _dbService.GetConnection();
            await connection.OpenAsync();

            try
            {
                var checkCmd = connection.CreateCommand();
                checkCmd.CommandText = "ALTER TABLE JournalEntries ADD COLUMN Category TEXT;";
                await checkCmd.ExecuteNonQueryAsync();
            }
            catch { }

            var tags = new List<string>();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT TagName FROM CustomTags ORDER BY TagName ASC";

            try
            {
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    tags.Add(reader.GetString(0));
                }
            }
            catch
            {
                var createCmd = connection.CreateCommand();
                createCmd.CommandText = "CREATE TABLE IF NOT EXISTS CustomTags (TagName TEXT PRIMARY KEY);";
                await createCmd.ExecuteNonQueryAsync();

                string[] defaults = { "Work", "Personal", "Goals" };
                foreach (var d in defaults) await SaveNewTag(d);
                return defaults.ToList();
            }
            return tags;
        }

        public async Task SaveNewTag(string tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName)) return;
            using var connection = _dbService.GetConnection();
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = "INSERT OR IGNORE INTO CustomTags (TagName) VALUES ($tag)";
            command.Parameters.AddWithValue("$tag", tagName.Trim());
            await command.ExecuteNonQueryAsync();
        }

        public async Task DeleteTag(string tagName)
        {
            using var connection = _dbService.GetConnection();
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM CustomTags WHERE TagName = $tag";
            command.Parameters.AddWithValue("$tag", tagName);
            await command.ExecuteNonQueryAsync();
        }

        // --- CALENDAR & DATE SPECIFIC METHODS ---

        public async Task<List<DateTime>> GetEntryDates()
        {
            using var connection = _dbService.GetConnection();
            await connection.OpenAsync();
            var dates = new List<DateTime>();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT EntryDate FROM JournalEntries";
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                if (DateTime.TryParse(reader.GetString(0), out var date))
                    dates.Add(date.Date);
            }
            return dates;
        }

        public async Task<JournalEntry?> GetEntryByDate(DateTime date)
        {
            using var connection = _dbService.GetConnection();
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Title, Content, Category, PrimaryMood, SecondaryMoods, Tags, EntryDate, CreatedAt, UpdatedAt FROM JournalEntries WHERE EntryDate = $date LIMIT 1";
            command.Parameters.AddWithValue("$date", date.ToString("yyyy-MM-dd"));
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapReaderToEntry(reader);
            }
            return null;
        }

        public async Task DeleteByDate(DateTime date)
        {
            using var connection = _dbService.GetConnection();
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM JournalEntries WHERE EntryDate = $date";
            command.Parameters.AddWithValue("$date", date.ToString("yyyy-MM-dd"));
            await command.ExecuteNonQueryAsync();
        }

        public async Task DeleteToday()
        {
            await DeleteByDate(DateTime.Today);
        }

        // --- GENERAL JOURNAL METHODS ---

        public async Task<List<JournalEntry>> GetAllEntries()
        {
            using var connection = _dbService.GetConnection();
            await connection.OpenAsync();
            var entries = new List<JournalEntry>();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Title, Content, Category, PrimaryMood, SecondaryMoods, Tags, EntryDate, CreatedAt, UpdatedAt FROM JournalEntries ORDER BY EntryDate DESC";
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                entries.Add(MapReaderToEntry(reader));
            }
            return entries;
        }

        public async Task DeleteEntry(int id)
        {
            using var connection = _dbService.GetConnection();
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM JournalEntries WHERE Id = $id";
            command.Parameters.AddWithValue("$id", id);
            await command.ExecuteNonQueryAsync();
        }

        public async Task SaveOrUpdate(JournalEntry entry)
        {
            using var connection = _dbService.GetConnection();
            await connection.OpenAsync();
            var command = connection.CreateCommand();

            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            command.CommandText =
            """
            INSERT INTO JournalEntries (Title, Content, Category, EntryDate, PrimaryMood, SecondaryMoods, Tags, CreatedAt, UpdatedAt)
            VALUES ($title, $content, $category, $date, $pMood, $sMood, $tags, $created, $updated)
            ON CONFLICT(EntryDate) DO UPDATE SET
                Title = excluded.Title,
                Content = excluded.Content,
                Category = excluded.Category,
                PrimaryMood = excluded.PrimaryMood,
                SecondaryMoods = excluded.SecondaryMoods,
                Tags = excluded.Tags,
                UpdatedAt = excluded.UpdatedAt;
            """;

            command.Parameters.AddWithValue("$title", entry.Title ?? "");
            command.Parameters.AddWithValue("$content", entry.Content ?? "");
            command.Parameters.AddWithValue("$category", entry.Category ?? "");
            command.Parameters.AddWithValue("$date", entry.EntryDate.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("$pMood", entry.PrimaryMood ?? "");
            command.Parameters.AddWithValue("$sMood", entry.SecondaryMoods ?? "");
            command.Parameters.AddWithValue("$tags", entry.Tags ?? "");
            command.Parameters.AddWithValue("$created", now);
            command.Parameters.AddWithValue("$updated", now);
            await command.ExecuteNonQueryAsync();
        }

        private JournalEntry MapReaderToEntry(SqliteDataReader reader)
        {
            return new JournalEntry
            {
                Id = reader.GetInt32(0),
                Title = reader.IsDBNull(1) ? "" : reader.GetString(1),
                Content = reader.IsDBNull(2) ? "" : reader.GetString(2),
                Category = reader.IsDBNull(3) ? "" : reader.GetString(3),
                PrimaryMood = reader.IsDBNull(4) ? "" : reader.GetString(4),
                SecondaryMoods = reader.IsDBNull(5) ? "" : reader.GetString(5),
                Tags = reader.IsDBNull(6) ? "" : reader.GetString(6),
                EntryDate = reader.IsDBNull(7) ? DateTime.Now : DateTime.Parse(reader.GetString(7)),
                CreatedAt = reader.IsDBNull(8) ? DateTime.Now : DateTime.Parse(reader.GetString(8)),
                UpdatedAt = reader.IsDBNull(9) ? DateTime.Now : DateTime.Parse(reader.GetString(9))
            };
        }

        // --- DASHBOARD & ANALYTICS ---

        public async Task<DashboardStats> GetDashboardStats(int days = 7)
        {
            var allEntries = await GetAllEntries();
            var stats = new DashboardStats();

            if (allEntries == null || !allEntries.Any()) return stats;

            stats.TotalEntries = allEntries.Count;
            var cutoffDate = DateTime.Today.AddDays(-days);
            var filteredEntries = allEntries
                .Where(e => e.EntryDate >= cutoffDate)
                .OrderBy(e => e.EntryDate)
                .ToList();

            stats.ChartLabels = filteredEntries.Select(e => e.EntryDate.ToString("MMM dd")).ToArray();
            stats.ChartData = filteredEntries.Select(e => {
                var plainText = Regex.Replace(e.Content ?? "", "<.*?>", string.Empty);
                return plainText.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
            }).ToArray();

            stats.AverageWords = stats.ChartData.Length > 0 ? (int)stats.ChartData.Average() : 0;

            var entryDates = allEntries.Select(e => e.EntryDate.Date).OrderByDescending(d => d).ToHashSet();

            int current = 0;
            DateTime checkDate = DateTime.Today;
            if (!entryDates.Contains(checkDate)) checkDate = DateTime.Today.AddDays(-1);

            while (entryDates.Contains(checkDate))
            {
                current++;
                checkDate = checkDate.AddDays(-1);
            }
            stats.CurrentStreak = current;

            int longest = 0;
            int tempStreak = 0;
            var sortedDates = entryDates.OrderBy(d => d).ToList();
            for (int i = 0; i < sortedDates.Count; i++)
            {
                tempStreak++;
                if (i == sortedDates.Count - 1 || sortedDates[i + 1] != sortedDates[i].AddDays(1))
                {
                    if (tempStreak > longest) longest = tempStreak;
                    tempStreak = 0;
                }
            }
            stats.LongestStreak = longest;

            return stats;
        }

        public async Task<JournalEntry?> GetEntryById(int id)
        {
            using var connection = _dbService.GetConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Title, Content, Category, PrimaryMood, SecondaryMoods, Tags, EntryDate, CreatedAt, UpdatedAt FROM JournalEntries WHERE Id = $id LIMIT 1";
            command.Parameters.AddWithValue("$id", id);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapReaderToEntry(reader);
            }
            return null;
        }

        public async Task<AnalyticsData> GetAnalyticsData(int days = 30)
        {
            var allEntries = await GetAllEntries();
            var data = new AnalyticsData();
            if (allEntries == null || !allEntries.Any()) return data;

            var cutoffDate = DateTime.Today.AddDays(-days + 1);
            var rangeEntries = allEntries.Where(e => e.EntryDate.Date >= cutoffDate).ToList();
            var entryDatesInRange = rangeEntries.Select(e => e.EntryDate.Date).ToHashSet();

            // 1. Sentiment Grouping
            var positiveMoods = new[] { "Happy", "Excited", "Relaxed", "Grateful", "Confident" };
            var neutralMoods = new[] { "Calm", "Thoughtful", "Focused", "Nostalgic" };
            var negativeMoods = new[] { "Anxious", "Sad", "Stressed", "Lonely", "Angry", "Bored" };

            // Use the existing property labels, just update counts
            data.SentimentCounts = new List<int>
    {
        rangeEntries.Count(e => positiveMoods.Contains(e.PrimaryMood)),
        rangeEntries.Count(e => neutralMoods.Contains(e.PrimaryMood)),
        rangeEntries.Count(e => negativeMoods.Contains(e.PrimaryMood))
    };

            var moodGroups = rangeEntries
                .Where(e => !string.IsNullOrEmpty(e.PrimaryMood))
                .GroupBy(e => e.PrimaryMood)
                .OrderByDescending(g => g.Count())
                .ToList();

            data.MostFrequentMood = moodGroups.FirstOrDefault()?.Key ?? "No Data";

            // 2. Missed Days
            for (int i = 0; i < days; i++)
            {
                var date = DateTime.Today.AddDays(-i);
                if (!entryDatesInRange.Contains(date))
                {
                    data.MissedDates.Add(date);
                }
            }
            data.MissedDaysCount = data.MissedDates.Count; // This now works with the 'set' added above

            // 3. Category Breakdown (Fixed Dictionary Type Mismatch)
            var totalWithCat = rangeEntries.Count(e => !string.IsNullOrEmpty(e.Category));
            if (totalWithCat > 0)
            {
                data.CategoryBreakdown = rangeEntries
                    .Where(e => !string.IsNullOrEmpty(e.Category))
                    .GroupBy(e => e.Category)
                    // Ensure this matches Dictionary<string, double>
                    .ToDictionary(g => g.Key, g => Math.Round((double)g.Count() / totalWithCat * 100, 1));
            }

            // 4. Tag Frequency
            data.TopTags = rangeEntries
                .Where(e => !string.IsNullOrEmpty(e.Tags))
                .SelectMany(e => e.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .GroupBy(t => t.Trim())
                .OrderByDescending(g => g.Count())
                .Take(10)
                .ToDictionary(g => g.Key, g => g.Count());

            // 5. Trend Chart
            var trendEntries = rangeEntries.OrderBy(e => e.EntryDate).ToList();
            data.TrendLabels = trendEntries.Select(e => e.EntryDate.ToString("MMM dd")).ToList();
            data.TrendCounts = trendEntries.Select(e => {
                var plainText = Regex.Replace(e.Content ?? "", "<.*?>", string.Empty);
                return plainText.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
            }).ToList();

            return data;
        }
    }
}