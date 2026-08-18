using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShoeStore.Data;
using ShoeStore.Services; // Thêm namespace chứa EmailService
using System.Text;

namespace ShoeStore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. ĐĂNG KÝ KẾT NỐI DATABASE SQL SERVER
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // 2. ĐĂNG KÝ MEMORY CACHE (Lưu OTP trong 5 phút) & DỊCH VỤ GỬI EMAIL
            builder.Services.AddMemoryCache();
            builder.Services.AddScoped<EmailService>();

            // =========================================================
            // BỔ SUNG DỊCH VỤ SESSION (SỬA LỖI 500 SYSTEM.INVALIDOPERATIONEXCEPTION)
            // =========================================================
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(60); // Thời gian sống của Session (60 phút)
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // 3. CẤU HÌNH JWT AUTHENTICATION
            var jwtSettings = builder.Configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? "MinheeShop_Super_Secret_Key_2026_DotNet8_JWT_Authentication");

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };

                // Đọc JWT Token từ Cookie "AuthToken"
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.TryGetValue("AuthToken", out var token))
                        {
                            context.Token = token;
                        }
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        // Chưa đăng nhập mà truy cập trang có [Authorize] -> Chuyển sang /TaiKhoan/DangNhap
                        context.HandleResponse();
                        context.Response.Redirect("/TaiKhoan/DangNhap");
                        return Task.CompletedTask;
                    }
                };
            });

            // 4. ĐĂNG KÝ MVC VIEWS + RAZOR RUNTIME COMPILATION (Hot Reload CSHTML)
            builder.Services.AddControllersWithViews()
                            .AddRazorRuntimeCompilation();

            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            // 5. CHO PHÉP ĐỌC FILE TĨNH (CSS, JS, Images từ wwwroot)
            app.UseStaticFiles();

            // 6. THỨ TỰ MIDDLEWARE ĐỊNH TUYẾN, SESSION & XÁC THỰC
            app.UseRouting();

            // BẮT BUỘC: UseSession phải nằm sau UseRouting và trước UseAuthentication
            app.UseSession();

            // BẮT BUỘC: UseAuthentication phải nằm TRƯỚC UseAuthorization
            app.UseAuthentication();
            app.UseAuthorization();

            // 7. ROUTE MẶC ĐỊNH MỞ TRANG CHỦ
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=TrangChu}/{action=TrangChu}/{id?}");

            app.Run();
        }
    }
}