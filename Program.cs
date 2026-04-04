using EcommerceApp.Data;
using EcommerceApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure Entity Framework Core (Assuming SQL Server, but you can use SQLite)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (builder.Environment.IsDevelopment() &&
    !string.IsNullOrWhiteSpace(connectionString) &&
    connectionString.Contains("Authentication=Active Directory", StringComparison.OrdinalIgnoreCase))
{
    connectionString = "Server=(localdb)\\mssqllocaldb;Database=EcommerceDb;Trusted_Connection=True;TrustServerCertificate=True;";
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

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
builder.Services.AddScoped<IPaymentGatewayService, DemoPaymentGatewayService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<ApplicationDbContext>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

    await dbContext.Database.MigrateAsync();

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