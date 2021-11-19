using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CryptoTrackerDomain;

namespace CryptoTracker.DAL.Contract
{
    public interface ITradeRepository
    {
        IObservable<int> SaveAll(List<Trade> trades);
    }
}