// SimTools
// Conversion Services
// Katy Perry's Sweet Treats Registry Conversion Manager
// (C) Archeon Industries, LLC. 2024 - 2026, All Rights Reserved.

using Microsoft.Win32;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace SimTools
{
    /// <summary>
    /// Manages the conversion of Katy Perry's Sweet Treats from Retail/Origin to Steam format
    /// by configuring the necessary registry entries for Steam to recognize the installation.
    /// </summary>
    public class SimsRegistryManager
    {
        // Constants based on the guides
        private const string SteamRegPath = @"SOFTWARE\WOW6432Node\Sims(Steam)";
        private const string EaRegPath = @"SOFTWARE\WOW6432Node\Electronic Arts\Sims(Steam)";
        private const string PackName = "The Sims 3 Katy Perry Sweet Treats";

        // Common installation paths to check
        private readonly string[] _commonInstallPaths = new[]
        {
            @"C:\Program Files (x86)\Steam\steamapps\common\The Sims 3",
            @"C:\Program Files\Steam\steamapps\common\The Sims 3",
            @"C:\Steam\steamapps\common\The Sims 3",
        };

        private const string SP6SubDir = "SP6";
        private const string RequiredExecutable = @"SP6\Game\Bin\TS3SP06.exe";

        /// <summary>
        /// Converts Katy Perry's Sweet Treats to Steam format by creating registry entries.
        /// Validates that required files exist before proceeding.
        /// </summary>
        /// <param name="steamInstallRoot">Root path to The Sims 3 Steam installation</param>
        /// <param name="ergcKey">Optional ERGC key for Origin/EA verification</param>
        /// <returns>Tuple containing success status and message</returns>
        public async Task<(bool Success, string Message)> ConvertToSteamAsync(string steamInstallRoot, string ergcKey = "")
        {
            return await Task.Run(() => ConvertToSteam(steamInstallRoot, ergcKey));
        }

        /// <summary>
        /// Attempts to locate the Steam installation directory by checking common paths.
        /// </summary>
        /// <returns>Valid Steam installation path, or empty string if not found</returns>
        public string FindSteamInstallation()
        {
            foreach (var path in _commonInstallPaths)
            {
                if (ValidateInstallation(path))
                {
                    return path;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Validates that the required Sweet Treats files exist in the specified directory.
        /// </summary>
        /// <param name="steamInstallRoot">Path to validate</param>
        /// <returns>True if all required files are present</returns>
        private bool ValidateInstallation(string steamInstallRoot)
        {
            if (string.IsNullOrWhiteSpace(steamInstallRoot) || !Directory.Exists(steamInstallRoot))
            {
                return false;
            }

            // Check if SP6 directory exists
            string sp6Path = Path.Combine(steamInstallRoot, SP6SubDir);
            if (!Directory.Exists(sp6Path))
            {
                return false;
            }

            // Check if the main executable exists
            string exePath = Path.Combine(steamInstallRoot, RequiredExecutable);
            if (!File.Exists(exePath))
            {
                return false;
            }

            return true;
        }

        private (bool Success, string Message) ConvertToSteam(string steamInstallRoot, string ergcKey = "")
        {
            try
            {
                // Validate that files have been copied to the expected location
                if (!ValidateInstallation(steamInstallRoot))
                {
                    return (false, $"Sweet Treats files not found in {steamInstallRoot}. Please ensure the SP6 directory and all files have been copied to the Steam installation directory.");
                }

                // 1. Determine the specific paths for this pack
                string installDir = Path.Combine(steamInstallRoot, "SP6\\");
                string exePath = Path.Combine(steamInstallRoot, RequiredExecutable);

                // 2. Sync with Base Game settings (Country/Locale)
                string country = GetBaseGameSetting("country", "US");
                string locale = GetBaseGameSetting("locale", "en-US");

                // 3. Create and edit the main Steam Registry Key
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey($@"{SteamRegPath}\{PackName}"))
                {
                    if (key != null)
                    {
                        // String Values
                        key.SetValue("contentid", "sims3_sp06_sku7");
                        key.SetValue("DisplayName", "The Sims 3 Katy Perry's Sweet Treats");
                        key.SetValue("country", country);
                        key.SetValue("locale", locale);
                        key.SetValue("install dir", installDir);
                        key.SetValue("exepath", exePath);
                        key.SetValue("ErgcRegPath", $@"{EaRegPath}\{PackName}\ergc");

                        // DWORD Values (Integers)
                        key.SetValue("productid", 13); // 0xd = 13
                        key.SetValue("sku", 7);
                        key.SetValue("telemetry", 0);
                    }
                }

                // 4. Handle the ERGC Key (Origin/EA App requirement)
                if (!string.IsNullOrEmpty(ergcKey))
                {
                    using (RegistryKey ergcBase = Registry.LocalMachine.CreateSubKey($@"{EaRegPath}\{PackName}"))
                    {
                        using (RegistryKey ergcVal = ergcBase.CreateSubKey("ergc"))
                        {
                            ergcVal.SetValue("", ergcKey); // Setting the (Default) value
                        }
                    }
                }

                return (true, "Registry successfully updated!");
            }
            catch (UnauthorizedAccessException)
            {
                return (false, "Error: You must run this application as Administrator.");
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}");
            }
        }

        private string GetBaseGameSetting(string valueName, string defaultValue)
        {
            try
            {
                using (RegistryKey? baseKey = Registry.LocalMachine.OpenSubKey($@"{SteamRegPath}\The Sims 3"))
                {
                    if (baseKey != null)
                    {
                        var val = baseKey.GetValue(valueName);
                        return val?.ToString() ?? defaultValue;
                    }
                }
            }
            catch { }
            return defaultValue;
        }
    }
}
