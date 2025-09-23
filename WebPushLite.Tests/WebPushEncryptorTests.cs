using System.Security.Cryptography;
using System.Text;
using WebPushLite.Utils;

namespace WebPushLite.Tests;

public class WebPushEncryptorTests
{
    [Test]
    public void DecryptPayload_Success()
    {
        // Setup
        using var ua = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
        var uaParams = ua.ExportParameters(true);
        byte[] uaPub = [0x04, ..uaParams.Q.X, ..uaParams.Q.Y];

        var payloadStr = "{ \"message\": \"test message!\" }";
        
        var auth = new byte[16];
        using var rnd = RandomNumberGenerator.Create();
        rnd.GetBytes(auth);
        
        // Act
        var encryptedMessage = WebPushEncryptor.EncryptAesgcm(
            Encoding.UTF8.GetBytes(payloadStr), 
            ConversionExtensions.EncodeToBase64(uaPub), 
            ConversionExtensions.EncodeToBase64(auth));
        
        // Verify
        var serverParams = new ECParameters
        {
            Curve = ECCurve.NamedCurves.nistP256,
            Q = new ECPoint
            {
                X = encryptedMessage.ServerPublicKey.AsSpan(1, 32).ToArray(),
                Y = encryptedMessage.ServerPublicKey.AsSpan(1 + 32, 32).ToArray()
            },
        };
        using var serverPublicOnly = ECDiffieHellman.Create(serverParams);
        
        
        byte[] prkKey = ua.DeriveKeyFromHmac(serverPublicOnly.PublicKey, HashAlgorithmName.SHA256, auth);

        var keyInfo = "Content-Encoding: auth\0"u8;
        var ikm = WebPushEncryptor.HmacSha256(prkKey, [..keyInfo, (byte)0x01]);
        
        var prk = WebPushEncryptor.HmacSha256(encryptedMessage.Salt, ikm);

        byte[] cekInfo = [
            .."Content-Encoding: aesgcm\0"u8,
            ..WebPushEncryptor.GetContextBuffer(uaPub, encryptedMessage.ServerPublicKey)];
        var cek = WebPushEncryptor.HmacSha256(prk, [..cekInfo, (byte)0x01], 16);

        byte[] nonceInfo = [
            .."Content-Encoding: nonce\0"u8,
            ..WebPushEncryptor.GetContextBuffer(uaPub, encryptedMessage.ServerPublicKey)];
        var nonce = WebPushEncryptor.HmacSha256(prk, [..nonceInfo, (byte)0x01], 12);

        var plainText = new byte[encryptedMessage.Body.Length - 16];
        var ciphertext = encryptedMessage.Body[..^16];
        var tag = encryptedMessage.Body[^16..];
        using (var aesGcm = new AesGcm(cek))
        {
            aesGcm.Decrypt(nonce, ciphertext, tag, plainText);
        }
        
        var text = Encoding.UTF8.GetString(plainText[2..]);
        
        Assert.That(text, Is.EqualTo(payloadStr));
    }
}