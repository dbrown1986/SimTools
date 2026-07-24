// SimTools
// Main Application
// SimTools Game Path Configuration Variables
// (C) Archeon Industries, LLC. 2024 - 2026, All Rights Reserved.

using System.IO;

namespace SimTools
{
    /// <summary>
    /// Centralised, project-wide access to every game and mods directory
    /// configured by the user in Settings. All properties read live from
    /// SimTools.ini via IniHelper — no caching, always up to date.
    ///
    /// Usage:
    ///   string modsFolder = GamePaths.Sims3Mods;
    ///   if (GamePaths.IsConfigured(GamePaths.Sims3Mods)) { ... }
    ///
    /// Download helper:
    ///   string dest = GamePaths.Resolve(GamePaths.Sims3Mods, "subfolder", "file.package");
    /// </summary>
    public static class GamePaths
    {
        // ── Internal read helpers ──────────────────────────────────────────────
        private static string Game(string key) =>
            IniHelper.Read("Directories", $"{key}_Game", "");

        private static string Mods(string key) =>
            IniHelper.Read("Directories", $"{key}_Mods", "");

        private static string UserData(string key) =>
            IniHelper.Read("Directories", $"{key}_UserData", "");

        // ── The Sims 1 ────────────────────────────────────────────────────────
        public static string Sims1Game               => Game("Sims1");

        // ── The Sims 2 ────────────────────────────────────────────────────────
        public static string Sims2Game               => Game("Sims2");
        public static string Sims2Mods               => Mods("Sims2");
        public static string Sims2UserData           => UserData("Sims2");

        // ── The Sims Life Stories ─────────────────────────────────────────────
        public static string SimsLifeStoriesGame     => Game("SimsLifeStories");
        public static string SimsLifeStoriesMods     => Mods("SimsLifeStories");
        public static string SimsLifeStoriesUserData => UserData("SimsLifeStories");

        // ── The Sims Pet Stories ──────────────────────────────────────────────
        public static string SimsPetStoriesGame      => Game("SimsPetStories");
        public static string SimsPetStoriesMods      => Mods("SimsPetStories");
        public static string SimsPetStoriesUserData => UserData("SimsPetStories");

        // ── The Sims Castaway Stories ─────────────────────────────────────────
        public static string SimsCastawayStoriesGame => Game("SimsCastawayStories");
        public static string SimsCastawayStoriesMods => Mods("SimsCastawayStories");
        public static string SimsCastawayStoriesUserData => UserData("SimsCastawayStories");

        // ── The Sims 3 ────────────────────────────────────────────────────────
        public static string Sims3Game               => Game("Sims3");
        public static string Sims3UserData           => UserData("Sims3");
        public static string Sims3Mods               => Mods("Sims3");

        // ── The Sims 4 ────────────────────────────────────────────────────────
        public static string Sims4Game               => Game("Sims4");
        public static string Sims4Mods               => Mods("Sims4");

        // ── The Sims Medieval ─────────────────────────────────────────────────
        public static string SimsMedievalGame        => Game("SimsMedieval");
        public static string SimsMedievalMods        => Mods("SimsMedieval");

        // ── SimCopter ─────────────────────────────────────────────────────────
        public static string SimCopterGame           => Game("SimCopter");

        // ── SimTower ────────────────────────────────────────────────
        public static string SimTowerGame => Game("SimTower");

        // ── Streets of SimCity ────────────────────────────────────────────────
        public static string StreetsOfSimCityGame    => Game("StreetsOfSimCity");

        // ── SimCity 2000 ──────────────────────────────────────────────────────
        public static string SimCity2000Game         => Game("SimCity2000");

        // ── SimCity 3000 ────────────────────────────────────────────
        public static string SimCity3000Game => Game("SimCity3000");

        // ── SimCity 3000 Unlimited ────────────────────────────────────────────
        public static string SimCity3000UnlimitedGame => Game("SimCity3000U");

        // ── SimCity 4 Deluxe ──────────────────────────────────────────────────
        public static string SimCity4Game => Game("SimCity4");

        // ── SimCity 4 Rush Hour ──────────────────────────────────────────────────
        public static string SimCity4RHGame => Game("SimCity4RH");

        // ── SimCity 4 Deluxe ──────────────────────────────────────────────────
        public static string SimCity4DeluxeGame            => Game("SimCity4Deluxe");

        // ── SimCity (2013) ────────────────────────────────────────────────────
        public static string SimCity2013Game         => Game("SimCity2013");

        // ── Utility helpers ───────────────────────────────────────────────────

        /// <summary>
        /// Returns true if the given path is non-empty and exists on disk.
        /// Use this before attempting to download or write to a game/mods folder.
        /// </summary>
        public static bool IsConfigured(string path) =>
            !string.IsNullOrWhiteSpace(path) && Directory.Exists(path);

        /// <summary>
        /// Combines a base path with optional sub-path segments and a filename,
        /// creating any missing directories along the way.
        /// Returns the full resolved file path.
        ///
        /// Example:
        ///   GamePaths.Resolve(GamePaths.Sims3Mods, "Mods", "Packages", "fix.package")
        ///   → C:\...\Mods\Packages\fix.package  (directory created if missing)
        /// </summary>
        public static string Resolve(string basePath, params string[] segments)
        {
            var combinedList = new List<string> { basePath };
            combinedList.AddRange(segments);
            var full = Path.Combine(combinedList.ToArray());
            var dir  = Path.GetExtension(segments[^1]) != ""
                ? Path.GetDirectoryName(full)!
                : full;
            Directory.CreateDirectory(dir);
            return full;
        }
    }
}
