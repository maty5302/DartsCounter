using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Interfaces;
using Domain.Models;
using MsBox.Avalonia.Enums;

namespace DesktopUI.ViewModels;

public partial class StatisticsViewModel : ObservableObject
{
    private readonly IDartsRepository _repository;

    [ObservableProperty] private ObservableCollection<PlayerDto> _players = new();

    [ObservableProperty] private PlayerDto? _selectedPlayer;

    [ObservableProperty] private string _newPlayerName = string.Empty;

    [ObservableProperty] private string _renamePlayerName = string.Empty;

    [ObservableProperty] private int _wins;
    [ObservableProperty] private double _average;
    [ObservableProperty] private int _highestOut;
    [ObservableProperty] private int _sixty;
    [ObservableProperty] private int _hundred;
    [ObservableProperty] private int _hundred20;
    [ObservableProperty] private int _hundred80;
    
    [ObservableProperty] private Bitmap? _achCupImage;
    [ObservableProperty] private Bitmap? _achCup20Image;
    [ObservableProperty] private Bitmap? _achCup100Image;
    [ObservableProperty] private Bitmap? _ach180Image;
    [ObservableProperty] private Bitmap? _achMore100Image;
    [ObservableProperty] private ObservableCollection<int> _availableYears = new();

    public StatisticsViewModel(IDartsRepository repository)
    {
        _repository = repository;
        _ = LoadDataAsync();
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        var list = await _repository.GetAllPlayersAsync();
        Players = new ObservableCollection<PlayerDto>(list);

        if (Players.Count > 0 && SelectedPlayer == null)
        {
            SelectedPlayer = Players[0];
        }
    }

    partial void OnSelectedPlayerChanged(PlayerDto? value)
    {
        if (value != null)
        {
            RenamePlayerName = value.PlayerName;
            _ = LoadPlayerProfileAsync(value.Id);
        }
    }

    private int? _selectedYear;
    public int? SelectedYear
    {
        get => _selectedYear;
        set
        {
            if (SetProperty(ref _selectedYear, value) && SelectedPlayer != null && value.HasValue && value.Value > 0)
            {
                _ = LoadStatsForSelectedYearAsync(SelectedPlayer.Id, value.Value);
            }
        }
    }
    
    private async Task LoadStatsForSelectedYearAsync(long playerId, int year)
    {
        var stats = await _repository.GetStatsForYearAsync(playerId, year);
        UpdateStatsUi(stats);
    }
    
    private async Task LoadPlayerProfileAsync(long playerId)
    {
        var years = await _repository.GetAvailableYearsAsync(playerId);
        AvailableYears.Clear();
        foreach (var year in years)
        {
            AvailableYears.Add(year);
        }

        int currentYear = DateTime.Now.Year;
        if (AvailableYears.Contains(currentYear))
        {
            SelectedYear = currentYear;
            // Ensure stats load for the new player even if SelectedYear value equals previous player's year
            await LoadStatsForSelectedYearAsync(playerId, SelectedYear.Value);
        }
        else if (AvailableYears.Count > 0)
        {
            SelectedYear = AvailableYears[0]; 
            await LoadStatsForSelectedYearAsync(playerId, SelectedYear.Value);
        }
        else
        {
            SelectedYear = null;
            UpdateStatsUi(null);
        }
    }

    [RelayCommand]
    private async Task CreatePlayerAsync()
    {
        if (string.IsNullOrWhiteSpace(NewPlayerName)) return;

        var created = await _repository.CreatePlayerAsync(NewPlayerName.Trim());
        if (created != null)
        {
            NewPlayerName = string.Empty;
            await LoadDataAsync();
            SelectedPlayer = Players.FirstOrDefault(p => p.Id == created.Id);
        }
    }

    [RelayCommand]
    private async Task DeletePlayerAsync()
    {
        if (SelectedPlayer == null) return;

        await _repository.DeletePlayerAsync(SelectedPlayer.Id);
        SelectedPlayer = null;
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task RenamePlayerAsync()
    {
        if (SelectedPlayer == null || string.IsNullOrWhiteSpace(RenamePlayerName)) return;

        await _repository.RenamePlayerAsync(SelectedPlayer.Id, RenamePlayerName.Trim());
        long currentId = SelectedPlayer.Id;
        await LoadDataAsync();
        SelectedPlayer = Players.FirstOrDefault(p => p.Id == currentId);
    }
    
    private void UpdateStatsUi(PlayerStatsDto? stats)
    {
        if (stats != null)
        {
            Wins = stats.Wins;
            Average = stats.Average;
            HighestOut = stats.HighestOut;
            Sixty = stats.Sixty;
            Hundred = stats.Hundred;
            Hundred20 = stats.Hundred20;
            Hundred80 = stats.Hundred80;
        }
        else
        {
            Wins = 0; Average = 0; HighestOut = 0; Sixty = 0; Hundred = 0; Hundred20 = 0; Hundred80 = 0;
        }
        
        UpdateAchievements();
    }
    private Bitmap? LoadImage(string uriString)
    {
        try
        {
            using var stream = AssetLoader.Open(new Uri(uriString));
            return new Bitmap(stream);
        }
        catch
        {
            return null;
        }
    }

    private void UpdateAchievements()
    {
        AchCupImage = LoadImage(Wins >= 1 
            ? "avares://DartsCounter/Assets/Achievements/a_cup.jpg" 
            : "avares://DartsCounter/Assets/Achievements/a_cup_no.jpg");
            
        AchCup20Image = LoadImage(Wins >= 20 
            ? "avares://DartsCounter/Assets/Achievements/a_cup_20.jpg" 
            : "avares://DartsCounter/Assets/Achievements/a_cup_20_no.jpg");
            
        AchCup100Image = LoadImage(Wins >= 100 
            ? "avares://DartsCounter/Assets/Achievements/a_cup_100.jpg" 
            : "avares://DartsCounter/Assets/Achievements/a_cup_100_no.jpg");
            
        Ach180Image = LoadImage(Hundred80 >= 1 
            ? "avares://DartsCounter/Assets/Achievements/a_180.png" 
            : "avares://DartsCounter/Assets/Achievements/a_180_no.png");
            
        AchMore100Image = LoadImage(Hundred >= 1 
            ? "avares://DartsCounter/Assets/Achievements/a_more100.png" 
            : "avares://DartsCounter/Assets/Achievements/a_more100_no.png");
    }
    
    [RelayCommand]
    private async Task ImportOldDatabaseAsync()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
            desktop.MainWindow?.StorageProvider is { } storageProvider)
        {
            string initialPath;
            
            if (OperatingSystem.IsWindows())
            {
                initialPath = @"C:\Program Files (x86)\Šipky"; 
            }
            else
            {
                initialPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            }

            IStorageFolder? startFolder = null;
            if (Directory.Exists(initialPath))
            {
                startFolder = await storageProvider.TryGetFolderFromPathAsync(initialPath);
            }

            var options = new FilePickerOpenOptions
            {
                Title = Strings.StatisticsImportSelect,
                AllowMultiple = false,
                SuggestedStartLocation = startFolder,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType(Strings.SqliteDb) { Patterns = new[] { "*.db", "*.sqlite", "*.sqlite3" } },
                    new FilePickerFileType(Strings.Allfiles) { Patterns = new[] { "*.*" } }
                }
            };

            var result = await storageProvider.OpenFilePickerAsync(options);

            if (result.Count > 0)
            {
                string oldDbPath = result[0].Path.LocalPath;

                if (!File.Exists(oldDbPath))
                    return;

                try
                {
                    var builder = new SqliteConnectionStringBuilder
                    {
                        DataSource = oldDbPath,
                        Mode = SqliteOpenMode.ReadOnly
                    };

                    await using var connection = new SqliteConnection(builder.ConnectionString);
                    await connection.OpenAsync();

                    var command = connection.CreateCommand();
                    command.CommandText = @"SELECT Name, Wins, Average, highestOut, sixty, hundred, hundred20, hundred80 FROM PlayerSettings";

                    await using var reader = await command.ExecuteReaderAsync();

                    int importedCount = 0;
                    while (await reader.ReadAsync())
                    {
                        string name = reader.GetString(0);
                        if (string.IsNullOrWhiteSpace(name)) continue;
                        
                        var newPlayer = await _repository.CreatePlayerAsync(name.Trim());
                        if (newPlayer == null) continue;
                        
                        var playerStats = new PlayerStatsDto
                        {
                            PlayerId = newPlayer.Id,
                            Year = DateTime.Now.Year,
                            Wins = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                            Average = reader.IsDBNull(2) ? 0.0 : reader.GetDouble(2),
                            HighestOut = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                            Sixty = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                            Hundred = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                            Hundred20 = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                            Hundred80 = reader.IsDBNull(7) ? 0 : reader.GetInt32(7)
                        };
                        await _repository.UpdateStatsAsync(playerStats);
                        importedCount++;
                    }

                    await LoadDataAsync();

                    await MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard(
                        "Info",
                        $"Import byl úspěšně dokončen (naimportováno {importedCount} hráčů).",
                        ButtonEnum.Ok, Icon.Info).ShowAsync();
                }
                catch (SqliteException ex)
                {
                    await MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard(
                        "Error",
                        $"Chyba při čtení SQLite databáze: {ex.Message}",
                        ButtonEnum.Ok, Icon.Error).ShowAsync();
                }
                catch (Exception ex)
                {
                    await MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard(
                        "Error",
                        $"Chyba při importu: {ex.Message}",
                        ButtonEnum.Ok, Icon.Error).ShowAsync();
                }
            }
        }
    }
}