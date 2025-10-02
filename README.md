# web-push-lite
.NET library for Web Push

**web-push-lite** is a minimal, dependency-free .NET library implementing the Web Push / VAPID protocol for sending push notifications from a server to browser clients.

Targets .NET Standard 2.1

# Installation

    dotnet add package web-push-lite

# Usage

```csharp
WebPushSubscription subscription = new WebPushSubscription(
    "<SUBSCRIPTION_ENDPOINT>",
    "<P256PH_PUBLIC_KEY>",
    "<SUBSCRIPTION_AUTH>");

var payload = new
{
    notification = new { title = "Test message from push library!" }
};

HttpResponseMessage request = await webPushService.SendAsync(subscription, JsonSerializer.Serialize(payload));
```
