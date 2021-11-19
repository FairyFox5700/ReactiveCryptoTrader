namespace CryptoTracker.BL.Models
{
    public class MessageTrade
    {
        public float Price { get; }
        public float Amount { get; }

        public MessageTrade(float price, float amount)
        {
            Price = price;
            Amount = amount;
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || !(o is MessageTrade trade)) return false;

            if (trade.Price != Price) return false;
            return trade.Amount == Amount;
        }

        public override int GetHashCode()
        {
            int result = (Price != +0.0f ? (int)Price : 0);
            result = 31 * result + (Amount != +0.0f ? (int)Amount : 0);
            return result;
        }
    }
}