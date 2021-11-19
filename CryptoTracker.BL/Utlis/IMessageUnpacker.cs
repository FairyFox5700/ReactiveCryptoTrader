using System;
using System.Collections.Generic;

namespace CryptoTracker.BL.Utlis
{
    public interface IMessageUnpacker
    {
        public bool Supports(string messageType);
        public Dictionary<string, Object> Unpack(string message);
    }
}