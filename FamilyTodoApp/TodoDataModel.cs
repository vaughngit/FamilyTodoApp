using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace TodoApi
{
    public class Todo
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("n");
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
        public string TaskDescription { get; set; }
        public string AssignedTo { get; set; }
        public bool IsStarted { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class TodoCreateModel
    {
        public string TaskDescription { get; set; }
        public string AssignedTo { get; set; }
    }

    public class TodoUpdateModel
    {
        public string TaskDescription { get; set; }
        public bool IsStarted { get; set; }
        public bool IsCompleted { get; set; }
        public string AssignedTo { get; set; }
    }

    public class TodoTableEntity : TableEntity
    {
        public DateTime CreatedTime { get; set; }
        public string TaskDescription { get; set; }
        public string AssignedTo { get; set; }
        public bool IsStarted { get; set; }
        public bool IsCompleted { get; set; }
    }

    public static class Mappings
    {
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
                AssignedTo = todo.AssignedTo
            };

        }

        public static Todo ToTodo(this TodoTableEntity todo)
        {
            return new Todo()
            {
                Id = todo.RowKey,
                CreatedTime = todo.CreatedTime,
                IsStarted = todo.IsStarted,
                IsCompleted = todo.IsCompleted,
                TaskDescription = todo.TaskDescription,
                AssignedTo = todo.AssignedTo
            };
        }
    }

}
