using System;
using System.Collections.Generic;
using CryptoTracker.BL.Models;
using CryptoTrackerDomain;

namespace CryptoTracker.BL.Utlis
{
    public class MessageMapper
    {
        private static readonly string TYPE_KEY = "TYPE";
        private static readonly string TIMESTAMP_KEY = "TIMESTAMP";
        private static readonly string PRICE_KEY = "PRICE";
        private static readonly string QUANTITY_KEY = "QUANTITY";
        private static readonly string CURRENCY_KEY = "FROMSYMBOL";
        private static readonly string MarketKey = "MARKET";
        private static readonly string FLAGS_KEY = "FLAGS";

        public static MessageDTO<float> MapToPriceMessage(Dictionary<string, object> messageEvent)
        {
            return MessageDTO<float>.Price(
                (float) messageEvent[PRICE_KEY],
                (string) messageEvent[CURRENCY_KEY],
                (string) messageEvent[MarketKey]);
        }

        public static MessageDTO<MessageTrade> MapToTradeMessage(Dictionary<string, object> messageEvent)
        {
            return MessageDTO<MessageTrade>.TradeMessage(
                (long) (float) messageEvent[TIMESTAMP_KEY] * 1000,
                (float) messageEvent[PRICE_KEY],
                messageEvent[FLAGS_KEY].Equals("1")
                    ? (float) messageEvent[QUANTITY_KEY]
                    : -(float) messageEvent[QUANTITY_KEY],
                (string) messageEvent[CURRENCY_KEY],
                (string) messageEvent[MarketKey]);
        }

        public static bool IsPriceMessageType(Dictionary<string, object> messageEvent)
        {
            return messageEvent.ContainsKey(TYPE_KEY) &&
                   messageEvent[TYPE_KEY].Equals("5");
        }

        public static bool IsValidPriceMessage(Dictionary<string, object> messageEvent)
        {
            return messageEvent.ContainsKey(PRICE_KEY) &&
                   messageEvent.ContainsKey(CURRENCY_KEY) &&
                   messageEvent.ContainsKey(MarketKey);
        }

        public static bool IsTradeMessageType(Dictionary<string, object> messageEvent)
        {
            return messageEvent.ContainsKey(TYPE_KEY) &&
                   messageEvent[TYPE_KEY].Equals("0");
        }
    }
}