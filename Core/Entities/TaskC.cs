using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Core.Entities
{
    public class TaskC
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public List<SubTask> SubTasks { get; set; }
        public List<TaskTag> TaskTags { get; set; }
        public int Priority { get; set; }
        public int State { get; set; }
        public bool Visible { get; set; }
        public int ElapsedTime { get; set; }
    }
}
