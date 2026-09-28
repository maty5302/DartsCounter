namespace Domain
{
    public class PlayerMatchStatistics
    {
        private readonly record struct ThrowRecord(string ScoreText, int ScoreValue, bool IsCheckout);

        private readonly List<ThrowRecord> _throws = new();

        public double CurrentAverage => _throws.Count > 0 ? _throws.Average(t => t.ScoreValue) : 0.0;
        public bool IsEmpty => _throws.Count == 0;
        
        public int Sixty => _throws.Count(t => t.ScoreValue >= 60 && t.ScoreValue < 100);
        public int Hundred => _throws.Count(t => t.ScoreValue >= 100 && t.ScoreValue < 120);
        public int Hundred20 => _throws.Count(t => t.ScoreValue >= 120 && t.ScoreValue < 180);
        public int Hundred80 => _throws.Count(t => t.ScoreValue == 180);
        
        public int Wins => _throws.Count(t => t.IsCheckout);
        public int HighestOut => _throws.Where(t => t.IsCheckout).Select(t => t.ScoreValue).DefaultIfEmpty(0).Max();

        public void AddThrow(string scoreText, int scoreValue, bool isCheckout = false)
        {
            _throws.Add(new ThrowRecord(scoreText, scoreValue, isCheckout));
        }

        public string? UndoLastThrow()
        {
            if (_throws.Count == 0) return null;

            var last = _throws.Last();
            _throws.RemoveAt(_throws.Count - 1);
            return last.ScoreText;
        }

        public void Clear()
        {
            _throws.Clear();
        }
    }
}