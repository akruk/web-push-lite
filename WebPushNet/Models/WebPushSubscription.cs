using System;

namespace WebPushNet.Models;

public class WebPushSubscription
{
    public string Endpoint { get; }
    public string P256DH { get; }
    public string Auth { get; }

    public WebPushSubscription(string endpoint, string p256dh, string auth)
    {
        if (!Uri.IsWellFormedUriString(endpoint, UriKind.Absolute))
            throw new ArgumentException("You must pass in a subscription with at least a valid endpoint");

        if (string.IsNullOrEmpty(p256dh))
            throw new ArgumentException("'P256DH' encryption key is required");
            
        if (string.IsNullOrEmpty(auth))
            throw new ArgumentException("'Auth' is required");
        
        Endpoint = endpoint;
        P256DH = p256dh;
        Auth = auth;
    }
}