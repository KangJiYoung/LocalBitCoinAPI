using System;
using System.Security.Cryptography;
using System.Text;

namespace LocalBitCoinAPI
{
    public static class Helpers
    {
        public static long GetCurrentUnixTimestampMillis()
        {
            var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)timeSpan.TotalMilliseconds;
        }

        public static string ToHexString(byte[] array)
        {
            var hex = new StringBuilder(array.Length * 2);
            foreach (var value in array)
                hex.AppendFormat("{0:x2}", value);
            return hex.ToString();
        }

        public static string GenerateHmac(string secret, string message)
        {
            var encoder = new ASCIIEncoding();
            var code = encoder.GetBytes(secret);
            var hmSha256 = new HMACSHA256(code);
            var hashMe = encoder.GetBytes(message);
            var hmBytes = hmSha256.ComputeHash(hashMe);
            return ToHexString(hmBytes).ToUpper();
        }
    }
}
