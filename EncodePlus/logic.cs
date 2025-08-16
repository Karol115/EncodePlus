using System;
using System.Text;
using System.Security.Cryptography;

namespace EncodePlus
{
    /*public static class UtfASCII
    {
        private static Encoding returnEncode(int utfASCII)
        {
            switch (utfASCII)
            {
                case 0:
                    return Encoding.ASCII;
                case 8:
                    return Encoding.UTF8;
                case 7:
                    return Encoding.UTF7;
                case 32:
                    return Encoding.UTF32;
                default:
                    return Encoding.UTF8;
            }
        }

        /// <param name="utfASCII">0-ASCII      7-UTF7      8-UTF8      32-UTF32</param>
        public static string Encode(string input, int utfASCII)
        {
            Encoding encoding = returnEncode(utfASCII);
        }

        /// <param name="utfASCII">0-ASCII      7-UTF7      8-UTF8      32-UTF32</param>
        public static string Decode(string input, int utfASCII)
        {
            Encoding encoding = returnEncode(utfASCII);

            string[] byteStrings = input.Split(' ');
            byte[] utf8Bytes = new byte[byteStrings.Length];

            for (int i = 0; i < byteStrings.Length; i++)
            {
                if (byte.TryParse(byteStrings[i], out byte b))
                {
                    utf8Bytes[i] = b;
                }
            }

            return encoding.GetString(utf8Bytes);
        }

    }*/

    /*public static class Hex
    {
        public static string HexToString(string hex)
        {
            hex = hex.Replace(" ", "");

            byte[] bytes = new byte[hex.Length / 2];

            for (int i = 0; i < hex.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return Encoding.UTF8.GetString(bytes);
        }

        public static string StringToHex(string input)
        {
            return BitConverter.ToString(Encoding.UTF8.GetBytes(input)).Replace("-", "").ToLower();
        }
    }*/

    public static class Logic
    {
        public static string HashInput(HashAlgorithm hash, string input)
        {
            return BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(input)))
                .Replace("-", "").ToLower();
        }

        public static string EncodeInput(Encoding encoding, string input, bool hex = false)
        {
            if (!hex)
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
                return BitConverter.ToString(Encoding.UTF8.GetBytes(input)).Replace("-", "").ToLower();
            }
        }

        public static string DecodeInput(Encoding encoding, string input, bool hex = false)
        {
            if (!hex)
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
                try
                {
                    string hexStr = input.Replace(" ", "");

                    byte[] bytes = new byte[hexStr.Length / 2];

                    for (int i = 0; i < hexStr.Length; i += 2)
                    {
                        bytes[i / 2] = Convert.ToByte(hexStr.Substring(i, 2), 16);
                    }

                    return Encoding.UTF8.GetString(bytes);
                }
                catch
                {
                    return "Error";
                }
            }
        }
    }
}
