using System.Net.Http;
using System.Threading.Tasks;
using WebPushNet.Models;

namespace WebPushNet;

public interface IWebPushService
{
    HttpRequestMessage GenerateRequestDetails(WebPushSubscription subscription, string payload);
    Task<HttpResponseMessage> SendAsync(WebPushSubscription subscription, string payload);
}