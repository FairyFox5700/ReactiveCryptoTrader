namespace CryptoTracker.DAL.Configuration
{
    public interface ITradingConfiguration
    {
        string CryptoTrackingCollectionName { get; set; }
        string TradesCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }

    public class TradingConfiguration : ITradingConfiguration
    {
        public string CryptoTrackingCollectionName { get; set; }
        public  string TradesCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
}