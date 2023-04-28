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
        private static readonly string WorkerName = Environment.MachineName;
        private static readonly long WorkCount = 100L;
 
        public static async Task Main(string[] args)
        {
            var confbuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            var config = confbuilder.Build();    
            // create zeebe client
            var client = CamundaCloudClientBuilder
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

                // blocks main thread, so that worker can run
                signal.WaitOne();
            }
             
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
            Console.WriteLine("Variables deserialized"); 

            string cardNumber = variables.cardNumber;
            Console.WriteLine("cardNumber " + cardNumber); 
            string cvc = variables.cvc;
            Console.WriteLine("cvc " + cvc); 
            string expiryDate = variables.expiryDate;
            Console.WriteLine("expiryDate " + expiryDate); 
            double amount = variables.openAmount;
            Console.WriteLine("openAmount " + amount); 

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
 