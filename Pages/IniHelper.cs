using System;
using System.Collections.Generic;
using System.IO;

namespace SimTools_v4
{
    /// <summary>
    /// Lightweight INI reader/writer. Stored at: {AppDirectory}\settings.ini
    /// Supports [Section] / Key=Value format. Lines starting with ; or # are comments.
    /// </summary>
    public static class IniHelper
    {
        public static readonly string IniPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.ini");

        // ── Internal: load entire file into nested dictionary ──────────────────
        private static Dictionary<string, Dictionary<string, string>> LoadAll()
        {
            var data = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            if (!File.Exists(IniPath)) return data;

            string? section = null;
            foreach (var raw in File.ReadLines(IniPath))
            {
                var line = raw.Trim();
                if (line.Length == 0 || line.StartsWith(';') || line.StartsWith('#')) continue;

                if (line.StartsWith('[') && line.EndsWith(']'))
                {
                    section = line[1..^1].Trim();
                    if (!data.ContainsKey(section))
                        data[section] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    continue;
                }

                if (section is null) continue;
                var eq = line.IndexOf('=');
                if (eq < 0) continue;

                data[section][line[..eq].Trim()] = line[(eq + 1)..].Trim();
            }
            return data;
        }

        // ── Internal: write entire dictionary back to file ─────────────────────
        private static void SaveAll(Dictionary<string, Dictionary<string, string>> data)
        {
            var lines = new List<string>();
            foreach (var (section, keys) in data)
            {
                lines.Add($"[{section}]");
                foreach (var (k, v) in keys)
                    lines.Add($"{k}={v}");
                lines.Add(string.Empty);
            }
            File.WriteAllLines(IniPath, lines);
        }

        // ── Public API ─────────────────────────────────────────────────────────

        public static string Read(string section, string key, string defaultValue = "")
        {
            var data = LoadAll();
            return data.TryGetValue(section, out var sec) && sec.TryGetValue(key, out var val)
                ? val : defaultValue;
        }

        public static void Write(string section, string key, string value)
        {
            var data = LoadAll();
            if (!data.ContainsKey(section))
                data[section] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            data[section][key] = value;
            SaveAll(data);
        }

        public static bool ReadBool(string section, string key, bool defaultValue = false) =>
            Read(section, key, defaultValue.ToString())
                .Equals("true", StringComparison.OrdinalIgnoreCase);

        public static void WriteBool(string section, string key, bool value) =>
            Write(section, key, value ? "true" : "false");
    }
}