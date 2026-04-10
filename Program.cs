using DateBoard.Data;
using DateBoard.Hubs;
using DateBoard.Middleware;
using DateBoard.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ===== НАСТРОЙКА ПОРТА ДЛЯ RAILWAY =====
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// ===== СТРОКА ПОДКЛЮЧЕНИЯ (PostgreSQL!) =====
var connectionString = GetConnectionString()
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string not found");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// ===== SignalR =====
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 10 * 1024 * 1024;
});
builder.Services.AddSingleton<IUserIdProvider, DefaultUserIdProvider>();

// ===== СЕРВИСЫ =====
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IOnlineStatusService, OnlineStatusService>();

// ===== IDENTITY =====
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// ===== Razor Pages =====
builder.Services.AddRazorPages();

// Логирование
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// ===== СТРОИМ ПРИЛОЖЕНИЕ =====
var app = builder.Build();

// ===== МИГРАЦИИ (при старте) =====
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}

// ===== MIDDLEWARE ПАЙПЛАЙН =====
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Middleware онлайн статуса (ДО MapRazorPages!)
app.UseMiddleware<OnlineStatusMiddleware>();

// ===== HUBS =====
app.MapHub<ChatHub>("/chatHub");
app.MapHub<PrivateHub>("/privateHub");
app.MapHub<NotificationHub>("/notificationHub");

// ===== Razor Pages =====
app.MapRazorPages();

// ===== ЗАПУСК =====
app.Run();

// ===== МЕТОД ДЛЯ RAILWAY CONNECTION STRING =====
static string GetConnectionString()
{
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (!string.IsNullOrEmpty(databaseUrl))
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':');
        return $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};" +
               $"Username={userInfo[0]};Password={userInfo[1]};SslMode=Require;TrustServerCertificate=true";
    }
    return null;
}