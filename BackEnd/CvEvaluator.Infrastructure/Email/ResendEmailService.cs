using System.Net.Http.Json;
using CvEvaluator.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CvEvaluator.Infrastructure.Email;

public class ResendEmailService : IEmailService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ResendEmailService> _logger;

    public ResendEmailService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<ResendEmailService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        _httpClient.BaseAddress = new Uri("https://api.resend.com/");
        _httpClient.DefaultRequestHeaders.Add(
            "Authorization",
            $"Bearer {_configuration["Email:ApiKey"]}");
    }

    public async Task SendEmailConfirmationAsync(string toEmail, string confirmationLink, CancellationToken ct = default)
    {
        var fromAddress = _configuration["Email:FromAddress"] ?? "onboarding@resend.dev";
        var fromName = _configuration["Email:FromName"] ?? "CvEvaluator";

        var payload = new
        {
            from = $"{fromName} <{fromAddress}>",
            to = new[] { toEmail },
            subject = "Confirm your email — CvEvaluator",
            html = BuildConfirmationHtml(confirmationLink)
        };

        await SendAsync(payload, ct);
        _logger.LogInformation("Confirmation email sent to {Email}", toEmail);
    }

    public async Task SendPasswordResetAsync(string toEmail, string resetLink, CancellationToken ct = default)
    {
        var fromAddress = _configuration["Email:FromAddress"] ?? "onboarding@resend.dev";
        var fromName = _configuration["Email:FromName"] ?? "CvEvaluator";

        var payload = new
        {
            from = $"{fromName} <{fromAddress}>",
            to = new[] { toEmail },
            subject = "Reset your password — CvEvaluator",
            html = BuildPasswordResetHtml(resetLink)
        };

        await SendAsync(payload, ct);
        _logger.LogInformation("Password reset email sent to {Email}", toEmail);
    }

    private async Task SendAsync(object payload, CancellationToken ct)
    {
        var response = await _httpClient.PostAsJsonAsync("emails", payload, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Resend API error {Status}: {Body}", response.StatusCode, body);
            throw new Exception($"Failed to send email: {response.StatusCode}");
        }
    }

    private static string BuildConfirmationHtml(string link)
    {
        return $"""
        <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;">
            <h2 style="color: #2563eb;">Welcome to CvEvaluator</h2>
            <p>Thanks for signing up! Please confirm your email address by clicking the button below:</p>
            <div style="text-align: center; margin: 30px 0;">
                <a href="{link}"
                   style="background-color: #2563eb; color: white; padding: 12px 30px; text-decoration: none; border-radius: 8px; font-weight: bold;">
                    Confirm Email
                </a>
            </div>
            <p style="color: #6b7280; font-size: 14px;">
                If the button doesn't work, copy and paste this link into your browser:<br/>
                <a href="{link}" style="color: #2563eb;">{link}</a>
            </p>
            <p style="color: #9ca3af; font-size: 12px; margin-top: 30px;">
                If you didn't create an account, you can safely ignore this email.
            </p>
        </div>
        """;
    }

    private static string BuildPasswordResetHtml(string link)
    {
        return $"""
        <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;">
            <h2 style="color: #2563eb;">Reset your password</h2>
            <p>We received a request to reset your password. Click the button below to choose a new one:</p>
            <div style="text-align: center; margin: 30px 0;">
                <a href="{link}"
                   style="background-color: #2563eb; color: white; padding: 12px 30px; text-decoration: none; border-radius: 8px; font-weight: bold;">
                    Reset Password
                </a>
            </div>
            <p style="color: #6b7280; font-size: 14px;">
                If the button doesn't work, copy and paste this link into your browser:<br/>
                <a href="{link}" style="color: #2563eb;">{link}</a>
            </p>
            <p style="color: #9ca3af; font-size: 12px; margin-top: 30px;">
                This link expires in 1 hour. If you didn't request a password reset, you can safely ignore this email.
            </p>
        </div>
        """;
    }
}
