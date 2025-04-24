using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TKPM_Project.Models;
using TKPM_Project.Repositories;
using TKPM_Project.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<ToolService>();

// Cấu hình SQLite Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Đăng ký Authentication & Authorization
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Cấu hình Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
});

// Đăng ký Repository
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IToolRepository, ToolRepository>();
builder.Services.AddScoped<IUserLikedToolRepository, UserLikedToolRepository>();
builder.Services.AddScoped<IUserPremiumRepository, UserPremiumRepository>();
builder.Services.AddTransient<IEmailSender, EmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var toolService = services.GetRequiredService<ToolService>();
    var toolRepository = services.GetRequiredService<IToolRepository>();

    // Đảm bảo database đã tồn tại (tạo nếu chưa có)
    context.Database.Migrate();

    string[] roleNames = { "Anonymous", "User", "Premium", "Admin" };
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    //tài khoản Admin
    var adminEmail = "admin@example.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser { UserName = adminEmail, Email = adminEmail };
        await userManager.CreateAsync(adminUser, "Admin@123"); //pass
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }


    // Thêm dữ liệu mẫu nếu chưa có
    if (!context.Tools.Any())
    {
        context.Tools.AddRange(
            new Tool { Id = 1, Name = "Tool 1", Description = "Description 1", CreatedAt = DateTime.UtcNow },
            new Tool { Id = 2, Name = "Tool 2", Description = "Description 2", CreatedAt = DateTime.UtcNow }
        );
        context.SaveChanges();
    }

    // Load tools from plugins and save their metadata to the database
    var loadedTools = toolService.GetTools(); // Get all tools loaded by ToolService

    // Get all tools from the database
    var dbTools = await toolRepository.GetAllAsync();

    // Check for tools in the database that don't exist in loadedTools and delete them
    foreach (var dbTool in dbTools)
    {
        if (!loadedTools.Any(lt => lt.Name == dbTool.Name))
        {
            // Tool exists in database but not in loaded tools, delete it
            await toolRepository.DeleteAsync(dbTool.Id);
        }
    }

    // Add new tools from plugins to the database if they don't already exist
    foreach (var tool in loadedTools)
    {
        // Check if the tool already exists in the database
        var existingTool = await toolRepository.GetByKeywordAsync(tool.Name)
            .ContinueWith(t => t.Result.FirstOrDefault(t => t.Name == tool.Name));

        if (existingTool == null)
        {
            // Create a new Tool entity with the metadata from the loaded tool
            var newTool = new Tool
            {
                Name = tool.Name,
                Description = tool.Description,
                IsPremium = tool.IsPremium,
                Category = tool.Category,
                IsAvailable = true, // Default value, adjust as needed
                CreatedAt = DateTime.UtcNow,
                CustomViewTemplate = tool.CustomViewTemplate ?? "defaultTemplate"
            };

            // Add the tool to the database
            await toolRepository.AddAsync(newTool);
        }
    }
}

app.Run();