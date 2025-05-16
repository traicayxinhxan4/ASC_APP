﻿using ASC.Model.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ASC.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public virtual DbSet<MasterDataKey> MasterDataKeys { get; set; }
        public virtual DbSet<MasterDataValue> MasterDataValues { get; set; }
        public virtual DbSet<ServiceRequest> ServiceRequests { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            Database.Migrate();
            //Database.EnsureCreated();
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<MasterDataKey>().HasKey(c => new { c.PartitionKey, c.RowKey });
            builder.Entity<MasterDataValue>().HasKey(c => new { c.PartitionKey, c.RowKey });
            builder.Entity<ServiceRequest>().HasKey(c => new { c.PartitionKey, c.RowKey });
            base.OnModelCreating(builder);
            builder.Entity<Product>().HasData(
                new Product { ProductId = 1, Name = "Xe TOYOTA" }
            );
        }
    }
}
