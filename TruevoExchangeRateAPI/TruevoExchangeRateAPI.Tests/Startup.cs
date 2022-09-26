using AutoMapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TruevoExchangeRateAPI.Data;
using TruevoExchangeRateAPI.Data.Options;
using TruevoExchangeRateAPI.Data.Repository;
using TruevoExchangeRateAPI.Services;


namespace TruevoExchangeRateAPI.Tests
{
    public class Startup
    {
        public void ConfigureHost(IHostBuilder hostBuilder)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
            hostBuilder.ConfigureHostConfiguration(builder =>
            {
                builder.AddJsonFile("appsettings.Testing.json", true);
            });
        }

        public void ConfigureServices(IServiceCollection services, HostBuilderContext hostBuilderContext)
        {
            // Auto Mapper Configurations
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new TruevoExchangeRateAutomapperProfile());
            });

            IMapper mapper = mappingConfig.CreateMapper();
            services.AddSingleton(mapper);

            services.Configure<ExchangeRatesSettingsOptions>(hostBuilderContext.Configuration.GetSection(ExchangeRatesSettingsOptions.ExchangeRatesSettings));
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            services.AddDbContext<TruevoExchangeRateDbContext>(options =>
            {
                options.UseSqlite(connection);
            });
            services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
            services.AddTransient<IExchangeRateService, ExchangeRateService>();
        }
    }
}
