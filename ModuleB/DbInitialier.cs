using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleB
{
    class DbInitialier: CreateDatabaseIfNotExists<DB>
    {
        protected override void Seed(DB context)
        {
            for (int i = 0; i < 100; i++)
            {
                context.Models.Add(new Model() { Name = $"Name{i + 1}" });
            }
            context.SaveChanges();
            base.Seed(context);
        }
    }
}
