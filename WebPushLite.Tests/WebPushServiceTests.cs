using System.Text.Json;
using WebPushLite.Models;

namespace WebPushLite.Tests;

public class WebPushServiceTests
{
    private VapidKeys _vapidKeys;
    
    [SetUp]
    public void Setup()
    {
        _vapidKeys = new VapidKeys(
            "mailto:a.kruk@tut.by",
            "BBtmHfzr6l3f5jCowx5CR6cpnF9hVTNCZBOeaayk2KmdYGJuno2HhY4DFaBl7DdvsRuLgvUa9k0I8vRWg5ovYdM",
            "iYEqf9yNLL4J4At2VNAOuUb1XGLA4-7VWNX_2TMnijc");
    }

    [Test]
    public async Task Test1()
    {
        WebPushService webPushService = new WebPushService(_vapidKeys);

        // PushSubscription subscription = new PushSubscription
        // {
        //     Endpoint = "https://web.push.apple.com/QJZV8mpbIKCc5JUrEqWyc_GGxWYvLnV2KuJ2lv7kVevZe-9YPXFABTjU8yyBc7Vo49CdO16l4rvEuBYZHtaQjoP6V5Fi7MHLM-iJ8ZJ55Ll02hVCU_Yt9ncvRvOGuChUA7yYiEkWrVW-bv5ny5GODpmFOOqRHC87h2HWR7nSqpg",
        //     P256DH = "BKl5QgtpQSBUHzBCQ1mA8ooxT0aO0rxSaVvwfulnLfib87-_Q6u-MUY5edafduFmRn1udWeIi9Tm5Yooo3euooc",
        //     Auth = "DV0mGoeM6CP7WFO_ur9YkA"
        // };
        
        WebPushSubscription subscription = new WebPushSubscription(
            "https://fcm.googleapis.com/fcm/send/cMvd6a0_bPM:APA91bEIGP83mIQUFDu3gZr9X9usEU81s-ZpXY8RxwWjCh7yd6DG3L7qgaF7McC8TZMqAM22zZoR_bOxsPMx3Vjj9UjG52rSIcCgB-WoJSCAvm3OfHI2LRntjc7Bdr8X8Tn1vtFIu8eo",
            "BP2qgfD_KMp7-m-LWo02mzem5uSxXtpyg3GSSOllpmB_Py8cWC4J8OhASVOcxnvRrB61N3rLlz1Noe3LLDFUNlw",
            "T47ws4Lj1kJ_8qMYJWjESw");

        var payload = new
        {
            notification = new { title = "Test message from push library!" }
        };
        
        var request = webPushService.GenerateRequestDetails(subscription, JsonSerializer.Serialize(payload));
        
        using var httpClient = new HttpClient();

        var response = await httpClient.SendAsync(request);
    }
    
    [Test]
    public async Task Test2()
    {
        WebPushService webPushService = new WebPushService(_vapidKeys);
        
        // PushSubscription subscription = new PushSubscription
        // {
        //     Endpoint = "https://web.push.apple.com/QJZV8mpbIKCc5JUrEqWyc_GGxWYvLnV2KuJ2lv7kVevZe-9YPXFABTjU8yyBc7Vo49CdO16l4rvEuBYZHtaQjoP6V5Fi7MHLM-iJ8ZJ55Ll02hVCU_Yt9ncvRvOGuChUA7yYiEkWrVW-bv5ny5GODpmFOOqRHC87h2HWR7nSqpg",
        //     P256DH = "BKl5QgtpQSBUHzBCQ1mA8ooxT0aO0rxSaVvwfulnLfib87-_Q6u-MUY5edafduFmRn1udWeIi9Tm5Yooo3euooc",
        //     Auth = "DV0mGoeM6CP7WFO_ur9YkA"
        // };
        
        WebPushSubscription subscription = new WebPushSubscription(
            "https://fcm.googleapis.com/fcm/send/fbZR3G5Bins:APA91bH3iJ-sX0jbyNX6Wb2TX0l0-7gqbGJi4Gm67TibmSQhSLfCUFvXf3ABzFyuGYzO_X9QiDsah7rxvtyjh4YxoMPcCKtip2k8_PynZeRGtieNvEDwFIMPKvFmeW91yrTWiKS_PLBZ",
            "BPKakX-tg-SwOSUWsiDNfCXRDyFyOxO8RtUXzMZrkfYPGqzYupfRHOYPwygjltoU6NOBtrP6SRN0NRDWTUl-c-k",
            "IS648W4J4NGiRzZizjZf9g");

        var payload = new
        {
            notification = new { title = "Test message from push library!" }
        };
        
        var request = webPushService.GenerateRequestDetails(subscription, JsonSerializer.Serialize(payload));
        
        using var httpClient = new HttpClient();

        var response = await httpClient.SendAsync(request);
    }
}