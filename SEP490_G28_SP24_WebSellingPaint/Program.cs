using Microsoft.EntityFrameworkCore;
using SEP490_G28_SP24_WebSellingPaint.Models;
using SEP490_G28_SP24_WebSellingPaint.FunctionCode.EmailSender;
using Microsoft.Extensions.WebEncoders;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Options;
using System.Text.Unicode;
using System.Text;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SEP490_G28_SP24_WebSellingPaint
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.ListenAnyIP(5286);
            });


            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddTransient<IEmailSender, EmailSender>();
            builder.Services.AddDistributedMemoryCache();
            builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            builder.Services.AddMvc().SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_2);
            builder.Services.AddDbContext<WebSellingPaintingContext>
            (options => options.UseSqlServer(builder.Configuration.GetConnectionString("DB")));
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CORSPolicy", builder => builder.AllowAnyMethod().AllowAnyHeader().AllowCredentials().SetIsOriginAllowed((hosts) => true));
            });

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(1800);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.Configure<WebEncoderOptions>(option =>
            {
                option.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
            }
            );

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseDeveloperExceptionPage();
            app.UseStaticFiles();
            app.UseStatusCodePages();
            app.UseRouting();
            app.UseCors("CORSPolicy");

            app.UseHttpsRedirection();

            app.UseAuthorization();
            app.UseSession();

            app.MapControllerRoute(
                name: "Home",
                pattern: "/Home",
                defaults: new { controller = "Customer", action = "Index" });

            app.MapControllerRoute(
                name: "Login",
                pattern: "/Login",
                defaults: new { controller = "Login", action = "Login" });

            app.MapControllerRoute(
                name: "Login",
                pattern: "/Signout",
                defaults: new { controller = "Login", action = "Signout" });

            app.MapControllerRoute(
                name: "Login",
                pattern: "/Signup",
                defaults: new { controller = "Login", action = "Signup" });

            app.MapControllerRoute(
                name: "Login",
                pattern: "/SignUpAsArtist",
                defaults: new { controller = "Login", action = "SignUpAsArtist" });

            app.MapControllerRoute(
                name: "Login",
                pattern: "/ForgotPassword",
                defaults: new { controller = "Login", action = "ForgotPassword" });

            app.MapControllerRoute(
                name: "Login",
                pattern: "/ChangePassword",
                defaults: new { controller = "Login", action = "ChangePassword" });

            app.MapControllerRoute(
                name: "Shop",
                pattern: "/PaintingDetail",
                defaults: new { controller = "Shop", action = "PaintingDetail" });

            app.MapControllerRoute(
                name: "Shop",
                pattern: "/Shop",
                defaults: new { controller = "Shop", action = "Shop" });

            app.MapControllerRoute(
                name: "Shop",
                pattern: "/FilShop",
                defaults: new { controller = "Shop", action = "PostShop" });

            app.MapControllerRoute(
                name: "Customer",
                pattern: "/Post",
                defaults: new { controller = "Customer", action = "Post" });

            
            app.MapControllerRoute(
                name: "Customer",
                pattern: "/Profile",
                defaults: new { controller = "Customer", action = "Profile" });

            app.MapControllerRoute(
                name: "Customer",
                pattern: "/Cart",
                defaults: new { controller = "Customer", action = "Cart" });

            app.MapControllerRoute(
                name: "Customer",
                pattern: "/Order",
                defaults: new { controller = "Customer", action = "Order" });

            app.MapControllerRoute(
                name: "Customer",
                pattern: "/OrderDetail",
                defaults: new { controller = "Customer", action = "OrderDetail" });

            app.MapControllerRoute(
                name: "Customer",
                pattern: "/PostDetail",
                defaults: new { controller = "Customer", action = "PostDetail" });

            app.MapControllerRoute(
                name: "Customer",
                pattern: "/CheckOut",
                defaults: new { controller = "Customer", action = "CheckOut" });

            app.MapControllerRoute(
                name: "Customer",
                pattern: "/UpgradeAccount",
                defaults: new { controller = "Customer", action = "UpgradeAccount" });

            app.MapControllerRoute(
                name: "Artist",
                pattern: "/ManagePainting",
                defaults: new { controller = "Artist", action = "Painting" });

            app.MapControllerRoute(
                name: "Artist",
                pattern: "/PaintingDetailArtist",
                defaults: new { controller = "Artist", action = "PaintingDetailArtist" });

            app.MapControllerRoute(
                    name: "Artist",
                    pattern: "/StatisticArtist",
                    defaults: new { controller = "Artist", action = "StatisticArtist" });

            app.MapControllerRoute(
                    name: "Artist",
                    pattern: "/Promotion",
                    defaults: new { controller = "Artist", action = "Promotion" });

            app.MapControllerRoute(
                    name: "Artist",
                    pattern: "/PromotionDetailArtist",
                    defaults: new { controller = "Artist", action = "PromotionDetailArtist" });

            app.MapControllerRoute(
                    name: "Artist",
                    pattern: "/ProfileArtist",
                    defaults: new { controller = "Artist", action = "ProfileArtist" });

            app.MapControllerRoute(
                    name: "Artist",
                    pattern: "/OrderArtist",
                    defaults: new { controller = "Artist", action = "OrderArtist" });

            app.MapControllerRoute(
                    name: "Artist",
                    pattern: "/OrderDetailArtist",
                    defaults: new { controller = "Artist", action = "OrderDetailArtist" });

            app.MapControllerRoute(
                name: "Supervisor",
                pattern: "/PaintingManagement",
                defaults: new { controller = "Supervisor", action = "PaintingManagement" });

            app.MapControllerRoute(
                    name: "Supervisor",
                    pattern: "/PaintingDetailSupervisor",
                    defaults: new { controller = "Supervisor", action = "PaintingDetailSupervisor" });

            app.MapControllerRoute(
                    name: "Supervisor",
                    pattern: "/PostManagementSupervisor",
                    defaults: new { controller = "Supervisor", action = "PostManagementSupervisor" });

            app.MapControllerRoute(
                    name: "Supervisor",
                    pattern: "/PostDetailSupervisor",
                    defaults: new { controller = "Supervisor", action = "PostDetailSupervisor" });

            app.MapControllerRoute(
                    name: "Supervisor",
                    pattern: "/SupervisorProfile",
                    defaults: new { controller = "Supervisor", action = "SupervisorProfile" });

            app.MapControllerRoute(
                name: "Manager",
                pattern: "/CooperatorManagement",
                defaults: new { controller = "Manager", action = "CooperatorManagement" });

            app.MapControllerRoute(
                name: "Manager",
                pattern: "/CooperatorDetails",
                defaults: new { controller = "Manager", action = "CooperatorDetails" });

            app.MapControllerRoute(
                name: "Manager",
                pattern: "/StatisticManager",
                defaults: new { controller = "Manager", action = "StatisticManager" });

            app.MapControllerRoute(
                name: "Admin",
                pattern: "/PromotionAdmin",
                defaults: new { controller = "Admin", action = "PromotionAdmin" });

            app.MapControllerRoute(
                name: "Admin",
                pattern: "/FilPromotionAdmin",
                defaults: new { controller = "Admin", action = "PostPromotionAdmin" });

            app.MapControllerRoute(
                name: "Admin",
                pattern: "/PromotionDetailAdmin",
                defaults: new { controller = "Admin", action = "PromotionDetailAdmin" });

            app.MapControllerRoute(
                name: "Admin",
                pattern: "/ProfileAdmin",
                defaults: new { controller = "Admin", action = "ProfileAdmin" });

            app.MapControllerRoute(
                name: "Admin",
                pattern: "/UserManagement",
                defaults: new { controller = "Admin", action = "UserManagement" });


            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Customer}/{action=Index}");

            app.Run();
        }


    }
}