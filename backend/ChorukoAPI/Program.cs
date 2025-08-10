using ChorukoAPI.Data;
using ChorukoAPI.Models;
using ChorukoAPI.Services.Abstraction;
using ChorukoAPI.Services.Implementation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

// Load .env file if it exists
DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey
    });
    options.OperationFilter<Swashbuckle.AspNetCore.Filters.SecurityRequirementsOperationFilter>();
});

// Add environment variables to configuration
builder.Configuration.AddEnvironmentVariables();

// Generate the connection string from environment variables
var config = builder.Configuration;
var dbHost = config["DB_HOST"] ?? "database"; // Default to 'database' if not set
var dbPort = config["DB_PORT"] ?? "5432"; // Default to '5432' if not set
var dbName = config["DB_DATABASE_NAME"];
var dbUser = config["DB_USERNAME"];
var dbPassword = config["DB_PASSWORD"];
var connectionString = $"Host={dbHost};Port={dbPort};Username={dbUser};Password={dbPassword};Database={dbName}";

// Use the connection string to configure the DbContext
builder.Services.AddDbContext<DataContext>(options => options
.UseNpgsql(connectionString).UseLazyLoadingProxies());

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // Password settings - reasonable for development
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false; // Relaxed for development
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // ORIGINAL PASSWORD SETTINGS (commented out):
    // options.Password.RequireDigit = true;
    // options.Password.RequireLowercase = true;
    // options.Password.RequireNonAlphanumeric = true;
    // options.Password.RequireUppercase = true;
    // options.Password.RequiredLength = 6;
    // options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;

    // Sign-in settings
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<DataContext>().AddDefaultTokenProviders();


builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "ChorukoCookie";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax; // Changed from None to Lax for development
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Allow HTTP in development
    options.ExpireTimeSpan = TimeSpan.FromHours(1);
    options.SlidingExpiration = true;

    // ORIGINAL COOKIE SETTINGS (commented out):
    // options.Cookie.SameSite = SameSiteMode.None; // Required HTTPS
    // options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Required HTTPS
    
    // Disable redirects for API - return status codes instead
    options.LoginPath = null;
    options.LogoutPath = null;
    options.AccessDeniedPath = null;
    
    // ORIGINAL REDIRECT SETTINGS (commented out):
    // options.LoginPath = "/api/Auth/Login";
    // options.LogoutPath = "/api/Auth/Logout";
    // options.AccessDeniedPath = "/api/Auth/AccessDenied";
    
    // Configure events to return proper status codes
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
    
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = 403;
        return Task.CompletedTask;
    };
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddControllers();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001") // Support both ports
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });

    // ORIGINAL CORS POLICY (commented out):
    // options.AddPolicy("AllowFrontend", policy =>
    // {
    //     policy.WithOrigins("http://localhost:3000") // Only port 3000
    //           .AllowAnyHeader()
    //           .AllowAnyMethod()
    //           .AllowCredentials();
    // });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roleNames = { "None", "Admin", "HouseholdAdmin", "Participant" };
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
            Console.WriteLine($"Role '{roleName}' created.");
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();