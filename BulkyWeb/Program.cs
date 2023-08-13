
using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Bulky.Utility;
using Stripe;
using Bulky.DataAccess.DbIntializer;


namespace Bulky
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<ApplicationDbContext>(
                options=>options.UseSqlServer
                (builder.Configuration.GetConnectionString("DefaultConnection")
                ));
            builder.Services.Configure<StripeConf>(builder.Configuration.GetSection("Stripe"));
            builder.Services.AddIdentity<IdentityUser ,IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
         
            builder.Services.ConfigureApplicationCookie(Options =>
            {
                Options.LoginPath = $"/Identity/Account/Login";
                Options.LogoutPath = $"/Identity/Account/Logout";
                Options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
            });
            builder.Services.AddAuthentication().AddFacebook(Options => { Options.AppId = "985038632664702"; Options.AppSecret = "53627fc406899f850c8e359f2ff51739"; });
            builder.Services.AddAuthentication().AddMicrosoftAccount(Options => { Options.ClientSecret = "Z5g8Q~tk5V-pn.52XT14tis7RVysq3gQdc3VqbvE"; Options.ClientId= "b44782a4-bf3a-4e4c-bcfe-3a3cb5faf5a4"; });
            builder.Services.AddDistributedMemoryCache();

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(100);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
           
            builder.Services.AddRazorPages();
            builder.Services.AddScoped<IUnitofwork,Unitofwork>();
             builder.Services.AddScoped< IDbIntializer, DbIntializer>();
            builder.Services.AddScoped<IEmailSender,EmailSender>();
           
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
            StripeConfiguration.ApiKey=builder.Configuration.GetSection("Stripe:Secretkey").Get<string>();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();
            Seeddatabase();
            app.MapRazorPages();
            app.MapControllerRoute(
                name: "default",
                pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

            app.Run();

            void Seeddatabase()
            {
                using(var scope = app.Services.CreateScope())
                {
                    var dbintializer = scope.ServiceProvider.GetRequiredService<IDbIntializer>();
                    dbintializer.Initialize();
                }
            }

        }
       
    }
}