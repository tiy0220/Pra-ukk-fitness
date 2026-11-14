using System;

namespace fitnesss.Models
{
    public class LogModel
    {
        public int Id { get; set; }
        public int IdUser { get; set; }
        public string Username { get; set; } = string.Empty; // Untuk tampilan JOIN
        public string Role { get; set; } = string.Empty;
        public string Activity { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}