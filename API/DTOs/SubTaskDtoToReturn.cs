using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.DTOs
{
    public class SubTaskDtoToReturn
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Priority { get; set; }
        public int State { get; set; }
    }
}
