using System.Security.Cryptography;
using System.Text;
using CvEvaluator.Api.Helpers;
using Microsoft.AspNetCore.Http;

namespace CvEvaluator.Api.Tests;

public class MercadoPagoSignatureValidatorTests
{
    private const string Secret = "test_secret_key";
    private const string RequestId = "req-123";
    private const string Ts = "1710000000";
    private const string DataId = "12345678";

    private static HttpRequest BuildRequest(
        string? body = null,
        string? xSignature = null,
        string? xRequestId = null)
    {
        var context = new DefaultHttpContext();
        var request = context.Request;

        var bodyBytes = Encoding.UTF8.GetBytes(body ?? $"{{\"data\":{{\"id\":\"{DataId}\"}}}}");
        request.Body = new MemoryStream(bodyBytes);
        request.ContentLength = bodyBytes.Length;

        if (xSignature != null) request.Headers["x-signature"] = xSignature;
        if (xRequestId != null) request.Headers["x-request-id"] = xRequestId;

        return request;
    }

    private static string ComputeV1(string dataId, string requestId, string ts, string secret)
    {
        var manifest = $"id:{dataId};request-id:{requestId};ts:{ts};";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(manifest))).ToLowerInvariant();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task EmptyOrNullSecret_Bypasses_ReturnsTrue(string? secret)
    {
        var result = await MercadoPagoSignatureValidator.IsValidAsync(BuildRequest(), secret);
        Assert.True(result);
    }

    [Fact]
    public async Task MissingXSignature_ReturnsFalse()
    {
        var request = BuildRequest(xRequestId: RequestId);
        var result = await MercadoPagoSignatureValidator.IsValidAsync(request, Secret);
        Assert.False(result);
    }

    [Fact]
    public async Task MissingXRequestId_ReturnsFalse()
    {
        var v1 = ComputeV1(DataId, RequestId, Ts, Secret);
        var request = BuildRequest(xSignature: $"ts={Ts},v1={v1}");
        var result = await MercadoPagoSignatureValidator.IsValidAsync(request, Secret);
        Assert.False(result);
    }

    [Fact]
    public async Task ValidSignature_ReturnsTrue()
    {
        var v1 = ComputeV1(DataId, RequestId, Ts, Secret);
        var request = BuildRequest(xSignature: $"ts={Ts},v1={v1}", xRequestId: RequestId);
        var result = await MercadoPagoSignatureValidator.IsValidAsync(request, Secret);
        Assert.True(result);
    }

    [Fact]
    public async Task WrongSecret_ReturnsFalse()
    {
        var v1 = ComputeV1(DataId, RequestId, Ts, Secret);
        var request = BuildRequest(xSignature: $"ts={Ts},v1={v1}", xRequestId: RequestId);
        var result = await MercadoPagoSignatureValidator.IsValidAsync(request, "wrong_secret");
        Assert.False(result);
    }

    [Fact]
    public async Task TamperedTimestamp_ReturnsFalse()
    {
        var v1 = ComputeV1(DataId, RequestId, Ts, Secret);
        var request = BuildRequest(xSignature: $"ts=9999999999,v1={v1}", xRequestId: RequestId);
        var result = await MercadoPagoSignatureValidator.IsValidAsync(request, Secret);
        Assert.False(result);
    }

    [Fact]
    public async Task InvalidJsonBody_ReturnsFalse()
    {
        var v1 = ComputeV1(DataId, RequestId, Ts, Secret);
        var request = BuildRequest(body: "not valid json", xSignature: $"ts={Ts},v1={v1}", xRequestId: RequestId);
        var result = await MercadoPagoSignatureValidator.IsValidAsync(request, Secret);
        Assert.False(result);
    }

    [Fact]
    public async Task MissingDataId_ReturnsFalse()
    {
        var v1 = ComputeV1(DataId, RequestId, Ts, Secret);
        var request = BuildRequest(body: "{\"other\":\"field\"}", xSignature: $"ts={Ts},v1={v1}", xRequestId: RequestId);
        var result = await MercadoPagoSignatureValidator.IsValidAsync(request, Secret);
        Assert.False(result);
    }
}
