using proyecto_escuela.Data;
using proyecto_escuela.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<DbConnectionPool>();
builder.Services.AddScoped<PaqueteRepository>();
builder.Services.AddScoped<RepartidorRepository>();
var app = builder.Build();

// 🔥 CLAVE PARA DOCKER
app.Urls.Add("http://0.0.0.0:80");

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ❌ QUITAR HTTPS
// app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();