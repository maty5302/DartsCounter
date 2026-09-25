namespace Domain
{
    public class PlayerMatchStatistics
    {
        private readonly List<string> _history = new();
        private readonly List<int> _throwValues = new();
        private readonly List<int> _checkouts = new();

        public double CurrentAverage => _throwValues.Count > 0 ? _throwValues.Average() : 0.0;
        public bool IsEmpty => _history.Count == 0;
        
        public int Sixty => _throwValues.Count(v => v >= 60 && v < 100);
        public int Hundred => _throwValues.Count(v => v >= 100 && v < 120);
        public int Hundred20 => _throwValues.Count(v => v >= 120 && v < 180);
        public int Hundred80 => _throwValues.Count(v => v == 180);
        
        public int Wins => _checkouts.Count;
        public int HighestOut => _checkouts.Count > 0 ? _checkouts.Max() : 0;

        public void AddThrow(string scoreText, int scoreValue, bool isCheckout = false)
        {
            _history.Add(scoreText);
            _throwValues.Add(scoreValue);
            if (isCheckout)
            {
                _checkouts.Add(scoreValue);
            }
        }

        public string? UndoLastThrow()
        {
            if (_history.Count == 0) return null;

            int lastValue = _throwValues.Last();
            string lastScore = _history.Last();
            _history.RemoveAt(_history.Count - 1);

            if (_throwValues.Count > 0)
            {
                _throwValues.RemoveAt(_throwValues.Count - 1);
            }
            
            if (_checkouts.Count > 0 && _checkouts.Last() == lastValue)
            {
                _checkouts.RemoveAt(_checkouts.Count - 1);
            }

            return lastScore;
        }

        public void Clear()
        {
            _history.Clear();
            _throwValues.Clear();
            _checkouts.Clear();
        }
    }
}