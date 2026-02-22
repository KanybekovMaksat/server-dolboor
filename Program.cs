using CodifyProjectsBackend.Models;
using CodifyProjectsBackend.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 268435456;
});
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 268435456;
});

// Add services to the container.
builder.Services.AddControllersWithViews();

string? argDbConnectionString = null;

if (args.Length > 0)
{
    Console.WriteLine(string.Join(", ", args));
    for (int i = 0; i < args.Length; i++)
    {
        var arg = args[i];

        if (arg == "-db" || arg == "-database")
        {
            if (i + 1 >= args.Length)
            {
                Console.WriteLine("Database value was not specified.");
            }
            else
            {
                argDbConnectionString = args[i + 1];
            }
        }
    }
}

var configuration = builder.Configuration;
string? connectionString = argDbConnectionString ?? configuration.GetConnectionString("MainDb") ?? null;

Console.WriteLine($"Database connection string: {connectionString}");

if (connectionString == null)
{
    Console.WriteLine("Database connection string not found!\n\nPress any key to exit program...");
    Console.ReadKey();
    return;
}

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddSingleton<IMediaService, MediaService>();
builder.Services.AddSingleton<IProjectService, ProjectService>();
builder.Services.AddSingleton<CodeStructureParser>();

builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;

        // Добавьте это:
        options.Cookie.MaxAge = TimeSpan.FromDays(7);

        options.LoginPath = "/Account";
        options.AccessDeniedPath = "/Account/Denied";
    });

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/keys")) // тут монтируется volume
    .SetApplicationName("MyApp");

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    logger?.LogInformation("Project runs at Production mode");
}
else
{
    logger?.LogInformation("Project runs at Development mode");
}


app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseDefaultFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

var storagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "storage");

if (!Directory.Exists(storagePath))
{
    Directory.CreateDirectory(storagePath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(storagePath),
    RequestPath = "/storage",
    ServeUnknownFileTypes = true, // Optional: serve all file types
    OnPrepareResponse = ctx =>
    {
        // Optional: Add caching headers
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=31536000");
    }
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
