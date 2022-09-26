using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using TruevoExchangeRateAPI.Data;
using TruevoExchangeRateAPI.Data.Options;
using TruevoExchangeRateAPI.Data.Repository;
using TruevoExchangeRateAPI.Services;

namespace TruevoExchangeRateAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddHealthChecks();
            builder.Services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
            builder.Services.AddTransient(typeof(IExchangeRateService), typeof(ExchangeRateService));
            builder.Services.AddAutoMapper(typeof(Program).Assembly);

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<TruevoExchangeRateDbContext>(options =>
            {
                options.UseNpgsql(builder.Configuration.GetConnectionString("TruevoCurrencyExchangeDatabase"));
            });

            builder.Services.AddHttpClient("ExchangeRateClient")
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    var handler = new HttpClientHandler();
                    handler.ServerCertificateCustomValidationCallback = (HttpRequestMessage httpRequestMessage, X509Certificate2 x509Certificate2, X509Chain x509Chain, SslPolicyErrors sslPolicyErrors) => true;
                    return handler;
                });

            builder.Services.Configure<ExchangeRatesSettingsOptions>(builder.Configuration.GetSection(ExchangeRatesSettingsOptions.ExchangeRatesSettings));

            var app = builder.Build();
            app.MapHealthChecks("/healthz");
            
            // Configure the HTTP request pipeline.
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
                endpoints.MapSwagger();
            });
            app.MapControllers();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("v1/swagger.json", "TruevoExchangeRates API V1");
                c.DocExpansion(DocExpansion.None);
            });


            app.UseAuthorization();



            app.Run();
        }
    }
}