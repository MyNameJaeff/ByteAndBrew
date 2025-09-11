using ByteAndBrew.Models;
using Microsoft.EntityFrameworkCore;

namespace ByteAndBrew.Data
{
    public class ByteAndBrewDbContext : DbContext
    {
        public ByteAndBrewDbContext(DbContextOptions<ByteAndBrewDbContext> options) : base(options) { }

        public DbSet<Table> Tables => Set<Table>();
        public DbSet<Admin> Admins => Set<Admin>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<MenuItem> MenuItems => Set<MenuItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure decimal precision for prices
            modelBuilder.Entity<MenuItem>()
                .Property(m => m.Price)
                .HasColumnType("decimal(18,2)");

            // Configure unique constraints
            modelBuilder.Entity<Table>()
                .HasIndex(t => t.TableNumber)
                .IsUnique();

            modelBuilder.Entity<Admin>()
                .HasIndex(a => a.Username)
                .IsUnique();

            // ✅ Correct booking relationships
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Customer)
                .WithMany(c => c.Bookings)
                .HasForeignKey(b => b.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Table)
                .WithMany(t => t.Bookings)
                .HasForeignKey(b => b.TableId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public void SeedData()
        {
            try
            {
                // Seed admin if it doesn't exist
                if (!Admins.Any())
                {
                    Admins.Add(new Admin
                    {
                        Username = "admin",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin")
                    });
                    SaveChanges();
                }

                // Seed menu items if none exist
                if (!MenuItems.Any())
                {
                    var menuItems = new List<MenuItem>
                    {
                        new MenuItem { Name="Espresso", Price=30, Description="Strong and bold espresso shot", IsPopular=true, ImageUrl="https://barista-espresso.se/cdn/shop/articles/espresso-101-lar-dig-grunderna-i-espresso-962632_1024x1024.webp?v=1718937966" },
                        new MenuItem { Name="Cappuccino", Price=40, Description="Espresso with steamed milk and foam", IsPopular=true, ImageUrl="https://guentercoffee.com/cdn/shop/articles/blog-header-cappuccino-zubereiten-anleitung_f1ce80fc-3b3c-43ec-84d6-26abe2482cf0.jpg?v=1738152927&width=1200" },
                        new MenuItem { Name="Latte", Price=45, Description="Smooth espresso with lots of steamed milk", IsPopular=false, ImageUrl="https://upload.wikimedia.org/wikipedia/commons/d/d8/Caffe_Latte_at_Pulse_Cafe.jpg" },
                        new MenuItem { Name="Mocha", Price=50, Description="Chocolate flavored coffee with whipped cream", IsPopular=true, ImageUrl="https://ichef.bbc.co.uk/ace/standard/1600/food/recipes/the_perfect_mocha_coffee_29100_16x9.jpg.webp" },
                        new MenuItem { Name="Americano", Price=35, Description="Espresso diluted with hot water", IsPopular=false, ImageUrl="https://johanochnystrom.se/cdn/shop/articles/johannystrom_americano.jpg?v=1683879013" },
                        new MenuItem { Name="Flat White", Price=42, Description="Espresso with velvety steamed milk", IsPopular=false, ImageUrl="https://www.foodandwine.com/thmb/xQZv2CX6FO5331PYK7uGPF1we9Q=/1500x0/filters:no_upscale():max_bytes(150000):strip_icc()/Partners-Flat-White-FT-BLOG0523-b11f6273c2d84462954c2163d6a1076d.jpg" },
                        new MenuItem { Name="Iced Coffee", Price=38, Description="Cold brewed coffee over ice", IsPopular=true, ImageUrl="https://cdn.loveandlemons.com/wp-content/uploads/2025/05/how-to-make-iced-coffee-at-home.jpg" },
                        new MenuItem { Name="Macchiato", Price=36, Description="Espresso with a small amount of foam", IsPopular=false, ImageUrl="https://coffeegeek.com/wp-content/uploads/2021/05/macchiatotraditonal-11.jpg" },
                        new MenuItem { Name="Chai Latte", Price=44, Description="Spiced tea with steamed milk", IsPopular=true, ImageUrl="https://cdn.loveandlemons.com/wp-content/uploads/2025/01/chai-latte.jpg" },
                        new MenuItem { Name="Hot Chocolate", Price=40, Description="Rich chocolate drink with whipped cream", IsPopular=true, ImageUrl="https://ichef.bbci.co.uk/food/ic/food_16x9_1600/recipes/hot_chocolate_78843_16x9.jpg" }
                    };

                    MenuItems.AddRange(menuItems);
                    SaveChanges();
                }

                // Seed tables if none exist
                if (!Tables.Any())
                {
                    var tables = new List<Table>
                    {
                        new Table { TableNumber = 1, Capacity = 2 },
                        new Table { TableNumber = 2, Capacity = 4 },
                        new Table { TableNumber = 3, Capacity = 6 },
                        new Table { TableNumber = 4, Capacity = 4 },
                        new Table { TableNumber = 5, Capacity = 8 },
                        new Table { TableNumber = 6, Capacity = 2 },
                        new Table { TableNumber = 7, Capacity = 10 },
                        new Table { TableNumber = 8, Capacity = 12 }
                    };

                    Tables.AddRange(tables);
                    SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to seed database", ex);
            }
        }
    }
}
