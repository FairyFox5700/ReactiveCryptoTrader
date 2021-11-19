using System;
using CryptoTracker.BL.Models;

namespace CryptoTracker.BL.Contract
{
    public interface ITradeService
    {
        IObservable<MessageDTO<MessageTrade>> TradeStream();
    }
}