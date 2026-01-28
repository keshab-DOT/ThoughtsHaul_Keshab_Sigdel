namespace MauiApp1.Models
{
    public class AnalyticsData
    {
        // Change: Added 'set' so the service can assign them
        public List<string> SentimentLabels { get; set; } = new() { "Positive", "Neutral", "Negative" };
        public List<int> SentimentCounts { get; set; } = new() { 0, 0, 0 };

        public List<string> MoodLabels { get; set; } = new();
        public List<double> MoodCounts { get; set; } = new();

        public string MostFrequentMood { get; set; } = "None";
        public List<DateTime> MissedDates { get; set; } = new();

        // Change: Added 'set' so the service can assign the count directly
        public int MissedDaysCount { get; set; }

        // Change: Changed 'int' to 'double' to allow the percentage decimals (9.5%)
        public Dictionary<string, double> CategoryBreakdown { get; set; } = new();

        public Dictionary<string, int> TopTags { get; set; } = new();
        public List<string> TrendLabels { get; set; } = new();
        public List<int> TrendCounts { get; set; } = new();
    }
}