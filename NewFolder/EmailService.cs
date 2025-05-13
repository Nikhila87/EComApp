using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Options;
using EComAPI.Models;
using EComAPI.NewFolder;

public class SendGridEmailService : IEmailService
{
    private readonly SendGridSettings _settings;
    private  string _sendGridApiKey;
    public SendGridEmailService(IOptions<SendGridSettings> settings)
    {
        _settings = settings.Value;
        
    }

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        _sendGridApiKey = Environment.GetEnvironmentVariable("SendGridApiKey");
        Console.WriteLine("SendGridApiKey from env: " + Environment.GetEnvironmentVariable("SendGridApiKey"));
        var client = new SendGridClient(_sendGridApiKey);
        var from = new EmailAddress(_settings.FromEmail, _settings.FromName);
        var to = new EmailAddress(toEmail);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, "", message);
        var response = await client.SendEmailAsync(msg);
        Console.WriteLine($"SendGrid Response Status: {response.StatusCode}");
        var responseBody = await response.Body.ReadAsStringAsync();
        Console.WriteLine($"SendGrid Response Body: {responseBody}");
    }
}
