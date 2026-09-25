using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopUI.Services;
using Domain.Interfaces;
using Domain.Models;
using Avalonia.Threading;

namespace DesktopUI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty] private int _playerCount = 10;
    [ObservableProperty] private int _score = 501;
    [ObservableProperty] private DuelViewModel _duelVM;
    [ObservableProperty] private bool _isDuelMode;
    [ObservableProperty] private bool _isTrainingMode;
    [ObservableProperty] private TrainingViewModel _trainingVM;
    [ObservableProperty] private Domain.Tournament? _activeTournament;
    [ObservableProperty] private bool _isTournamentMode;
    [ObservableProperty] private TimeSpan _elapsedGameTime;
    [ObservableProperty] private bool _isTimerVisible = false;
    [ObservableProperty] private bool _isTimerRunning;
    [ObservableProperty] private bool _isGameActive = false;
    
    public string FormattedTime => ElapsedGameTime.ToString(@"hh\:mm\:ss");

    private DispatcherTimer _gameTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
    
    public SettingsManager Settings { get; }
    
    private readonly IDartsRepository _repo;
    
    public IDartsRepository Repo => _repo;

    private int _currentPlayerIndex;
    public ObservableCollection<PlayerViewModel> Players { get; } = new();

    public MainViewModel(IDartsRepository repo)
    {
        _repo = repo;
        _trainingVM = new TrainingViewModel();
        Settings = new SettingsManager();
        _duelVM = new DuelViewModel(_repo);
        _ = Settings.CheckForUpdatesAsync();
        ApplyTheme(Settings.ThemePreference);

        SoundManagerDarts.SoundEffects.IsMusicPlaying = Settings.PlayMusicOnStartup;
        
        Settings.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(SettingsManager.ThemePreference))
                ApplyTheme(Settings.ThemePreference);
        };
        
        _gameTimer.Tick += (s, e) => 
        {
            ElapsedGameTime = ElapsedGameTime.Add(TimeSpan.FromSeconds(1));
            OnPropertyChanged(nameof(FormattedTime));
        };
    }

    private void ApplyTheme(string theme)
    {
        if (Application.Current != null)
        {
            Application.Current.RequestedThemeVariant = theme switch
            {
                "Light" => ThemeVariant.Light,
                "Dark" => ThemeVariant.Dark,
                _ => ThemeVariant.Default 
            };
        }
    }
    
    [RelayCommand]
    private void StartGame()
    {
        IsDuelMode = false;
        IsTrainingMode = false;
        Players.Clear();  
        PlayerViewModel.ResetGlobalPlacement();
        for (int i = 0; i < PlayerCount; i++)
        {
            Players.Add(new PlayerViewModel(_repo)
            {
                PlayerId = i,
                Name = GetPlayerName(i),              
                CardBackground = GetPlayerColor(i),   
                OpacityEnabled = Settings.OpacityEnabled,   
                OpacityValue = Settings.OpacityValue,       
                SoundEffectsEnabled = Settings.SoundEffectsEnabled, 
                Score = Score,
                Average = 0.00,
                OnThrowSubmitted = SwitchToNextPlayer,
                OnInputFocused = SetActivePlayer
            });
            Players[i].ResetPlacement();
        }
        
        _currentPlayerIndex = 0;
        if (Players.Count > 0)
        {
            Players[_currentPlayerIndex].IsActive = true;
        }
        
        if (Settings.SoundEffectsEnabled)
            _ = SoundManagerDarts.SoundEffects.PlayGameOn();
        
        if (IsTimerVisible)
            StartNewGameTimer();
    }
    
    private string GetPlayerName(int playerId)
    {
        if (playerId >= 0 && playerId < Settings.PlayerNames.Count && !string.IsNullOrWhiteSpace(Settings.PlayerNames[playerId]))
        {
            return Settings.PlayerNames[playerId];
        }
        return $"Hráč {playerId + 1}";
    }

    private string GetPlayerColor(int playerId)
    {
        if (playerId >= 0 && playerId < Settings.PlayerColors.Count && !string.IsNullOrWhiteSpace(Settings.PlayerColors[playerId]))
        {
            return Settings.PlayerColors[playerId];
        }
        return "#228B22";
    }

    private void SwitchToNextPlayer(PlayerViewModel throwingPlayer)
    {
        if (Players.Count == 0) return;

        int currentIndex = Players.IndexOf(throwingPlayer);
        if (currentIndex == -1) return;

        _currentPlayerIndex = currentIndex;
        throwingPlayer.IsActive = false;
        
        for (int step = 1; step <= Players.Count; step++)
        {
            int nextIndex = (currentIndex + step) % Players.Count;
            if (!Players[nextIndex].HasFinished)
            {
                SetActivePlayer(Players[nextIndex]);
                return;
            }
        }
        
        StopGameTimer();
    }

    private void SetActivePlayer(PlayerViewModel player)
    {
        int selectedIndex = Players.IndexOf(player);
        if (selectedIndex == -1 || player.HasFinished) return;

        if (_currentPlayerIndex == selectedIndex && Players[selectedIndex].IsActive) return;

        for (int i = 0; i < Players.Count; i++)
        {
            Players[i].IsActive = i == selectedIndex;
        }

        _currentPlayerIndex = selectedIndex;
    }

    public void RefreshActivePlayers()
    {
        foreach (var player in Players)
        {
            player.Name = GetPlayerName(player.PlayerId);
            player.CardBackground = GetPlayerColor(player.PlayerId);
        
            player.OpacityEnabled = Settings.OpacityEnabled;
            player.OpacityValue = Settings.OpacityValue;
            player.SoundEffectsEnabled = Settings.SoundEffectsEnabled;
        }
    }
    
    [RelayCommand]
    private void ToggleTimer()
    {
        if (!IsGameActive) return;

        if (IsTimerRunning)
            _gameTimer.Stop();
        else
            _gameTimer.Start();
            
        IsTimerRunning = !IsTimerRunning;
    }

    [RelayCommand]
    private void ToggleTimerVisibility() => IsTimerVisible = !IsTimerVisible;

    private void StartNewGameTimer()
    {
        ElapsedGameTime = TimeSpan.Zero;
        IsGameActive = true;     
        IsTimerVisible = true;   
        
        _gameTimer.Start();
        IsTimerRunning = true;
        OnPropertyChanged(nameof(FormattedTime));
    }
    
    public void StopGameTimer()
    {
        _gameTimer.Stop();
        IsTimerRunning = false;
        IsGameActive = false; 
    }

    [RelayCommand]
    private void RedoLast()
    {
        if (IsDuelMode)
        {
            if (DuelVM.Player1.IsActive)
            {
                DuelVM.Player1.UndoThrow();
            }
            else if (DuelVM.Player2.IsActive)
            {
                DuelVM.Player2.UndoThrow();
            }
        }
        else
        {
            if (_currentPlayerIndex >= 0 && _currentPlayerIndex < Players.Count)
            {
                var currentPlayer = Players[_currentPlayerIndex];
            
                currentPlayer.UndoThrow();
            }
        }
    }
    
    public void StartDuel(DuelSetupViewModel config)
    {
        string team1Name;
        string team2Name;
        IsTrainingMode = false; 

        DuelVM.Is2V2Mode = config.Is2v2;

        if (config.Is2v2)
        {
            team1Name = $"{config.Player1?.PlayerName} & {config.Player2?.PlayerName}";
            team2Name = $"{config.Player3?.PlayerName} & {config.Player4?.PlayerName}";

            // Tým 1 IDs
            DuelVM.Player1.PlayerId = (int)(config.Player1?.Id ?? 0);
            DuelVM.Team1Player2Id = (int)(config.Player2?.Id ?? 0);
        
            // Tým 2 IDs
            DuelVM.Player2.PlayerId = (int)(config.Player3?.Id ?? 0);
            DuelVM.Team2Player2Id = (int)(config.Player4?.Id ?? 0);
        }
        else
        {
            team1Name = config.Player1?.PlayerName ?? "Hráč 1";
            team2Name = config.Player2?.PlayerName ?? "Hráč 2"; 

            DuelVM.Player1.PlayerId = (int)(config.Player1?.Id ?? 0);
            DuelVM.Player2.PlayerId = (int)(config.Player2?.Id ?? 0);
        }
        DuelVM.InitializeDuel(team1Name, team2Name, config.Score, config.Legs, config.IsSets, Settings.SoundEffectsEnabled);

        DuelVM.OnDuelFinished = (winnerId) =>
        {
            StopGameTimer();
        };
        IsDuelMode = true;
    
        if (Settings.SoundEffectsEnabled)
        {
            _ = SoundManagerDarts.SoundEffects.PlayGameOn();
        }
        if (IsTimerVisible)
        {
            StartNewGameTimer();
        }
    }
    
    public async Task<List<PlayerDto>> GetDatabasePlayersAsync()
    {
        return await _repo.GetAllPlayersAsync();
    }

    [RelayCommand]
    private void OpenTraining()
    {
        IsDuelMode = false;
        Players.Clear();

        IsTrainingMode = true;

        if (TrainingVM.ResetTrainingCommand.CanExecute(null))
        {
            TrainingVM.ResetTrainingCommand.Execute(null);
        }
    }
    
   public void StartTournament(List<PlayerDto> selectedPlayers)
    {
        ActiveTournament = new Domain.Tournament(selectedPlayers);
        IsTournamentMode = true;
        
        DuelVM.OnDuelFinished = (winnerId) =>
        {
            if (IsTournamentMode && ActiveTournament != null)
            {
                var match = ActiveTournament.GetNextMatch();
                if (match != null)
                {
                    match.WinnerId = winnerId;
                    if (Settings.SoundEffectsEnabled)
                        SoundManagerDarts.SoundEffects.PlayWinnerSong();
                    PlayNextTournamentMatch();
                }
            }
        };
        PlayNextTournamentMatch();
        
        if (IsTimerVisible)
        {
            StartNewGameTimer();
        }
    }

    private void PlayNextTournamentMatch()
    {
        var nextMatch = ActiveTournament?.GetNextMatch();
        
        if (nextMatch == null)
        {
            ActiveTournament?.GenerateNextRound();
            nextMatch = ActiveTournament?.GetNextMatch();
        }

        if (nextMatch != null)
        {
            var p1 = ActiveTournament!.Players.FirstOrDefault(p => p.Id == nextMatch.Player1Id);
            var p2 = ActiveTournament!.Players.FirstOrDefault(p => p.Id == nextMatch.Player2Id);

            DuelVM.Is2V2Mode = false;
            DuelVM.Player1.PlayerId = nextMatch.Player1Id;
            DuelVM.Player2.PlayerId = nextMatch.Player2Id;
            
            DuelVM.InitializeDuel(p1?.PlayerName ?? "Hráč 1", p2?.PlayerName ?? "Hráč 2", Score, 3, false, Settings.SoundEffectsEnabled);
            
            IsDuelMode = true;
            IsTrainingMode = false;
        }
        else
        {
            IsTournamentMode = false;
            StopGameTimer();
        }
    }
}