using System;
using System.Collections.Generic;
using System.IO;

namespace SimTools_v4
{
    public static class LanguageManager
    {
        private static Dictionary<string, Dictionary<string, string>> _data
            = new(StringComparer.OrdinalIgnoreCase);

        // ── Load from the language code stored in settings.ini ─────────────────
        public static void Load()
        {
            var code = IniHelper.Read("Language", "SelectedLanguage", "en");
            LoadCode(code);
        }

        // ── Load a specific language code (called from SettingsWindow on save) ─
        public static void LoadCode(string langCode)
        {
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Languages");
            var path = Path.Combine(dir, $"{langCode}.lang");

            if (!File.Exists(path))
                path = Path.Combine(dir, "en.lang");  // fall back to English

            _data = File.Exists(path) ? Parse(path) : new();
        }

        // ── Get a string by section + key, returning fallback if missing ────────
        public static string Get(string section, string key, string fallback = "")
        {
            if (_data.TryGetValue(section, out var sec) && sec.TryGetValue(key, out var val))
                return val;
            return fallback;
        }

        // ── Get a string and substitute {0}, {1} … placeholders ────────────────
        public static string Format(string section, string key, params object[] args)
        {
            var template = Get(section, key);
            if (string.IsNullOrEmpty(template)) return string.Join(" ", args);
            try { return string.Format(template, args); }
            catch { return template; }
        }

        // ── Internal INI-style parser ───────────────────────────────────────────
        private static Dictionary<string, Dictionary<string, string>> Parse(string path)
        {
            var data = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            string? sec = null;

            foreach (var raw in File.ReadLines(path))
            {
                var line = raw.Trim();
                if (line.Length == 0 || line.StartsWith(';') || line.StartsWith('#')) continue;

                if (line.StartsWith('[') && line.EndsWith(']'))
                {
                    sec = line[1..^1].Trim();
                    if (!data.ContainsKey(sec))
                        data[sec] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    continue;
                }

                if (sec is null) continue;
                var eq = line.IndexOf('=');
                if (eq < 0) continue;
                data[sec][line[..eq].Trim()] = line[(eq + 1)..].Trim();
            }
            return data;
        }
    }
}