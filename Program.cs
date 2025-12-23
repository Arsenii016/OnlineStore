using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Data;
using OnlineStore.Models;

var builder = WebApplication.CreateBuilder(args);

// Добавляем Razor Pages
builder.Services.AddRazorPages();

// Подключаем БД
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Добавляем сессии (ОБЯЗАТЕЛЬНО для корзины анонимных пользователей)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Identity с ролями
builder.Services.AddDefaultIdentity<IdentityUser>(options => 
    options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

var app = builder.Build();

// Стандартный middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Важный порядок: сессии ДО аутентификации!
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// SEEDING (остаётся без изменений — у тебя уже отличный вариант)
using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

await db.Database.MigrateAsync();

// Роль Admin
if (!await roleManager.RoleExistsAsync("Admin"))
    await roleManager.CreateAsync(new IdentityRole("Admin"));

// Admin пользователь
var adminEmail = "admin@store.com";
var admin = await userManager.FindByEmailAsync(adminEmail);
if (admin == null)
{
    admin = new IdentityUser
    {
        UserName = adminEmail,
        Email = adminEmail,
        EmailConfirmed = true
    };
    var createResult = await userManager.CreateAsync(admin, "Admin123!");
    if (!createResult.Succeeded)
    {
        throw new Exception("Не удалось создать админа: " + string.Join(", ", createResult.Errors.Select(e => e.Description)));
    }
}

// Присваиваем роль "Admin" (даже если пользователь уже существует)
if (!await userManager.IsInRoleAsync(admin, "Admin"))
{
    var addRoleResult = await userManager.AddToRoleAsync(admin, "Admin");
    if (!addRoleResult.Succeeded)
    {
        throw new Exception("Не удалось добавить роль Admin: " + string.Join(", ", addRoleResult.Errors.Select(e => e.Description)));
    }
}

// Категории и товары
if (!await db.Categories.AnyAsync())
{
    var cat1 = new Category { Name = "Смартфоны" };
    var cat2 = new Category { Name = "Ноутбуки" };
    var cat3 = new Category { Name = "Наушники" };

    db.Categories.AddRange(cat1, cat2, cat3);
    await db.SaveChangesAsync(); // сохраняем, чтобы появились Id

    db.Products.AddRange(
        new Product { Name = "iPhone 15", Price = 999m, Stock = 10, Description = "Apple smartphone", ImageUrl = "/images/placeholder-product.jpg", CategoryId = cat1.Id },
        new Product { Name = "MacBook Air", Price = 1299m, Stock = 5, Description = "Apple laptop", ImageUrl = "/images/placeholder-product.jpg", CategoryId = cat2.Id },
        new Product { Name = "AirPods Pro", Price = 249m, Stock = 25, Description = "Wireless earbuds", ImageUrl = "/images/placeholder-product.jpg", CategoryId = cat3.Id }
    );
    await db.SaveChangesAsync();
}

app.Run();