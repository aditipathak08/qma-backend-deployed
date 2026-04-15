using QuantityMeasurementApp.Service;
using QuantityMeasurementApp.Repository;
using QuantityMeasurementApp.API.Middleware;
using QuantityMeasurementApp.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text;
using NLog;
using NLog.Web;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Setup NLog (Quiet Mode)
var logger = LogManager.Setup()
    .LoadConfigurationFromFile("Logging/NLog.config")
    .GetCurrentClassLogger();

builder.Logging.ClearProviders();
builder.Logging.AddConsole(); 

// 🚀 FIX: Add Npgsql legacy timestamp support (Required for PostgreSQL)
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Add CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

try
{
    // -------------------- EF Core DbContext --------------------
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    // Check if DATABASE_URL (Render format) is provided and convert to standard Npgsql format if needed
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (!string.IsNullOrEmpty(databaseUrl) && (databaseUrl.StartsWith("postgres://") || databaseUrl.StartsWith("postgresql://")))
    {
        // Handle both postgres:// and postgresql:// schemes
        var uri = new Uri(databaseUrl.Replace("postgresql://", "postgres://"));
        var userInfo = uri.UserInfo.Split(':');
        
        var host = uri.Host;
        var port = uri.Port > 0 ? uri.Port : 5432; // Default to 5432 if port is missing (-1)
        var database = uri.AbsolutePath.TrimStart('/');
        var username = userInfo[0];
        var password = userInfo.Length > 1 ? userInfo[1] : "";

        connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
    }

    if (string.IsNullOrEmpty(connectionString))
    {
        connectionString = "Host=localhost;Database=QuantityMeasurementDB;Username=postgres;Password=password";
    }

    builder.Services.AddDbContext<QuantityMeasurementDbContext>(options =>
        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
        }));

    // -------------------- Redis --------------------
    var redisConnection = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
    var redisOptions = ConfigurationOptions.Parse(redisConnection);
    redisOptions.AbortOnConnectFail = false; 
    builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisOptions));
    builder.Services.AddScoped<RedisCacheService>();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    
    // --- 🔐 Configure Swagger for JWT ---
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Quantity Measurement API", Version = "v1" });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                new string[] { }
            }
        });
    });

    builder.Services.AddScoped<IQuantityMeasurementRepository, QuantityMeasurementDatabaseRepository>();
    builder.Services.AddScoped<IQuantityMeasurementService, QuantityMeasurementServiceImpl>();

    var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") 
                 ?? builder.Configuration["Jwt:Key"] 
                 ?? "THIS_IS_A_SECURE_32_CHARACTER_KEY_!!!";
    
    builder.Services.AddSingleton(new JwtService(jwtKey));
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IAuthService, AuthService>();

    var key = Encoding.UTF8.GetBytes(jwtKey);
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<QuantityMeasurementDbContext>();
        try
        {
            // EnsureCreated() works by checking the model
            dbContext.Database.EnsureCreated();
            
            // Raw SQL fallback to be 100% sure tables exist in the public schema
            using (var command = dbContext.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS users (
                        id SERIAL PRIMARY KEY,
                        name TEXT NOT NULL,
                        email TEXT NOT NULL UNIQUE,
                        password_hash TEXT NOT NULL,
                        salt TEXT NOT NULL
                    );
                    CREATE TABLE IF NOT EXISTS quantity_measurements (
                        id SERIAL PRIMARY KEY,
                        operation TEXT NOT NULL,
                        error_message TEXT,
                        operand1_value DOUBLE PRECISION,
                        operand1_unit TEXT,
                        operand2_value DOUBLE PRECISION,
                        operand2_unit TEXT,
                        result_value DOUBLE PRECISION,
                        result_unit TEXT
                    );
                ";
                dbContext.Database.OpenConnection();
                command.ExecuteNonQuery();
            }
            Console.WriteLine("✅ Database and tables verified/created successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Database check failed: {ex.Message}");
            // Continue anyway, as the DB might be ready but migration might have partial issues
        }
    }

    app.UseMiddleware<ExceptionMiddleware>();
    app.UseCors("AllowAll");
    
    // Enable Swagger in all environments
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Quantity Measurement API v1");
    });

    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.MapGet("/", () => "Quantity Measurement API is online and healthy.");

    Console.WriteLine($"🚀 Backend is running at: http://localhost:5248");
    Console.WriteLine("👉 Open Swagger at: http://localhost:5248/swagger");
    
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Startup failed: {ex.Message}");
    throw;
}
finally
{
    LogManager.Shutdown();
}
