using Byte___Brew.Data;
using Byte___Brew.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Byte___Brew
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<ByteAndBrewDbContext>(opt =>
                opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Read JWT settings from environment variables, fallback to appsettings.json
            var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
                         ?? builder.Configuration["Jwt:Key"];
            var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
                            ?? builder.Configuration["Jwt:Issuer"];
            var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
                              ?? builder.Configuration["Jwt:Audience"];

            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new Exception("JWT_KEY not set");
            }

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsync(
                            "{\"error\":\"Unauthorized. Please log in with a valid token.\"}");
                    }
                };
            });

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "Byte & Brew API", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Enter your JWT token below."
                });

                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            builder.Services.AddAuthorization();

            // Changed
            builder.Services.AddControllersWithViews()
                   .AddRazorRuntimeCompilation();

            builder.Services.AddEndpointsApiExplorer();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                using (var scope = app.Services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<ByteAndBrewDbContext>();

                    // Seed admin if it doesn't exist
                    if (!db.Admins.Any())
                    {
                        db.Admins.Add(new Admin
                        {
                            Username = "admin",
                            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin")
                        });
                        db.SaveChanges();
                    }

                    // Seed menu items if none exist
                    if (!db.MenuItems.Any())
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
                                new MenuItem { Name="Macchiato", Price=36, Description="Espresso with a small amount of foam", IsPopular=false, ImageUrl="https://gospecialtycoffee.com/medialibrary/2023/02/everything-you-need-to-know-about-macchiato-bfcoffee77.jpg" },
                                new MenuItem { Name="Chai Latte", Price=44, Description="Spiced tea with steamed milk", IsPopular=true, ImageUrl="https://cdn.loveandlemons.com/wp-content/uploads/2025/01/chai-latte.jpg" },
                                new MenuItem { Name="Hot Chocolate", Price=40, Description="Rich chocolate drink with whipped cream", IsPopular=true, ImageUrl="https://ichef.bbci.co.uk/food/ic/food_16x9_1600/recipes/hot_chocolate_78843_16x9.jpg" }
                            };

                        foreach (var item in menuItems)
                        {
                            db.MenuItems.Add(item);
                            db.SaveChanges();
                        }
                    }

                    // Seed tables if none exist
                    if (!db.Tables.Any())
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

                        foreach (var table in tables)
                        {
                            db.Tables.Add(table);
                            db.SaveChanges();
                        }
                    }
                }

                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // Changed
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapControllers(); // Keep API routes working


            app.Run();
        }
    }
}
