﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentClawlet1
{
    class MyDbContext : DbContext
    {
        public MyDbContext() : base("MydbContext")
        {

            //
            // TODO: Add constructor logic here
            //
        }
        public DbSet<News> News { get; set; }
    }
}
