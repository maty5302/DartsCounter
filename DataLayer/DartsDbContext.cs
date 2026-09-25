using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using DataLayer.Models;

namespace DataLayer
{
    public class DartsDbContext : DbContext
    {
        public DbSet<Player> Players { get; set; }
        public DbSet<YearlyStatistic> YearlyStatistics { get; set; }
        
        private readonly string dbPath;

        public DartsDbContext() 
        { 
            dbPath = GetDatabasePath();
        }

        public DartsDbContext(DbContextOptions<DartsDbContext> options) : base(options) 
        { 
            dbPath = GetDatabasePath();
        }

        private string GetDatabasePath()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            
            var appFolder = Path.Combine(path, "DartsCounter");
            
            Directory.CreateDirectory(appFolder);
            if (!OperatingSystem.IsWindows())
            {
                try
                {
                    File.SetUnixFileMode(appFolder, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
                }
                catch
                {
                    // Ignorujeme v případě souborového systému bez podpory POSIX práv
                }
            }

            return Path.Combine(appFolder, "Darts.db");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = new SqliteConnectionStringBuilder
                {
                    DataSource = dbPath
                }.ConnectionString;

                optionsBuilder.UseSqlite(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Player>()
                .HasIndex(p => p.PlayerName)
                .IsUnique();
        }
    }
}