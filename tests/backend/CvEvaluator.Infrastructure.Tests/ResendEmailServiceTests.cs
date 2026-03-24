using CvEvaluator.Infrastructure.Email;
using CvEvaluator.Infrastructure.Tests.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;

namespace CvEvaluator.Infrastructure.Tests;

public class ResendEmailServiceTests
{
    private readonly Mock<ILogger<ResendEmailService>> _loggerMock = new();

    private IConfiguration CreateConfig(string? apiKey = "re_test_key", string? fromAddress = null, string? fromName = null)
    {
        var dict = new Dictionary<string, string?>
        {
            ["Email:ApiKey"] = apiKey
        };
        if (fromAddress != null) dict["Email:FromAddress"] = fromAddress;
        if (fromName != null) dict["Email:FromName"] = fromName;
        return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
    }

    [Fact]
    public async Task SendEmailConfirmationAsync_PostsToEmailsEndpoint()
    {
        string? capturedUrl = null;
        var handler = new MockHttpMessageHandler((req, _) =>
        {
            capturedUrl = req.RequestUri?.PathAndQuery;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });

        var service = new ResendEmailService(new HttpClient(handler), CreateConfig(), _loggerMock.Object);

        await service.SendEmailConfirmationAsync("user@test.com", "http://example.com/confirm");

        Assert.Equal("/emails", capturedUrl);
    }

    [Fact]
    public async Task SendEmailConfirmationAsync_ApiError_ThrowsException()
    {
        var handler = MockHttpMessageHandler.WithStatusCode(HttpStatusCode.Unauthorized);
        var service = new ResendEmailService(new HttpClient(handler), CreateConfig(), _loggerMock.Object);

        await Assert.ThrowsAsync<Exception>(() =>
            service.SendEmailConfirmationAsync("user@test.com", "http://example.com/confirm"));
    }

    [Fact]
    public async Task SendEmailConfirmationAsync_UsesConfiguredFromAddress()
    {
        string? capturedBody = null;
        var handler = new MockHttpMessageHandler(async (req, _) =>
        {
            capturedBody = await req.Content!.ReadAsStringAsync();
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var config = CreateConfig(fromAddress: "noreply@myapp.com", fromName: "MyApp");
        var service = new ResendEmailService(new HttpClient(handler), config, _loggerMock.Object);

        await service.SendEmailConfirmationAsync("user@test.com", "http://example.com/confirm");

        Assert.Contains("MyApp", capturedBody!);
        Assert.Contains("noreply@myapp.com", capturedBody!);
    }

    [Fact]
    public async Task SendPasswordResetAsync_PostsToEmailsEndpoint()
    {
        string? capturedUrl = null;
        var handler = new MockHttpMessageHandler((req, _) =>
        {
            capturedUrl = req.RequestUri?.PathAndQuery;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });

        var service = new ResendEmailService(new HttpClient(handler), CreateConfig(), _loggerMock.Object);

        await service.SendPasswordResetAsync("user@test.com", "http://example.com/reset");

        Assert.Equal("/emails", capturedUrl);
    }

    [Fact]
    public async Task SendPasswordResetAsync_ApiError_ThrowsException()
    {
        var handler = MockHttpMessageHandler.WithStatusCode(HttpStatusCode.InternalServerError);
        var service = new ResendEmailService(new HttpClient(handler), CreateConfig(), _loggerMock.Object);

        await Assert.ThrowsAsync<Exception>(() =>
            service.SendPasswordResetAsync("user@test.com", "http://example.com/reset"));
    }

    [Fact]
    public async Task SendPasswordResetAsync_UsesDefaultFromWhenNotConfigured()
    {
        string? capturedBody = null;
        var handler = new MockHttpMessageHandler(async (req, _) =>
        {
            capturedBody = await req.Content!.ReadAsStringAsync();
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var service = new ResendEmailService(new HttpClient(handler), CreateConfig(), _loggerMock.Object);

        await service.SendPasswordResetAsync("user@test.com", "http://example.com/reset");

        Assert.Contains("CvEvaluator", capturedBody!);
        Assert.Contains("onboarding@resend.dev", capturedBody!);
    }
}
