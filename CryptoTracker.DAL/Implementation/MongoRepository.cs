using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using CryptoTracker.DAL.Configuration;
using CryptoTracker.DAL.Contract;
using CryptoTrackerDomain;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CryptoTracker.DAL.Implementation
{
    public class MongoRepository:ITradeRepository
    {
        private readonly IMongoCollection<Trade> _trades;
        private readonly ILogger<MongoRepository> _logger;

        public MongoRepository(ITradingConfiguration settings, ILogger<MongoRepository> logger)
        {
            _logger = logger;
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _trades= database.GetCollection<Trade>(settings.TradesCollectionName);
            ReportDbStatistics();
        }
        
        private void ReportDbStatistics()
        {
            Observable.Interval(TimeSpan.FromSeconds(5))
                .SelectMany(i => GetTradeStats())
                .Do(count => _logger.LogWarning("------------- [DB STATS] ------------ Trades " + "stored in DB: " + count))
                .SubscribeOn(NewThreadScheduler.Default)
                .Subscribe();
        }

        private IObservable<long> GetTradeStats()
        {
            return Observable.Return(_trades.CountDocuments(new BsonDocument()));
        }

        public IObservable<int> SaveAll(List<Trade> trades)
        {
            return StoreInMongo(trades);
        }

        private IObservable<int> StoreInMongo(List<Trade> trades)
        {
            try
            {
                _trades.InsertMany(trades);
                return Observable.Return(trades.Count);
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception while storing to mongo db: {e.Message}");
                throw;
            }
        }
    }
        
}