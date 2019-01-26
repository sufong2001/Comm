using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Net.Http.Headers;

namespace Sufong2001.Comm.Tests.Base
{
    public class AppicationBaseFixture
    {
        public IConfiguration Configuration { get; }
        public CommRepository Repository { get; }

        public AppicationBaseFixture()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);


            Configuration = configurationBuilder.Build();


            Repository = new CommRepository(Configuration["Values:AzureWebJobsStorage"]);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //services.BuildServiceProvider().GetService<>()

            services.AddHttpClient<IGithubClient, GithubClient>(client =>
            {
                client.BaseAddress = new Uri("https://api.github.com");
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Yolo", "0.1.0"));
            });
        }

        //public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        //{
        //    if (env.IsDevelopment())
        //    {
        //        //app.UseDeveloperExceptionPage();
        //    }
        //    else
        //    {
        //        //app.UseExceptionHandler("/Error");
        //        //app.UseHsts();
        //    }

        //    app.UseMvc();
        //}
    }
}