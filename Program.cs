using Microsoft.EntityFrameworkCore;
using POSSystem.Data;
using POSSystem.Services;
using POSSystem.Configuration;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configure JSON serialization for enums
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Keep PascalCase
    });

// Configure Entity Framework with SQL Server
builder.Services.AddDbContext<POSContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Server=(localdb)\\mssqllocaldb;Database=POSSystemDB;Trusted_Connection=true;MultipleActiveResultSets=true";
    options.UseSqlServer(connectionString);
});

// Register services
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddSingleton<IConfigurationService, XmlConfigurationService>();

// Configure OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "POS System API",
        Version = "v1",
        Description = "A modern Point of Sale system backend API demonstrating .NET, C#, Entity Framework, and SQL Server skills",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "POS System Team",
            Email = "info@possystem.com"
        }
    });
    
    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Configure CORS for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("Development", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "POS System API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
    app.UseCors("Development");
}

// Create database if it doesn't exist
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<POSContext>();
    try
    {
        await context.Database.EnsureCreatedAsync();
        app.Logger.LogInformation("Database initialized successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error initializing database");
    }
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Add health check endpoint
app.MapGet("/health", async (POSContext context) =>
{
    try
    {
        await context.Database.CanConnectAsync();
        return Results.Ok(new { 
            Status = "Healthy", 
            Timestamp = DateTime.UtcNow,
            Database = "Connected"
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            title: "Database Connection Failed",
            statusCode: 503
        );
    }
});

app.Logger.LogInformation("POS System API started successfully");

app.Run();

// Make Program class accessible for testing
public partial class Program { }
