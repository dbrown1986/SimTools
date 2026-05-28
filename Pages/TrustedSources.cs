namespace SimTools
{
    /// <summary>
    /// Trusted repository domains and IP addresses used by RepoValidator.
    /// Compiled into the assembly — edit here and rebuild to update trusted sources.
    /// </summary>
    internal static class TrustedSources
    {
        /// <summary>
        /// Trusted hostnames. Subdomains are matched automatically
        /// (e.g. "repo.simtools-app.com" is covered by "simtools-app.com").
        /// </summary>
        public static readonly string[] Domains =
        [
            "simtools-app.com", // SimTools root domain (covers all subdomains)
            "repo.simtools-app.com", // SimTools repository domain (explicitly listed for clarity)
            "localhost", // Localhost (for repo caching with RepoMaker)
            "github.com" // GitHub (for potential future use, e.g. hosting repos or assets)
        ];

        /// <summary>
        /// Trusted IPv4/IPv6 addresses for the above domains.
        /// Add all IPs your server may present (primary, CDN, failover, etc.).
        /// </summary>
        public static readonly string[] IPs =
        [
            "194.238.26.127", // Primary SimTools Repo IP
            "140.82.116.4", // GitHub IP (for potential future use, e.g. hosting repos or assets)
            "127.0.0.1" // Localhost (for repo caching with RepoMaker)
        ];
    }
}

// Note: This class is used by RepoValidator to determine if a repository URL is trusted.
// To update trusted sources, edit the Domains and IPs arrays and rebuild the assembly.

// GitHub IPs can change frequently due to their global infrastructure, so consider using domain names for GitHub resources instead of hardcoding IPs.