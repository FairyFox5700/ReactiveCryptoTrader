using System;
using CryptoTrackerDomain;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CryptoTracker.BL.Models
{

    public class MessageDTO<T>
    {
        public long Timestamp { get; }
        public T Data { get; }
        public string Currency { get; }
        public string Market { get; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Type Type { get; }

        public MessageDTO(long timestamp, T data, string currency, string market, Type type)
        {
            Timestamp = timestamp;
            Data = data;
            Currency = currency;
            Market = market;
            Type = type;
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || !(o is MessageDTO<T> that)) return false;

            if (Timestamp != that.Timestamp) return false;
            if (!Data.Equals(that.Data)) return false;
            if (!Currency.Equals(that.Currency)) return false;
            if (!Market.Equals(that.Market)) return false;

            return Type == that.Type;
        }

        public override int GetHashCode()
        {
            int result = (int) (Timestamp ^ (Timestamp >> 32));
            result = 31 * result + Data.GetHashCode();
            result = 31 * result + Currency.GetHashCode();
            result = 31 * result + Market.GetHashCode();
            result = 31 * result + Type.GetHashCode();
            return result;
        }

        public override string ToString()
        {
            return "MessageDTO{" +
                   "timestamp=" + Timestamp +
                   ", data=" + Data +
                   ", currency='" + Currency + '\'' +
                   ", market='" + Market + '\'' +
                   ", type=" + Type +
                   '}';
        }

        public static MessageDTO<float> Avg(float avg, string currency, string market)
        {
            return new MessageDTO<float>(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), avg, currency, market,
                Type.AVG);
        }

        public static MessageDTO<float> Price(float price, string currency, string market)
        {
            return new MessageDTO<float>(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), price, currency, market,
                Type.PRICE);
        }

        public static MessageDTO<MessageTrade> TradeMessage(long timestamp, float price, float amount, string currency,
            string market)
        {
            return new MessageDTO<MessageTrade>(timestamp, new MessageTrade(price, amount), currency, market,
                Type.TRADE);
        }


    }

    public enum Type
    {
        PRICE,
        AVG,
        TRADE
    }
}
