using Microsoft.EntityFrameworkCore;
using PetHealthEcosystem.Api.Models;

namespace PetHealthEcosystem.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Pet> Pets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Pet>().ToTable("PETS");
            modelBuilder.Entity<Pet>().HasKey(p => p.Id);
            modelBuilder.Entity<Pet>().Property(p => p.Name).IsRequired().HasMaxLength(100);
            modelBuilder.Entity<Pet>().Property(p => p.Breed).HasMaxLength(50);
            modelBuilder.Entity<Pet>().Property(p => p.Species).HasMaxLength(30);
            modelBuilder.Entity<Pet>().Property(p => p.Size).HasMaxLength(20);
            modelBuilder.Entity<Pet>().Property(p => p.Weight).HasPrecision(5, 2);
            modelBuilder.Entity<Pet>().Property(p => p.TutorName).IsRequired().HasMaxLength(100);
            modelBuilder.Entity<Pet>().Property(p => p.TutorPhone).HasMaxLength(30);
            modelBuilder.Entity<Pet>().Property(p => p.SurgeryType).HasMaxLength(100);
            modelBuilder.Entity<Pet>().Property(p => p.RecoveryStatus).HasMaxLength(50);
            modelBuilder.Entity<Pet>().Ignore(p => p.Links);
        }
    }
}
