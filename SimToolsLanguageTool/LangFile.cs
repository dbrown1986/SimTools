using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class LangFile
{
    // Dictionary<SectionName, Dictionary<Key, Value>>
    public Dictionary<string, Dictionary<string, string>> Data { get; private set; }
        = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

    public void Load(string filePath)
    {
        Data.Clear();
        if (!File.Exists(filePath)) return;

        string currentSection = "Default";
        string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);

        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith(';') || trimmed.StartsWith('#')) continue;

            if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
            {
                currentSection = trimmed.Substring(1, trimmed.Length - 2).Trim();
                if (!Data.ContainsKey(currentSection))
                {
                    Data[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }
            }
            else if (trimmed.Contains("="))
            {
                int idx = trimmed.IndexOf('=');
                string key = trimmed.Substring(0, idx).Trim();
                string value = trimmed.Substring(idx + 1).Trim();

                if (!Data.ContainsKey(currentSection))
                {
                    Data[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }
                Data[currentSection][key] = value;
            }
        }
    }

    public void Save(string filePath)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var section in Data)
        {
            sb.AppendLine($"[{section.Key}]");
            foreach (var kvp in section.Value)
            {
                sb.AppendLine($"{kvp.Key}={kvp.Value}");
            }
            sb.AppendLine(); // Empty line between sections
        }
        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
    }
}