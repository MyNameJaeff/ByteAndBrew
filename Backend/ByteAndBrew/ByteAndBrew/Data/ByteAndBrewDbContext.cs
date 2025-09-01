using Byte___Brew.Models;
using Microsoft.EntityFrameworkCore;

namespace Byte___Brew.Data
{
    public class ByteAndBrewDbContext : DbContext
    {
        public ByteAndBrewDbContext(DbContextOptions<ByteAndBrewDbContext> options) : base(options) { }

        public DbSet<Table> Tables => Set<Table>();
        public DbSet<Admin> Admins => Set<Admin>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    }
}
