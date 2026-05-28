using System;
using System.Runtime.Versioning;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace SimTools
{
    /// <summary>
    /// Probes common install locations and user-data folders to auto-populate
    /// game and mod directories in Settings. Detection is opportunistic — the
    /// first matching path wins for each game. Fields already set by the user
    /// are never overwritten by the caller.
    /// </summary>
    internal static class GamePathDetector
    {
        // ── Result type ──────────────────────────────────────────────────────────
        public sealed record DetectionResult(string? GamePath, string? ModPath);

        // ── Well-known root paths ────────────────────────────────────────────────
        private static readonly string PF86 =
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        private static readonly string PF =
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        private static readonly string Docs =
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        // ── Steam ────────────────────────────────────────────────────────────────
        private static string? _steamCommon;

        [SupportedOSPlatform("windows")]
        private static string? SteamCommon()
        {
            if (_steamCommon is not null) return _steamCommon;
            try
            {
                // Primary: registry
                var steamRoot =
                    Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam",
                                      "SteamPath", null)?.ToString()?.Replace('/', '\\');

                // Fallback: common install locations
                if (string.IsNullOrEmpty(steamRoot) || !Directory.Exists(steamRoot))
                {
                    steamRoot = FindFirst(new[]
                    {
                        Path.Combine(PF86, "Steam"),
                        Path.Combine(PF,   "Steam"),
                        @"C:\Steam",
                    });
                }

                if (steamRoot is not null)
                {
                    var common = Path.Combine(steamRoot, "steamapps", "common");
                    if (Directory.Exists(common))
                        _steamCommon = common;
                }
            }
            catch { /* registry access denied — skip */ }
            return _steamCommon;
        }

        // ── GOG ──────────────────────────────────────────────────────────────────
        private static string? _gogRoot;

        private static string? GogRoot()
        {
            if (_gogRoot is not null) return _gogRoot;
            try
            {
                // Try common GOG Games directories
                _gogRoot = FindFirst(new[]
                {
                    @"C:\GOG Games",
                    Path.Combine(PF86, "GOG Games"),
                    Path.Combine(PF,   "GOG Games"),
                });
            }
            catch { }
            return _gogRoot;
        }

        // ── EA / Origin root directories ─────────────────────────────────────────
        //   Yields every vendor subfolder that actually exists on this machine.
        private static IEnumerable<string> EaRoots()
        {
            foreach (var pf in new[] { PF86, PF })
            foreach (var vendor in new[] { "EA Games", "Electronic Arts", "Origin Games" })
            {
                var p = Path.Combine(pf, vendor);
                if (Directory.Exists(p)) yield return p;
            }
        }

        // ── Maxis root directories ────────────────────────────────────────────────
        private static IEnumerable<string> MaxisRoots()
        {
            foreach (var pf in new[] { PF86, PF })
            {
                var p = Path.Combine(pf, "Maxis");
                if (Directory.Exists(p)) yield return p;
            }
        }

        // ── Helper: first existing path ───────────────────────────────────────────
        private static string? FindFirst(IEnumerable<string> candidates)
            => candidates.FirstOrDefault(Directory.Exists);

        // ── Game detectors ────────────────────────────────────────────────────────

        private static DetectionResult DetectSims1() => new(
            GamePath: FindFirst(
                MaxisRoots().Select(r => Path.Combine(r, "The Sims"))
                .Concat(EaRoots().Select(r => Path.Combine(r, "The Sims")))),
            ModPath: null);

        private static DetectionResult DetectSims2()
        {
            var game = FindFirst(
                EaRoots().SelectMany(r => new[]
                {
                    Path.Combine(r, "The Sims 2"),
                    Path.Combine(r, "The Sims 2 Legacy Collection"),
                    Path.Combine(r, "The Sims 2 Ultimate Collection"),
                }));

            var mods = FindFirst(new[]
            {
                Path.Combine(Docs, "EA Games", "The Sims 2", "Downloads"),
                Path.Combine(Docs, "EA Games", "The Sims 2 Legacy Collection", "Downloads"),
                Path.Combine(Docs, "EA Games", "The Sims 2 Ultimate Collection", "Downloads"),
            });

            return new DetectionResult(game, mods);
        }

        private static DetectionResult DetectSimsLifeStories() => new(
            GamePath: FindFirst(EaRoots().Select(r => Path.Combine(r, "The Sims Life Stories"))),
            ModPath:  FindFirst(new[]
            {
                Path.Combine(Docs, "EA Games", "The Sims Life Stories", "Collections"),
            }));

        private static DetectionResult DetectSimsPetStories() => new(
            GamePath: FindFirst(EaRoots().Select(r => Path.Combine(r, "The Sims Pet Stories"))),
            ModPath:  FindFirst(new[]
            {
                Path.Combine(Docs, "EA Games", "The Sims Pet Stories", "Collections"),
            }));

        private static DetectionResult DetectSimsCastawayStories() => new(
            GamePath: FindFirst(EaRoots().Select(r => Path.Combine(r, "The Sims Castaway Stories"))),
            ModPath:  FindFirst(new[]
            {
                Path.Combine(Docs, "EA Games", "The Sims Castaway Stories", "Collections"),
            }));

        private static DetectionResult DetectSims3()
        {
            var candidates = EaRoots().Select(r => Path.Combine(r, "The Sims 3"));
            var sc = SteamCommon();
            if (sc is not null)
                candidates = candidates.Append(Path.Combine(sc, "The Sims 3"));

            return new DetectionResult(
                GamePath: FindFirst(candidates),
                ModPath:  FindFirst(new[]
                {
                    Path.Combine(Docs, "Electronic Arts", "The Sims 3", "Mods"),
                }));
        }

        private static DetectionResult DetectSims4()
        {
            var candidates = EaRoots().Select(r => Path.Combine(r, "The Sims 4"));
            var sc = SteamCommon();
            if (sc is not null)
                candidates = candidates.Append(Path.Combine(sc, "The Sims 4"));

            return new DetectionResult(
                GamePath: FindFirst(candidates),
                ModPath:  FindFirst(new[]
                {
                    Path.Combine(Docs, "Electronic Arts", "The Sims 4", "Mods"),
                }));
        }

        private static DetectionResult DetectSimsMedieval()
        {
            var candidates = EaRoots().Select(r => Path.Combine(r, "The Sims Medieval"));
            var sc = SteamCommon();
            if (sc is not null)
                candidates = candidates.Append(Path.Combine(sc, "The Sims Medieval"));

            return new DetectionResult(
                GamePath: FindFirst(candidates),
                ModPath:  FindFirst(new[]
                {
                    Path.Combine(Docs, "Electronic Arts", "The Sims Medieval", "Mods"),
                }));
        }

        private static DetectionResult DetectSimCopter() => new(
            GamePath: FindFirst(
                MaxisRoots().Select(r => Path.Combine(r, "SimCopter"))
                .Concat(EaRoots().Select(r => Path.Combine(r, "SimCopter")))),
            ModPath: null);

        private static DetectionResult DetectStreetsOfSimCity() => new(
            GamePath: FindFirst(
                MaxisRoots().Select(r => Path.Combine(r, "Streets of SimCity"))
                .Concat(EaRoots().Select(r => Path.Combine(r, "Streets of SimCity")))),
            ModPath: null);

        private static DetectionResult DetectSimCity2000()
        {
            var candidates =
                MaxisRoots().SelectMany(r => new[]
                {
                    Path.Combine(r, "Sim City 2000"),
                    Path.Combine(r, "SimCity 2000"),
                });

            var gog = GogRoot();
            if (gog is not null)
                candidates = candidates.Append(Path.Combine(gog, "SimCity 2000 Special Edition"));

            return new DetectionResult(FindFirst(candidates), null);
        }

        private static DetectionResult DetectSimCity3000() => new(
            GamePath: FindFirst(
                MaxisRoots().SelectMany(r => new[]
                {
                    Path.Combine(r, "SimCity 3000 Unlimited"),
                    Path.Combine(r, "SimCity 3000"),
                })),
            ModPath: null);

        private static DetectionResult DetectSimCity4()
        {
            var candidates =
                MaxisRoots().SelectMany(r => new[]
                {
                    Path.Combine(r, "SimCity 4 Deluxe"),
                    Path.Combine(r, "SimCity 4"),
                })
                .Concat(EaRoots().Select(r => Path.Combine(r, "SimCity 4 Deluxe")));

            var sc = SteamCommon();
            if (sc is not null)
                candidates = candidates.Append(Path.Combine(sc, "SimCity 4 Deluxe Edition"));

            var gog = GogRoot();
            if (gog is not null)
                candidates = candidates.Append(Path.Combine(gog, "SimCity 4 Deluxe Edition"));

            return new DetectionResult(FindFirst(candidates), null);
        }

        private static DetectionResult DetectSimCity2013()
        {
            var candidates = EaRoots().Select(r => Path.Combine(r, "SimCity"))
                .Concat(new[]
                {
                    Path.Combine(PF86, "Origin Games", "SimCity"),
                    Path.Combine(PF,   "Origin Games", "SimCity"),
                });

            return new DetectionResult(FindFirst(candidates), null);
        }

        // ── Public entry point ───────────────────────────────────────────────────
        /// <summary>
        /// Probes all supported games and returns the best match found for each.
        /// Null values mean the path could not be determined automatically.
        /// </summary>
        [SupportedOSPlatform("windows")]
        public static Dictionary<string, DetectionResult> DetectAll() => new()
        {
            ["Sims1"]               = DetectSims1(),
            ["Sims2"]               = DetectSims2(),
            ["SimsLifeStories"]     = DetectSimsLifeStories(),
            ["SimsPetStories"]      = DetectSimsPetStories(),
            ["SimsCastawayStories"] = DetectSimsCastawayStories(),
            ["Sims3"]               = DetectSims3(),
            ["Sims4"]               = DetectSims4(),
            ["SimsMedieval"]        = DetectSimsMedieval(),
            ["SimCopter"]           = DetectSimCopter(),
            ["StreetsOfSimCity"]     = DetectStreetsOfSimCity(),
            ["SimCity2000"]         = DetectSimCity2000(),
            ["SimCity3000"]         = DetectSimCity3000(),
            ["SimCity4"]            = DetectSimCity4(),
            ["SimCity2013"]         = DetectSimCity2013(),
        };
    }
}
