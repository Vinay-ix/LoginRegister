using LoginRegister.Data;
using Microsoft.EntityFrameworkCore;
/* NEW → */
using Npgsql.EntityFrameworkCore.PostgreSQL;   // Postgres provider

var builder = WebApplication.CreateBuilder(args);

/* ----------  DATABASE  ---------- */
builder.Services.AddDbContext<LoginDbContext>(options =>
{   // switched from UseSqlServer ➜ UseNpgsql
    options.UseNpgsql(Environment.GetEnvironmentVariable("DATABASE_URL"));
});

/* ----------  MVC  ---------- */
builder.Services.AddControllersWithViews();

/* ----------  SESSION  ---------- */
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

/* ----------  PIPELINE  ---------- */
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();          // keep session before authorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
