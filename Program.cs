using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Artisans.Infrastructure.Data;
using Artisans.Core.Entities; 

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
var connectionString = builder.Configuration.GetConnectionString("ArtisansDBConnection");
builder.Services.AddDbContext<ArtisansDBContext>(options =>
    options.UseSqlServer(connectionString));

// Add ASP.NET Core Identity
builder.Services.AddIdentity<User, Role>(options => // Specify User and Role classes
{
    // Configure Identity options here if needed (e.g., password complexity)
    options.SignIn.RequireConfirmedAccount = false; // False is simpler. True for production.
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6; 
    options.Password.RequiredUniqueChars = 1;
})
.AddEntityFrameworkStores<ArtisansDBContext>()
.AddDefaultTokenProviders(); // For things like password reset tokens

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddRazorOptions(options =>
    {
        options.ViewLocationFormats.Clear();
        options.ViewLocationFormats.Add("/Features/{1}/Views/{0}.cshtml");
        options.ViewLocationFormats.Add("/Features/Shared/Views/{0}.cshtml");
        options.ViewLocationFormats.Add("/Views/Shared/{0}.cshtml");
    });

// Configure application cookie (important for web apps)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login"; 
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Standard way to serve files from wwwroot

app.UseRouting();

app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed Roles and Admin User (do this once at startup)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<Role>>();
        await SeedIdentityDataAsync(userManager, roleManager); 
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the Identity database.");
    }
}

app.Run();

// Helper method to seed Identity data
async Task SeedIdentityDataAsync(UserManager<User> userManager, RoleManager<Role> roleManager)
{
    string[] roleNames = { "Admin", "Artisan", "Buyer", "Influencer" };
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new Role { Name = roleName });
        }
    }

    var adminEmail = "admin@myanmarartisans.com";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var adminUser = new User
        {
            UserName = "admin", // Or use email as UserName
            Email = adminEmail,
            EmailConfirmed = true, // Confirm email for simplicity in dev
            CustomRole = Artisans.Core.Enums.UserRoleType.Admin,
            RegistrationDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), // Static date
            IsActive = true
        };
        var result = await userManager.CreateAsync(adminUser, "AdminPa$$w0rd"); // default password
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}