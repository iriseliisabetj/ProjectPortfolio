using DAL;
using Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

connectionString = connectionString.Replace("<%location%>", FileHelper.BasePath);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// use file system instead
//builder.Services.AddScoped<IConfigRepository, ConfigRepositoryJson>();
//builder.Services.AddScoped<IGameRepository, GameRepositoryJson>();

builder.Services.AddScoped<IConfigRepository, ConfigRepositoryDb>();
builder.Services.AddScoped<IGameRepository, GameRepositoryDb>();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();


builder.Services.AddRazorPages();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<IPasswordHasher<Game>, PasswordHasher<Game>>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
} else {
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();

app.Run();