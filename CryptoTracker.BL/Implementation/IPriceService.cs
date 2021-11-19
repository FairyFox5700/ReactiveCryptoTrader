using System;
using CryptoTracker.BL.Models;

namespace CryptoTracker.BL.Implementation
{
    public interface IPriceService
    {
        public  IObservable<MessageDTO<float>> PricesStream(IObservable<long> intervalPreferencesStream);
    }
}