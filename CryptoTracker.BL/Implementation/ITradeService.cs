using System;
using CryptoTracker.BL.Models;
using CryptoTrackerDomain;

namespace CryptoTracker.BL.Implementation
{
    public interface ITradeService
    {
        IObservable<MessageDTO<MessageTrade>> TradeStream();
    }
}