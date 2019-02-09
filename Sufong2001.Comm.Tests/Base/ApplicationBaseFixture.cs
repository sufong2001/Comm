using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sufong2001.Comm.AzureStorage;
using Sufong2001.Comm.BusinessEntities;
using System;
using System.IO;
using System.Net.Http.Headers;

namespace Sufong2001.Comm.Tests.Base
{
    public class ApplicationBaseFixture
    {
        public IConfiguration Configuration { get; }
        public CommRepository Repository { get; }

        public IdGenerator IdGenerator => new IdGenerator();

        public App App { get; }

        public ApplicationBaseFixture()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);

            Configuration = configurationBuilder.Build();

            Repository = new CommRepository(StorageAccount.NewFromConnectionString(Configuration["Values:AzureWebJobsStorage"]));
            App = new App(Repository);
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