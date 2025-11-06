using System;
using System.Threading.Tasks;
using YahooFinanceApi;

namespace WheelTracker.Services
{
    /// <summary>
    /// Yahoo Finance API service for real-time prices. Graceful fallback on errors (e.g., market closed).
    /// </summary>
    public class YahooQuoteService : IQuoteService
    {
        public async Task<double?> GetPriceAsync(string ticker)
        {
            try
            {
                var securities = await Yahoo.Symbols(ticker).Fields(Field.RegularMarketPrice).QueryAsync();
                return securities[ticker].RegularMarketPrice;
            }
            catch (Exception)
            {
                // Log via injected ILogger in prod; fallback to null
                return null;
            }
        }
    }
}