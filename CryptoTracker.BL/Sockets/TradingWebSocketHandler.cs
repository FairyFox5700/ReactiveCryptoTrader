using System;
using System.Reactive.Linq;
using CryptoTracker.BL.Contract;
using Microsoft.Extensions.Logging;

namespace CryptoTracker.BL.Sockets
{
    public class TradingWebSocketHandler
    {
        private readonly IPriceService _priceService;
        private readonly ITradeService _tradeService;
        private readonly ILogger<TradingWebSocketHandler> _logger;
        
        public TradingWebSocketHandler(IPriceService priceService,
            ITradeService tradeService, 
            ILogger<TradingWebSocketHandler> logger)
        {
            _priceService = priceService;
            _tradeService = tradeService;
            _logger = logger;
        }
        public IObservable<dynamic> Handle(IObservable<string> inbound)
        {
            return inbound.Let(HandleRequestedAveragePriceIntervalValue)
                .Let(_priceService.PricesStream)
                .Merge<dynamic>(_tradeService.TradeStream());
        }

        private IObservable<long> HandleRequestedAveragePriceIntervalValue(IObservable<String> requestedInterval) {
            return requestedInterval.Where(e => Int64.TryParse(e, out var longVal) && longVal >= 0 && longVal < 60)
                .Select(long.Parse)
                .Do(e=>_logger.LogInformation($"DATA:{e}"));
        }
    }
}