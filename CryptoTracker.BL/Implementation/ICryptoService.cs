using System;
using System.Collections.Generic;

namespace CryptoTracker.BL.Implementation
{
    public interface ICryptoService
    {
        public IObservable<Dictionary<string, object>> EventStream();
    }
}