using DateBoard.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Получаем строку подключения и конвертируем из Railway формата
var connectionString = GetConnectionString();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDefaultIdentity<IdentityUser>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages();

var app = builder.Build();

// Автоприменение миграций
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.Run();

// ⬇️ Метод конвертации строки подключения
static string GetConnectionString()
{
    // Пробуем Railway формат
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

    if (!string.IsNullOrEmpty(databaseUrl))
    {
        // Конвертируем postgres:// в Npgsql формат
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':');

        return $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};" +
               $"Username={userInfo[0]};Password={userInfo[1]};SslMode=Require;TrustServerCertificate=true";
    }

    // Fallback на appsettings.json для локальной разработки
    return null; // Или получи из Configuration
}