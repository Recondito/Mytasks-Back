using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.DTOs
{
    public class ChangePasswordDto
    {
        public string currentPassword { get; set; }
        public string newPassword { get; set; }
    }
}
