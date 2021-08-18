using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SampleShared;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class SendMessageHandler : BackgroundService
    {
        private readonly AppSettings _appSettings;
        private ServiceBusClient client;
        private ServiceBusProcessor processor;

        public SendMessageHandler(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings?.Value ?? throw new ArgumentNullException(nameof(appSettings));
        }

        // handle received messages
        public async Task MessageHandler(ProcessMessageEventArgs args)
        {
            try
            {
                var body = args.Message.Body.ToString();

                // deserialize message
                var personObject = JsonConvert.DeserializeObject<Person>(body);

                var msg = $"Hello {personObject.FirstName} {personObject.LastName}, Welcome to Azure Bus Service testing with .NET Core 5.";

                // send email
                await SendingEmail(personObject.Email, msg).ConfigureAwait(false);

                // complete the message. messages is deleted from the queue.
                await args.CompleteMessageAsync(args.Message).ConfigureAwait(false);
            }
            catch (Exception)
            {
                await args.AbandonMessageAsync(args.Message).ConfigureAwait(false);
            }
        }

        // handle any errors when receiving messages
        public Task ErrorHandler(ProcessErrorEventArgs args)
        {
            //Console.WriteLine(args.Exception.Message.ToString());
            Console.WriteLine($"Message handler encountered an exception: {args.Exception}");
            Console.WriteLine($"- ErrorSource: {args.ErrorSource}");
            Console.WriteLine($"- Entity Path: {args.EntityPath}");
            Console.WriteLine($"- FullyQualifiedNamespace: {args.FullyQualifiedNamespace}");

            return Task.CompletedTask;
        }

        // handle email sending
        private async Task<bool> SendingEmail(string email, string message)
        {
            bool isSent = false;

            try
            {
                var emailUrl = _appSettings.EmailService.Url;

                var emailModel = new EmailModel
                {
                    To = email,
                    from = _appSettings.EmailService.Sender,
                    mail_subject = _appSettings.EmailService.Subject,
                    mail_message = message,
                    attachement = "",
                    Cc = "",
                    Bcc = "",
                    isBodyHtml = true
                };

                var dataAsString = JsonConvert.SerializeObject(emailModel);

                var stringContent = new StringContent(dataAsString, Encoding.UTF8, "application/json");

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = await client.PostAsync(emailUrl, stringContent);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResult = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<EmailResponse>(jsonResult);
                        if (result != null && result.response_code == "00")
                        {
                            Console.WriteLine($"[success]: Email successfully sent to {email}");
                            isSent = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                isSent = false;
            }

            return isSent;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // get queue connection and name
            var queueConnString = _appSettings.AzureServiceBus.QueueConnectionString;
            var queueName = _appSettings.AzureServiceBus.QueueName;

            // create the options to use for configuring the processor
            var options = new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false,
                MaxConcurrentCalls = 2
            };

            // Create the client object that will be used to create sender and receiver objects
            client = new ServiceBusClient(queueConnString);

            // create a processor that we can use to process the messages
            processor = client.CreateProcessor(queueName, options);

            // add handler to process messages
            processor.ProcessMessageAsync += MessageHandler;

            // add handler to process any errors
            processor.ProcessErrorAsync += ErrorHandler;

            // start processing 
            await processor.StartProcessingAsync().ConfigureAwait(false);

            Console.WriteLine($"**************************************************************");
            Console.WriteLine($"************{nameof(SendMessageHandler)} service has started************");
            Console.WriteLine($"**************************************************************");
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            try
            {
                await processor.StopProcessingAsync().ConfigureAwait(false);

                Console.WriteLine($"**************************************************************");
                Console.WriteLine($"************{nameof(SendMessageHandler)} service has stopped************");
                Console.WriteLine($"**************************************************************");
            }
            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                if (processor != null)
                {
                    await processor.DisposeAsync().ConfigureAwait(false);
                }

                if(client != null)
                {
                    await client.DisposeAsync().ConfigureAwait(false);
                }
            }
        }
    }
}
