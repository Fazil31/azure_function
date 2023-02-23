using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace Company.Function
{
    public static class func_notifyanomaly
    {
        private static string logicAppUri = @"https://prod-07.australiaeast.logic.azure.com:443/workflows/99250cea29094b0d9197ed1d0cf92ccc/triggers/manual/paths/invoke?api-version=2016-10-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=LiTb95Y7gKzIvCY8PXgz-2jgaSf0kHeUqUqvknfwL68";
        private static HttpClient httpClient = new HttpClient();

        [FunctionName("func_notifyanomaly")]
        public static async Task Run([EventHubTrigger("eh1312", Connection = "eventhub1312_RootManageSharedAccessKey_EVENTHUB")] EventData[] events, ILogger log)
        {
            var exceptions = new List<Exception>();

            foreach (EventData eventData in events)
            {
                try
                {
                    string messageBody = eventData.EventBody.ToString();
                    // Replace these two lines with your processing logic.
                    log.LogInformation($"C# Event Hub trigger function processed a message: {messageBody}");

                    var response = await httpClient.PostAsync(logicAppUri, new StringContent(messageBody, Encoding.UTF8, "application/json"));
                    log.LogInformation($"C# IoT Hub trigger function processed another message: {messageBody}");

                    await Task.Yield();
                }
                catch (Exception e)
                {
                    // We need to keep processing the rest of the batch - capture this exception and continue.
                    // Also, consider capturing details of the message that failed processing so it can be processed again later.
                    exceptions.Add(e);
                }
            }

            // Once processing of the batch is complete, if any messages in the batch failed processing throw an exception so that there is a record of the failure.

            if (exceptions.Count > 1)
                throw new AggregateException(exceptions);

            if (exceptions.Count == 1)
                throw exceptions.Single();
        }
    }
}
