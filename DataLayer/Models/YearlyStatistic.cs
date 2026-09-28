namespace DataLayer.Models
{
    public class YearlyStatistic
    {
        public long Id { get; set; }
        
        public long PlayerId { get; set; }
        public Player? Player { get; set; }

        public int Year { get; set; }

        public int Wins { get; set; }
        public int MatchesPlayed { get; set; }
        public double Average { get; set; }
        public int HighestOut { get; set; }
        public int Sixty { get; set; }
        public int Hundred { get; set; }
        public int Hundred20 { get; set; }
        public int Hundred80 { get; set; }
    }
}