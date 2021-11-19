using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using CryptoTracker.BL.Contract;
using CryptoTracker.BL.Mappers;
using CryptoTracker.BL.Models;
using CryptoTracker.BL.Utlis;
using CryptoTracker.DAL.Contract;
using CryptoTrackerDomain;
using Microsoft.Extensions.Logging;

namespace CryptoTracker.BL.Implementation
{
    public class DefaultTradeService:ITradeService
    {
        private readonly ILogger<DefaultTradeService> _logger;
        private readonly IObservable<MessageDTO<Trade>> _sharedStream;
        private readonly ICryptoService _cryptoComparerService;
        private readonly IEnumerable<ITradeRepository> _tradeRepositories;

        public DefaultTradeService(ICryptoService cryptoComparerService,
            IEnumerable<ITradeRepository> tradeRepositories,
            ILogger<DefaultTradeService> logger)
        {
            _cryptoComparerService = cryptoComparerService;
            _tradeRepositories = tradeRepositories;
            _logger = logger;
        }

        IObservable<MessageDTO<MessageTrade>> FilterAndMapTradingEvents(IObservable<Dictionary<string, object>> input)
        {
            return input
                .Where(MessageMapper.IsTradeMessageType)
                .Select(MessageMapper.MapToTradeMessage);
        }

        IObservable<Trade> MapToDomainTrade(IObservable<MessageDTO<MessageTrade>> input)
        {
            return input.Select(TradeDomainMapper.MapToDomain);
        }

        IObservable<int> ResilientlyStoreByBatchesToAllRepositories(IObservable<Trade> input, ITradeRepository tradeRepository,
            ITradeRepository tradeRepository2)
        {
           return input.Buffer(5)
                .SelectMany(e=>
                    SaveIntoMongoDatabase(tradeRepository, e.ToList())
                    .Merge(SaveIntoRelationalDatabase(tradeRepository2,e.ToList())))
                .Do(e=>_logger.LogInformation("Saved to db"));
        }

        IObservable<int> SaveIntoMongoDatabase(ITradeRepository tradeRepository1, List<Trade> trades)
        {
            return tradeRepository1.SaveAll(trades).RetryWithBackoffStrategy();
        }

        IObservable<int> SaveIntoRelationalDatabase(ITradeRepository tradeRepository2, List<Trade> trades)
        {
            return tradeRepository2.SaveAll(trades).RetryWithBackoffStrategy();
        }

        public IObservable<MessageDTO<MessageTrade>> TradeStream()
        {
            return _cryptoComparerService.EventStream()
                .Let(FilterAndMapTradingEvents)
                .Let(trades =>
                {
                    trades.Let(MapToDomainTrade)
                        .Let(f => ResilientlyStoreByBatchesToAllRepositories(f, _tradeRepositories.First(),
                            _tradeRepositories.Last()))
                        .Subscribe(new Subject<int>());

                    return trades;
                });
        }
    }
}