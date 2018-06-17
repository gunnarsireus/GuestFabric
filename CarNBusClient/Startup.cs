using System;
using System.IO;
using System.Threading.Tasks;
using CarNBusClient.DAL;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CarNBusClient.Models;
using CarNBusClient.Services;
using Microsoft.AspNetCore.Http;

namespace CarNBusClient
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
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddMvc()
                .AddSessionStateTempDataProvider();

            services.AddSession();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            var task = ConfigureServicesAsync(services);

            task.Wait();
        }

        public async Task ConfigureServicesAsync(IServiceCollection services)
        {
            string sqlServerConnectionString = null;
            var aspNetDbLocation = new AspNetDbLocation();
            try
            {
                sqlServerConnectionString = await aspNetDbLocation.GetAspNetDbAsync(Configuration.GetSection("ConnectionStrings:ApiAddress").Get<string>());
            }
            catch (Exception e)
            {
                //Do nothing
            }
            if (sqlServerConnectionString != null)
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(sqlServerConnectionString));

            }
            else
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlite("Data Source=" + Directory.GetCurrentDirectory() + "\\App_Data\\AspNet.db"));
            }
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseSession();

            app.Use(async (context, next) =>
            {
                context.Session.SetString("ApiAddress", Configuration.GetSection("ConnectionStrings:ApiAddress").Get<string>());
                await next();
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    "default",
                    "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}