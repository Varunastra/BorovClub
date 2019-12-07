using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BorovClub.Models;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using System.IO;
using System.Reflection;
using RazorLight;
using Microsoft.Extensions.Configuration;

namespace BorovClub.Data
{
    public class BackgroundMessagesService: BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<BackgroundMessagesService> _logger;
        private readonly IConfiguration _configuration;

        public BackgroundMessagesService(IServiceScopeFactory serviceScopeFactory, ILogger<BackgroundMessagesService> logger, 
            IConfiguration configuration)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _configuration = configuration;
        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var unreadMessages = dbContext.Messages.Include(m => m.Sender)
                        .Include(m => m.Reciever).Where(m => m.Status == MessageStatus.Unread && m.EmailSent == false).ToList();

                    var currentTime = DateTime.UtcNow;

                    foreach (var message in unreadMessages)
                    {
                        if ((currentTime - message.When).TotalMinutes >= 120)
                        {
                            _logger.LogInformation("Message: " + message.Text + " From " + message.Sender.UserName + " To " + message.Reciever.UserName);
                            await SendEmailNotification(message);
                            message.EmailSent = true;

                            await dbContext.SaveChangesAsync();
                        }
                    }
                }
                await Task.Delay(10000, stoppingToken);
            }
        }

        private async Task SendEmailNotification(Message message)
        {
            MailAddress from = new MailAddress("admin@borovclub.com", "Varunastra");
            MailAddress to = new MailAddress(message.Reciever.Email);

            var template = await BuildEmailTemplate(message);

            MailMessage m = new MailMessage(from, to)
            {
                Subject = "New Message - BorovClub",
                Body = template,
                IsBodyHtml = true
            };

            SmtpClient smtp = new SmtpClient(_configuration["SMTP:Server"], 587)
            {
                Credentials = new NetworkCredential("apikey", _configuration["SMTP:ApiKey"]),
                EnableSsl = true
            };

            await smtp.SendMailAsync(m);
            _logger.LogInformation("Message sent");
        }

        private async Task<string> BuildEmailTemplate(Message model)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("BorovClub.EmailMessageNotification.cshtml");
            TextReader tr = new StreamReader(stream);
            string razorTemplate = tr.ReadToEnd();

            var engine =
                new RazorLightEngineBuilder().UseMemoryCachingProvider().UseFileSystemProject("C:/tor").Build();

            var cacheResult = engine.Handler.Cache.RetrieveTemplate("UnreadMessageTemplate");
            string renderedTemplate;

            if (cacheResult.Success)
            {
                renderedTemplate = await engine.RenderTemplateAsync(cacheResult.Template.TemplatePageFactory.Invoke(), model);
            }
            else
            {
                renderedTemplate = await engine.CompileRenderStringAsync("UnreadMessageTemplate", razorTemplate, model);
            }

            return renderedTemplate;
        }

    }
}