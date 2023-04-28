using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Zeebe.Client;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;
using Zeebe.Client.Impl.Builder;
using NLog.Extensions.Logging;
using CamundaTraining.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System.Text.Json;

 
namespace CamundaTraining.Workers
{
    internal class Program
    {
        private static readonly string JobCreditDeduction = "credit-deduction";
        private static readonly string JobChargeCreditCard = "credit-card-charging";
        private static readonly string JobPaymentInvocation = "payment-invocation";
        private static readonly string JobPaymentCompleted = "payment-completion";
        private static readonly string WorkerName = Environment.MachineName;
        private static readonly long WorkCount = 100L;
        private static IZeebeClient client;
 
        public static async Task Main(string[] args)
        {
            var confbuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            var config = confbuilder.Build();    
            // create zeebe client
            client = CamundaCloudClientBuilder
                .Builder()
                .UseClientId(config["CLIENT_ID"])
                .UseClientSecret(config["CLIENT_SECRET"])
                .UseContactPoint(config["ZEEBE_ADDRESS"])
                .UseLoggerFactory(new NLogLoggerFactory())
                .Build();

            var topology = await client.TopologyRequest()
                .Send();
            Console.WriteLine(topology);
 
            // open job worker
            using (var signal = new EventWaitHandle(false, EventResetMode.AutoReset))
            {
                client.NewWorker()
                      .JobType(JobCreditDeduction)
                      .Handler(HandleJobCreditDeduction)
                      .MaxJobsActive(5)
                      .Name(WorkerName)
                      .AutoCompletion()
                      .PollInterval(TimeSpan.FromSeconds(1))
                      .Timeout(TimeSpan.FromSeconds(10))
                      .Open();

                client.NewWorker()
                      .JobType(JobChargeCreditCard)
                      .Handler(HandleJobChargeCreditCard)
                      .MaxJobsActive(5)
                      .Name(WorkerName)
                      .AutoCompletion()
                      .PollInterval(TimeSpan.FromSeconds(1))
                      .Timeout(TimeSpan.FromSeconds(10))
                      .Open();

                client.NewWorker()
                      .JobType(JobPaymentInvocation)
                      .Handler(HandleJobPaymentInvocation)
                      .MaxJobsActive(5)
                      .Name(WorkerName)
                      .AutoCompletion()
                      .PollInterval(TimeSpan.FromSeconds(1))
                      .Timeout(TimeSpan.FromSeconds(10))
                      .Open();

                client.NewWorker()
                      .JobType(JobPaymentCompleted)
                      .Handler(HandleJobPaymentCompletion)
                      .MaxJobsActive(5)
                      .Name(WorkerName)
                      .AutoCompletion()
                      .PollInterval(TimeSpan.FromSeconds(1))
                      .Timeout(TimeSpan.FromSeconds(10))
                      .Open();

                // blocks main thread, so that worker can run
                signal.WaitOne();
            }
             
        }
        //Start Payment Process with message
        private async static void HandleJobPaymentInvocation(IJobClient jobClient, IJob job)
        {
            var variablesObj = JsonSerializer.Deserialize<MyVar>(job.Variables);
            await client.NewPublishMessageCommand()
                    .MessageName("paymentRequestMessage")
                    .CorrelationKey("useless") //start process doesn't require 
                    .Variables(job.Variables)
                    .Send();
        }  

        //Send correlation message back to OrderProcess
        private async static void HandleJobPaymentCompletion(IJobClient jobClient, IJob job)
        {
            var variablesObj = JsonSerializer.Deserialize<MyVar>(job.Variables);
            await client.NewPublishMessageCommand()
                    .MessageName("paymentCompletedMessage")
                    .CorrelationKey(variablesObj.orderId)
                    .Variables(job.Variables)
                    .Send();
        }   

  

        private static void HandleJobCreditDeduction(IJobClient jobClient, IJob job)
        {
            // business logic
            var jobKey = job.Key;
            // Deserialize the JSON-formatted string into a C# object
            var variablesObj = JsonSerializer.Deserialize<MyVar>(job.Variables);

            var customerId = variablesObj.customerId;
            var orderTotal = variablesObj.orderTotal;

            // Create an instance of CustomerService
            var customerService = new CustomerService();

            // Call the GetCustomerCredit method and store the result in a variable
            var customerCredit = customerService.GetCustomerCredit(customerId);

            var openAmount = customerService.DeductCredit(customerCredit, orderTotal);

            Dictionary<string, object> variablesOut = new Dictionary<string, object>();
            variablesOut["customerCredit"] = customerCredit;
            variablesOut["openAmount"] = openAmount;

            Console.WriteLine("Handling job: " + job + " customerId: " + customerId);

            jobClient.NewCompleteJobCommand(jobKey)
                    .Variables(JsonSerializer.Serialize(variablesOut))
                    .Send()
                    .GetAwaiter()
                    .GetResult();
            
        }
        private static void HandleJobChargeCreditCard(IJobClient jobClient, IJob job)
        {
            // business logic
            var jobKey = job.Key;
            
            var variables = JsonSerializer.Deserialize<MyVar>(job.Variables);
            string cardNumber = variables.cardNumber;
            string cvc = variables.cvc;
            string expiryDate = variables.expiryDate;
            double amount = variables.openAmount;

            new CreditCardService().ChargeAmount(cardNumber, cvc, expiryDate, amount);

            Console.WriteLine("Managing job: " + job); 
            jobClient.NewCompleteJobCommand(jobKey)
                    .Variables("{\"foo\":2}")
                    .Send()
                    .GetAwaiter()
                    .GetResult();
            }
        
    }
}
