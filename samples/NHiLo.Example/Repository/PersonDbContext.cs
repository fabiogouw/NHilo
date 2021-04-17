using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using NHiLo.Example.Model;
using System.ComponentModel.DataAnnotations.Schema;

namespace NHiLo.Example.Repository
{
    public class PersonDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=blogging.db");
        }

        public DbSet<Person> People { get; set; }
        public DbSet<Contact> Contacts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // IDENTITY option is the Evil on Earth :-)
            modelBuilder.Entity<Person>().Property(p => p.Id).ValueGeneratedNever();
            modelBuilder.Entity<Person>().Property(p => p.Name).HasMaxLength(100);
            modelBuilder.Entity<Contact>().Property(p => p.Id).ValueGeneratedNever();
            modelBuilder.Entity<Contact>().Property(p => p.TelephoneNumber).HasMaxLength(10);
            //modelBuilder.Entity<Person>().HasMany(p => p.Contacts).OnDelete(DeleteBehavior.ClientCascade);
        }
    }
}
