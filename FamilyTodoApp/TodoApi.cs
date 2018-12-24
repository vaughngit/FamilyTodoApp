using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;

namespace TodoApi
{
    public static class TodoApi
    {

        // static List<Todo> items = new List<Todo>();

        [FunctionName("CreateTodo")]
        public static async Task<IActionResult> CreateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")]HttpRequest req,
            [Table("todos", Connection = "AzureWebJobsStorage")]IAsyncCollector<TodoTableEntity> todoTable, // todo out parameter 
            [Queue("todos", Connection = "AzureWebJobsStorage")]IAsyncCollector<Todo> todoQueue, // creates queue out parameter named todos that will allow a serialized json todo object message to be sent to the queue
            ILogger log) //The IAsysncCollector allows you to create a new allows us to add a new row in the TodoTableEntity method within an async method
        {
            log.LogInformation("Create a new todo list item");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<TodoCreateModel>(requestBody);

            var todo = new Todo() { TaskDescription = input.TaskDescription, AssignedTo = input.AssignedTo, IsDue = input.IsDue };
            //items.Add(todo);  // remove this as we are no longer adding the todo item to an in memory list
            await todoTable.AddAsync(todo.ToTableEntity());  //Convertes the item into a ToTableEntity using the mapping method the model class and adds it to tablestorage table
            await todoQueue.AddAsync(todo); // send the todo item to the queue
            return new OkObjectResult(todo);
        }


        [FunctionName("GetTodos")]
        public static async Task<IActionResult> GetTodos(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo")]HttpRequest req,
            [Table("todos", Connection = "AzureWebJobsStorage")]CloudTable todoTable, ILogger log)
        {
            log.LogInformation("Getting todo list items");
            var query = new TableQuery<TodoTableEntity>();
            var segment = await todoTable.ExecuteQuerySegmentedAsync(query, null); //CloudTable allows create a simple table query to request all the rows in the table 
            return new OkObjectResult(segment.Select(Mappings.ToTodo)); //converts the results back from TotableEntities to TodoItems
        }

        [FunctionName("GetTodoById")]
        public static IActionResult GetToById([HttpTrigger(
            AuthorizationLevel.Anonymous,"get",Route = "todo/{id}")]HttpRequest req,
            [Table("todos", "TODO", "{id}", Connection = "AzureWebJobsStorage")] TodoTableEntity todo, ILogger log, string id)
        // The "TODO" and "{id}" parameters are the partition key and row key for the row we are looking up in tablestorage
        {
            log.LogInformation("Getting todo item by id");
            //var todo = items.FirstOrDefault(t => t.Id == id);
            if (todo == null)
            {
                log.LogInformation($"Item {id} not found");
                return new NotFoundResult(); //Http404
            }
            return new OkObjectResult(todo.ToTodo()); //Http200
        }

        [FunctionName("UpdateTodo")]
        public static async Task<IActionResult> UpdateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todo/{id}")]HttpRequest req,
            [Table("todos", Connection = "AzureWebJobsStorage")]CloudTable todoTable, ILogger log, string id)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updated = JsonConvert.DeserializeObject<TodoUpdateModel>(requestBody);

            var findOperation = TableOperation.Retrieve<TodoTableEntity>("TODO", id);  // try to find the row in the table using its partition and rowid
            var findResult = await todoTable.ExecuteAsync(findOperation);
            if (findResult.Result == null)
            {
                return new NotFoundResult();
            }
            var existingRow = (TodoTableEntity)findResult.Result;
            existingRow.IsCompleted = updated.IsCompleted;  // if found update the row with the http request
            existingRow.IsStarted = updated.IsStarted;
       

            if (!string.IsNullOrEmpty(updated.TaskDescription))
            {
                existingRow.TaskDescription = updated.TaskDescription; //if found update the row with the http request
            }

            if (!string.IsNullOrEmpty(updated.AssignedTo))
            {
                existingRow.AssignedTo = updated.AssignedTo; //if found update the row with the http request
            }

            if (updated.IsDue > DateTime.MinValue) 
            {
                existingRow.IsDue = updated.IsDue;
            }


            var replaceOperation = TableOperation.Replace(existingRow);
            await todoTable.ExecuteAsync(replaceOperation); // replace the previous row with the updated row

            return new OkObjectResult(existingRow.ToTodo());
        }

        // tablestorage way of protecting you from concurrenctly issues
        // Creates delete operation to delete a row from the TODO table with an RowKey value of id regardless of the version (ETag)
        // returns simple 200 ok 
        // returns 404 Not found if you attempt to delete a non-existant ite
        [FunctionName("DeleteTodo")]
        public static async Task<IActionResult> DeleteTodo(
                 [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todo/{id}")]HttpRequest req,
                 [Table("todos", Connection = "AzureWebJobsStorage")] CloudTable todoTable, ILogger log, string id)
        {
            var deleteOperation = TableOperation.Delete(new TableEntity()
            { PartitionKey = "TODO", RowKey = id, ETag = "*" });
            try
            {
                var deleteResult = await todoTable.ExecuteAsync(deleteOperation);
                log.LogInformation("Deleted "+req.Body);
            }
            catch (StorageException e) when (e.RequestInformation.HttpStatusCode == 404)
            {
                return new NotFoundResult();
            }
            return new OkResult();
        }



    }


}
