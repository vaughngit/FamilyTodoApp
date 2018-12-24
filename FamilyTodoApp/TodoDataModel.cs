using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace TodoApi
{
    //Todo non-persistent .net data model 
    public class Todo
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("n");
        public DateTime CreatedTime { get; set; } = DateTime.Today;
        public string TaskDescription { get; set; }
        public string AssignedTo { get; set; }
        public bool IsStarted { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime IsDue { get; set; } = DateTime.Today; 
    }

    // Fields exposed to end-user when creating a Todo object
    public class TodoCreateModel
    {
        public string TaskDescription { get; set; }
        public string AssignedTo { get; set; }
        public DateTime IsDue { get; set; }
    }

    // Fields exposed to end-device when updating Todo object
    public class TodoUpdateModel
    {
        public string TaskDescription { get; set; }
        public bool IsStarted { get; set; }
        public bool IsCompleted { get; set; }
        public string AssignedTo { get; set; }
        public DateTime IsDue { get; set; }
    }

    // Azure Table Storage data model 
    public class TodoTableEntity : TableEntity
    {
        public DateTime CreatedTime { get; set; }
        public string TaskDescription { get; set; }
        public string AssignedTo { get; set; }
        public bool IsStarted { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime IsDue { get; set; }
    }

    // Todo persist storage converstion: 
    public static class Mappings
    {
        // Convert Todo to Azure Table Storage Format
        public static TodoTableEntity ToTableEntity(this Todo todo)
        {
            return new TodoTableEntity()
            {
                PartitionKey = "TODO",
                RowKey = todo.Id,
                CreatedTime = todo.CreatedTime,
                IsStarted = todo.IsStarted,
                IsCompleted = todo.IsCompleted,
                TaskDescription = todo.TaskDescription,
                AssignedTo = todo.AssignedTo,
                IsDue = todo.IsDue
            };

        }

        // Convert Todo from Azure Table Storage
        public static Todo ToTodo(this TodoTableEntity todo)
        {
            return new Todo()
            {
                Id = todo.RowKey,
                CreatedTime = todo.CreatedTime,
                IsStarted = todo.IsStarted,
                IsCompleted = todo.IsCompleted,
                TaskDescription = todo.TaskDescription,
                AssignedTo = todo.AssignedTo,
                IsDue = todo.IsDue
            };
        }
    }

}
