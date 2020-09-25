using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using CRL.EFCore.Extensions;
namespace EFTest
{
    public class Context1 : DbContext
    {
        public DbSet<TestClass> Set1 { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var e = modelBuilder.Entity<TestClass>();
            e.ToTable("TestClass", "dbo");
            e.HasKey(b => b.Id);
            e.Property(b => b.Name).HasMaxLength(50) ;
            e.ConfigEntityTypeBuilder();
            base.OnModelCreating(modelBuilder);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var conn = "server=.;database=testDb; uid=sa;pwd=123;";
            optionsBuilder.UseSqlServer(conn);
            base.OnConfiguring(optionsBuilder);
        }
    }
}
