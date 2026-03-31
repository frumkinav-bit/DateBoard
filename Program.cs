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
    options.UseNpgsql(connectionString));  // ← ИСПРАВЛЕНО: UseNpgsql вместо UseSqlServer

// ===== SignalR (ДО builder.Build()!) =====
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
    options.SignIn.RequireConfirmedAccount = false;  // ← false для простоты
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages();

// ===== СТРОИМ ПРИЛОЖЕНИЕ (после всех сервисов!) =====
var app = builder.Build();

// ===== МИГРАЦИИ (при старте) =====
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}

// ===== MIDDLEWARE ПАЙПЛАЙН =====
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // app.UseHsts();  // ← закомментировано для Railway
}

// app.UseHttpsRedirection();  // ← УБРАНО для Railway!
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();  // ← ДОБАВЛЕНО (было пропущено!)
app.UseAuthorization();

// ===== HUBS (ДО app.Run()!) =====
app.MapHub<ChatHub>("/chatHub");
app.MapHub<PrivateHub>("/privateHub");
app.MapHub<NotificationHub>("/notificationHub");  // ← добавлен

app.MapRazorPages();

// ===== MIDDLEWARE ОНЛАЙН СТАТУСА =====
app.UseMiddleware<OnlineStatusMiddleware>();

// ===== ЗАПУСК (В САМОМ КОНЦЕ!) =====
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