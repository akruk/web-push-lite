using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
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
        
        _publicKeyBytes = publicKey.DecodeFromBase64();
        
        if (_publicKeyBytes.Length != 65 || _publicKeyBytes[0] != 0x04)
            throw new ArgumentException("Public key must be an uncompressed P-256 point (65 bytes, starts with 0x04)");
        
        PublicKey = publicKey;
        
        if (string.IsNullOrEmpty(privateKey))
            throw new ArgumentException("Private key cannot be empty");
        
        _privateKeyBytes = privateKey.DecodeFromBase64();

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

        var ecParams = new ECParameters
        {
            Curve = ECCurve.NamedCurves.nistP256,
            D = _privateKeyBytes
        };

        var ecdsa = ECDsa.Create(ecParams);

        var securityKey = new ECDsaSecurityKey(ecdsa)
        {
            KeyId = null // VAPID typically omits kid
        };
        
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.EcdsaSha256);

        var token = new JwtSecurityToken(
            audience: audience,
            expires: DateTime.UtcNow.Add(expiration ?? DefaultExpiration),
            signingCredentials: credentials,
            claims: [new Claim("sub", Subject)]
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}