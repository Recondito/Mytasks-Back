using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.DTOs
{
    public class TaskDtoForCreate
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public List<SubTaskDtoForCreate> SubTasks { get; set; }
        public List<int> TagIds { get; set; }
        public int Priority { get; set; }
    }
}
