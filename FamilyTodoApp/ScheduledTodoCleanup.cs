using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;  // supports use of CloudTable which we use to query the table storage
using System.Threading.Tasks;

namespace TodoApi
{
    public static class ScheduledTodoCleanup
    {
        [FunctionName("ScheduledTodoCleanup")]
        public static async Task RunAsync([TimerTrigger("* 8 * * * ")]TimerInfo myTimer,
             [Table("todos", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            ILogger log)
        {
            var query = new TableQuery<TodoTableEntity>();
            var segment = await todoTable.ExecuteQuerySegmentedAsync(query, null);
            var deleted = 0;
            foreach (var todo in segment)
            {
                if (todo.IsCompleted)
                {
                    await todoTable.ExecuteAsync(TableOperation.Delete(todo));
                    deleted++;
                }
            }

            log.LogInformation($"Deleted {deleted} items at {DateTime.Now}");
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}
