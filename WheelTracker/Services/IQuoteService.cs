using System.Threading.Tasks;

namespace WheelTracker.Services
{
    /// <summary>
    /// Interface for fetching live stock quotes. Supports wheel risk calcs (e.g., ITM checks).
    /// </summary>
    public interface IQuoteService
    {
        Task<double?> GetPriceAsync(string ticker);
    }
}