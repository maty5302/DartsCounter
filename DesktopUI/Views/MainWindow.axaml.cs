using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using DesktopUI.Services;
using DesktopUI.ViewModels;
using DesktopUI.ViewModels.Tournament;
using Domain;
using Microsoft.Extensions.DependencyInjection;
using TournamentSetupViewModel = DesktopUI.ViewModels.Tournament.TournamentSetupViewModel;

namespace DesktopUI.Views;

public partial class MainWindow : Window
{
    private MainViewModel? _boundViewModel;

    public MainWindow()
    {
        InitializeComponent();
    }

    protected override async void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        if (_boundViewModel != null)
        {
            _boundViewModel.Settings.PropertyChanged -= SettingsOnPropertyChanged;
            await SoundManagerDarts.SoundEffects.StopPlayer();
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        if (_boundViewModel != null)
        {
            _boundViewModel.Settings.PropertyChanged -= SettingsOnPropertyChanged;
            _boundViewModel = null;
        }
    }
    
    protected override async void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        if (await GithubIntegration.CheckForUpdates())
        {
            var updateWindow = new UpdateWindow();
            await updateWindow.ShowDialog(this);
        }
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
    
        if (_boundViewModel != null)
        {
            _boundViewModel.Settings.PropertyChanged -= SettingsOnPropertyChanged;
        }
    
        _boundViewModel = DataContext as MainViewModel;
        if (_boundViewModel == null)
        {
            return;
        }
    
        _boundViewModel.Settings.PropertyChanged += SettingsOnPropertyChanged;
        
        ApplyWallpaper(_boundViewModel.Settings.MainBackgroundUri);
    }
    
    private void SettingsOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SettingsManager.MainBackgroundUri) && _boundViewModel != null)
        {
            ApplyWallpaper(_boundViewModel.Settings.MainBackgroundUri);
        }
    }

    private void ApplyWallpaper(string wallpaperUri)
    {
        var uri = Uri.TryCreate(wallpaperUri, UriKind.Absolute, out var parsedUri)
            ? parsedUri
            : new Uri("avares://DartsCounter/Assets/Backgrounds/darts-3-develop.jpg");

        if (!AssetLoader.Exists(uri))
        {
            uri = new Uri("avares://DartsCounter/Assets/Backgrounds/darts-3-develop.jpg");
        }

        using var stream = AssetLoader.Open(uri);
        var bitmap = new Bitmap(stream);
        Background = new ImageBrush(bitmap) { Stretch = Stretch.UniformToFill };
    }

    private void Button_OnClickAbout(object? sender, RoutedEventArgs e)
    {
        AboutApp app = new AboutApp
        {
            DataContext = new AboutAppViewModel() 
        };
        app.ShowDialog(this);
    }

    private async void Button_OnClickDuel(object? sender, RoutedEventArgs e)
    {
        if (_boundViewModel != null)
        {
            var dbPlayers = await _boundViewModel.GetDatabasePlayersAsync();
            if (dbPlayers == null || dbPlayers.Count < 2)
            {
                var box = MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard(
                    "Error", 
                    Strings.DuelError, 
                    MsBox.Avalonia.Enums.ButtonEnum.Ok, 
                    MsBox.Avalonia.Enums.Icon.Error, null, WindowStartupLocation.CenterOwner);

                await box.ShowAsync();
                return;
            }

            DuelSetupWindow setup = new DuelSetupWindow(_boundViewModel, dbPlayers);
            await setup.ShowDialog(this);
        }
    }

    private void PlayMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (SoundManagerDarts.SoundEffects.IsMusicPlaying)
        {
            _ = SoundManagerDarts.SoundEffects.StopPlayer();
            SoundManagerDarts.SoundEffects.IsMusicPlaying = false;
        }
        else
        {
            
            _ = SoundManagerDarts.SoundEffects.PlayDartsSong();
            SoundManagerDarts.SoundEffects.IsMusicPlaying = true;
        }
    }

    private async void Button_OnClickSettings(object? sender, RoutedEventArgs e)
    {
       if (DataContext is not MainViewModel vm)
       {
            return;
       }
       var settingsVM = new SettingsViewModel(vm.Settings);

       SettingsWindow settingsWindow = new SettingsWindow(settingsVM);
    
       await settingsWindow.ShowDialog(this);
       vm.RefreshActivePlayers();
        
       if (settingsWindow.RequiresMainWindowReload)
       {
           var newWindow = new MainWindow
           {
               DataContext = vm,
               Width = Width,
               Height = Height
           };
        
           newWindow.Show();
           Close();
       }
    }

    private void Button_OnClickStatistics(object? sender, RoutedEventArgs e)
    {
        var vm = App.Services?.GetRequiredService<StatisticsViewModel>();
        var window = new StatisticsWindow { DataContext = vm };
        window.Show();
    }

    private void Button_OnClickDartBoard(object? sender, RoutedEventArgs e)
    {
        DartboardWindow experiment = new DartboardWindow(_boundViewModel);
        experiment.Show();
    }

    private async void Button_OnClickTournament(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            var setupVm = new TournamentSetupViewModel(vm.Repo);
            var setupWindow = new TournamentSetupWindow(setupVm);
            
            var players = await setupWindow.ShowDialog<List<Domain.Models.PlayerDto>>(this);
            
            if (players != null && players.Count > 0)
            {
                vm.StartTournament(players);
            }
        }
    }
    
    private void Button_OnClickTournamentDetail(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm && vm.ActiveTournament != null)
        {
            var bracketVm = new TournamentBracketViewModel();
            bracketVm.LoadTournament(vm.ActiveTournament);
            
            var bracketWindow = new TournamentBracketWindow 
            { 
                DataContext = bracketVm 
            };
            bracketWindow.Show(this); 
        }
    }
}