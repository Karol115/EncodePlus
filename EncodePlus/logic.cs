using System;
using System.Text;
using System.Security.Cryptography;
using System.Linq;

namespace EncodePlus
{
    public static class Logic
    {
        public static string HashInput(HashAlgorithm hash, string input)
        {
            return BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(input)))
                .Replace("-", "").ToLower();
        }

        public static string EncodeInput(Encoding encoding, string input, int hexType = -1)
        {
            if (hexType == -1)
            {
                byte[] utf8Bytes = encoding.GetBytes(input);
                StringBuilder encoded = new StringBuilder();

                foreach (byte b in utf8Bytes)
                {
                    encoded.Append(b);
                    encoded.Append(" ");
                }

                return encoded.ToString().Trim();
            }
            else
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                string hex = BitConverter.ToString(bytes);

                switch (hexType)
                {
                    case 1: // Spaces
                        return hex.Replace("-", " ");
                    case 2: // C-Style
                        return "0x" + hex.Replace("-", ", 0x");
                    default:
                        return hex.Replace("-", "").ToLower();
                }
            }
        }

        public static string DecodeInput(Encoding encoding, string input, bool hex = false)
        {
            if (!hex)
            {
                try {
                    byte[] bytes = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(byte.Parse)
                                        .ToArray();
                    return encoding.GetString(bytes);
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                try
                {
                    string clearHexStr = input.Replace(" ", "").Replace("0x", "").Replace("-", "").Replace(",", "");

                    byte[] bytes = new byte[clearHexStr.Length / 2];

                    for (int i = 0; i < clearHexStr.Length; i += 2)
                    {
                        bytes[i / 2] = Convert.ToByte(clearHexStr.Substring(i, 2), 16);
                    }

                    return Encoding.UTF8.GetString(bytes);
                }
                catch
                {
                    return null;
                }
            }
        }

        public static string FormatTime(TimeSpan elapsed)
        {
            if(elapsed.TotalHours >= 1)
            {
                return $"Time: {(int)elapsed.TotalHours}h {elapsed.Minutes}m {elapsed.Seconds}s";
            }
            else if(elapsed.TotalMinutes >= 1)
            {
                return $"Time: {elapsed.Minutes}m {elapsed.Seconds}s";
            }

            return $"Time: {elapsed.TotalSeconds:F2}s";
        }

        public static string DetectType(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            string clean = input.Replace("-", "").Replace(" ", "").Trim();

            // hashes
            bool isHex = clean.All(c => "0123456789abcdefABCDEF".Contains(c));

            if (isHex)
            {
                switch (clean.Length)
                {
                    case 32: return "MD5";
                    case 40: return "SHA-1";
                    case 64: return "SHA-256";
                    case 128: return "SHA-512";
                }
            }

            // url
            if(input.Contains("%") && System.Text.RegularExpressions.Regex.IsMatch(input, @"%[0-9a-fA-F]{2}"))
                return "URL";

            // base64
            if(input.Trim().EndsWith("=") || (input.Length > 20 && (input.Contains("+") || input.Contains("/"))))
            {
                try
                {
                    Convert.FromBase64String(input.Trim());
                    return "Base64";
                }
                catch { }
            }

            return null;
        }
    }
}
