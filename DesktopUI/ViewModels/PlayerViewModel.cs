using System;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain;
using Domain.Interfaces;
using Domain.Models;

namespace DesktopUI.ViewModels
{
    public partial class PlayerViewModel : ViewModelBase
    {
        private readonly IDartsRepository _repository;

        public PlayerViewModel(IDartsRepository repository)
        {
            _repository = repository;
        }
        [ObservableProperty]
        private bool _isActive;
        private int _playerId;
        private static int _nextPlacement = 1;
        private int? _placement;
        private bool isEnabled = true;
        [ObservableProperty]
        private bool _soundEffectsEnabled;

        public PlayerMatchStatistics MatchStats { get; } = new();
        public bool IsInDuel { get; set; }
        
        private double _opacityValue = 0.7;
        
        public Action<PlayerViewModel>? OnThrowSubmitted { get; set; }
        public Action<PlayerViewModel>? OnInputFocused { get; set; }
        
        public int PlayerId
        {
            get => _playerId;
            set => SetProperty(ref _playerId, value);
        }
        
        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value); 
        }

        private int _score;
        public int Score
        {
            get => _score;
            set 
            {
                if (SetProperty(ref _score, value))
                {
                    OnPropertyChanged(nameof(ScoreText)); 
                    OnPropertyChanged(nameof(DisplayText));
                    OnPropertyChanged(nameof(DisplayFontSize));
                }
            }
        }
        
        public string ScoreText => $"{Score}";
        
        public string DisplayText => _placement.HasValue ? $"{_placement.Value}. místo" : ScoreText;
        public double DisplayFontSize => _placement.HasValue ? 26 : 42;
        public bool HasFinished => _placement.HasValue;

        private double _average;
        public double Average
        {
            get => _average;
            set
            {
                if (SetProperty(ref _average, value))
                {
                    OnPropertyChanged(nameof(AverageText)); 
                }
            }
        }
        
        public string AverageText => $"Ø {Average:F2}";

        private string _cardBackground = "#228B22";
        public string CardBackground
        {
            get => _cardBackground;
            set => SetProperty(ref _cardBackground, value);
        }
        
        private string _currentThrow = "";

        public string CurrentThrow
        {
            get => _currentThrow;
            set => SetProperty(ref _currentThrow, value);
        }

        [RelayCommand]
        public async Task SubmitThrow()
        {
            if (_placement.HasValue)
            {
                CurrentThrow = "";
                return;
            }

            if (!int.TryParse(CurrentThrow, out int value))
            {
                CurrentThrow = "";
                return; 
            }

            if (value < 0 || value > 180)
            {
                CurrentThrow = "";
                return; 
            }

            var res = Score - value;

           
            if (res > 0) 
            {
                MatchStats.AddThrow(CurrentThrow, value);
                Score -= value;
                Average = MatchStats.CurrentAverage; 
                if (SoundEffectsEnabled)
                    _ = SoundManagerDarts.SoundEffects.PlayScoreAsync(value);
                Checkout = Domain.Checkout.checkout(Score); 
            }
            else if (res == 0)
            {
                MatchStats.AddThrow(CurrentThrow, value, isCheckout: true);
                Score = 0;
                _placement = _nextPlacement++;
                IsEnabled = false;
                OnPropertyChanged(nameof(DisplayText));
                OnPropertyChanged(nameof(DisplayFontSize));
                OnPropertyChanged(nameof(IsEnabled));
                Average = MatchStats.CurrentAverage; 
                if(SoundEffectsEnabled && !IsInDuel)
                    _ = SoundManagerDarts.SoundEffects.PlayWinnerSong();
                if (!IsInDuel)
                {
                    var allDbPlayers = await _repository.GetAllPlayersAsync();
                    
                    var matchedDbPlayer = allDbPlayers.FirstOrDefault(p => p.PlayerName == this.Name);

                    if (matchedDbPlayer != null)
                    {
                        long realDbId = matchedDbPlayer.Id;
                        int currentYear = DateTime.Now.Year;

                        var existingStats = await _repository.GetStatsForYearAsync(realDbId, currentYear);
                        
                        if (existingStats == null)
                        {
                            existingStats = new PlayerStatsDto 
                            {
                                PlayerId = realDbId,
                                Year = currentYear,
                                Wins = 1,
                                MatchesPlayed = 1,
                                Average = Math.Round(MatchStats.CurrentAverage, 2),
                                HighestOut = MatchStats.HighestOut,
                                Sixty = MatchStats.Sixty,
                                Hundred = MatchStats.Hundred,
                                Hundred20 = MatchStats.Hundred20,
                                Hundred80 = MatchStats.Hundred80
                            };
                        }
                        else
                        {
                            existingStats.Wins += 1;
                            
                            if (MatchStats.HighestOut > existingStats.HighestOut)
                            {
                                existingStats.HighestOut = MatchStats.HighestOut;
                            }

                            existingStats.Sixty += MatchStats.Sixty;
                            existingStats.Hundred += MatchStats.Hundred;
                            existingStats.Hundred20 += MatchStats.Hundred20;
                            existingStats.Hundred80 += MatchStats.Hundred80;
                            
                            int previousMatches = existingStats.MatchesPlayed > 0
                                ? existingStats.MatchesPlayed
                                : (existingStats.Average > 0 ? Math.Max(1, existingStats.Wins) : 0);

                            if (previousMatches == 0)
                            {
                                existingStats.Average = Math.Round(MatchStats.CurrentAverage, 2);
                            }
                            else
                            {
                                existingStats.Average = Math.Round(((existingStats.Average * previousMatches) + MatchStats.CurrentAverage) / (previousMatches + 1), 2);
                            }

                            existingStats.MatchesPlayed = previousMatches + 1;
                        }
                        
                        await _repository.UpdateStatsAsync(existingStats);
                    }
                }
                Checkout = Domain.Checkout.checkout(Score);
            }
            
            CurrentThrow = "";
            OnThrowSubmitted?.Invoke(this);
        }

        private string _checkout = "";

        public string Checkout
        {
            get => _checkout;
            set => SetProperty(ref _checkout, value);
        }

        public void ResetPlacements()
        {
            _placement = null;
            _nextPlacement = 1;
        }
        
        public bool IsEnabled
        {
            get => isEnabled;
            set => SetProperty(ref isEnabled, value);
        }

        public void NotifyInputFocused()
        {
            if (!HasFinished)
            {
                OnInputFocused?.Invoke(this);
            }
        }
        
        private bool _opacityEnabled;

        public bool OpacityEnabled
        {
            get => _opacityEnabled;
            set
            {
                if (SetProperty(ref _opacityEnabled, value))
                {
                    OnPropertyChanged(nameof(OpacityPlayerCard));
                }
            }
        }
        
        public double OpacityValue
        {
            get => _opacityValue;
            set
            {
                if (SetProperty(ref _opacityValue, value))
                {
                    OnPropertyChanged(nameof(OpacityPlayerCard));
                }
            }
        }

        public double OpacityPlayerCard => OpacityEnabled ? OpacityValue : 1.0;
        
        [RelayCommand]
        public void UndoThrow()
        {
            if (MatchStats.IsEmpty) return;

            var lastValue = MatchStats.UndoLastThrow();
            
            if (!String.IsNullOrEmpty(lastValue))
            {
                Score += Convert.ToInt32(lastValue);
                
                Average = MatchStats.CurrentAverage;
                Checkout = Domain.Checkout.checkout(Score);
                
                if (HasFinished)
                {
                    _placement = null;
                    _nextPlacement--; 
                    IsEnabled = true;
                    OnPropertyChanged(nameof(DisplayText));
                    OnPropertyChanged(nameof(DisplayFontSize));
                    OnPropertyChanged(nameof(IsEnabled));
                }
                
            }
        }

        public void ResetPlacement()
        {
            _placement = null;
        }
        
        public static void ResetGlobalPlacement()
        {
            _nextPlacement = 1;
        }
    }
}