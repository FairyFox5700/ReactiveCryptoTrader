using CryptoTracker.BL.Clients;
using CryptoTracker.BL.Contract;
using CryptoTracker.BL.Implementation;
using CryptoTracker.BL.Sockets;
using CryptoTracker.BL.Utlis;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoTracker.BL
{
    public static class BALDependencyInjector
    {
        public static IServiceCollection InstallBALServices(this IServiceCollection services)
        {
            services.AddTransient<CryptoComparerClient>();
            services.AddTransient<ICryptoService, CryptoComparerService>();
            services.AddTransient<IPriceService, DefaultPriceService>();
            services.AddTransient<IMessageUnpacker, PriceMessageUnpacker>();
            services.AddTransient<IMessageUnpacker, TradeMessageUnpacker>();
            services.AddTransient<ITradeService, DefaultTradeService>();
            services.AddTransient<TradingWebSocketHandler>();
            return services;
        }
    }
}