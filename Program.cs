using EcommerceApp.Data;
using EcommerceApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

var builder = WebApplication.CreateBuilder(args);


var connection = builder.Configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING");

if (string.IsNullOrWhiteSpace(connection))
{
    connection = Environment.GetEnvironmentVariable("AZURE_SQL_CONNECTIONSTRING");
}

if (string.IsNullOrWhiteSpace(connection))
{
    throw new InvalidOperationException(
        "Database connection string 'AZURE_SQL_CONNECTIONSTRING' is not configured. Set it using user-secrets or environment variables.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connection));

// 2. Configure ASP.NET Core Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // Set to true if you want email confirmation
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Home/AccessDenied";
    options.AccessDeniedPath = "/Home/AccessDenied";
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddScoped<IProductCatalogService, DbProductCatalogService>();
builder.Services.AddScoped<ICartService, SessionCartService>();
builder.Services.AddScoped<IWishlistService, SessionWishlistService>();
builder.Services.AddScoped<IPaymentGatewayService, DemoPaymentGatewayService>();

var app = builder.Build();

const string skipAutoMigrateEnvVar = "SKIP_AUTO_MIGRATE_IN_PRODUCTION";
var skipAutoMigrateInProduction =
    app.Environment.IsProduction() &&
    bool.TryParse(Environment.GetEnvironmentVariable(skipAutoMigrateEnvVar), out var skipMigration) &&
    skipMigration;

try
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var dbContext = services.GetRequiredService<ApplicationDbContext>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

    if (skipAutoMigrateInProduction)
    {
        logger.LogWarning("Skipping automatic database migration because environment variable '{EnvVar}' is enabled in Production.", skipAutoMigrateEnvVar);
    }
    else
    {
        logger.LogInformation("Applying database migrations at startup.");
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully.");
    }

    var roles = new[] { AppRoles.Buyer, AppRoles.Admin, AppRoles.Staff };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    const string adminEmail = "admin@techcurator.local";
    const string adminPassword = "Admin@123";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser is null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var createAdminResult = await userManager.CreateAsync(adminUser, adminPassword);
        if (createAdminResult.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, AppRoles.Admin);
        }
    }
    else if (!await userManager.IsInRoleAsync(adminUser, AppRoles.Admin))
    {
        await userManager.AddToRoleAsync(adminUser, AppRoles.Admin);
    }
}
catch (Exception ex)
{
    app.Logger.LogCritical(ex, "Application startup initialization failed. Check database connection, permissions, and migration state. Set '{EnvVar}=true' in Production to skip automatic migration.", skipAutoMigrateEnvVar);
    throw;
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

// 3. Add Authentication Middleware (MUST be placed before Authorization)
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();