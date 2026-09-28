using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Domain.Models;

namespace DesktopUI.ViewModels.Tournament
{
    public class TournamentBracketViewModel : ViewModelBase
    {
        public ObservableCollection<RoundViewModel> Rounds { get; set; } = new();

        public void LoadTournament(Domain.Tournament tournament)
        {
            Rounds.Clear();
            int totalPlayers = tournament.Players.Count;
            int totalRounds = (int)Math.Log2(totalPlayers);

            var validMatches = tournament.AllMatches.Where(m => m != null && m.Count > 0).ToList();

            for (int roundIndex = 0; roundIndex < totalRounds; roundIndex++)
            {
                var roundVm = new RoundViewModel();
                int expectedMatches = totalPlayers / (int)Math.Pow(2, roundIndex + 1);

                List<Match> realMatches = new();
                if (roundIndex < validMatches.Count)
                {
                    realMatches = validMatches[roundIndex];
                }

                for (int matchIndex = 0; matchIndex < expectedMatches; matchIndex++)
                {
                    bool isTop = expectedMatches > 1 && (matchIndex % 2 == 0);
                    bool isBottom = expectedMatches > 1 && (matchIndex % 2 != 0);
                    bool isFirst = (roundIndex == 0);

                    if (matchIndex < realMatches.Count)
                    {
                        var match = realMatches[matchIndex];
                        roundVm.Matches.Add(new MatchViewModel
                        {
                            Player1Name = GetPlayerName(tournament, match.Player1Id),
                            Player2Name = GetPlayerName(tournament, match.Player2Id),
                            IsPlayer1Winner = match.WinnerId == match.Player1Id && match.WinnerId != 0,
                            IsPlayer2Winner = match.WinnerId == match.Player2Id && match.WinnerId != 0,
                            HasNextRound = true,
                            
                            IsTopMatch = isTop,
                            IsBottomMatch = isBottom,
                            IsFirstRound = isFirst
                        });
                    }
                    else
                    {
                        roundVm.Matches.Add(new MatchViewModel
                        {
                            Player1Name = "?",
                            Player2Name = "?",
                            HasNextRound = true,
                            
                            IsTopMatch = isTop,
                            IsBottomMatch = isBottom,
                            IsFirstRound = isFirst
                        });
                    }
                }
                Rounds.Add(roundVm);
            }

            var winnerRound = new RoundViewModel();
            
            var finalMatch = validMatches.LastOrDefault()?.FirstOrDefault();
            
            bool isFinished = validMatches.Count == totalRounds && finalMatch?.WinnerId != 0;

            winnerRound.Matches.Add(new MatchViewModel
            {
                Player1Name = isFinished ? GetPlayerName(tournament, finalMatch!.WinnerId) : "",
                IsPlayer1Winner = isFinished,
                IsFinalWinnerBox = true,
                HasNextRound = false,
                
                IsTopMatch = false,
                IsBottomMatch = false,
                IsFirstRound = false 
            });
            
            Rounds.Add(winnerRound);
        }

        private string GetPlayerName(Domain.Tournament tournament, int playerId)
        {
            if (playerId == 0) return "?"; 
            var player = tournament.Players.FirstOrDefault(p => p.Id == playerId);
            return player?.PlayerName ?? $"Hráč {playerId}";
        }
    }
}