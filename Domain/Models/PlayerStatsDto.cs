namespace Domain.Models
{
    public class PlayerStatsDto
    {
        public long PlayerId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
    
        public int Year { get; set; }
    
        // Aktuální rok
        public int Wins { get; set; }
        public double Average { get; set; }
        public int HighestOut { get; set; }
        public int Sixty { get; set; }
        public int Hundred { get; set; }
        public int Hundred20 { get; set; }
        public int Hundred80 { get; set; }
    }
}