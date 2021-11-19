using CryptoTrackerDomain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CryptoTracker.DAL.Configuration
{
    public class TradesContext:DbContext
    {
        public TradesContext(DbContextOptions<TradesContext> options)
            :base(options)
        {
        }
        public  virtual  DbSet<Trade> Trades { get; set; }
    }
    
}