using System;
using System.Threading.Tasks;
using Xunit;
using DesktopUI.ViewModels;
using DataLayer;
using Microsoft.EntityFrameworkCore;
using Domain.Models;

namespace Integration.Tests
{
    public class DatabaseIntegrationTests
    {
        [Fact]
        public async Task MainViewModel_GetDatabasePlayersAsync_ShouldReturnDataFromRealDatabase()
        {
            var options = new DbContextOptionsBuilder<DartsDbContext>()
                .UseInMemoryDatabase(databaseName: $"IntegrationDB_Players_{Guid.NewGuid()}")
                .Options;

            await using (var seedContext = new DartsDbContext(options))
            {
                seedContext.Players.Add(new DataLayer.Models.Player { PlayerName = "Luke Littler" });
                seedContext.Players.Add(new DataLayer.Models.Player { PlayerName = "Michael van Gerwen" });
                await seedContext.SaveChangesAsync();
            }

            var repository = new DartsRepository(options);
            var mainVm = new MainViewModel(repository);

            var playersFromDb = await mainVm.GetDatabasePlayersAsync();

            Assert.NotNull(playersFromDb);
            Assert.Equal(2, playersFromDb.Count);
            
            Assert.Contains(playersFromDb, p => p.PlayerName == "Luke Littler");
            Assert.Contains(playersFromDb, p => p.PlayerName == "Michael van Gerwen");
            Assert.All(playersFromDb, p => Assert.True(p.Id > 0)); 
        }

        [Fact]
        public async Task Repository_ShouldCorrectlySaveAndRetrieveComplexStatistics()
        {
            var options = new DbContextOptionsBuilder<DartsDbContext>()
                .UseInMemoryDatabase(databaseName: $"IntegrationDB_Stats_{Guid.NewGuid()}")
                .Options;
            var repository = new DartsRepository(options);

            var newPlayer = await repository.CreatePlayerAsync("Matěj");
            Assert.NotNull(newPlayer);

            var matchStats = new PlayerStatsDto
            {
                PlayerId = newPlayer.Id,
                Year = 2026,
                Wins = 15,
                Average = 65.4,
                HighestOut = 170
            };
            
            await repository.UpdateStatsAsync(matchStats);

            var retrievedStats = await repository.GetStatsForYearAsync(newPlayer.Id, 2026);

            Assert.NotNull(retrievedStats);
            Assert.Equal(newPlayer.Id, retrievedStats.PlayerId);
            Assert.Equal(15, retrievedStats.Wins);
            Assert.Equal(65.4, retrievedStats.Average);
            Assert.Equal(170, retrievedStats.HighestOut);
        }
        
        [Fact]
        public async Task Repository_RenamePlayerAsync_ShouldUpdateExistingPlayerName()
        {
            var options = new DbContextOptionsBuilder<DartsDbContext>()
                .UseInMemoryDatabase(databaseName: $"IntegrationDB_UpdateName_{Guid.NewGuid()}")
                .Options;
            var repository = new DartsRepository(options);

            var player = await repository.CreatePlayerAsync("Staré Jméno");
            Assert.NotNull(player);

            await repository.RenamePlayerAsync(player.Id, "Luke Littler");

            var updatedPlayer = await repository.GetPlayerByIdAsync(player.Id);
            Assert.NotNull(updatedPlayer);
            Assert.Equal("Luke Littler", updatedPlayer.PlayerName);
        }

        [Fact]
        public async Task Repository_UpdateStatsAsync_ShouldModifyExistingRecord_WhenYearMatches()
        {
            var options = new DbContextOptionsBuilder<DartsDbContext>()
                .UseInMemoryDatabase(databaseName: $"IntegrationDB_UpdateStats_{Guid.NewGuid()}")
                .Options;
            var repository = new DartsRepository(options);

            var player = await repository.CreatePlayerAsync("Tréninkový Hráč");
            Assert.NotNull(player);

            var initialStats = new PlayerStatsDto
            {
                PlayerId = player.Id,
                Year = 2026,
                Wins = 10,
                Average = 50.5
            };
            await repository.UpdateStatsAsync(initialStats);

            var updatedStats = new PlayerStatsDto
            {
                PlayerId = player.Id,
                Year = 2026,   
                Wins = 11,     
                Average = 62.3 
            };
            await repository.UpdateStatsAsync(updatedStats); 

            var retrievedStats = await repository.GetStatsForYearAsync(player.Id, 2026);
            
            Assert.NotNull(retrievedStats);
            Assert.Equal(11, retrievedStats.Wins);
            Assert.Equal(62.3, retrievedStats.Average);
        }

        [Fact]
        public async Task Duel_MultipleMatches_ShouldProperlyCalculateWeightedAverage()
        {
            var options = new DbContextOptionsBuilder<DartsDbContext>()
                .UseInMemoryDatabase(databaseName: $"IntegrationDB_WeightedAvg_{Guid.NewGuid()}")
                .Options;
            var repository = new DartsRepository(options);

            var player = await repository.CreatePlayerAsync("Matematik");

            int year = DateTime.Now.Year;
            var record = await repository.GetStatsForYearAsync(player.Id, year) ?? new PlayerStatsDto
            {
                PlayerId = player.Id,
                Year = year
            };

            // First game: avg 60.0
            record.Average = 60.0;
            record.MatchesPlayed = 1;
            await repository.UpdateStatsAsync(record);

            // Second game: avg 80.0
            int prevMatches = record.MatchesPlayed;
            record.Average = Math.Round(((record.Average * prevMatches) + 80.0) / (prevMatches + 1), 2);
            record.MatchesPlayed = prevMatches + 1;
            await repository.UpdateStatsAsync(record);

            var check2 = await repository.GetStatsForYearAsync(player.Id, year);
            Assert.NotNull(check2);
            Assert.Equal(70.0, check2.Average);
            Assert.Equal(2, check2.MatchesPlayed);

            // Third game: avg 40.0
            prevMatches = check2.MatchesPlayed;
            record.Average = Math.Round(((check2.Average * prevMatches) + 40.0) / (prevMatches + 1), 2);
            record.MatchesPlayed = prevMatches + 1;
            await repository.UpdateStatsAsync(record);

            var check3 = await repository.GetStatsForYearAsync(player.Id, year);
            Assert.NotNull(check3);
            Assert.Equal(60.0, check3.Average); // NOT 55.0!
            Assert.Equal(3, check3.MatchesPlayed);
        }
    }
}