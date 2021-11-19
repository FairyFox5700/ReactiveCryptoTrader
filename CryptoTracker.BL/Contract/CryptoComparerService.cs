using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using CryptoTracker.BL.Clients;
using CryptoTracker.BL.Implementation;
using CryptoTracker.BL.Utlis;
using Microsoft.Extensions.Logging;

namespace CryptoTracker.BL.Contract
{
    public class CryptoComparerService :ICryptoService
    {
        private readonly ILogger<CryptoComparerService> _logger;
        public const int CacheSize = 3;

        private  readonly  IObservable<Dictionary<string,object>> _reactiveCryptoListener;

        public CryptoComparerService(CryptoComparerClient cryptoComparerClient,ILogger<CryptoComparerService> logger)
        {
            _logger = logger;
            _reactiveCryptoListener = cryptoComparerClient
                .Connect(new List<string>()
                     {
                        "5~CCCAGG~BTC~USD", "0~Coinbase~BTC~USD", "0~Cexio~BTC~USD",
                    }.ToObservable(),
                    new List<IMessageUnpacker>()
                    {
                        new PriceMessageUnpacker(),
                        new TradeMessageUnpacker(),
                    })
                .Let(ProvideResilience)
                .Let(ProvideCaching);
        }
        
        private static IObservable<T> ProvideResilience<T>(IObservable<T> input)
        {
            return input.RetryWithBackoffStrategy(retryCount:4);
        }
        
        private IObservable<T> ProvideCaching<T>(IObservable<T> input)
        {
            return input.Replay(3)
                .AutoConnect();
        }

        public IObservable<Dictionary<string, object>> EventStream()
        {
            return  _reactiveCryptoListener;
        }
    }
}