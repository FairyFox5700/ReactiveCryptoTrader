using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using CryptoTracker.BL.Implementation;
using CryptoTracker.BL.Models;
using CryptoTracker.BL.Utlis;
using Microsoft.Extensions.Logging;

namespace CryptoTracker.BL.Contract
{
	public class DefaultPriceService : IPriceService
	{
		private readonly ILogger<DefaultPriceService> _logger;
		private static long _defaultAvgPriceInterval = 30L;
		private readonly IObservable<MessageDTO<float>> _sharedStream;


		public DefaultPriceService(ICryptoService cryptoService, ILogger<DefaultPriceService> logger)
		{
			_logger = logger;
			_sharedStream = cryptoService.EventStream()
				.Let(SelectOnlyPriceUpdateEvents)
				.Let(CurrentPrice)
				.Do((e) => _logger.LogInformation($"Price event {e}"));
		}

		public IObservable<MessageDTO<float>> PricesStream(IObservable<long> intervalPreferencesStream)
		{
			return _sharedStream.
				Merge(AveragePrice(intervalPreferencesStream, _sharedStream)
					.Do(e=>_logger.LogInformation($"Average price: {e}")))
				.Do((e) => _logger.LogInformation($"Price stream {e}"));
		}
		
		IObservable<Dictionary<string, object>> SelectOnlyPriceUpdateEvents(
			IObservable<Dictionary<string, object>> input)
		{

			return input.Where(e => MessageMapper.IsPriceMessageType(e)
			                        && MessageMapper.IsValidPriceMessage(e));
		}
		IObservable<MessageDTO<float>> CurrentPrice(IObservable<Dictionary<string, object>> input)
		{
			return SelectOnlyPriceUpdateEvents(input)
				.Select(MessageMapper.MapToPriceMessage);
		}
		
		IObservable<MessageDTO<float>> AveragePrice(IObservable<long> requestedInterval,
			IObservable<MessageDTO<float>> priceData)
		{
			return
				priceData
					.Window(requestedInterval.DefaultIfEmpty(_defaultAvgPriceInterval))
					.Switch()
					.GroupBy(e => e.Currency)
					.SelectMany(grp =>
						grp.Average(s => s.Data)
							.Select(av => MessageDTO<float>.Avg(av, grp.Key, "") //market??????
							));
		}
	}
}