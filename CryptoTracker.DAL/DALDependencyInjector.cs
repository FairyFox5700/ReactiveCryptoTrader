using CryptoTracker.DAL.Configuration;
using CryptoTracker.DAL.Contract;
using CryptoTracker.DAL.Implementation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoTracker.DAL
{
    public static class DALDependencyInjector
    {
        public static IServiceCollection AddDALDependency(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ITradingConfiguration>(provider =>
                provider.GetRequiredService<TradingConfiguration>());
            services.AddDbContext<TradesContext>(opt
                => opt.UseSqlServer(configuration.GetConnectionString("TradingDatabase")),
                ServiceLifetime.Transient);
            services.AddTransient<ITradeRepository, RelationalDbRepository>();
            services.AddTransient<ITradeRepository, MongoRepository>();
            return services;
        }
    }
}