using Microsoft.EntityFrameworkCore;
using ShoeStore.Data;

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

            // 2. ĐĂNG KÝ MVC VIEWS + RAZOR RUNTIME COMPILATION (Hot Reload CSHTML)
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

            // 3. CHO PHÉP ĐỌC FILE TĨNH (CSS, JS, Images từ wwwroot)
            app.UseStaticFiles();

            // 4. THỨ TỰ MIDDLEWARE ĐỊNH TUYẾN
            app.UseRouting();

            app.UseAuthorization();

            // 5. ROUTE MẶC ĐỊNH MỞ TRANG CHỦ KHI F5 HOẶC TRUY CẬP LOGO/GỐC WEBSITE
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=TrangChu}/{action=TrangChu}/{id?}");

            app.Run();
        }
    }
}