﻿#region using directives

using Newtonsoft.Json.Linq;
using DraconiusGoGUI.UI;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Media;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

#endregion

namespace DraconiusGoGUI.Extensions
{
    public class VersionCheckState
    {
        public const string VersionUri = "https://raw.githubusercontent.com/Account-Managers/Account-Manager/draco_manager/DraconiusGoGUI/Properties/AssemblyInfo.cs";

        public const string RemoteReleaseUrl = "https://github.com/Account-Managers/Account-Manager/releases/download/v";

        public const string ChangelogUri = "https://github.com/Account-Managers/Account-Manager/blob/draco_manager/README.md#account-manager-draconiusgo-is-now-compatible-with-192-api";

        public static Version RemoteVersion;

        public static async Task<bool> Execute()
        {
            await CleanupOldFiles();

            var isLatest = await IsLatest();

            SystemSounds.Asterisk.Play();

            string zipName = $"DraconiusGoGUI.zip";
            var downloadLink = $"{RemoteReleaseUrl}{RemoteVersion}/{zipName}";
            var baseDir = Directory.GetCurrentDirectory();
            var downloadFilePath = Path.Combine(baseDir, zipName);
            var tempPath = Path.Combine(baseDir, "tmp");
            var extractedDir = Path.Combine(tempPath, "ACM");
            var destinationDir = baseDir + Path.DirectorySeparatorChar;
            bool updated = false;
            AutoUpdateForm autoUpdateForm = new AutoUpdateForm()
            {
                DownloadLink = downloadLink,
                ChangelogLink = ChangelogUri,
                Destination = downloadFilePath,
                AutoUpdate = true,
                CurrentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                LatestVersion = $"{RemoteVersion}"
            };

            updated = (autoUpdateForm.ShowDialog() == DialogResult.OK);

            if (!updated)
            {
                //Logger.Write("Update Skipped", LogLevel.Update);
                return false;
            }

            if (!UnpackFile(downloadFilePath, extractedDir))
                return false;

            if (!MoveAllFiles(extractedDir, destinationDir))
                return false;

            Process.Start(Assembly.GetEntryAssembly().Location);
            Environment.Exit(-1);
            return true;
        }

        public static async Task CleanupOldFiles()
        {
            var tmpDir = Path.Combine(Directory.GetCurrentDirectory(), "tmp");

            if (Directory.Exists(tmpDir))
            {
                Directory.Delete(tmpDir, true);
            }

            var di = new DirectoryInfo(Directory.GetCurrentDirectory());
            var files = di.GetFiles("*.old", SearchOption.AllDirectories);
            await Task.Delay(200);
        }
        
        private async static Task<string> DownloadServerVersion()
        {
            using (HttpClient client = new HttpClient())
            {
                var responseContent = await client.GetAsync(VersionUri);
                return await responseContent.Content.ReadAsStringAsync();
            }
        }

        private static JObject GetJObject(string filePath)
        {
            return JObject.Parse(File.ReadAllText(filePath));
        }

        public static async Task<bool> IsLatest()
        {
            try
            {
                var regex = new Regex(@"\[assembly\: AssemblyVersion\(""(\d{1,})\.(\d{1,})\.(\d{1,})\.(\d{1,})""\)\]");
                var match = regex.Match(await DownloadServerVersion());

                if (!match.Success)
                    return false;

                var gitVersion = new Version($"{match.Groups[1]}.{match.Groups[2]}.{match.Groups[3]}.{match.Groups[4]}");
                RemoteVersion = gitVersion;
                if (gitVersion > Assembly.GetExecutingAssembly().GetName().Version)
                    return false;
            }
            catch (Exception)
            {
                return true; //better than just doing nothing when git server down
            }
            return true;
        }

        public static bool MoveAllFiles(string sourceFolder, string destFolder)
        {
            if (!Directory.Exists(destFolder))
                Directory.CreateDirectory(destFolder);

            var oldfiles = Directory.GetFiles(destFolder);
            foreach (var old in oldfiles)
            {
                if (old.Contains("data.json.gz") || old.Contains("chromedriver.exe") || old.Contains("HashKeys.txt")) continue;
                if (File.Exists(old + ".old")) continue;
                File.Move(old, old + ".old");
            }

            try
            {
                var files = Directory.GetFiles(sourceFolder);
                foreach (var file in files)
                {
                    var name = Path.GetFileName(file);
                    var dest = Path.Combine(destFolder, name);
                    try {
                        File.Copy(file, dest, true);
                    }
                    catch(Exception )
                    {
                        //Logger.Write($"Error occurred while copy {file}, This seem like chromedriver.exe is being locked, you need manually copy after you close all chrome instance or ignore it");
                    }
                }

                var folders = Directory.GetDirectories(sourceFolder);

                foreach (var folder in folders)
                {
                    var name = Path.GetFileName(folder);
                    if (name == null) continue;
                    var dest = Path.Combine(destFolder, name);
                    MoveAllFiles(folder, dest);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static bool UnpackFile(string sourceTarget, string destPath)
        {
            var source = sourceTarget;
            var dest = destPath;
            try
            {
                ZipFile.ExtractToDirectory(source, dest);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
