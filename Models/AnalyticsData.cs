namespace MauiApp1.Models
{
    public class AnalyticsData
    {
        public List<string> SentimentLabels { get; set; } = new() { "Positive", "Neutral", "Negative" };
        public List<int> SentimentCounts { get; set; } = new() { 0, 0, 0 };

        public List<string> MoodLabels { get; set; } = new();
        public List<double> MoodCounts { get; set; } = new();

        public string MostFrequentMood { get; set; } = "None";
        public List<DateTime> MissedDates { get; set; } = new();
        public int MissedDaysCount { get; set; }

        public Dictionary<string, double> CategoryBreakdown { get; set; } = new();

        public Dictionary<string, int> TopTags { get; set; } = new();
        public List<string> TrendLabels { get; set; } = new();
        public List<int> TrendCounts { get; set; } = new();
    }
}