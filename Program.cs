using DateBoard.Data;
using DateBoard.Hubs;
using DateBoard.Middleware;
using DateBoard.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Получаем строку подключения
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// === СЕРВИСЫ ===

// БД PostgreSQL (для Railway)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Или если пока тестируешь локально — SQL Server:
// builder.Services.AddDbContext<ApplicationDbContext>(options =>
//     options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// SignalR (настроен ДО Build!)
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 10 * 1024 * 1024;
});

// Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// Razor Pages
builder.Services.AddRazorPages();

// Кастомные сервисы
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IOnlineStatusService, OnlineStatusService>();

// === ПРИЛОЖЕНИЕ ===

var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Замена UseStaticAssets для совместимости

app.UseRouting();
app.UseAuthentication(); // Добавлено (было пропущено!)
app.UseAuthorization();

// Middleware
app.UseMiddleware<OnlineStatusMiddleware>();

// === МАРШРУТЫ ===

app.MapRazorPages();

// SignalR Hubs (только ОДИН раз!)
app.MapHub<ChatHub>("/chatHub");
app.MapHub<PrivateHub>("/privateHub");

// Запуск
app.Run();