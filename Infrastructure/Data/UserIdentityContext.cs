using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Data
{
    public class UserIdentityContext : IdentityDbContext
    {
        public UserIdentityContext(DbContextOptions<UserIdentityContext> options) : base(options)
        {
        }
    }
}
