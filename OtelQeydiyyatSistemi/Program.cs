using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OtelQeydiyyatSistemi.Models;
using OtelQeydiyyatSistemi.Data;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Verilənlər bazası kontekstini əlavə et
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity xidmətlərini əlavə et
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Cookie parametrləri
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(3);
});

var app = builder.Build();

// Verilənlər bazasını ilkin məlumatlarla doldur
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        
        // Əvvəlcə verilənlər bazasının yaradıldığına əmin ol
        context.Database.EnsureCreated();
        
        // Verilənlər bazasını və ilkin məlumatları yarat
        await DbInitializer.Initialize(context, userManager, roleManager);
        
        Console.WriteLine("Verilənlər bazası uğurla yaradıldı və ilkin məlumatlarla dolduruldu.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Verilənlər bazasını ilkin məlumatlarla doldurarkən xəta baş verdi.");
        Console.WriteLine($"Xəta: {ex.Message}");
        Console.WriteLine($"Xətanın ətraflı məlumatı: {ex.StackTrace}");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// Identity middleware əlavə et
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
