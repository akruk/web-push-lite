using WebPushLite.Models;

namespace WebPushLite.Tests;

public class Tests
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
    public void Test1()
    {
        string jwt = _vapidKeys.GenerateJwt("https://web.push.apple.com");
    }
}