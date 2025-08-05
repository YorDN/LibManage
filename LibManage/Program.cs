using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LibManage.Data;
using LibManage.Data.Models.Library;
using LibManage.Services.Core.Contracts;
using LibManage.Services.Core;
using LibManage.Common;
using LibManage.Web.Areas.Identity.Custom;
using LibManage.Web.Areas.Identity.Middleware;

namespace LibManage.Web;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddScoped<SignInManager<User>, LibrarySignInManager>();
        builder.Services.AddScoped<IBookService, BookService>();
        builder.Services.AddScoped<IRatingService, RatingService>();
        builder.Services.AddScoped<IAuthorService, AuthorService>();
        builder.Services.AddScoped<IFileUploadService, FileUploadService>();
        builder.Services.AddScoped<ICountryService, CountryService>();
        builder.Services.AddScoped<IPublisherService, PublisherService>();
        builder.Services.AddScoped<IEpubReaderService, EpubReaderService>();
        builder.Services.AddScoped<IBorrowService, BorrowService>();
        builder.Services.AddScoped<IAdminService, AdminService>();

        builder.Services.AddScoped<HttpClient>();
        builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
        })
                .AddDefaultUI()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();




        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            await DbSeeder.SeedAsync(services);
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseMiddleware<ActiveUserMiddleware>();
        app.UseRouting();

        app.UseAuthorization();
        app.MapControllerRoute(
           name: "areas",
           pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        app.MapRazorPages();
        app.Run();
    }


}
