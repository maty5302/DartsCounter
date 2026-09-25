using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain;
using Domain.Interfaces;
using Domain.Models;

namespace DesktopUI.ViewModels
{
    public partial class DuelViewModel : ViewModelBase
    {
        private readonly IDartsRepository _repo;
        // The two players bound to the UI
        [ObservableProperty]
        private PlayerViewModel _player1;

        [ObservableProperty]
        private PlayerViewModel _player2;

        // Track how many legs and sets each player has won
        [ObservableProperty]
        private int _player1Legs;

        [ObservableProperty]
        private int _player2Legs;

        [ObservableProperty] 
        private int _player1Sets;
        
        [ObservableProperty] 
        private int _player2Sets;
        
        // Track how many legs and sets needs to be won
        [ObservableProperty]
        private int _numberOfLegs;
        
        [ObservableProperty]
        private int _numberOfSets;

        [ObservableProperty]
        private int _numToBeat;

        [ObservableProperty]
        private int _scoreToBeat;

        [ObservableProperty]
        private bool _showWinner;
        
        [ObservableProperty]
        private bool _isSetsMode;

        private bool _soundEffects;
        
        public Action<int>? OnDuelFinished { get; set; }
        
        //sets winner name to View
        public string WinnerName
        {
            get => field;
            set => SetProperty(ref field, value);
        }
        
        public bool Is2V2Mode { get; set; }
        public int Team1Player2Id { get; set; }
        public int Team2Player2Id { get; set; }

        public DuelViewModel(IDartsRepository repo)
        {
            _repo = repo;
            // Reset domain logic if needed for a fresh duel
            WinnerName = "";
            // Initialize Players
            Player1 = new PlayerViewModel(_repo)
            {
                PlayerId = 0,
                Name = "Player 1", // You can pass actual names from a setup window later
                Score = _scoreToBeat,       // Standard starting score for the duel
                Average = 0.00,
                IsActive = true,   // Player 1 starts
                OnThrowSubmitted = SwitchTurn,
                OnInputFocused = SetActivePlayer
            };

            Player2 = new PlayerViewModel(_repo)
            {
                PlayerId = 1,
                Name = "Player 2",
                Score = _scoreToBeat,
                Average = 0.00,
                IsActive = false,
                OnThrowSubmitted = SwitchTurn,
                OnInputFocused = SetActivePlayer
            };

            Player1Legs = 0;
            Player2Legs = 0;

            Player1Sets = 0;
            Player2Sets = 0;
        }
        
        /// <summary>
        /// Initializes duel
        /// </summary>
        /// <param name="t1Name">Team one or Player one name (upon if you play 2V2 or not)</param>
        /// <param name="t2Name">Team two or Player two name (upon if you play 2V2 or not)</param>
        /// <param name="startingScore">Score to beat and starting score</param>
        /// <param name="targetLegs">Sets target legs or sets beat to win</param>
        /// <param name="isSets">Determines if you play sets</param>
        /// <param name="soundEffects">Sets if sound effect are played</param>
        public void InitializeDuel(string t1Name, string t2Name, int startingScore, int targetLegs, bool isSets, bool soundEffects)
        {
            ScoreToBeat = startingScore;
            IsSetsMode = isSets;
            _soundEffects = soundEffects;
            WinnerName = "";
            ShowWinner = false;
            NumToBeat = targetLegs;
            
            if (IsSetsMode)
            {
                NumberOfLegs = 3;
                NumberOfSets = targetLegs;
                
            }
            else
                NumberOfLegs = targetLegs;
            
            Player1.Name = string.IsNullOrWhiteSpace(t1Name) ? "Tým 1" : t1Name;
            Player2.Name = string.IsNullOrWhiteSpace(t2Name) ? "Tým 2" : t2Name;

            Player1Legs = 0;
            Player2Legs = 0;
            Player1Sets = 0;
            Player2Sets = 0;

            foreach (var player in new List<PlayerViewModel>(){Player1, Player2})
            {
                player.SoundEffectsEnabled = soundEffects;
                player.IsEnabled = true;
                player.CurrentThrow = "";
                player.Score = startingScore;
                player.ResetPlacements();
                player.IsInDuel = true;
                player.MatchStats.Clear();
            }
            Player1.IsActive = true;
            Player2.IsActive = false;
            
        }

        /// <summary>
        /// Automatically called by PlayerViewModel when a throw is submitted (Enter is pressed)
        /// </summary>
        private void SwitchTurn(PlayerViewModel throwingPlayer)
        {
            // If Player 1 just threw, switch to Player 2
            if (throwingPlayer == Player1)
            {
                Player1.IsActive = false;
                
                // Only switch if the other player hasn't already finished the game
                if (!Player2.HasFinished)
                {
                    Player2.IsActive = true;
                }
            }
            // If Player 2 just threw, switch to Player 1
            else if (throwingPlayer == Player2)
            {
                Player2.IsActive = false;
                
                if (!Player1.HasFinished)
                {
                    Player1.IsActive = true;
                }
            }
            
            if(Player1.HasFinished || Player2.HasFinished)
            {
                _ = NextLeg();
            }
        }

        /// <summary>
        /// Called when the user manually clicks into one of the text boxes
        /// </summary>
        private void SetActivePlayer(PlayerViewModel focusedPlayer)
        {
            if (focusedPlayer.HasFinished) return;

            if (focusedPlayer == Player1)
            {
                Player1.IsActive = true;
                Player2.IsActive = false;
            }
            else if (focusedPlayer == Player2)
            {
                Player2.IsActive = true;
                Player1.IsActive = false;
            }
        }
        
        /// <summary>
        ///  Called when players finish leg
        /// </summary>
        [RelayCommand]
        private async Task NextLeg()
        {
            if (Player1.HasFinished)
            {
                Player1Legs++;
            }
            else if (Player2.HasFinished)
            {
                Player2Legs++;
            }
            
            if (IsSetsMode)
            {
                if (Player1Legs == NumberOfLegs)
                {
                    Player1Sets++;
                    Player1Legs = 0; 
                    Player2Legs = 0;
                }
                else if (Player2Legs == NumberOfLegs)
                {
                    Player2Sets++;
                    Player1Legs = 0;
                    Player2Legs = 0;
                }

                if (Player1Sets == NumberOfSets || Player2Sets == NumberOfSets)
                {
                    ShowWinner = true;
                    bool team1Won = Player1Legs == NumberOfLegs;
                    WinnerName = team1Won ? Player1.Name : Player2.Name;
        
                    Player1.IsEnabled = false;
                    Player2.IsEnabled = false;
                    if(_soundEffects)
                        _ = SoundManagerDarts.SoundEffects.PlayWinnerSong();
                    
                    await ProcessDuelEndAsync(team1Won);
                    
                    return; 
                }
            }
            else 
            {
                if (Player1Legs == NumberOfLegs || Player2Legs == NumberOfLegs)
                {
                    ShowWinner = true;
                    bool team1Won = Player1Legs == NumberOfLegs;
                    WinnerName = team1Won ? Player1.Name : Player2.Name;
        
                    Player1.IsEnabled = false;
                    Player2.IsEnabled = false;
                    if(_soundEffects)
                        _ = SoundManagerDarts.SoundEffects.PlayWinnerSong();
                    
                    await ProcessDuelEndAsync(team1Won);
                    return; 
                }
            }
            
            
            //Cuz player card has placement, we need to reset it for the next leg
            Player1.ResetPlacements();
            Player2.ResetPlacements();
                
            Player1.Score = ScoreToBeat; 
            Player1.CurrentThrow = "";
            Player1.IsEnabled = true; 
        
            Player2.Score = ScoreToBeat;
            Player2.CurrentThrow = "";
            Player2.IsEnabled = true;

            // Determine who starts the leg
            if (IsSetsMode)
            {
                int totalSetsPlayed = Player1Sets + Player2Sets;
                bool player1StartsSet = (totalSetsPlayed % 2 == 0);

                int totalLegsInCurrentSet = Player1Legs + Player2Legs;
                bool isEvenLegInSet = (totalLegsInCurrentSet % 2 == 0);
                
                Player1.IsActive = isEvenLegInSet ? player1StartsSet : !player1StartsSet;
                Player2.IsActive = !Player1.IsActive;
            }
            else
            {
                int totalLegsPlayed = Player1Legs + Player2Legs;
    
                Player1.IsActive = (totalLegsPlayed % 2 == 0);
                Player2.IsActive = !Player1.IsActive;
            }
        }
        
        /// <summary>
        ///
        /// </summary>
        private async Task ProcessDuelEndAsync(bool isTeam1Winner)
        {
            var team1Ids = GetTeamIds(Player1.PlayerId, Team1Player2Id);
            var team2Ids = GetTeamIds(Player2.PlayerId, Team2Player2Id);

            
            await UpdateTeamStatisticsAsync(Player1, team1Ids, isWinner: isTeam1Winner);
            await UpdateTeamStatisticsAsync(Player2, team2Ids, isWinner: !isTeam1Winner);
            
            int winnerId = isTeam1Winner ? Player1.PlayerId : Player2.PlayerId;
            OnDuelFinished?.Invoke(winnerId);
        }

        /// <summary>
        /// 
        /// </summary>
        private List<long> GetTeamIds(long mainPlayerId, long secondPlayerId)
        {
            var ids = new List<long>();
            
            if (mainPlayerId > 0) 
                ids.Add(mainPlayerId);
                
            if (Is2V2Mode && secondPlayerId > 0) 
                ids.Add(secondPlayerId);
                
            return ids;
        }

        /// <summary>
        /// 
        /// </summary>
        private async Task UpdateTeamStatisticsAsync(PlayerViewModel teamVm, List<long> playerIds, bool isWinner)
        {
            foreach (var playerId in playerIds)
            {
                await UpdateSinglePlayerStatsAsync(playerId, teamVm.MatchStats, isWinner);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private async Task UpdateSinglePlayerStatsAsync(long playerId, PlayerMatchStatistics matchStats, bool isWinner)
        {
            int currentYear = DateTime.Now.Year;
            var stats = await _repo.GetStatsForYearAsync(playerId, currentYear);
            
            if (stats == null)
            {
                stats = new PlayerStatsDto
                {
                    PlayerId = playerId,
                    Year = currentYear
                };
            }

            if (isWinner)
            {
                stats.Wins++;
                // stats.AllWins++; 
            }

            if (matchStats != null && !matchStats.IsEmpty)
            {
                stats.Sixty += matchStats.Sixty;
                stats.Hundred += matchStats.Hundred;
                stats.Hundred20 += matchStats.Hundred20;
                stats.Hundred80 += matchStats.Hundred80;

                if (matchStats.HighestOut > stats.HighestOut)
                {
                    stats.HighestOut = matchStats.HighestOut;
                }

                if (stats.Average == 0)
                {
                    stats.Average = matchStats.CurrentAverage;
                }
                else
                {
                    stats.Average = (stats.Average + matchStats.CurrentAverage) / 2.0;
                }
            }

            await _repo.UpdateStatsAsync(stats);
        }
    }
}