using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp1
{
    public class AppSettings
    {
        public AzureServiceBus AzureServiceBus { get; set; }
        public EmailService EmailService { get; set; }
    }

    public class AzureServiceBus
    {
        public string QueueConnectionString { get; set; }
        public string QueueName { get; set; }
    }

    public class EmailService
    {
        public string Sender { get; set; }
        public string Subject { get; set; }
        public string Url { get; set; }
    }
}
