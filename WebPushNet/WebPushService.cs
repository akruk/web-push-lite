using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using WebPushNet.Models;

namespace WebPushNet
{
    public class WebPushService : IWebPushService, IDisposable
    {
        private const int DefaultTtl = 2419200;
        
        private readonly VapidKeys _vapidKeys;
        private readonly HttpClient _httpClient;
        
        public WebPushService(VapidKeys vapidKeys)
        {
            _vapidKeys = vapidKeys;
            _httpClient = new HttpClient();
        }

        public HttpRequestMessage GenerateRequestDetails(WebPushSubscription subscription, string payload)
        {
            if (string.IsNullOrEmpty(payload))
                throw new ArgumentNullException(nameof(payload));

            var request = new HttpRequestMessage(HttpMethod.Post, subscription.Endpoint);

            var timeToLive = DefaultTtl;

            request.Headers.Add("TTL", timeToLive.ToString());

            if (string.IsNullOrEmpty(subscription.P256DH) || string.IsNullOrEmpty(subscription.Auth))
            {
                throw new ArgumentException(
                    "Unable to send a message with payload to this subscription since it doesn't have the required encryption key");
            }

            EncryptedWebPushMessage encryptedPayload = WebPushEncryptor.EncryptAesgcm(
                Encoding.UTF8.GetBytes(payload),
                subscription.P256DH,
                subscription.Auth);

            request.Content = new ByteArrayContent(encryptedPayload.Body);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            request.Content.Headers.ContentEncoding.Add(encryptedPayload.ContentEncoding);
            
            // According to RFC8188 / RFC8291, the salt is carried in Encryption header (param "salt"),
            // and the server public key in the Crypto-Key header under the "dh" parameter:
            // Encryption: salt=<salt>
            // Crypto-Key: dh=<dh>
            request.Headers.Add("Encryption", $"salt={encryptedPayload.SaltBase64}");
            request.Headers.Add("Crypto-Key", $"dh={encryptedPayload.ServerPublicKeyBase64};p256ecdsa={_vapidKeys.PublicKey}");

            var uri = new Uri(subscription.Endpoint);
            var audience = uri.Scheme + @"://" + uri.Host;

            var jwt = _vapidKeys.GenerateJwt(audience);

            request.Headers.Add(@"Authorization", $"WebPush {jwt}");
            
            return request;
        }

        public async Task<HttpResponseMessage> SendAsync(WebPushSubscription subscription, string payload)
        {
            var requestMessage = GenerateRequestDetails(subscription, payload);

            return await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}