using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq;

namespace LocalBitCoinAPI
{
    public class LocalBitcoinApi
    {
        private const string BASE_URL = "https://localbitcoins.com";

        private readonly string apiKey;
        private readonly string apiSecret;

        public LocalBitcoinApi(string apiKey, string apiSecret)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentNullException(nameof(apiKey));
            if (string.IsNullOrWhiteSpace(apiSecret))
                throw new ArgumentNullException(nameof(apiSecret));

            this.apiKey = apiKey;
            this.apiSecret = apiSecret;
        }

        public dynamic Myself() => Request("/api/myself/", HttpMethod.Get);
        public dynamic GetAd(int id) => Request($"/api/ad-get/{id}/", HttpMethod.Get);
        public dynamic ReleaseBitcoins(string address, double amount) => Request("/api/wallet-send/", HttpMethod.Post, $"address={address}", $"amount={amount}");

        private dynamic Request(string endpoint, HttpMethod httpMethod, params object[] parameters)
        {
            var parametersValue = string.Empty;
            if (parameters.Length > 0)
                parametersValue = string.Join("&", parameters);

            var timestamp = Helpers.GetCurrentUnixTimestampMillis();
            var message = timestamp + apiKey + endpoint + parametersValue;
            var signature = Helpers.GenerateHmac(apiSecret, message);

            var request = (HttpWebRequest)WebRequest.Create(BASE_URL + endpoint + (httpMethod == HttpMethod.Get ? $"?{parametersValue}" : string.Empty));
            request.Method = httpMethod.ToString();
            request.Headers.Add("Apiauth-Key", apiKey);
            request.Headers.Add("Apiauth-Nonce", timestamp.ToString());
            request.Headers.Add("Apiauth-Signature", signature);

            if (httpMethod == HttpMethod.Post)
            {
                var dataStream = Encoding.UTF8.GetBytes(parametersValue);
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = dataStream.Length;
                var newStream = request.GetRequestStream();
                newStream.Write(dataStream, 0, dataStream.Length);
                newStream.Close();
            }

            string responseMessage;
            using (var stream = new StreamReader(request.GetRequestStream()))
                responseMessage = stream.ReadToEnd();

            return JObject.Parse(responseMessage);
        }
    }
}
