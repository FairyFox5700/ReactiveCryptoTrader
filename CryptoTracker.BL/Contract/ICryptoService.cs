using System;
using System.Collections.Generic;

namespace CryptoTracker.BL.Contract
{
    public interface ICryptoService
    {
        public IObservable<Dictionary<string, object>> EventStream();
    }
}