using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace HealthDiary.Model.Context
{
    class ApplicationContext : DbContext
    {
        private string _databasePath;

        public DbSet<Unit> Units { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Gender> Genders { get; set; }
        public DbSet<Cup> Cups { get; set; }
        public DbSet<Plan> Plans { get; set; }
        public DbSet<PlanCompletion> PlanCompletions { get; set; }
        public DbSet<PhysicalActivityType> PhysicalActivityTypes { get; set; }
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<ProductInDish> ProductInDish { get; set; }

        public ApplicationContext(string databasePath)
        {
            _databasePath = databasePath;
            //TODO: Remove this test deleting
            //Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Filename={_databasePath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            /*modelBuilder
                .Entity<Dish>()
                .HasMany(d => d.Products)
                .WithMany(p => p.Dishes)
                .UsingEntity<ProductInDish>(
                   j => j
                    .HasOne(pt => pt.Product)
                    .WithMany(t => t.ProductInDish)
                    .HasForeignKey(pt => pt.ProductId),
                j => j
                    .HasOne(pt => pt.Dish)
                    .WithMany(p => p.ProductInDish)
                    .HasForeignKey(pt => pt.DishId),
                j =>
                {
                    j.HasKey(t => new { t.DishId, t.ProductId });
                    j.ToTable("ProductInDish");
                });*/

            modelBuilder.Entity<ProductInDish>()
                .HasKey(t => new { t.DishId, t.ProductId });
            modelBuilder.Entity<ProductInDish>()
                .HasOne(pd => pd.Dish)
                .WithMany(d => d.ProductInDish)
                .HasForeignKey(d => d.DishId);
            modelBuilder.Entity<ProductInDish>()
                .HasOne(pd => pd.Product)
                .WithMany(p => p.ProductInDish)
                .HasForeignKey(p => p.ProductId);
        }
    }
}
