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
        public DbSet<CartLog> CartLogs { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Linen> Linens { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Employee> Employees { get; set; }

        // Add additional DbSets as needed
    }
}
