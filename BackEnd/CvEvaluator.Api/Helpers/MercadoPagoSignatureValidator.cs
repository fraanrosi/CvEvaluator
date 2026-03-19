using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CvEvaluator.Api.Helpers;

public static class MercadoPagoSignatureValidator
{
    public static async Task<bool> IsValidAsync(HttpRequest request, string? secret, ILogger? logger = null)
    {
        if (string.IsNullOrEmpty(secret)) return true; // bypass en local dev

        var xSignature = request.Headers["x-signature"].FirstOrDefault();
        var xRequestId = request.Headers["x-request-id"].FirstOrDefault();
        if (string.IsNullOrEmpty(xSignature) || string.IsNullOrEmpty(xRequestId))
        {
            logger?.LogDebug("Webhook: missing x-signature or x-request-id");
            return false;
        }

        string? ts = null, v1 = null;
        foreach (var part in xSignature.Split(','))
        {
            var kv = part.Split('=', 2);
            if (kv.Length == 2)
            {
                if (kv[0].Trim() == "ts") ts = kv[1].Trim();
                if (kv[0].Trim() == "v1") v1 = kv[1].Trim();
            }
        }
        if (ts == null || v1 == null)
        {
            logger?.LogDebug("Webhook: malformed x-signature (ts/v1 missing)");
            return false;
        }

        request.Body.Position = 0;
        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        var bodyText = await reader.ReadToEndAsync();
        request.Body.Position = 0;

        string? dataId = null;
        try
        {
            using var doc = JsonDocument.Parse(bodyText);
            if (doc.RootElement.TryGetProperty("data", out var data) &&
                data.TryGetProperty("id", out var idProp))
                dataId = idProp.ValueKind == JsonValueKind.String
                    ? idProp.GetString()
                    : idProp.GetRawText();
        }
        catch (JsonException) { return false; }
        if (dataId == null)
        {
            logger?.LogDebug("Webhook: data.id not found in body");
            return false;
        }

        var manifest = $"id:{dataId};request-id:{xRequestId};ts:{ts};";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var computed = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(manifest))).ToLowerInvariant();

        var valid = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computed),
            Encoding.UTF8.GetBytes(v1.ToLowerInvariant()));

        if (!valid)
            logger?.LogDebug("Webhook: HMAC mismatch. manifest={Manifest}", manifest);

        return valid;
    }
}
