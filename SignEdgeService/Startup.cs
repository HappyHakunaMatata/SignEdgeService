

using System.Configuration;
using System.Net;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;

namespace SignEdgeService
{
    public class Startup
    {
        private readonly string? _token;


        public Startup(IConfiguration configuration)
        {
            _token = configuration["token"];
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var trustedProxyIp = Environment.GetEnvironmentVariable("KnownProxy");
            Console.WriteLine($"Proxy: {trustedProxyIp}");
            services.AddControllersWithViews();
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                if (IPAddress.TryParse(trustedProxyIp, out var ip))
                {
                    options.KnownProxies.Add(ip);
                }
                else
                {
                    options.KnownProxies.Add(IPAddress.Loopback);
                }
            });
            
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Configure the HTTP request pipeline.
            if (!env.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            //app.UseHttpsRedirection();
            app.UseStaticFiles();


            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                name: "acme-challenge",
                pattern: $".well-known/acme-challenge/{_token}",
                defaults: new { controller = "AcmeChallenge", action = "Index" });
                
                endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}