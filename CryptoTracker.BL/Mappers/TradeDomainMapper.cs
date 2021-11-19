using System;
using CryptoTracker.BL.Models;
using CryptoTrackerDomain;

namespace CryptoTracker.BL.Mappers
{
    public static class TradeDomainMapper
    {
        public static Trade MapToDomain(MessageDTO<MessageTrade> messageDto)
        {
            var trade = new Trade()
            {
                Id = Guid.NewGuid().ToString(),
                Price = messageDto.Data.Price,
                Amount = messageDto.Data.Amount,
                Market = messageDto.Market,
                Timestamp = messageDto.Timestamp,
                Currency = messageDto.Currency,

            };
            return trade;
        }
    }
}