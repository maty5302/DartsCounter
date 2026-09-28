using Microsoft.EntityFrameworkCore;
using DataLayer.Models;
using Domain.Interfaces;
using Domain.Models;   
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataLayer;

public class DartsRepository : IDartsRepository
{
    private readonly DbContextOptions<DartsDbContext>? _testOptions;
    public DartsRepository()
    {
        using var context = new DartsDbContext();
        context.Database.EnsureCreated();
        EnsureDatabaseSchema(context);
    }
    public DartsRepository(DbContextOptions<DartsDbContext> testOptions)
    {
        _testOptions = testOptions;
        using var context = CreateContext();
        context.Database.EnsureCreated();
        EnsureDatabaseSchema(context);
    }

    private static void EnsureDatabaseSchema(DartsDbContext context)
    {
        try
        {
            if (context.Database.IsSqlite())
            {
                using var conn = context.Database.GetDbConnection();
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "PRAGMA table_info(YearlyStatistics);";
                using var reader = cmd.ExecuteReader();
                bool hasMatchesPlayed = false;
                while (reader.Read())
                {
                    var colName = reader["name"]?.ToString();
                    if (string.Equals(colName, "MatchesPlayed", StringComparison.OrdinalIgnoreCase))
                    {
                        hasMatchesPlayed = true;
                        break;
                    }
                }
                reader.Close();

                if (!hasMatchesPlayed)
                {
                    using var alterCmd = conn.CreateCommand();
                    alterCmd.CommandText = "ALTER TABLE YearlyStatistics ADD COLUMN MatchesPlayed INTEGER NOT NULL DEFAULT 0;";
                    alterCmd.ExecuteNonQuery();
                }
            }
        }
        catch
        {
            // Ignore schema migration issues for in-memory or already migrated dbs
        }
    }
    private DartsDbContext CreateContext()
    {
        return _testOptions != null 
            ? new DartsDbContext(_testOptions) 
            : new DartsDbContext();
    }
    
    public async Task<List<PlayerDto>> GetAllPlayersAsync()
    {
        await using var context = CreateContext();
        
        return await context.Players
            .Select(p => new PlayerDto 
            { 
                Id = p.Id, 
                PlayerName = p.PlayerName 
            })
            .ToListAsync();
    }
    
    public async Task<PlayerDto?> GetPlayerByIdAsync(long playerId)
    {
        await using var context = CreateContext();
        var player = await context.Players.FirstOrDefaultAsync(p => p.Id == playerId);
        
        if (player == null) return null;
        
        return new PlayerDto 
        { 
            Id = player.Id, 
            PlayerName = player.PlayerName 
        };
    }
    
    public async Task<PlayerDto?> CreatePlayerAsync(string playerName)
    {
        await using var context = CreateContext();
        
        var existingPlayer = await context.Players.FirstOrDefaultAsync(p => p.PlayerName == playerName);
        if (existingPlayer != null)
        {
            return new PlayerDto 
            { 
                Id = existingPlayer.Id, 
                PlayerName = existingPlayer.PlayerName 
            };
        }
            
        var player = new Player { PlayerName = playerName };
        context.Players.Add(player);
        await context.SaveChangesAsync();
        
        return new PlayerDto 
        { 
            Id = player.Id, 
            PlayerName = player.PlayerName 
        };
    }
    
    public async Task DeletePlayerAsync(long playerId)
    {
        await using var context = CreateContext();
        var player = await context.Players.FindAsync(playerId);
        
        if (player != null)
        {
            context.Players.Remove(player);
            await context.SaveChangesAsync();
        }
    }
    
    public async Task RenamePlayerAsync(long playerId, string newName)
    {
        await using var context = CreateContext();
        var player = await context.Players.FindAsync(playerId);
        
        if (player != null)
        {
            player.PlayerName = newName;
            await context.SaveChangesAsync();
        }
    }
    
    /// <summary>
    /// Methods for statistics
    /// </summary>
    /// 
    public async Task<PlayerStatsDto?> GetStatsForYearAsync(long playerId, int year)
    {
        await using var context = CreateContext();
        var stats = await context.YearlyStatistics
            .FirstOrDefaultAsync(s => s.PlayerId == playerId && s.Year == year);

        if (stats == null) return null;

        return new PlayerStatsDto
        {
            PlayerId = stats.PlayerId,
            Year = stats.Year,
            Wins = stats.Wins,
            MatchesPlayed = stats.MatchesPlayed,
            Average = stats.Average,
            HighestOut = stats.HighestOut,
            Sixty = stats.Sixty,
            Hundred = stats.Hundred,
            Hundred20 = stats.Hundred20,
            Hundred80 = stats.Hundred80
        };
    }
    
    public async Task UpdateStatsAsync(PlayerStatsDto statsDto)
    {
        await using var context = CreateContext();
    
        var existing = await context.YearlyStatistics
            .FirstOrDefaultAsync(s => s.PlayerId == statsDto.PlayerId && s.Year == statsDto.Year);

        if (existing == null)
        {
            var newStats = new YearlyStatistic
            {
                PlayerId = statsDto.PlayerId, 
                Year = statsDto.Year,
                Wins = statsDto.Wins,
                MatchesPlayed = statsDto.MatchesPlayed,
                Average = statsDto.Average,
                HighestOut = statsDto.HighestOut,
                Sixty = statsDto.Sixty,
                Hundred = statsDto.Hundred,
                Hundred20 = statsDto.Hundred20,
                Hundred80 = statsDto.Hundred80
            };
            context.YearlyStatistics.Add(newStats);
        }
        else
        {
            existing.Wins = statsDto.Wins;
            existing.MatchesPlayed = statsDto.MatchesPlayed;
            existing.Average = statsDto.Average;
            existing.HighestOut = statsDto.HighestOut;
            existing.Sixty = statsDto.Sixty;
            existing.Hundred = statsDto.Hundred;
            existing.Hundred20 = statsDto.Hundred20;
            existing.Hundred80 = statsDto.Hundred80;
        }

        await context.SaveChangesAsync();
    }
    
    public async Task<List<int>> GetAvailableYearsAsync(long playerId)
    {
        await using var context = CreateContext();
        return await context.YearlyStatistics
            .Where(s => s.PlayerId == playerId)
            .Select(s => s.Year)
            .Distinct()
            .OrderByDescending(y => y) 
            .ToListAsync();
    }
}