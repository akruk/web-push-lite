using System.Net.Http;
using System.Threading.Tasks;
using WebPushLite.Models;

namespace WebPushLite;

public interface IWebPushService
{
    HttpRequestMessage CreateRequest(WebPushSubscription subscription, string payload);
    Task<HttpResponseMessage> SendAsync(WebPushSubscription subscription, string payload);
}