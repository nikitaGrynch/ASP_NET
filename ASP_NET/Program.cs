using ASP_NET.Data;
using ASP_NET.Middleware;
using ASP_NET.Services;
using ASP_NET.Services.Hash;
using ASP_NET.Services.Kdf;
using ASP_NET.Services.Random;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Pomelo.EntityFrameworkCore.MySql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<TimeService>();
builder.Services.AddTransient<DateService>();
builder.Services.AddScoped<DtService>();

builder.Services.AddSingleton<IHashService, Md5HashService>();
builder.Services.AddSingleton<IRandomService, RandomServiceV1>();
builder.Services.AddSingleton<IKdfService, HashKdfService>();

// Конфигурация НТТР-сессий
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});




String? connectionString = builder.Configuration.GetConnectionString("MainDb");
MySqlConnection connection = new(connectionString);
builder.Services.AddDbContext<DataContext>(options =>
    options.UseMySql(
        connection,
        ServerVersion.AutoDetect(connection)
    )
);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Включение HTTP - сессий
app.UseSession();

// Подключаем собственные Middleware
app.UseMiddleware<SessionAuthMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();