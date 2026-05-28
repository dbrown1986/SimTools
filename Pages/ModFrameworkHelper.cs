using System;
using System.IO;
using System.Linq;
using System.Windows;

using MessageBox = System.Windows.MessageBox;

namespace SimTools
{
    /// <summary>
    /// Shared helper for checking whether the SimTools Mod Framework is installed
    /// in a given Sims 3 Mods directory.  Used by MainWindow and GameplayFixesWindow.
    /// </summary>
    internal static class ModFrameworkHelper
    {
        // The specific line in Resource.cfg that identifies the SimTools framework
        private const string SignatureLine = "PackedFile SimTools/Packages/*.package";

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Returns true when:
        ///   1. <paramref name="modsPath"/> exists on disk, AND
        ///   2. Resource.cfg is present inside it, AND
        ///   3. Resource.cfg contains the SimTools signature line.
        /// </summary>
        public static bool IsInstalled(string modsPath)
        {
            if (string.IsNullOrWhiteSpace(modsPath) || !Directory.Exists(modsPath))
                return false;

            string cfgPath = Path.Combine(modsPath, "Resource.cfg");
            if (!File.Exists(cfgPath))
                return false;

            try
            {
                return File.ReadLines(cfgPath)
                    .Any(line => line.TrimEnd()
                        .Equals(SignatureLine, StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Shows a blocking warning dialog and returns false if the framework is
        /// not installed.  Returns true when the framework is present (caller may
        /// proceed with the download).
        /// </summary>
        public static bool EnsureInstalled(string modsPath)
        {
            if (IsInstalled(modsPath)) return true;

            MessageBox.Show(
                "The SimTools Mod Framework does not appear to be installed, or " +
                "your Resource.cfg is missing the required SimTools package path.\n\n" +
                "Before downloading .package files, please install the Mod Framework " +
                "using the 'Mod Framework' button on the main window.\n\n" +
                "This ensures the game can find and load your packages correctly.",
                "SimTools — Mod Framework Required",
                MessageBoxButton.OK, MessageBoxImage.Warning);

            return false;
        }
    }
}
