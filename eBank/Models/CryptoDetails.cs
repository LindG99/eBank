using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace eBank.Models
{
    public class CryptoDetails
    {
        // Properties to hold cryptocurrency details
        // nullable before getting data from API
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public decimal? PriceUsd { get; set; } 
        public decimal? PriceSek { get; set; }   
        public decimal? PriceChange24h { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
