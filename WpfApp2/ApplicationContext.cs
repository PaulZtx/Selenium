using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WpfApp2
{
    class ApplicationContext : DbContext
    {
        public DbSet<VkNews> Vknews1 => Set<VkNews>();
        //public DbSet<VkNewsTwo> Vknews2 => Set<VkNewsTwo>();
        public DbSet<VkNewsThree> Vknews3 => Set<VkNewsThree>();
        public ApplicationContext() => Database.EnsureCreated();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=news.db");
        }
    }
}
