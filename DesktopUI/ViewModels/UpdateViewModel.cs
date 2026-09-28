using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Domain;
using MsBox.Avalonia.Enums;

namespace DesktopUI.ViewModels
{
    public partial class UpdateViewModel : ViewModelBase
    {
        [ObservableProperty] private string _version = "x.x.x.x";
        [ObservableProperty] private string _changelog = "";
        
        [ObservableProperty] private bool _isDownloading;
        [ObservableProperty] private double _downloadPercentage;
        [ObservableProperty] private string _progressText = "";

        private readonly string _cacheFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
            "DartsCounter", 
            "update_cache.txt");
        
        public Action? CloseAction { get; set; }

        private static readonly HttpClient _httpClient = new HttpClient();
        
        private CancellationTokenSource? _cancellationTokenSource;

        public UpdateViewModel()
        {
            _ = LoadChangelogAsync();
        }

        private async Task LoadChangelogAsync()
        {
            try 
            {
                // Načíst data z cache, pokud je soubor mladší než 1 hodina
                if (File.Exists(_cacheFilePath))
                {
                    var fileInfo = new FileInfo(_cacheFilePath);
                    if ((DateTime.Now - fileInfo.LastWriteTime).TotalHours < 1 || await GithubIntegration.CheckForUpdates())
                    {
                        var cachedLines = await File.ReadAllLinesAsync(_cacheFilePath);
                        if (cachedLines.Length >= 2)
                        {
                            Version = cachedLines[0];
                            Changelog = string.Join(Environment.NewLine, cachedLines[1..]);
                    
                            return; 
                        }
                    }
                }

                Version = await GithubIntegration.GetGitVersion();
                Changelog = await GithubIntegration.GetReleaseNotes(Version);
            }
            catch (Exception)
            {
                Changelog = Strings.InternetNot;
            }
        }

        [RelayCommand]
        private async Task DownloadAsync()
        {
            IsDownloading = true;
            _cancellationTokenSource = new CancellationTokenSource();
            
            var progress = new Progress<(double Percentage, long BytesReceived, long TotalBytes)>(data =>
            {
                DownloadPercentage = data.Percentage;
                ProgressText = $"{data.BytesReceived / 1024 / 1024}MB / {data.TotalBytes / 1024 / 1024}MB";
            });
            
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    await DownloadWindowsInstaller(progress, _cancellationTokenSource.Token);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    await DownloadLinuxAppImage("DartsCounter.AppImage", progress, _cancellationTokenSource.Token);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    await DownloadMacOsInstaller(progress, _cancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                
            }
            catch (HttpRequestException)
            {
                
            }
            finally
            {
                CloseAction?.Invoke();
            }
        }

        private static string CreateSecureTempDirectory()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "DartsCounterUpdate_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
            return tempDir;
        }

        private static async Task<bool> VerifyFileChecksumAsync(string filePath, string checksumUrl, CancellationToken cancellationToken)
        {
            try
            {
                using var response = await _httpClient.GetAsync(checksumUrl, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

                string rawChecksum = await response.Content.ReadAsStringAsync(cancellationToken);
                var tokens = rawChecksum.Trim().Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length == 0)
                {
                    return false;
                }
                string expectedHash = tokens[0];

                using var sha256 = SHA256.Create();
                await using var stream = File.OpenRead(filePath);
                byte[] hashBytes = await sha256.ComputeHashAsync(stream, cancellationToken);
                string computedHash = Convert.ToHexString(hashBytes);

                return string.Equals(computedHash, expectedHash, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private async Task DownloadFileWithProgressAsync(string url, string destinationPath, IProgress<(double, long, long)> progress, CancellationToken cancellationToken)
        {
            using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? 0L;

            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            var buffer = new byte[8192];
            var totalRead = 0L;
            var isMoreToRead = true;

            while (isMoreToRead)
            {
                var read = await contentStream.ReadAsync(buffer, cancellationToken);
                if (read == 0)
                {
                    isMoreToRead = false;
                }
                else
                {
                    await fileStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                    totalRead += read;

                    if (totalBytes > 0)
                    {
                        var percentage = Math.Round((double)totalRead / totalBytes * 100, 2);
                        progress?.Report((percentage, totalRead, totalBytes)); 
                    }
                }
            }
        }

        private async Task DownloadWindowsInstaller(IProgress<(double, long, long)> progress, CancellationToken token)
        {
            string tempDir = CreateSecureTempDirectory();
            string filePath = Path.Combine(tempDir, "DartsCounter.msi");
            const string downloadUrl = "https://github.com/maty5302/DartsCounter/releases/latest/download/DartsCounter.msi";
            const string checksumUrl = "https://github.com/maty5302/DartsCounter/releases/latest/download/DartsCounter.msi.sha256";

            try
            {
                await DownloadFileWithProgressAsync(downloadUrl, filePath, progress, token);

                bool isChecksumValid = await VerifyFileChecksumAsync(filePath, checksumUrl, token);
                if (!isChecksumValid)
                {
                    if (File.Exists(filePath)) File.Delete(filePath);
                    throw new InvalidOperationException("Chyba ověření integrity: Kontrolní součet SHA-256 neodpovídá.");
                }
                
                var processInfo = new ProcessStartInfo()
                {
                    FileName = "msiexec.exe",
                    Arguments = $"/i \"{filePath}\"",
                    UseShellExecute = false,
                };
                Process.Start(processInfo);
                Process.GetCurrentProcess().Kill();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                if (File.Exists(filePath))
                {
                    try { File.Delete(filePath); } catch { }
                }

                await MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard(
                    "Error",
                    ex.Message, ButtonEnum.Ok, Icon.Error).ShowAsync();
            }
        }

        private async Task DownloadLinuxAppImage(string defaultFilePath, IProgress<(double, long, long)> progress, CancellationToken token)
        {
            string tempDir = CreateSecureTempDirectory();
            string tempPath = Path.Combine(tempDir, "DartsCounter.AppImage.download");
            const string downloadUrl = "https://github.com/maty5302/DartsCounter/releases/latest/download/DartsCounter.AppImage";
            const string checksumUrl = "https://github.com/maty5302/DartsCounter/releases/latest/download/DartsCounter.AppImage.sha256";

            try
            {
                string? currentAppImagePath = Environment.GetEnvironmentVariable("APPIMAGE");
                string targetPath;

                if (!string.IsNullOrWhiteSpace(currentAppImagePath) &&
                    Path.IsPathRooted(currentAppImagePath) &&
                    File.Exists(currentAppImagePath))
                {
                    targetPath = currentAppImagePath;
                }
                else
                {
                    string downloadsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                    Directory.CreateDirectory(downloadsDir);
                    targetPath = Path.Combine(downloadsDir, defaultFilePath);
                }

                await DownloadFileWithProgressAsync(downloadUrl, tempPath, progress, token);

                bool isChecksumValid = await VerifyFileChecksumAsync(tempPath, checksumUrl, token);
                if (!isChecksumValid)
                {
                    if (File.Exists(tempPath)) File.Delete(tempPath);
                    throw new InvalidOperationException("Chyba ověření integrity: Kontrolní součet SHA-256 neodpovídá.");
                }
                
                if (File.Exists(targetPath))
                {
                    File.Delete(targetPath);
                }
                File.Move(tempPath, targetPath);

                // Nativní bezpečné nastavení spustitelných práv bez volání externího shellu (bash -c)
                if (!OperatingSystem.IsWindows())
                {
                    File.SetUnixFileMode(targetPath,
                        UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                        UnixFileMode.GroupRead | UnixFileMode.GroupExecute |
                        UnixFileMode.OtherRead | UnixFileMode.OtherExecute);
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = targetPath,
                    UseShellExecute = true
                });
                
                Process.GetCurrentProcess().Kill();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                if (File.Exists(tempPath))
                {
                    try { File.Delete(tempPath); } catch { }
                }

                await MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard(
                    "Error",
                    ex.Message, ButtonEnum.Ok, Icon.Error).ShowAsync();
            }
            finally
            {
                try
                {
                    if (Directory.Exists(tempDir))
                    {
                        Directory.Delete(tempDir, true);
                    }
                }
                catch { }
            }
        }

        private async Task DownloadMacOsInstaller(IProgress<(double, long, long)> progress, CancellationToken token)
        {
            string tempDir = CreateSecureTempDirectory();
            string filePath = Path.Combine(tempDir, "DartsCounter.dmg");
            const string downloadUrl = "https://github.com/maty5302/DartsCounter/releases/latest/download/DartsCounter.dmg";
            const string checksumUrl = "https://github.com/maty5302/DartsCounter/releases/latest/download/DartsCounter.dmg.sha256";

            try
            {
                await DownloadFileWithProgressAsync(downloadUrl, filePath, progress, token);

                bool isChecksumValid = await VerifyFileChecksumAsync(filePath, checksumUrl, token);
                if (!isChecksumValid)
                {
                    if (File.Exists(filePath)) File.Delete(filePath);
                    throw new InvalidOperationException("Chyba ověření integrity: Kontrolní součet SHA-256 neodpovídá.");
                }
                
                Process.Start(new ProcessStartInfo
                {
                    FileName = "open",
                    Arguments = $"\"{filePath}\"",
                    UseShellExecute = true
                });
                Process.GetCurrentProcess().Kill();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                if (File.Exists(filePath))
                {
                    try { File.Delete(filePath); } catch { }
                }

                await MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard(
                    "Error",
                    ex.Message, ButtonEnum.Ok, Icon.Error).ShowAsync();
            }
        }

        [RelayCommand]
        private void CancelDownload()
        {
            IsDownloading = false;
            
            _cancellationTokenSource?.Cancel();
            
            CloseAction?.Invoke();
        }

        [RelayCommand]
        private void ManualUpdate()
        {
            var psi = new ProcessStartInfo
            {
                FileName = "https://github.com/maty5302/DartsCounter/releases/latest",
                UseShellExecute = true
            };
            Process.Start(psi);
            CloseAction?.Invoke();
        }

        [RelayCommand]
        private void Close() => CloseAction?.Invoke();
    }
}