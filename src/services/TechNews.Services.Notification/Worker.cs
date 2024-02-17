using HandlebarsDotNet;
using MailKit.Net.Smtp;
using MimeKit;
using Serilog;
using TechNews.Common.Library.MessageBus;
using TechNews.Common.Library.Messages.Events;
using TechNews.Services.Notification.Configurations;
using TechNews.Services.Notification.Email.Templates.EmailConfirmation;

namespace TechNews.Services.Notification;

public class Worker
(
    IMessageBus bus
) : BackgroundService
{
    private const string HostAndPort = "https://localhost:7283";
    private const string LogoUrl = "https://tech04.blob.core.windows.net/logo/TechNews.png";

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.Information("Worker has started");
        bus.Consume<UserRegisteredEvent>(EnvironmentVariables.BrokerConfirmEmailQueueName, ExecuteAfterConsumed);
        return Task.CompletedTask;
    }

    private void ExecuteAfterConsumed(UserRegisteredEvent? message)
    {
        Log.Information("New message received: {@message}", message);

        if (message is null)
        {
            Log.Warning("Message is null. Skipping e-mail notification");
            return;
        }

        try
        {
            var mailMessage = GenerateMessage(message);
            SendEmail(mailMessage);
        }
        catch (Exception e)
        {
            Log.Error(e, "Error while sending notification");
            throw;
        }
    }

    private MimeMessage GenerateMessage(UserRegisteredEvent userRegisteredDetails)
    {
        var mailMessage = new MimeMessage();
        mailMessage.From.Add(new MailboxAddress("TechNews", EnvironmentVariables.SmtpEmail));
        mailMessage.To.Add(new MailboxAddress(userRegisteredDetails.UserName, userRegisteredDetails.Email));
        mailMessage.Subject = "TechNews - Email confirmation";
        
        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = GetHtmlTemplate(
                new EmailConfirmationTemplateModel(
                    HostAndPort: HostAndPort, 
                    LogoUrl: LogoUrl, 
                    UserName: userRegisteredDetails.UserName ?? userRegisteredDetails.Email, 
                    EmailBase64: Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(userRegisteredDetails.Email)),
                    ValidateEmailTokenBase64: Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(userRegisteredDetails.ValidateEmailToken))
                )
            )
        };

        mailMessage.Body = bodyBuilder.ToMessageBody();

        return mailMessage;
    }

    private void SendEmail(MimeMessage message)
    {
        using var client = new SmtpClient();
        client.Connect(EnvironmentVariables.SmtpHost, EnvironmentVariables.SmtpPort, false);
        client.AuthenticationMechanisms.Remove("XOAUTH2");
        client.Authenticate(EnvironmentVariables.SmtpEmail, EnvironmentVariables.SmtpPassword);

        client.Send(message);
        client.Disconnect(true);
    }

    private static string GetHtmlTemplate(EmailConfirmationTemplateModel emailConfirmationTemplateModel)
    {
        //TODO: check if it will work on an app running after publish
        var sourceTemplate = File.ReadAllText("./Email/Templates/EmailConfirmation/EmailConfirmationTemplate.html");
        var template = Handlebars.Compile(sourceTemplate);

        return template(emailConfirmationTemplateModel);
    }
}
