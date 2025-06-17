using LoginRegister.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

/* ----------  DATABASE  ---------- */
builder.Services.AddDbContext<LoginDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

/* ----------  MVC  ---------- */
builder.Services.AddControllersWithViews();

/* ----------  SESSION (NEW)  ---------- */
builder.Services.AddDistributedMemoryCache();   // required for session
builder.Services.AddSession(options =>
{
    // optional fine‑tuning
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;          // GDPR‑compliant default
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

app.UseSession();        // <<‑‑ add THIS before auth/authorization

/* You’re not doing cookie‑based auth yet, 
   so no app.UseAuthentication() here.   */
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
