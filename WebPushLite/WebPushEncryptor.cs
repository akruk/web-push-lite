using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using WebPushLite.Utils;
using WebPushLite.Models;

[assembly: InternalsVisibleTo("WebPushLite.Tests")]

namespace WebPushLite;

internal static class WebPushEncryptor
{
    /// <summary>
    ///  
    /// </summary>
    /// <param name="payload">UTF-8 bytes to encrypt (if null, use zero-length)</param>
    /// <param name="uaPublicKeyBase64Url">subscription "p256dh" (base64url of uncompressed point, 65 bytes)</param>
    /// <param name="authSecretBase64Url">subscription "auth" (base64url, 16 bytes)</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    internal static EncryptedWebPushMessage EncryptAesgcm(byte[] payload, string uaPublicKeyBase64Url, string authSecretBase64Url)
    {
        // Decode subscription inputs (base64url)
        var uaPublic = uaPublicKeyBase64Url.DecodeFromBase64();    // expected 65 bytes: 0x04 || X(32) || Y(32)
        var authSecret = authSecretBase64Url.DecodeFromBase64();   // expected 16 bytes

        if (uaPublic == null || uaPublic.Length != 65 || uaPublic[0] != 0x04)
            throw new ArgumentException("User agent public key must be an uncompressed P-256 point (65 bytes, starts with 0x04).", nameof(uaPublicKeyBase64Url));
        
        if (authSecret == null || authSecret.Length != 16)
            throw new ArgumentException("Auth secret must be 16 bytes (base64url).", nameof(authSecretBase64Url));

        // 1) Generate ephemeral server ECDH keypair (P-256)
        using var server = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
        var serverParams = server.ExportParameters(false);
        byte[] serverPub = [0x04, ..serverParams.Q.X, ..serverParams.Q.Y]; // 65 bytes

        // 2) Build a public-only ECDH object for the UA public key
        var uaParams = new ECParameters
        {
            Curve = ECCurve.NamedCurves.nistP256,
            Q = new ECPoint
            {
                X = uaPublic.AsSpan(1, 32).ToArray(),
                Y = uaPublic.AsSpan(1 + 32, 32).ToArray()
            },
        };
        using var uaPublicOnly = ECDiffieHellman.Create(uaParams);

        // 3) Derive ECDH shared secret
        // 4) HKDF-ish steps per RFC 8291 (see section 3.3 and 3.4)
        byte[] prkKey = server.DeriveKeyFromHmac(uaPublicOnly.PublicKey, HashAlgorithmName.SHA256, authSecret);

        // key_info = "WebPush: info" || 0x00 || ua_public || as_public
        //var keyInfo = BuildKeyInfo(uaPublic, serverPub);
        var keyInfo = "Content-Encoding: auth\0"u8;
        
        // IKM = HMAC-SHA256(PRK_key, key_info || 0x01)
        var ikm = HmacSha256(prkKey, [..keyInfo, (byte)0x01]);
        
        // salt = random(16)
        var salt = new byte[16];
        using var rnd = RandomNumberGenerator.Create();
        rnd.GetBytes(salt);

        // PRK = HMAC-SHA256(salt, IKM)
        var prk = HmacSha256(salt, ikm);

        // cek_info = "Content-Encoding: aesgcm" || 0x00 + context buffer
        byte[] cekInfo = [
            .."Content-Encoding: aesgcm\0"u8,
            ..GetContextBuffer(uaPublic, serverPub)];
        
        // CEK = HMAC-SHA256(PRK, cek_info || 0x01)[0..15]
        var cek = HmacSha256(prk, [..cekInfo, (byte)0x01], 16);

        // nonce_info = "Content-Encoding: nonce" || 0x00 + context buffer
        byte[] nonceInfo = [
            .."Content-Encoding: nonce\0"u8,
            ..GetContextBuffer(uaPublic, serverPub)];
        
        // NONCE = HMAC-SHA256(PRK, nonce_info || 0x01)[0..11]
        var nonce = HmacSha256(prk, [..nonceInfo, (byte)0x01], 12);

        // 5) Prepare plaintext per RFC8188 single-record format:
        //    plaintext || 0x02  (minimum: one padding delimiter octet 0x02)
        byte[] paddedPlain = [0x00, 0x00, ..payload];

        // 6) AES-GCM encrypt: ciphertext and 16-byte tag
        var ciphertext = new byte[paddedPlain.Length];
        var tag = new byte[16];
        using (var aesGcm = new AesGcm(cek))
        {
            aesGcm.Encrypt(nonce, paddedPlain, ciphertext, tag);
        }
        
        // Body to send is ciphertext concatenated with the 16-byte tag.
        byte[] body = [..ciphertext, ..tag];

        return new EncryptedWebPushMessage("aesgcm", body, serverPub, salt);
    }
    
    #region Helpers

    internal static byte[] GetContextBuffer(byte[] uaPublicUncompressed, byte[] asPublicUncompressed)
    {
        return
        [
            .."P-256\0"u8.ToArray(),
            ..ConvertInt(uaPublicUncompressed.Length),
            ..uaPublicUncompressed,
            ..ConvertInt(asPublicUncompressed.Length),
            ..asPublicUncompressed
        ];
    }
    
    private static byte[] ConvertInt(int number)
    {
        var output = BitConverter.GetBytes(Convert.ToUInt16(number));
        if (BitConverter.IsLittleEndian)
            Array.Reverse(output);

        return output;
    }

    internal static byte[] HmacSha256(byte[] key, byte[] data, int length = 32)
    {
        using var h = new HMACSHA256(key);
        var hash = h.ComputeHash(data);

        return hash.Length > length ? hash[..length] : hash;
    }
    
    #endregion
}