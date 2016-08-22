using System;
using System.Collections.Generic;
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

        public dynamic AccountInfo(string username) => Request($"/api/account_info/{username}/", HttpMethod.Get);
        public dynamic Myself() => Request("/api/myself/", HttpMethod.Get);
        public dynamic PinCode(string pinCode) => Request("/api/pincode/", HttpMethod.Post, $"pincode={pinCode}");
        public dynamic Dashboard() => Request("/api/dashboard/", HttpMethod.Get);
        public dynamic DashboardReleased() => Request("/api/dashboard/released/", HttpMethod.Get);
        public dynamic DashboardCanceled() => Request("/api/dashboard/canceled/", HttpMethod.Get);
        public dynamic DashboardClosed() => Request("/api/dashboard/closed/", HttpMethod.Get);
        public dynamic ReleaseContact(int idContact) => Request($"/api/contact_release/{idContact}/", HttpMethod.Post);
        public dynamic ReleaseContactPin(int idContact, string pinCode) => Request($"/api/contact_release_pin/{idContact}/", HttpMethod.Post, $"pincode={pinCode}");
        public dynamic ContactMessages(int idContact) => Request($"/api/contact_messages/{idContact}/", HttpMethod.Get);
        public dynamic MarkContactAsPaid(int idContact) => Request($"/api/contact_mark_as_paid/{idContact}/", HttpMethod.Get);
        public dynamic SendMessageToContact(int idContact, string message) => Request($"/api/contact_message_post/{idContact}/", HttpMethod.Post, $"msg={message}");
        public dynamic StartDispute(int idContact, string topic = "") => Request($"/api/contact_dispute/{idContact}/", HttpMethod.Post, string.IsNullOrWhiteSpace(topic) ? string.Empty : $"topic={topic}");
        public dynamic CancelContact(int idContact) => Request($"/api/contact_cancel/{idContact}/", HttpMethod.Post);
        public dynamic FundContact(int idContact) => Request($"/api/contact_fund/{idContact}/", HttpMethod.Post);
        public dynamic CreateContact(int idContact, double ammount, string message = "") => Request($"/api/contact_create/{idContact}/", HttpMethod.Post, $"ammount={ammount}", string.IsNullOrWhiteSpace(message) ? string.Empty : $"&msg={message}");
        public dynamic ContactInfo(int idContact) => Request($"/api/contact_info/{idContact}/", HttpMethod.Get);
        public dynamic ContactsInfo(IEnumerable<int> idContacts) => Request("/api/contact_info/", HttpMethod.Get, $"contacts={string.Join(",", idContacts)}");
        public dynamic RecentMessages(int idContact) => Request("/api/recent_messages/", HttpMethod.Get);
        public dynamic PostFeedbackToUser(string username, UserFeedback feedback, string message) => Request($"/api/feedback/{username}/", HttpMethod.Post, $"feedback={feedback.ToString().ToLower()}", string.IsNullOrWhiteSpace(message) ? string.Empty : $"&msg={message}");
        public dynamic Wallet() => Request("/api/wallet", HttpMethod.Get);
        public dynamic WalletBallance() => Request("/api/wallet-balance/", HttpMethod.Get);
        public dynamic WalletSend(string address, double ammount) => Request("/api/wallet-send/", HttpMethod.Post, $"address={address}", $"amount={ammount}");
        public dynamic WalletSendWithPin(string address, double ammount, string pinCode) => Request("/api/wallet-send-pin/", HttpMethod.Post, $"address={address}", $"amount={ammount}", $"pincode={pinCode}");
        public dynamic WalletAddress() => Request("/api/wallet-addr", HttpMethod.Post);
        public dynamic LogOut() => Request("/api/logout/", HttpMethod.Post);
        public dynamic OwnAds() => Request("/api/ads/", HttpMethod.Post);
        public dynamic GetAdByID(int id) => Request($"/api/ad-get/{id}/", HttpMethod.Get);
        //public dynamic GetAdsByIds(List<int> idAds) => Request($"/api/ad-get/", HttpMethod.Get, $"ads={string.Join(",", idAds)}");

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
