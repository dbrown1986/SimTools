using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SimTools
{
    /// <summary>
    /// Handles generation, validation, and machine-locked token management for
    /// donor personalisation keys.
    ///
    /// ── Donor key ────────────────────────────────────────────────────────────
    /// Keys are AES-128-CBC encrypted payloads encoding "FirstName|LastName".
    /// They are issued by KeyGeneratorWindow and validated here.
    /// The key itself is NEVER written to disk after validation — it exists only
    /// in memory for the duration of the unlock dialog.
    ///
    /// ── Machine-locked token file (SimTools.token) ───────────────────────────
    /// When a donor enters their key, WriteTokenFile() creates SimTools.token
    /// alongside SimTools.exe.  The token stores:
    ///
    ///     {MachineGuid}|{FirstName}|{LastName}
    ///
    /// encrypted with an AES-128 key derived from the machine's Windows
    /// MachineGuid via SHA-256.  Storing names (not the donor key) means there
    /// is nothing in the token that could be pasted into the unlock dialog on
    /// another machine.  Because the AES encryption key itself is derived from
    /// the MachineGuid, copying the token file to a different PC produces a
    /// different AES key, causing decryption to fail.  Even if decryption
    /// somehow succeeded, the embedded MachineGuid is verified as a second check.
    ///
    /// The donor key string is therefore never stored anywhere on disk.
    /// </summary>
    internal static class DonorKeyHelper
    {
        // ── Donor key AES-128 credentials ────────────────────────────────────
        // "SimToolsDonorKey"
        private static readonly byte[] _key =
        {
            0x53, 0x69, 0x6D, 0x54, 0x6F, 0x6F, 0x6C, 0x73,
            0x44, 0x6F, 0x6E, 0x6F, 0x72, 0x4B, 0x65, 0x79
        };

        // "ThankYouSupport!"
        private static readonly byte[] _iv =
        {
            0x54, 0x68, 0x61, 0x6E, 0x6B, 0x59, 0x6F, 0x75,
            0x53, 0x75, 0x70, 0x70, 0x6F, 0x72, 0x74, 0x21
        };

        private const string Sep = "|";

        // ── Token file path ───────────────────────────────────────────────────
        private static string TokenPath =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SimTools.token");

        // ─────────────────────────────────────────────────────────────────────
        //  DONOR KEY — Generate / Validate (in-memory only; never stored)
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Generates a donor key encoding <paramref name="firstName"/> and
        /// <paramref name="lastName"/> as URL-safe Base64.
        /// </summary>
        public static string GenerateKey(string firstName, string lastName)
        {
            byte[] plain = Encoding.UTF8.GetBytes(firstName.Trim() + Sep + lastName.Trim());

            using var aes = Aes.Create();
            aes.Key = _key; aes.IV = _iv;
            aes.Mode = CipherMode.CBC; aes.Padding = PaddingMode.PKCS7;

            using var enc = aes.CreateEncryptor();
            byte[] cipher = enc.TransformFinalBlock(plain, 0, plain.Length);

            return Convert.ToBase64String(cipher)
                          .Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }

        /// <summary>
        /// Attempts to decode a donor key.  Returns <c>true</c> and populates
        /// <paramref name="firstName"/> / <paramref name="lastName"/> on success.
        /// </summary>
        public static bool TryDecodeKey(string key,
                                        out string firstName,
                                        out string lastName)
        {
            firstName = string.Empty;
            lastName  = string.Empty;
            try
            {
                string b64 = key.Trim().Replace('-', '+').Replace('_', '/');
                int pad = b64.Length % 4;
                if (pad > 0) b64 += new string('=', 4 - pad);

                byte[] cipher = Convert.FromBase64String(b64);

                using var aes = Aes.Create();
                aes.Key = _key; aes.IV = _iv;
                aes.Mode = CipherMode.CBC; aes.Padding = PaddingMode.PKCS7;

                using var dec = aes.CreateDecryptor();
                string text = Encoding.UTF8.GetString(
                    dec.TransformFinalBlock(cipher, 0, cipher.Length));

                int sep = text.IndexOf(Sep, StringComparison.Ordinal);
                if (sep <= 0 || sep == text.Length - 1) return false;

                firstName = text[..sep];
                lastName  = text[(sep + 1)..];

                return !string.IsNullOrWhiteSpace(firstName) &&
                       !string.IsNullOrWhiteSpace(lastName);
            }
            catch { return false; }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  MACHINE-LOCKED TOKEN FILE
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Derives AES-128 Key+IV from the machine GUID via SHA-256 so the
        /// token file is undecryptable on any other machine.
        /// </summary>
        private static (byte[] Key, byte[] IV) DeriveTokenCredentials(string machineGuid)
        {
            using var sha  = SHA256.Create();
            byte[] gb      = Encoding.UTF8.GetBytes(machineGuid);
            byte[] keyHash = sha.ComputeHash(gb);
            byte[] ivHash  = sha.ComputeHash(Encoding.UTF8.GetBytes(machineGuid + "SimToolsToken"));
            return (keyHash[..16], ivHash[..16]);
        }

        /// <returns><c>true</c> if SimTools.token exists on disk.</returns>
        public static bool TokenFileExists() => File.Exists(TokenPath);

        /// <summary>
        /// Creates (or overwrites) the machine-locked token file.
        /// Stores the donor's <b>decoded names</b> — not the key — so there is
        /// nothing in the file that can be reused to unlock another machine.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the machine GUID cannot be read from the registry.
        /// </exception>
        public static void WriteTokenFile(string donorKey)
        {
            if (!TryDecodeKey(donorKey, out string firstName, out string lastName))
                throw new ArgumentException("Invalid donor key.", nameof(donorKey));

            string machineGuid = MachineIdentity.GetMachineGuid();
            if (string.IsNullOrWhiteSpace(machineGuid))
                throw new InvalidOperationException(
                    "Unable to read the machine identifier from the registry.");

            // Payload: "{MachineGuid}|{FirstName}|{LastName}"
            // The donor key itself is NOT included — it is discarded after validation.
            byte[] payload = Encoding.UTF8.GetBytes(
                machineGuid + Sep + firstName + Sep + lastName);

            var (aesKey, aesIv) = DeriveTokenCredentials(machineGuid);

            using var aes = Aes.Create();
            aes.Key = aesKey; aes.IV = aesIv;
            aes.Mode = CipherMode.CBC; aes.Padding = PaddingMode.PKCS7;

            using var enc = aes.CreateEncryptor();
            File.WriteAllBytes(TokenPath,
                enc.TransformFinalBlock(payload, 0, payload.Length));
        }

        /// <summary>
        /// Decrypts the token file on the current machine and returns the stored
        /// donor names.  Returns <c>false</c> for any failure: file missing,
        /// wrong machine (different AES key), tampered content, or mismatched GUID.
        /// </summary>
        public static bool TryReadTokenFile(out string firstName, out string lastName)
        {
            firstName = string.Empty;
            lastName  = string.Empty;

            if (!File.Exists(TokenPath)) return false;

            string machineGuid = MachineIdentity.GetMachineGuid();
            if (string.IsNullOrWhiteSpace(machineGuid)) return false;

            try
            {
                byte[] cipher = File.ReadAllBytes(TokenPath);
                var (aesKey, aesIv) = DeriveTokenCredentials(machineGuid);

                using var aes = Aes.Create();
                aes.Key = aesKey; aes.IV = aesIv;
                aes.Mode = CipherMode.CBC; aes.Padding = PaddingMode.PKCS7;

                using var dec = aes.CreateDecryptor();
                string text = Encoding.UTF8.GetString(
                    dec.TransformFinalBlock(cipher, 0, cipher.Length));

                // Expected: "{MachineGuid}|{FirstName}|{LastName}"
                // Split on first two pipes only (last name may theoretically contain one)
                int pipe1 = text.IndexOf(Sep, StringComparison.Ordinal);
                if (pipe1 <= 0) return false;

                int pipe2 = text.IndexOf(Sep, pipe1 + 1, StringComparison.Ordinal);
                if (pipe2 <= pipe1) return false;

                string embeddedGuid  = text[..pipe1];
                string embeddedFirst = text[(pipe1 + 1)..pipe2];
                string embeddedLast  = text[(pipe2 + 1)..];

                // Both machine GUID and names must be non-empty and GUID must match
                if (!string.Equals(embeddedGuid, machineGuid,
                        StringComparison.OrdinalIgnoreCase)) return false;

                if (string.IsNullOrWhiteSpace(embeddedFirst) ||
                    string.IsNullOrWhiteSpace(embeddedLast))  return false;

                firstName = embeddedFirst;
                lastName  = embeddedLast;
                return true;
            }
            catch
            {
                // Decryption failure = wrong machine key or corrupted file
                return false;
            }
        }

        /// <summary>
        /// Deletes the token file. Best-effort — silent on failure.
        /// </summary>
        public static void DeleteTokenFile()
        {
            try { if (File.Exists(TokenPath)) File.Delete(TokenPath); }
            catch { }
        }

        /// <summary>
        /// Wipes all persisted personalisation data: removes the token file and
        /// deletes any legacy INI entries left over from older builds.
        /// </summary>
        public static void ClearPersonalization()
        {
            DeleteTokenFile();

            // Delete legacy INI keys from earlier builds that stored the donor
            // key and names in plain text.  These are no longer written by new
            // builds, but may be present in existing installations being upgraded.
            // Using DeleteKey rather than writing empty strings ensures the keys
            // are fully removed from SimTools.ini rather than left as empty entries.
            IniHelper.DeleteKey("Personalization", "DonorKey");
            IniHelper.DeleteKey("Personalization", "FirstName");
            IniHelper.DeleteKey("Personalization", "LastName");
        }
    }
}
