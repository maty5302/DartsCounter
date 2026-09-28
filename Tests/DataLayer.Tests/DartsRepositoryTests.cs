using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using DataLayer;
using DataLayer.Models;
using Domain.Models;

namespace DataLayer.Tests
{
    public class DartsRepositoryTests
    {
        private DbContextOptions<DartsDbContext> GetInMemoryOptions()
        {
            return new DbContextOptionsBuilder<DartsDbContext>()
                .UseInMemoryDatabase(databaseName: $"DartsTestDb_{Guid.NewGuid()}")
                .Options;
        }

        // ==========================================
        // TESTS FOR PLAYERS
        // ==========================================

        [Fact]
        public async Task CreatePlayerAsync_ShouldAddNewPlayer_WhenNameIsUnique()
        {
            var options = GetInMemoryOptions();
            var repository = new DartsRepository(options);
            string newPlayerName = "Karel";

            var result = await repository.CreatePlayerAsync(newPlayerName);

            Assert.NotNull(result);
            Assert.Equal(newPlayerName, result.PlayerName);
            Assert.True(result.Id > 0);
        }

        [Fact]
        public async Task CreatePlayerAsync_ShouldReturnExistingPlayer_WhenNameAlreadyExists()
        {
            var options = GetInMemoryOptions();
            var repository = new DartsRepository(options);
            string playerName = "Duplikát";
    
            var firstResult = await repository.CreatePlayerAsync(playerName); 

            var duplicateResult = await repository.CreatePlayerAsync(playerName);

            Assert.NotNull(firstResult);
            Assert.NotNull(duplicateResult);
            Assert.Equal(firstResult.Id, duplicateResult.Id);
            Assert.Equal(playerName, duplicateResult.PlayerName);
        }

        [Fact]
        public async Task GetAllPlayersAsync_ShouldReturnAllPlayers()
        {
            var options = GetInMemoryOptions();
            var repository = new DartsRepository(options);
            await repository.CreatePlayerAsync("Hráč 1");
            await repository.CreatePlayerAsync("Hráč 2");

            var players = await repository.GetAllPlayersAsync();

            Assert.NotNull(players);
            Assert.Equal(2, players.Count);
            Assert.Contains(players, p => p.PlayerName == "Hráč 1");
            Assert.Contains(players, p => p.PlayerName == "Hráč 2");
        }

        [Fact]
        public async Task GetPlayerByIdAsync_ShouldReturnPlayer_WhenExists()
        {
            var options = GetInMemoryOptions();
            var repository = new DartsRepository(options);
            var createdPlayer = await repository.CreatePlayerAsync("Cílový Hráč");
            Assert.NotNull(createdPlayer);

            var player = await repository.GetPlayerByIdAsync(createdPlayer.Id);

            Assert.NotNull(player);
            Assert.Equal("Cílový Hráč", player.PlayerName);
        }

        [Fact]
        public async Task GetPlayerByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            var options = GetInMemoryOptions();
            var repository = new DartsRepository(options);

            var player = await repository.GetPlayerByIdAsync(999);

            Assert.Null(player);
        }

        [Fact]
        public async Task RenamePlayerAsync_ShouldChangeName()
        {
            var options = GetInMemoryOptions();
            var repository = new DartsRepository(options);
            var player = await repository.CreatePlayerAsync("Staré Jméno");
            Assert.NotNull(player);

            await repository.RenamePlayerAsync(player.Id, "Nové Jméno");

            var updatedPlayer = await repository.GetPlayerByIdAsync(player.Id);
            Assert.NotNull(updatedPlayer);
            Assert.Equal("Nové Jméno", updatedPlayer.PlayerName);
        }

        [Fact]
        public async Task DeletePlayerAsync_ShouldRemovePlayer()
        {
            var options = GetInMemoryOptions();
            var repository = new DartsRepository(options);
            var player = await repository.CreatePlayerAsync("Smazat Mě");
            Assert.NotNull(player);

            await repository.DeletePlayerAsync(player.Id);

            var deletedPlayer = await repository.GetPlayerByIdAsync(player.Id);
            Assert.Null(deletedPlayer);
        }

        // ==========================================
        // TESTS FOR STATISTICS
        // ==========================================

        [Fact]
        public async Task UpdateStatsAsync_ShouldAddNewStats_WhenNotExists()
        {
            var options = GetInMemoryOptions();
            var repository = new DartsRepository(options);
            var player = await repository.CreatePlayerAsync("Hráč Pro Statistiky");
            Assert.NotNull(player);

            var newStats = new PlayerStatsDto
            {
                PlayerId = player.Id,
                Year = 2024,
                Wins = 5,
                MatchesPlayed = 6,
                Average = 45.5
            };

            await repository.UpdateStatsAsync(newStats);

            var savedStats = await repository.GetStatsForYearAsync(player.Id, 2024);
            Assert.NotNull(savedStats);
            Assert.Equal(5, savedStats.Wins);
            Assert.Equal(6, savedStats.MatchesPlayed);
            Assert.Equal(45.5, savedStats.Average);
        }

        [Fact]
        public async Task UpdateStatsAsync_ShouldUpdateExistingStats_WhenExists()
        {
            var options = GetInMemoryOptions();
            var repository = new DartsRepository(options);
            var player = await repository.CreatePlayerAsync("Hráč Pro Aktualizaci");
            Assert.NotNull(player);

            var initialStats = new PlayerStatsDto { PlayerId = player.Id, Year = 2024, Wins = 2, MatchesPlayed = 2 };
            await repository.UpdateStatsAsync(initialStats); 

            var updatedStats = new PlayerStatsDto { PlayerId = player.Id, Year = 2024, Wins = 10, MatchesPlayed = 12, Average = 60.0 };
            await repository.UpdateStatsAsync(updatedStats); 

            var savedStats = await repository.GetStatsForYearAsync(player.Id, 2024);
            Assert.NotNull(savedStats);
            Assert.Equal(10, savedStats.Wins);
            Assert.Equal(12, savedStats.MatchesPlayed);
            Assert.Equal(60.0, savedStats.Average);
        }

        [Fact]
        public async Task GetStatsForYearAsync_ShouldReturnNull_WhenStatsDoNotExist()
        {
            var options = GetInMemoryOptions();
            var repository = new DartsRepository(options);

            var stats = await repository.GetStatsForYearAsync(999, 2024);

            Assert.Null(stats);
        }
    }
}