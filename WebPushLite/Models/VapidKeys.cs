using System;
using System.Security.Cryptography;
using System.Text;
using WebPushLite.Utils;

namespace WebPushLite.Models;

public class VapidKeys
{
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromHours(12);

    private readonly byte[] _privateKeyBytes;
    private readonly byte[] _publicKeyBytes;

    public string Subject { get; }
    public string PublicKey { get; }
    public string PrivateKey { get; }
    
    /// <param name="subject">Contact URI for the application server as either a "mailto:" (email) or an "https:"</param>
    /// <param name="publicKey">Public key as a base64 encoded string</param>
    /// <param name="privateKey">Private key as a base64 encoded string</param>
    public VapidKeys(string subject, string publicKey, string privateKey)
    {
        if (string.IsNullOrEmpty(subject))
            throw new ArgumentException("Subject cannot be empty");

        if (!subject.StartsWith("mailto:") && !Uri.IsWellFormedUriString(subject, UriKind.Absolute))
            throw new ArgumentException("Subject has incorrect format. It must be a valid URL or mailto: address");
        
        Subject = subject;
        
        if (string.IsNullOrEmpty(publicKey))
            throw new ArgumentException("Public key cannot be empty");
        
        _publicKeyBytes = publicKey.DecodeFromBase64Url();
        
        if (_publicKeyBytes.Length != 65 || _publicKeyBytes[0] != 0x04)
            throw new ArgumentException("Public key must be an uncompressed P-256 point (65 bytes, starts with 0x04)");
        
        PublicKey = publicKey;
        
        if (string.IsNullOrEmpty(privateKey))
            throw new ArgumentException("Private key cannot be empty");
        
        _privateKeyBytes = privateKey.DecodeFromBase64Url();

        if (_privateKeyBytes.Length != 32)
            throw new ArgumentException("Private key must be 32 bytes long");
        
        PrivateKey = privateKey;
    }

    public string GenerateJwt(string audience, TimeSpan? expiration = null)
    {
        if (string.IsNullOrEmpty(audience))
            throw new ArgumentException("Audience is required");

        if (!Uri.IsWellFormedUriString(audience, UriKind.Absolute))
            throw new ArgumentException("Audience must be a valid URL");
        
        var exp = DateTimeOffset.UtcNow.Add(expiration ?? DefaultExpiration).ToUnixTimeSeconds();
        
        var header = "{ \"alg\": \"ES256\", \"typ\": \"JWT\" }";
        string headerB64 = Encoding.UTF8.GetBytes(header).EncodeToBase64Url();

        var payload = $"{{ \"sub\": \"{Subject}\", \"exp\": {exp}, \"aud\": \"{audience}\" }}";
        string payloadB64 = Encoding.UTF8.GetBytes(payload).EncodeToBase64Url();
        
        string message = $"{headerB64}.{payloadB64}";
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);

        var ecParams = new ECParameters
        {
            Curve = ECCurve.NamedCurves.nistP256,
            D = _privateKeyBytes
        };

        using var ecdsa = ECDsa.Create(ecParams);
        
        byte[] rawSignature = ecdsa.SignData(messageBytes, HashAlgorithmName.SHA256);

        string signatureB64 = rawSignature.EncodeToBase64Url();
            
        return $"{message}.{signatureB64}";
    }
}