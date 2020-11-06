using System;
using System.Security.Cryptography;

namespace TotpAuth
{
    public class Authenticator
    {
        private readonly Func<DateTime> NowFunc;
        private readonly int IntervalSeconds;
        private readonly int VerificationRange;

        private static readonly RandomNumberGenerator Random = RandomNumberGenerator.Create();
        private static readonly string AvailableKeyChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

        public Authenticator()
        {
            this.NowFunc = (() => DateTime.Now);
            this.IntervalSeconds = 30;
            this.VerificationRange = 2;
        }

        public Authenticator(Func<DateTime>? nowFunc = null, int intervalSeconds = 30, int verificationRange = 2)
        {
            this.NowFunc = nowFunc ?? (() => DateTime.Now);
            this.IntervalSeconds = intervalSeconds;
            this.VerificationRange = verificationRange;
        }

        public string GetCode(string secret)
        {
            return GetCode(secret, NowFunc());
        }

        public string GetCode(string secret, DateTime date)
        {
            return GetCodeInternal(secret, (ulong)GetInterval(date));
        }

        public bool CheckCode(string secret, string code)
        {
            return CheckCode(secret, code, out _);
        }


        public bool CheckCode(string secret, string code, out DateTime usedDateTime)
        {
            var baseTime = NowFunc();
            DateTime successfulTime = DateTime.MinValue;
            var codeMatch = false;
            for (int i = -VerificationRange; i < VerificationRange; i++)
            {
                var checkTime = baseTime.AddSeconds(IntervalSeconds * i);
                var checkInterval = GetInterval(checkTime);

                if (ConstantTimeEquals(GetCode(secret, checkTime), code))
                {
                    codeMatch = true;
                    successfulTime = checkTime;
                }
            }

            usedDateTime = successfulTime;
            return codeMatch;
        }

        private long GetInterval(DateTime dateTime)
        {
            TimeSpan ts = (dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            return (long)ts.TotalSeconds / IntervalSeconds;
        }


        public static string GenerateKey(int keyLength = 16)
        {
            var keyChars = new char[keyLength];
            for (int i = 0; i < keyChars.Length; i++)
            {
                keyChars[i] = AvailableKeyChars[RandomInt(AvailableKeyChars.Length)];
            }
            return new String(keyChars);
        }

        protected string GetCodeInternal(string secret, ulong challengeValue)
        {
            ulong chlg = challengeValue;
            byte[] challenge = new byte[8];
            for (int j = 7; j >= 0; j--)
            {
                challenge[j] = (byte)((int)chlg & 0xff);
                chlg >>= 8;
            }

            var key = Base32Encoding.ToBytes(secret);
            for (int i = secret.Length; i < key.Length; i++)
            {
                key[i] = 0;
            }

            HMACSHA1 mac = new HMACSHA1(key);
            var hash = mac.ComputeHash(challenge);

            int offset = hash[hash.Length - 1] & 0xf;

            int truncatedHash = 0;
            for (int j = 0; j < 4; j++)
            {
                truncatedHash <<= 8;
                truncatedHash |= hash[offset + j];
            }

            truncatedHash &= 0x7FFFFFFF;
            truncatedHash %= 1000000;

            string code = truncatedHash.ToString();
            return code.PadLeft(6, '0');
        }

        protected bool ConstantTimeEquals(string a, string b)
        {
            uint diff = (uint)a.Length ^ (uint)b.Length;

            for (int i = 0; i < a.Length && i < b.Length; i++)
            {
                diff |= (uint)a[i] ^ (uint)b[i];
            }

            return diff == 0;
        }

        protected static int RandomInt(int max)
        {
            var randomBytes = new byte[4];
            Random.GetBytes(randomBytes);

            return Math.Abs((int)BitConverter.ToUInt32(randomBytes, 0) % max);
        }
    }
}
