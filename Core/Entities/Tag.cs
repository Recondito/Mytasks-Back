using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Entities
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<TaskTag> TaskTags { get; set; }
    }
}
