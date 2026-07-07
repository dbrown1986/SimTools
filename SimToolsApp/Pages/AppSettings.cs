// SimTools
// Main Application
// Project-wide (Global) App Settings
// (C) Archeon Industries, LLC. 2024 - 2026, All Rights Reserved.    

namespace SimTools
{
    /// <summary>
    /// Project-wide application settings that don't belong to a specific
    /// domain (game paths, language, etc.).
    ///
    /// Currently exposes:
    ///   BaseUrl  — the root URL used for all %baseurl% download placeholders.
    ///              Reads live from SimTools.ini so changes in Settings take
    ///              effect on the next download without restarting.
    ///
    /// Usage in a download URL:
    ///   url: AppSettings.ResolveUrl("%baseurl%/bin/x86/TS3_GPU_Addon.exe")
    ///   → "https://repo.ts3tools.com/bin/x86/TS3_GPU_Addon.exe"
    /// </summary>
    public static class AppSettings
    {
        /// <summary>Default base URL shown in Settings and used when no override is saved.</summary>
        public const string DefaultBaseUrl = "us1-repo.simtools-app.com";

        /// <summary>
        /// The configured base URL, read live from SimTools.ini.
        /// Always returns a value with an https:// prefix and no trailing slash.
        /// </summary>
        public static string BaseUrl
        {
            get
            {
                var raw = IniHelper.Read("Network", "BaseUrl", DefaultBaseUrl)
                                   .Trim()
                                   .TrimEnd('/');

                if (string.IsNullOrWhiteSpace(raw))
                    raw = DefaultBaseUrl;

                // If explicitly pointing to a local instance, default to http:// if no scheme is specified
                if (!raw.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !raw.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    if (raw.Equals("127.0.0.1") || raw.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                    {
                        raw = "http://" + raw;
                    }
                    else
                    {
                        raw = "https://" + raw;
                    }
                }

                return raw;
            }
        }

        /// <summary>
        /// Replaces every occurrence of %baseurl% in a URL string with
        /// the currently configured BaseUrl value.
        /// </summary>
        public static string ResolveUrl(string url) =>
            url.Replace("%baseurl%", BaseUrl, StringComparison.OrdinalIgnoreCase);
    }
}
