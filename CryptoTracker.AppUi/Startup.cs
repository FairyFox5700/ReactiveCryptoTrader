using System;
using System.Reactive.Subjects;
using CryptoTracker.AppUi.Middleware;
using CryptoTracker.BL;
using CryptoTracker.DAL;
using CryptoTracker.DAL.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace CryptoTracker.AppUi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        private ReplaySubject<byte[]> _dataStream;
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var config = new TradingConfiguration();
            Configuration.Bind(nameof(TradingConfiguration), config);
            services.AddSingleton(config);
            services.AddSingleton<WebSocketServerConnectionManager>();
            services.AddDALDependency(Configuration);
            services.InstallBALServices();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseRouting();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            var webSocketOptions = new WebSocketOptions() 
            {
                KeepAliveInterval = TimeSpan.FromSeconds(1200),
            };
            
            app.UseWebSockets(webSocketOptions);
            app.UseWebSocketServer();
            app.UseDefaultFiles();
            app.UseStaticFiles();
        }
    }
}

       
        