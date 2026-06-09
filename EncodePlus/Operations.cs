using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace EncodePlus
{
    public static  class Operations
    {
        public enum OperationType
        {
            Encoding,
            Hash,
            Hex,
        }
        public class Codec
        {
            public OperationType Type { get; set; }
            public Func<HashAlgorithm> HashFactory { get; set; }
            public Func<string, string> Encode { get; set; }
            public Func<string, string> Decode { get; set; }
            public Func<string, CancellationToken, string> BruteForce { get; set; }
        }

        public static Dictionary<string, Codec> operations = new Dictionary<string, Codec>
        {
            { "Base64", new Codec {
                Type = OperationType.Encoding,
                Encode = input => Convert.ToBase64String(Encoding.UTF8.GetBytes(input)),
                Decode = input => {
                    try {
                        byte[] data = Convert.FromBase64String(input.Trim());
                        string result = Encoding.UTF8.GetString(data);
            
                        if (result.Any(c => char.IsControl(c) && c != '\r' && c != '\n' && c != '\t'))
                            return null;
                
                        return result;
                    } catch {
                        return null;
                    }
                }
            }},
            { "URL", new Codec {
                Type = OperationType.Encoding,
                Encode = input => Uri.EscapeDataString(input),
                Decode = input => Uri.UnescapeDataString(input),
            }},
            { "ASCII", new Codec {
                Type = OperationType.Encoding,
                Encode = input => Logic.EncodeInput(Encoding.ASCII, input),
                Decode = input => Logic.DecodeInput(Encoding.ASCII, input),
            }},
            { "Utf-7", new Codec {
                Type = OperationType.Encoding,
                Encode = input => Logic.EncodeInput(Encoding.UTF7, input),
                Decode = input => Logic.DecodeInput(Encoding.UTF7, input)
            }},
            { "Utf-8", new Codec {
                Type = OperationType.Encoding,
                Encode = input => Logic.EncodeInput(Encoding.UTF8, input),
                Decode = input => Logic.DecodeInput(Encoding.UTF8, input),
            }},
            { "Utf-32", new Codec {
                Type = OperationType.Encoding,
                Encode = input => Logic.EncodeInput(Encoding.UTF32, input),
                Decode = input => Logic.DecodeInput(Encoding.UTF32, input),
            }},
            { "Hex", new Codec {
                Type = OperationType.Hex,
                Encode = input => {
                    int type = 0; // default
                    if (MainPage.EncodingVariant == "Spaces") type = 1;
                    else if (MainPage.EncodingVariant == "C-Style(0xAA, 0xBB)") type = 2;

                    return Logic.EncodeInput(null, input, type);
                },

                Decode = input => Logic.DecodeInput(null, input, true),
            }},
            { "SHA-1", new Codec {
                Type = OperationType.Hash,
                Encode = input => Logic.HashInput(SHA1.Create(), input),
                HashFactory = () => SHA1.Create(),
                BruteForce = (input, token) => BruteForce.BruteforceHash(() => SHA1.Create(), input, token)
            }},
            { "SHA-256", new Codec {
                Type = OperationType.Hash,
                Encode = input => Logic.HashInput(SHA256.Create(), input),
                HashFactory = () => SHA256.Create(),
                BruteForce = (input, token) => BruteForce.BruteforceHash(() => SHA256.Create(), input, token)
            }},
            { "SHA-512", new Codec {
                Type = OperationType.Hash,
                Encode = input => Logic.HashInput(SHA512.Create(), input),
                HashFactory = () => SHA512.Create(),
                BruteForce = (input, token) => BruteForce.BruteforceHash(() => SHA512.Create(), input, token)
            }},
            { "MD5", new Codec {
                Type = OperationType.Hash,
                HashFactory = () => MD5.Create(),
                Encode = input => Logic.HashInput(MD5.Create(), input),
                BruteForce = (input, token) => BruteForce.BruteforceHash(() => MD5.Create(), input, token)
            }},
        };
    }
}
