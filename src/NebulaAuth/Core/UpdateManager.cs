using AutoUpdaterDotNET;
using NebulaAuth.Model;
using NebulaAuth.Model.Update;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Windows;

namespace NebulaAuth.Core;

public static class UpdateManager
{
    private const string BASE_URL =
        "https://raw.githubusercontent.com/achiez/NebulaAuth-Steam-Desktop-Authenticator-by-Achies/master/";
    private const string UPDATE_URL = BASE_URL + "NebulaAuth/update.xml";

    private const string CHANGELOG_BASE_URL = BASE_URL + "changelog/";

    private static readonly HttpClient HttpClient = new();
    private static bool _isManualCheck;

    public static bool HasPendingUpdate { get; private set; }
    public static event Action? PendingUpdateDetected;

    static UpdateManager()
    {
        AutoUpdater.CheckForUpdateEvent += HandleCheckForUpdateEvent;
    }

    public static void CheckForUpdates(bool manual = false)
    {
        _isManualCheck = manual;
        var jsonPath = Path.Combine(Environment.CurrentDirectory, "update-settings.json");
        AutoUpdater.PersistenceProvider = new JsonFilePersistenceProvider(jsonPath);
        AutoUpdater.ShowSkipButton = false;
        AutoUpdater.RunUpdateAsAdmin = RequiresAdminAccess();
        AutoUpdater.Start(UPDATE_URL);
    }

    public static void SkipVersion(string version)
    {
        var settings = UpdateSettings.Load();
        settings.SkipVersion(version);
    }

    public static void SetRemindAfter(TimeSpan delay)
    {
        var settings = UpdateSettings.Load();
        settings.SetRemindAfter(DateTime.Now + delay);
    }

    private static async void HandleCheckForUpdateEvent(UpdateInfoEventArgs args)
    {
        try
        {
            var isManual = _isManualCheck;
            _isManualCheck = false;

            if (args.Error != null)
            {
                if (isManual)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                        SnackbarController.SendSnackbar(LocManager.GetCommonOrDefault("Request error", "RequestError")));
                }

                return;
            }

            if (!args.IsUpdateAvailable)
            {
                if (isManual)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                        SnackbarController.SendSnackbar(
                            LocManager.GetCodeBehindOrDefault("You are using the latest version", "UpdateService",
                                "LatestVersion")));
                }

                return;
            }

            var version = args.CurrentVersion.ToString();
            var settings = UpdateSettings.Load();

            if (!isManual && !settings.ShouldShow(version))
            {
                HasPendingUpdate = true;
                Application.Current.Dispatcher.Invoke(() => PendingUpdateDetected?.Invoke());
                return;
            }

            ChangelogEntry? changelog = null;
            try
            {
                var jsonUrl = $"{CHANGELOG_BASE_URL}{version}.json";
                var response = await HttpClient.GetAsync(jsonUrl).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    changelog = JsonConvert.DeserializeObject<ChangelogEntry>(json);
                }
            }
            catch
            {
                // fallback to HTML changelog URL
            }

            var htmlFallbackUrl = changelog == null ? args.ChangelogURL : null;

            await Application.Current.Dispatcher.BeginInvoke(async () =>
            {
                await DialogsController.ShowUpdateDialog(args, changelog, htmlFallbackUrl);
            });
        }
        catch (Exception e)
        {
            Shell.Logger.Error(e, "Error while checking updates");
        }
    }

    private static bool RequiresAdminAccess()
    {
        try
        {
            var testFile = Path.Combine(Environment.CurrentDirectory, "test.tmp");
            using (File.Create(testFile))
            {
            }

            File.Delete(testFile);
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return true;
        }
    }
}