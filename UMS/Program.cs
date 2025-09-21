using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using UMS.API.GlobalAcceptionHandler;
using UMS.Application.Interfaces;
using UMS.Infrastructure.Extensions;
using UMS.Infrastructure.Persistance;
using UMS.Infrastructure.Persistance.Seeding;
using UMS.Infrastructure.Repositories;
using UMS.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IjwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(IApplicationMarker).Assembly);
});
builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.ExecutionStrategy(deps => new ExponentialBackoffExecutionStrategy(
            deps, maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30)));
    });
});

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireDigit = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "JWTToken_Authentication",
        Version = "v1"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserPolicy", policy => policy.RequireRole("User"));
    options.AddPolicy("GuestPolicy", policy => policy.RequireRole("Guest"));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMVC", policy =>
    {
        policy.WithOrigins("https://localhost:7299")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();
await app.Services.ApplyMigrationsAndSeedAsync();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")),
    RequestPath = ""
});
app.UseCors("AllowMVC");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

/// <summary>
/// Custom EF Core execution strategy with exponential backoff
/// </summary>
public class ExponentialBackoffExecutionStrategy : ExecutionStrategy
{
    private int _currentRetryCount = 0;

    public ExponentialBackoffExecutionStrategy(
        ExecutionStrategyDependencies dependencies,
        int maxRetryCount,
        TimeSpan maxRetryDelay)
        : base(dependencies, maxRetryCount, maxRetryDelay)
    {
    }

  
    protected override bool ShouldRetryOn(Exception exception)
    {
        if (exception is SqlException sqlEx)
        {
            // Common transient SQL error codes
            int[] transientErrors = { 40613, 10928, 10929, 40197, 40501, 4060 };

            foreach (SqlError error in sqlEx.Errors)
            {
                if (Array.Exists(transientErrors, e => e == error.Number))
                {
                    _currentRetryCount++; 
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Exponential backoff delay calculate 
    /// </summary>
    protected override TimeSpan? GetNextDelay(Exception lastException)
    {
        // Exponential backoff: 2, 4, 8, 16 sec...
        var delaySeconds = Math.Min(Math.Pow(2, _currentRetryCount), MaxRetryDelay.TotalSeconds);
        return TimeSpan.FromSeconds(delaySeconds);
    }
}
