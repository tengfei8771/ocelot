using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConsulBuilder;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;
using Ocelot.Provider.Polly;

namespace APIGateWay
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
            var config = new ConfigurationBuilder().AddJsonFile("ocelot.json").Build();
            services.AddControllers();
            services.Configure<ConsulServiceOptions>(config);
            services.AddOcelot(config)
                .AddCacheManager(x =>
                {
                    x.WithDictionaryHandle();
                })
                .AddConsul()
                .AddPolly();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("ApiGateway", new OpenApiInfo { Title = "网关服务", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            //https重定向中间件
            //app.UseHttpsRedirection();

            app.UseRouting();

            //app.UseAuthorization();
            List<string> Names = new List<string>()
            {
                {"PublicWebApi"}
            };
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            app.UseSwagger()
                .UseSwaggerUI(options =>
                {
                    Names.ForEach(m =>
                    {
                        options.SwaggerEndpoint($"/{m}/swagger.json", m);
                    });
                }); 
            app.UseOcelot().Wait();

        }
    }
}
