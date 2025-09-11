using ByteAndBrew.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ByteAndBrew
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<ByteAndBrewDbContext>(opt =>
                opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Configure HttpClient to bypass SSL validation in development
            builder.Services.AddHttpClient("ByteAndBrewAPI", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7145/api/");
                client.Timeout = TimeSpan.FromMinutes(2); // Add timeout
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler();

                // Only bypass SSL validation in development environment
                if (builder.Environment.IsDevelopment())
                {
                    handler.ServerCertificateCustomValidationCallback =
                        (message, cert, chain, errors) => true;
                }

                return handler;
            });

            // Add session support for JWT token storage
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(2); // Match JWT expiration
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
                    ? CookieSecurePolicy.None
                    : CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;
            });

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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ClockSkew = TimeSpan.FromMinutes(5) // Allow 5 minutes clock skew
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

            builder.Services.AddControllersWithViews()
                   .AddRazorRuntimeCompilation();

            builder.Services.AddEndpointsApiExplorer();

            var app = builder.Build();

            // Seed database in development
            if (app.Environment.IsDevelopment())
            {
                using (var scope = app.Services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<ByteAndBrewDbContext>();
                    try
                    {
                        db.Database.EnsureCreated();
                        db.SeedData(); // Call our seeding method
                    }
                    catch (Exception ex)
                    {
                        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                        logger.LogError(ex, "An error occurred while seeding the database.");
                    }
                }

                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Use exception handling
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            // IMPORTANT: Session must come before authentication
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            // More specific route first
            app.MapControllerRoute(
                name: "admin",
                pattern: "admin/{action=Index}",
                defaults: new { controller = "AdminPanel" });

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // API routes
            app.MapControllers();

            app.Run();
        }
    }
}