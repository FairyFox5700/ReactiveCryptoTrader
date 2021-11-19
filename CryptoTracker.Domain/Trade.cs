using System;
using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CryptoTrackerDomain
{
    public class Trade
    {
       [BsonId]
       [Column("id")]
        public string Id { get; set; }
        [Column("trade_timestamp")]
        public long Timestamp { get; set; }
        [Column("price")]
        public float Price { get; set; }
        [Column("amount")]
        public float Amount { get; set; }
        [Column("currency")]
        public String Currency { get; set; }
        [Column("market")]
        public String Market { get; set; }
        public override string ToString()
        {
            return "Trade{" + "id=" + this.Id + ", timestamp=" + this.Timestamp + ", price=" + this.Price + 
                   ", amount=" + this.Amount + ", currency='" + this.Currency + '\'' + ", market='" + this.Market + '\'' + '}';
        }
    }
}