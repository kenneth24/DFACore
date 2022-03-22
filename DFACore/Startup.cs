using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using DFACore.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DFACore.Models;
using NETCore.MailKit.Extensions;
using NETCore.MailKit.Infrastructure.Internal;
using DFACore.Repository;
using Wkhtmltopdf.NetCore;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Http;
using DFACore.Helpers;
using Microsoft.AspNetCore.Authentication.Cookies;
using UnionBankApi;
using UnionBankPayment;

namespace DFACore
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;
                options.SignIn.RequireConfirmedAccount = true;
            }).AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
            //services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
            //    .AddEntityFrameworkStores<ApplicationDbContext>();

            //services.AddSession(options =>
            //{
            //    options.IdleTimeout = TimeSpan.FromSeconds(3600);
            //});

            //services.AddDistributedMemoryCache();
            //services.AddSession();
            //services.AddSession(options =>
            //{
            //    options.IdleTimeout = TimeSpan.FromSeconds(10);
            //    options.Cookie.HttpOnly = true;
            //    options.Cookie.IsEssential = true;
            //});

            //services.Configure<CookiePolicyOptions>(options =>
            //{
            //    options.MinimumSameSitePolicy = SameSiteMode.Strict;
            //    options.OnAppendCookie = cookieContext =>
            //        CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
            //    options.OnDeleteCookie = cookieContext =>
            //        CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
            //});

            //services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            //.AddCookie();

            services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = new Microsoft.AspNetCore.Http.PathString("/Administration/AccessDenied");
                //options.ExpireTimeSpan = TimeSpan.FromSeconds(10);
                //options.SlidingExpiration = true;
            });

            services.AddControllersWithViews();
            services.AddMvc().AddRazorRuntimeCompilation();
            services.AddRazorPages();


            services.AddSession(options => {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
            });

            services.AddMailKit(config =>
            {
                config.UseMailKit(Configuration.GetSection("Email").Get<MailKitOptions>());
            });
            services.AddTransient<IMessageService, MessageService>();
            services.Configure<ReCAPTCHASetting>(Configuration.GetSection("GoogleReCAPTCHA"));
            services.AddTransient<GoogleCaptchaService>();
            services.AddTransient<ApplicantRecordRepository>();
            services.AddTransient<AdministrationRepository>();
            services.AddTransient<UnionBankPaymentClient>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddBrowserDetection();
            services.AddWkhtmltopdf();
            //services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddSingleton(serviceProvider => new UnionBankClient(Configuration.GetSection("UnionBankApiConfiguration").Get<UnionBankClientConfiguration>()));
            services.AddSingleton<Helpers.Payment.PaymentDataCache>();
        }

        private void CheckSameSite(HttpContext httpContext, CookieOptions options)
        {
            if (options.SameSite == SameSiteMode.None)
            {
                var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
                if (MyUserAgentDetectionLib.DisallowsSameSiteNone(userAgent))
                {
                    options.SameSite = SameSiteMode.Unspecified;
                }
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        //[Obsolete]
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }


            //app.UseDeveloperExceptionPage();
            //app.UseDatabaseErrorPage();

            //app.UseExceptionHandler("/Home/Error");
            //app.UseHsts();


            app.UseHttpsRedirection();
            app.UseStaticFiles();

        

            app.UseRouting();

            app.UseCookiePolicy();
            //app.UseSession();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();



            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    //pattern: "{controller=Account}/{action=Login}/{id?}");
                    pattern: "{controller=Home}/{action=Initial}/");
                endpoints.MapRazorPages();
            });
        }
    }



}
