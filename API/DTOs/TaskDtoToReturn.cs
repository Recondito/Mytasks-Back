using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.DTOs
{
    public class TaskDtoToReturn
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public List<SubTaskDtoToReturn> SubTasks { get; set; }
        public List<TagDtoToReturn> Tags { get; set; }
        public int Priority { get; set; }
        public int State { get; set; }
        public bool? Visible { get; set; }
        public int ElapsedTime { get; set; }
    }
}
