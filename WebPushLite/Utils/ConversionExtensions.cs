using System;

namespace WebPushLite.Utils;

internal static class ConversionExtensions
{
    public static byte[] DecodeFromBase64Url(this string base64)
    {
        base64 = base64.Replace('-', '+').Replace('_', '/');

        while (base64.Length % 4 != 0)
        {
            base64 += "=";
        }

        return Convert.FromBase64String(base64);
    }
    
    public static string EncodeToBase64Url(this byte[] bytes)
    {
        return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}