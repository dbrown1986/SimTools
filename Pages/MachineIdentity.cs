using System;
using Microsoft.Win32;

namespace SimTools
{
    /// <summary>
    /// Retrieves a stable, unique identifier for the current Windows installation.
    ///
    /// The MachineGuid is written to the registry by Windows Setup and does not
    /// change unless the OS is reinstalled.  It is used to bind the donor token
    /// file to this specific machine so that copying both SimTools.ini and
    /// SimTools.token to another PC fails the machine-identity check.
    /// </summary>
    internal static class MachineIdentity
    {
        private const string RegPath = @"SOFTWARE\Microsoft\Cryptography";
        private const string RegKey  = "MachineGuid";

        /// <summary>
        /// Returns the Windows MachineGuid string, or <see cref="string.Empty"/>
        /// if it cannot be read (restricted environment, etc.).
        /// </summary>
        public static string GetMachineGuid()
        {
            try
            {
                using RegistryKey? key = Registry.LocalMachine.OpenSubKey(RegPath);
                return key?.GetValue(RegKey)?.ToString() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
