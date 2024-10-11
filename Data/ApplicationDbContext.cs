using Microsoft.EntityFrameworkCore;
using LinenManagementSystem.Models;

namespace LinenManagementSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Define the DbSet for CartLog and other entities
        public DbSet<CartLog> CartLog { get; set; }
        public DbSet<Carts> Carts { get; set; }
        public DbSet<Linen> Linens { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Employees> Employees { get; set; }
        public DbSet<CartLogDetail> CartLogDetails { get; set; }

        // Add additional DbSets as needed
    }
}
