using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CryptoTracker.DAL.Contract;
using CryptoTrackerDomain;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Z.Dapper.Plus;

namespace CryptoTracker.DAL.Implementation
{
    public class RelationalDbRepository : ITradeRepository
    {
        private readonly ILogger<RelationalDbRepository> _logger;

        private static string TRADES_COUNT_QUERY = @"SELECT COUNT(*) as cnt FROM trades";

        private static string INSERT_TRADE_QUERY =
            @"INSERT INTO trades (id, trade_timestamp, price, amount, currency, market) " +
            "VALUES ($1, $2, $3, $4, $5, $6)";

        private static string SELECT_COUNT_OF_6 = "SELECT COUNT(id) FROM trades";
        private readonly string _connectionString;

        public RelationalDbRepository(ILogger<RelationalDbRepository> logger, IConfiguration configuration)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("TradingDatabase");
            PingDatabase();
            ReportDbStatistics();
        }
        private void PingDatabase()
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();
            try
            {
                var result = connection.QueryFirstOrDefault<int>(SELECT_COUNT_OF_6);
                _logger.LogWarning($"RESULT FOR SELECT COUNT QUERY: {result}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        private void ReportDbStatistics()
        {
            Observable.Interval(TimeSpan.FromSeconds(5))
                .SelectMany(e => GetTradesStatistics())
                .Do(count =>
                    _logger.LogInformation($"------------- [DB STATS] ------------ Trades stored in DB: {count}"))
                .SubscribeOn(NewThreadScheduler.Default)
                .Subscribe();
        }

        private IObservable<long> GetTradesStatistics()
        {
            return Observable.Defer(() =>
                {
                    using SqlConnection con = new SqlConnection(_connectionString);
                    con.Open();
                    try
                    {
                        long result = con.QueryFirstOrDefault<long>(TRADES_COUNT_QUERY);
                        return Observable.Return(result);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message);
                        throw;
                    }
                }
            );
        }

        public IObservable<int> SaveAll(List<Trade> trades)
        {
            var result = SaveTrades(trades);
            return Observable.FromAsync(async ()=>await result)
                .Do(e => _logger.LogInformation($"--- [DB] --- Inserted {e} trades into DB"));
        }

        private async  Task<int> SaveTrades(List<Trade> trades)
        {
            try
            {
                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                DapperPlusManager.Entity<Trade>().Table("Trades")
                    .Identity(e => e.Id);
                connection.BulkInsert(trades);
                return trades.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }
    }
}