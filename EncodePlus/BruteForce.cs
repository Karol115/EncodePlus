using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EncodePlus
{
    public static class BruteForce
    {
        private static volatile bool _isFinished = false;
        public static int ThreadCount { get; set; } = Environment.ProcessorCount;
        public static int SearchDepth = 5;

        private static long _totalAttempts;
        public static long TotalAttempts
        {
            get => _totalAttempts;
            set => _totalAttempts = value;
        }

         public static string BruteforceHash(Func<HashAlgorithm> hashFactory, string targetHash, CancellationToken token)
         {
            string foundResult = null;
            _isFinished = false;

            byte[] targetBytes = HexToBytes(targetHash);

            byte minChar = 32;
            byte maxChar = 126;

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = ThreadCount,
                CancellationToken = token,
            };

            try
            {
                for (int len = 1; len <= SearchDepth; len++)
                {
                    Parallel.For((int)minChar, (int)maxChar + 1, options, (firstChar, state) =>
                    {
                        if (token.IsCancellationRequested) state.Stop();

                        using (var threadHash = hashFactory()) // create separate instances
                        {
                            string result = AttemptBruteForce(threadHash, targetBytes, len, (byte)firstChar, token);

                            if (result != null)
                            {
                                foundResult = result;
                                _isFinished = true;
                                state.Stop();
                            }
                        }
                    });

                    if (_isFinished || token.IsCancellationRequested) break;
                }
            }
            catch (OperationCanceledException) { }

            return foundResult;
         }

        private static string AttemptBruteForce(HashAlgorithm hash, byte[] targetBytes, int len, byte firstByte, CancellationToken token)
        {
            byte[] combo = new byte[len];
            for (int i = 0; i < len; i++) combo[i] = 32;
            combo[0] = firstByte;

            // optimization
            long localCounter = 0;

            while (!_isFinished)
            {
                localCounter++;

                if (localCounter % 100000 == 0)
                {
                    System.Threading.Interlocked.Add(ref _totalAttempts, 100000);

                    if (token.IsCancellationRequested) return null;
                }

                byte[] currentHashBytes = hash.ComputeHash(combo);

                bool match = true;
                for(int i = 0; i < currentHashBytes.Length; i++)
                {
                    if(currentHashBytes[i] != targetBytes[i])
                    {
                        match = false;
                        break;
                    }
                }

                if (match) return Encoding.ASCII.GetString(combo);

                int pos = len - 1;
                while (pos > 0)
                {
                    if (combo[pos] < 126)
                    {
                        combo[pos]++;
                        break;
                    }
                    else
                    {
                        combo[pos] = 32;
                        pos--;
                    }
                }

                if (pos <= 0) break;
            }

            System.Threading.Interlocked.Add(ref _totalAttempts, localCounter % 100000);
            return null;
        }

        private static byte[] HexToBytes(string hex)
        {
            hex = hex.Replace(" ", "").Replace("-", "");
            if (hex.Length % 2 != 0) return new byte[0];

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }

        public static bool IsHashValid(string selectedKey, string hash)
        {
            if (string.IsNullOrWhiteSpace(hash)) return false;

            string cleanHash = hash.Replace("-", "").Replace(" ", "").Trim();

            var expectedLengths = new Dictionary<string, int>
            {
                { "MD5", 32 },
                { "SHA-1", 40 },
                { "SHA-256", 64 },
                { "SHA-512", 128 }
            };

            if (!expectedLengths.ContainsKey(selectedKey)) return true;

            if (cleanHash.Length != expectedLengths[selectedKey]) return false;

            // hex
            return cleanHash.All(c => "0123456789abcdefABCDEF".Contains(c));
        }
    }
}
