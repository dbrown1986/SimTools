using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SimTools
{
    /// <summary>
    /// Validates that the configured %baseurl% resolves to a trusted domain and IP,
    /// and that the local hosts file has not redirected it to an unexpected address.
    ///
    /// Trusted sources are read from TrustedSources.cs during assembly. New mirrors should be
    /// added there and included in the build process. Warnings will continue until the user
    /// updates to a version of the app with the new trusted source included.
    /// 
    /// All checks are advisory — failures generate a warning dialog but never blocks downloads.
    ///
    /// Validation runs at most once per domain per application session (cached).
    /// Call InvalidateCache() if the user changes the BaseUrl in Settings.
    /// </summary>
    public static class RepoValidator
    {
        // ── Validation report ─────────────────────────────────────────────────

        public sealed record ValidationReport(
            string   Domain,
            bool     DomainTrusted,
            bool     IPTrusted,
            bool     HostsFileClean,
            string[] ResolvedIPs,
            string?  HostsOverrideIP)
        {
            /// <summary>True if any check found a potential issue.</summary>
            public bool HasWarnings => !DomainTrusted || !IPTrusted || !HostsFileClean;
        }

        // ── Session cache ─────────────────────────────────────────────────────
        // Tracks domains for which a warning has already been shown this session.

        private static readonly HashSet<string> _warnedDomains = new(StringComparer.OrdinalIgnoreCase);

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Validates the current AppSettings.BaseUrl and shows a warning dialog if any
        /// check fails. Shows at most one warning per domain per session.
        /// Safe to call from any thread — dialog is marshalled to the UI thread.
        /// </summary>
        public static async Task ValidateAndWarnAsync()
        {
            string baseUrl = AppSettings.BaseUrl;

            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out Uri? uri))
                return;

            string domain = uri.Host.ToLowerInvariant();

            // Already warned about this domain this session — skip
            if (_warnedDomains.Contains(domain))
                return;

            // Mark domain as seen regardless of result (prevents duplicate dialogs
            // if validation is called multiple times concurrently before it completes)
            _warnedDomains.Add(domain);

            ValidationReport report;
            try
            {
                report = await RunChecksAsync(baseUrl, uri);
            }
            catch
            {
                // Validation errors are non-fatal — never block the application
                return;
            }

            if (report.HasWarnings)
            {
                App.Current.Dispatcher.Invoke(() => ShowWarningDialog(report));
            }
        }

        /// <summary>
        /// Clears the session cache so the next network call re-validates from scratch.
        /// Call this when the user saves a new BaseUrl in Settings.
        /// </summary>
        public static void InvalidateCache() => _warnedDomains.Clear();

        // ── Core validation ───────────────────────────────────────────────────

        private static async Task<ValidationReport> RunChecksAsync(string baseUrl, Uri uri)
        {
            var (domains, ips) = LoadTrustedSources();
            string domain = uri.Host;

            // 1. Domain check — is the hostname (or its parent domain) trusted?
            bool domainTrusted = IsDomainTrusted(domain, domains);

            // 2. DNS → IP check — does the domain resolve to a trusted IP?
            string[] resolvedIPs = await ResolveIPsAsync(domain);
            bool ipTrusted = resolvedIPs.Any(ip =>
                ips.Any(t => t.Equals(ip, StringComparison.OrdinalIgnoreCase)));

            // 3. Hosts file check — is the domain being silently redirected locally?
            string? hostsOverrideIP = GetHostsFileOverride(domain);
            bool hostsClean = hostsOverrideIP is null ||
                ips.Any(t => t.Equals(hostsOverrideIP, StringComparison.OrdinalIgnoreCase));

            return new ValidationReport(domain, domainTrusted, ipTrusted, hostsClean,
                                        resolvedIPs, hostsOverrideIP);
        }

        // ── Domain check ──────────────────────────────────────────────────────

        private static bool IsDomainTrusted(string domain, string[] trustedDomains)
        {
            foreach (string trusted in trustedDomains)
            {
                // Exact match  (e.g. "repo.simtools-app.com" == "repo.simtools-app.com")
                if (domain.Equals(trusted, StringComparison.OrdinalIgnoreCase))
                    return true;

                // Subdomain match  (e.g. "sub.simtools-app.com" ends with ".simtools-app.com")
                if (domain.EndsWith("." + trusted, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        // ── DNS resolution ────────────────────────────────────────────────────

        private static async Task<string[]> ResolveIPsAsync(string domain)
        {
            try
            {
                IPAddress[] addresses = await Dns.GetHostAddressesAsync(domain);
                return addresses.Select(a => a.ToString()).ToArray();
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        // ── Local hosts file check ────────────────────────────────────────────
        //
        //  Parses C:\Windows\System32\drivers\etc\hosts looking for lines that
        //  map the repo domain to a specific IP address.  If an override is found
        //  and that IP is not in the trusted list, HostsFileClean = false.
        //
        //  Note: Router-level DNS overrides (dnsmasq / Pi-hole / etc.) are not
        //  detectable from user-space without credentials — out of scope.

        private static string? GetHostsFileOverride(string domain)
        {
            string hostsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.System),
                @"drivers\etc\hosts");

            if (!File.Exists(hostsPath))
                return null;

            try
            {
                foreach (string rawLine in File.ReadLines(hostsPath))
                {
                    string line = rawLine.Trim();

                    // Skip blank lines and full-line comments
                    if (line.Length == 0 || line.StartsWith('#'))
                        continue;

                    // Strip inline comments
                    int commentIdx = line.IndexOf('#');
                    if (commentIdx >= 0)
                        line = line[..commentIdx].Trim();

                    // Format: <ip-address>  <hostname>  [alias ...]
                    string[] parts = line.Split(
                        Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length < 2) continue;

                    string mappedIP = parts[0];

                    for (int i = 1; i < parts.Length; i++)
                    {
                        if (parts[i].Equals(domain, StringComparison.OrdinalIgnoreCase))
                            return mappedIP;   // Domain found — return the mapped IP
                    }
                }
            }
            catch
            {
                // Read failure (permissions, locked file) is non-fatal
            }

            return null;   // No override found
        }

        // ── Load trusted sources from compiled TrustedSources.cs ─────────────

        private static (string[] Domains, string[] IPs) LoadTrustedSources() =>
            (TrustedSources.Mirrors, TrustedSources.IPs);

        // ── Warning dialog ────────────────────────────────────────────────────

        private static void ShowWarningDialog(ValidationReport report)
        {
            var bullets = new StringBuilder();

            if (!report.DomainTrusted)
                bullets.AppendLine(LanguageManager.Format("Repo", "Domain_Untrusted",
                    report.Domain));

            if (!report.IPTrusted)
            {
                string ips = report.ResolvedIPs.Length > 0
                    ? string.Join(", ", report.ResolvedIPs)
                    : "(unable to resolve)";
                bullets.AppendLine(LanguageManager.Format("Repo", "IP_Untrusted", ips));
            }

            if (!report.HostsFileClean)
                bullets.AppendLine(LanguageManager.Format("Repo", "Hosts_Tampered",
                    report.Domain, report.HostsOverrideIP ?? ""));

            string body = LanguageManager.Format("Repo", "Warning",
                report.Domain, bullets.ToString());

            System.Windows.MessageBox.Show(
                body,
                LanguageManager.Get("Repo", "Warning_Title", "Untrusted Repository Source"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }
}
