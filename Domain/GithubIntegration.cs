using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Domain
{
    public sealed class GitHubReleaseDto
    {
        [JsonPropertyName("tag_name")]
        public string? TagName { get; set; }

        [JsonPropertyName("body")]
        public string? Body { get; set; }
    }

    public static class GithubIntegration
    {
        private static readonly HttpClient _httpClient = CreateHttpClient();

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.UserAgent.TryParseAdd("DartsCounter-App");
            return client;
        }

        public static async Task<string> GetLatestRelease()
        {
            try
            {
                const string apiUrl = "https://api.github.com/repos/maty5302/DartsCounter/releases/latest";
                using var response = await _httpClient.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode)
                {
                    return string.Empty;
                }

                string responseJson = await response.Content.ReadAsStringAsync();
                var release = JsonSerializer.Deserialize<GitHubReleaseDto>(responseJson);

                return (release?.TagName ?? "") + "\n" + (release?.Body ?? "");
            }
            catch
            {
                return string.Empty;
            }
        }

        public static async Task<string> GetGitVersion()
        {
            try
            {
                const string apiUrl = "https://api.github.com/repos/maty5302/DartsCounter/releases/latest";
                using var response = await _httpClient.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode)
                {
                    return string.Empty;
                }

                string responseJson = await response.Content.ReadAsStringAsync();
                var release = JsonSerializer.Deserialize<GitHubReleaseDto>(responseJson);

                return release?.TagName ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
        
        public static async Task<string> GetReleaseNotes(string version)
        {
            try
            {
                string encodedVersion = Uri.EscapeDataString(version);
                string apiUrl = $"https://api.github.com/repos/maty5302/DartsCounter/releases/tags/{encodedVersion}";

                using var response = await _httpClient.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode)
                {
                    return string.Empty;
                }

                string responseJson = await response.Content.ReadAsStringAsync();
                var release = JsonSerializer.Deserialize<GitHubReleaseDto>(responseJson);

                return release?.Body ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static async Task<bool> CheckForUpdates()
        {
            try
            {
                var gitVersionString = await GetGitVersion();
                var appVersionString = Assembly.GetEntryAssembly()?.GetName().Version?.ToString();

                if (string.IsNullOrEmpty(gitVersionString) || string.IsNullOrEmpty(appVersionString))
                    return false;

                gitVersionString = gitVersionString.Replace("v", "", StringComparison.OrdinalIgnoreCase)
                    .Replace("beta", "", StringComparison.OrdinalIgnoreCase);

                int indexof = appVersionString.IndexOf('+');
                if (indexof != -1)
                    appVersionString = appVersionString.Remove(indexof);

                if (Version.TryParse(gitVersionString, out var gitVersion) && 
                    Version.TryParse(appVersionString, out var appVersion))
                {
                    return gitVersion > appVersion;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
