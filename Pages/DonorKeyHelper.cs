using System;
using System.Security.Cryptography;
using System.Text;

namespace SimTools
{
    /// <summary>
    /// Handles generation and validation of donor personalisation keys.
    ///
    /// Keys are AES-128-CBC encrypted payloads encoded as URL-safe Base64.
    /// Change _key and _iv before distributing a new build to invalidate old keys.
    /// </summary>
    internal static class DonorKeyHelper
    {
        // ── AES-128 key (16 bytes) ────────────────────────────────────────────
        // "SimToolsDonorKey"
        private static readonly byte[] _key =
        {
            0x53, 0x69, 0x6D, 0x54, 0x6F, 0x6F, 0x6C, 0x73,
            0x44, 0x6F, 0x6E, 0x6F, 0x72, 0x4B, 0x65, 0x79
        };

        // ── AES-128 IV (16 bytes) ─────────────────────────────────────────────
        // "ThankYouSupport!"
        private static readonly byte[] _iv =
        {
            0x54, 0x68, 0x61, 0x6E, 0x6B, 0x59, 0x6F, 0x75,
            0x53, 0x75, 0x70, 0x70, 0x6F, 0x72, 0x74, 0x21
        };

        private const string Separator = "|";

        // ─────────────────────────────────────────────────────────────────────
        /// <summary>
        /// Generates a donor key that encodes <paramref name="firstName"/> and
        /// <paramref name="lastName"/>.  The returned string is URL-safe Base64
        /// with no padding characters and is safe to store in an INI file.
        /// </summary>
        public static string GenerateKey(string firstName, string lastName)
        {
            string plaintext      = firstName.Trim() + Separator + lastName.Trim();
            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);

            using var aes   = Aes.Create();
            aes.Key         = _key;
            aes.IV          = _iv;
            aes.Mode        = CipherMode.CBC;
            aes.Padding     = PaddingMode.PKCS7;

            using var enc   = aes.CreateEncryptor();
            byte[] cipher   = enc.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);

            return Convert.ToBase64String(cipher)
                          .Replace('+', '-')
                          .Replace('/', '_')
                          .TrimEnd('=');
        }

        // ─────────────────────────────────────────────────────────────────────
        /// <summary>
        /// Attempts to decode a donor key produced by <see cref="GenerateKey"/>.
        /// Returns <c>true</c> and sets <paramref name="firstName"/> /
        /// <paramref name="lastName"/> on success; returns <c>false</c> for any
        /// invalid or tampered key.
        /// </summary>
        public static bool TryDecodeKey(string key,
                                        out string firstName,
                                        out string lastName)
        {
            firstName = string.Empty;
            lastName  = string.Empty;

            try
            {
                // Restore standard Base64 from URL-safe variant
                string b64 = key.Trim().Replace('-', '+').Replace('_', '/');
                int pad = b64.Length % 4;
                if (pad > 0) b64 += new string('=', 4 - pad);

                byte[] cipher = Convert.FromBase64String(b64);

                using var aes   = Aes.Create();
                aes.Key         = _key;
                aes.IV          = _iv;
                aes.Mode        = CipherMode.CBC;
                aes.Padding     = PaddingMode.PKCS7;

                using var dec   = aes.CreateDecryptor();
                byte[] plain    = dec.TransformFinalBlock(cipher, 0, cipher.Length);
                string text     = Encoding.UTF8.GetString(plain);

                int sep = text.IndexOf(Separator, StringComparison.Ordinal);
                if (sep <= 0 || sep == text.Length - 1) return false;

                firstName = text[..sep];
                lastName  = text[(sep + 1)..];

                return !string.IsNullOrWhiteSpace(firstName) &&
                       !string.IsNullOrWhiteSpace(lastName);
            }
            catch
            {
                return false;
            }
        }
    }
}
