using System;
using System.Collections.Generic;
using Domain.Models;
using Xunit;

namespace Domain.Tests
{
    public class TournamentTests
    {
        private List<PlayerDto> CreatePlayers(int count)
        {
            var list = new List<PlayerDto>();
            for (int i = 1; i <= count; i++)
            {
                list.Add(new PlayerDto { Id = i, PlayerName = $"Player {i}" });
            }
            return list;
        }

        [Fact]
        public void Constructor_LessThanTwoPlayers_ThrowsArgumentException()
        {
            var onePlayer = CreatePlayers(1);
            Assert.Throws<ArgumentException>(() => new Tournament(onePlayer));
            Assert.Throws<ArgumentException>(() => new Tournament(null!));
        }

        [Fact]
        public void Constructor_FourPlayers_InitializesRoundOneCorrectly()
        {
            var players = CreatePlayers(4);
            var tournament = new Tournament(players);

            Assert.Equal(1, tournament.Round);
            Assert.Equal(2, tournament.Matches.Count);
            Assert.Single(tournament.AllMatches);
            Assert.False(tournament.IsFinished);
            Assert.Null(tournament.Winner);

            // First match: seed 1 vs seed 4 (id 1 vs id 4)
            Assert.Equal(1, tournament.Matches[0].Player1Id);
            Assert.Equal(4, tournament.Matches[0].Player2Id);

            // Second match: seed 2 vs seed 3 (id 2 vs id 3)
            Assert.Equal(2, tournament.Matches[1].Player1Id);
            Assert.Equal(3, tournament.Matches[1].Player2Id);
        }

        [Fact]
        public void GetNextMatch_ReturnsFirstMatchWithoutWinner()
        {
            var tournament = new Tournament(CreatePlayers(4));

            var next = tournament.GetNextMatch();
            Assert.NotNull(next);
            Assert.Equal(1, next.Player1Id);

            // Mark first match won
            next.WinnerId = 1;

            var second = tournament.GetNextMatch();
            Assert.NotNull(second);
            Assert.Equal(2, second.Player1Id);

            // Mark second match won
            second.WinnerId = 2;

            Assert.Null(tournament.GetNextMatch());
        }

        [Fact]
        public void GenerateNextRound_WhenRoundHasUnplayedMatches_ReturnsFalseAndPreservesMatches()
        {
            var tournament = new Tournament(CreatePlayers(4));
            // Only 1 of 2 matches played
            tournament.Matches[0].WinnerId = 1;

            bool result = tournament.GenerateNextRound();

            Assert.False(result);
            Assert.Equal(1, tournament.Round);
            Assert.Equal(2, tournament.Matches.Count);
            Assert.Single(tournament.AllMatches);
        }

        [Fact]
        public void GenerateNextRound_WhenRoundComplete_AdvancesRoundAndPairsWinners()
        {
            var tournament = new Tournament(CreatePlayers(4));
            tournament.Matches[0].WinnerId = 1;
            tournament.Matches[1].WinnerId = 2;

            bool result = tournament.GenerateNextRound();

            Assert.True(result);
            Assert.Equal(2, tournament.Round);
            Assert.Single(tournament.Matches);
            Assert.Equal(2, tournament.AllMatches.Count);

            // Final match: winner 1 vs winner 2
            Assert.Equal(1, tournament.Matches[0].Player1Id);
            Assert.Equal(2, tournament.Matches[0].Player2Id);
            Assert.False(tournament.IsFinished);
        }

        [Fact]
        public void GenerateNextRound_WhenTournamentFinished_ReturnsFalseAndDoesNotAddEmptyMatches()
        {
            var tournament = new Tournament(CreatePlayers(4));
            // Semi-finals
            tournament.Matches[0].WinnerId = 1;
            tournament.Matches[1].WinnerId = 2;
            tournament.GenerateNextRound();

            // Final
            tournament.Matches[0].WinnerId = 1;

            Assert.True(tournament.IsFinished);
            Assert.NotNull(tournament.Winner);
            Assert.Equal("Player 1", tournament.Winner.PlayerName);

            // Attempting to generate next round when already finished
            bool result = tournament.GenerateNextRound();

            Assert.False(result);
            Assert.Equal(2, tournament.Round);
            Assert.Equal(2, tournament.AllMatches.Count); // Should NOT have added a 3rd empty round
            Assert.Single(tournament.Matches); // Final match remains
        }
    }
}
