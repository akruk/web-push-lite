using WebPushLite.Utils;

namespace WebPushLite.Models;

internal class EncryptedWebPushMessage
{
    public string ContentEncoding {get; }
    public byte[] Body { get; }
    public byte[] ServerPublicKey { get; }
    public string ServerPublicKeyBase64 { get; }
    public byte[] Salt { get; }
    public string SaltBase64 { get; }

    public EncryptedWebPushMessage(
        string contentEncoding,
        byte[] body,
        byte[] serverPublicKey,
        byte[] salt)
    {
        ContentEncoding = contentEncoding;
        Body = body;
        
        ServerPublicKey = serverPublicKey;
        ServerPublicKeyBase64 = ServerPublicKey.EncodeToBase64();
        
        Salt = salt;
        SaltBase64 = Salt.EncodeToBase64();
    }
}
