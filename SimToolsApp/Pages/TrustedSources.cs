// SimTools
// Main Application
// Trusted Sources for RepoValidator
// (C) Archeon Industries, LLC. 2024 - 2026, All Rights Reserved.

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
        public static readonly string[] Mirrors =
        {
            "us1-repo.simtools-app.com", // US West SimTools repository domain
            "us2-repo.simtools-app.com", // US East SimTools repository domain
            "de1-repo.simtools-app.com", // Frankfurt SimTools repository domain
            "jp1-repo.simtools-app.com", // Japan SimTools repository domain
            "nl1-repo.simtools-app.com", // Netherlands SimTools repository domain
            "sg1-repo.simtools-app.com", // Singapore SimTools repository domain
            "localhost", // Localhost (for repo caching with RepoMaker)
        };

        /// <summary>
        /// Trusted IPv4/IPv6 addresses for the above domains.
        /// Add all IPs your server may present (primary, CDN, failover, etc.).
        /// </summary>
        public static readonly string[] IPs =
        {
            "194.238.26.127", // US West SimTools Repo IP
            "149.28.230.105", // US East SimTools Repo IP
            "45.32.158.78", // Frankfurt SimTools Repo IP
            "45.76.99.6", // Japan SimTools Repo IP
            "136.244.108.197", // Netherlands SimTools Repo IP
            "45.77.249.76", // Singapore SimTools Repo IP
            "127.0.0.1" // Localhost (for repo caching with RepoMaker)
        };
    }
}

// Note: This class is used by RepoValidator to determine if a repository URL is trusted.
// To update trusted sources, edit the Domains and IPs arrays and rebuild the assembly.

// GitHub IPs can change frequently due to their global infrastructure, so consider using domain names for GitHub resources instead of hardcoding IPs.
