using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Entities
{
    public class SubTask
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public TaskC Task { get; set; }
        public string Name { get; set; }
        public int Priority { get; set; }
        public int State { get; set; }
    }
}
