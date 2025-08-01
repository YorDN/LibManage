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
        builder.Services.AddScoped<IGoogleDriveService, GoogleDriveService>();
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
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            await SeedRolesAndUsersAsync(services);
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
    public static async Task SeedRolesAndUsersAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        string[] roleNames = { UserRoles.Admin, UserRoles.Manager, UserRoles.User };

        foreach (var role in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }

        string adminEmail = "admin@abv.bg";
        if(await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new User()
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                ProfilePicture = "/uploads/pfps/user/DefaultUser.png"
            };
            await userManager.CreateAsync(admin, "Admin123");
            await userManager.AddToRoleAsync(admin, "Admin");

        }

        var managerEmail = "manager@abv.bg";
        if (await userManager.FindByEmailAsync(managerEmail) == null)
        {
            var manager = new User
            {
                UserName = managerEmail,
                Email = managerEmail,
                EmailConfirmed = true,
                ProfilePicture = "/uploads/pfps/user/DefaultUser.png"
            };

            await userManager.CreateAsync(manager, "Manager123");
            await userManager.AddToRoleAsync(manager, "Manager");
        }

        var userEmail = "user@abv.bg";
        if (await userManager.FindByEmailAsync(userEmail) == null)
        {
            var user = new User
            {
                UserName = userEmail,
                Email = userEmail,
                EmailConfirmed = true,
                ProfilePicture = "/uploads/pfps/user/DefaultUser.png"
            };

            await userManager.CreateAsync(user, "User1234");
            await userManager.AddToRoleAsync(user, "User");
        }

    }


}
