using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;

namespace TodoApi
{
    public static class QueueListener
    {
        [FunctionName("QueueListener")]
        public static async Task RunAsync([QueueTrigger("todos", Connection = "AzureWebJobsStorage")]Todo todo,
            [Blob("todos", Connection = "AzureWebJobsStorage")]CloudBlobContainer container,
            ILogger log)
        {
            await container.CreateIfNotExistsAsync();  // create the container 
            var blob = container.GetBlockBlobReference($"{todo.Id}.txt"); // create a txt file with the name specified
            await blob.UploadTextAsync($"Created a new task: {todo.TaskDescription}");  // add the following text to the text file and upload
            log.LogInformation($"C# Queue trigger function processed: {todo.TaskDescription}");
        }
    }
}
