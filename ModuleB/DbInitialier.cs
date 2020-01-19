using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleB
{
    class DbInitialier : MigrateDatabaseToLatestVersion<DB, InitializerConfiguration> { }
    class InitializerConfiguration : DbMigrationsConfiguration<DB>
    {
        public InitializerConfiguration()
        {
            this.AutomaticMigrationsEnabled = true;
            this.AutomaticMigrationDataLossAllowed = true;
        }
        protected override void Seed(DB context)
        {
            var models = new List<Model>();
            for (int i = 0; i < 100; i++)
            {
                models.Add(new Model() { Id = i + 1, Name = $"Name{i + 1}" });
            }
            context.Models.AddOrUpdate(models.ToArray());
            context.SaveChanges();
            base.Seed(context);
        }
    }
}
