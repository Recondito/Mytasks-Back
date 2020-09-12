using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Entities
{
    public class TaskTag
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public TaskC Task { get; set; }
        public int TagId { get; set; }
        public Tag Tag { get; set; }
    }
}
