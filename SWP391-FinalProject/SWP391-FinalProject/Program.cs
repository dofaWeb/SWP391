using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Filters;

namespace SWP391_FinalProject
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<DBContext>(options => { options.UseMySql(builder.Configuration.GetConnectionString("SWP391"), ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("SWP391"))); });

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
            {
                options.LoginPath = "/Acc/Login";
                options.LogoutPath = "/Acc/Logout";
            });

            // Cấu hình dịch vụ (services)
            var services = builder.Services;
            services.AddControllersWithViews();

            // Cấu hình Authentication với Google
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
            })
            .AddGoogle(googleOptions =>
            {
                // Đọc thông tin Authentication:Google từ appsettings.json
                IConfigurationSection googleAuthNSection = builder.Configuration.GetSection("Authentication:Google");

                // Thiết lập ClientID và ClientSecret để truy cập API google
                googleOptions.ClientId = googleAuthNSection["ClientId"];
                googleOptions.ClientSecret = googleAuthNSection["ClientSecret"];

                // Cấu hình URL callback từ Google (không thiết lập thì mặc định là /signin-google)
                googleOptions.CallbackPath = "/signin-google";
            });

            builder.Services.AddControllersWithViews();

            builder.Services.AddScoped<ProManAuthorizationFilter>();

            builder.Services.AddControllersWithViews(option =>
            {
                // Apply the filter globally
                option.Filters.Add<ProManAuthorizationFilter>();
            });

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

            // Middleware để xử lý lỗi xác thực
            app.Use(async (context, next) =>
            {
                try
                {
                    await next();
                }
                catch (AuthenticationFailureException ex)
                {
                    // Xử lý lỗi xác thực
                    context.Items["Error"] = $"Fail to Login: {ex.Message}. Please try again.";
                    context.Response.Redirect("/Acc/Login"); // Chuyển hướng đến trang chính
                    await context.Response.CompleteAsync();
                }
            });

            app.UseAuthentication(); // Đảm bảo Authentication được gọi trước UseAuthorization
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Pro}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
