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
            Console.WriteLine("Handling job: " + job);

            jobClient.NewCompleteJobCommand(jobKey)
                    .Variables("{\"foo\":2}")
                    .Send()
                    .GetAwaiter()
                    .GetResult();
            
        }
        private static void HandleJobChargeCreditCard(IJobClient jobClient, IJob job)
        {
            // business logic
            var jobKey = job.Key;
            
            Console.WriteLine("Managing job: " + job); 
            jobClient.NewCompleteJobCommand(jobKey)
                    .Variables("{\"foo\":2}")
                    .Send()
                    .GetAwaiter()
                    .GetResult();
            }
        
    }
}
