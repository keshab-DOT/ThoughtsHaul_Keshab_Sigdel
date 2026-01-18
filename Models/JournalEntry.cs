using SQLite;
using System;

namespace MauiApp1.Models
{
    public class JournalEntry
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public string Category { get; set; } = "";
        public string PrimaryMood { get; set; } = "";
        public string SecondaryMoods { get; set; } = "";
        public string Tags { get; set; } = "";

        public DateTime EntryDate { get; set; } = DateTime.Today;

        public DateTime Date { get => EntryDate; set => EntryDate = value; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}