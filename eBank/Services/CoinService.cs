using System.Net.Http;
using System.Text.Json;
using eBank.Models;

namespace eBank.Services
{
    public class CoinService
    {
        // Injected HttpClient to make API requests
        private readonly HttpClient _httpClient;

        public List<CryptoListItem> Coins { get; private set; } = new List<CryptoListItem>();

        public CoinService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task LoadCoinsAsync()
        {
            var url = "https://api.coingecko.com/api/v3/coins/list";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return; 

            var json = await response.Content.ReadAsStringAsync();

            Coins = JsonSerializer.Deserialize<List<CryptoListItem>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<CryptoListItem>();
        }
        public async Task LoadPriceAsync()
        {

            var first20 = Coins.Take(20).ToList();
            var ids = string.Join(",", first20.Select(c => c.Id));

            var url = $"https://api.coingecko.com/api/v3/simple/price?ids={ids}&vs_currencies=usd";
            var json = await _httpClient.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            foreach (var coin in first20)
            {
                if (root.TryGetProperty(coin.Id, out var coinData) && coinData.TryGetProperty("usd", out var usd))
                {
                    coin.PriceUsd = usd.GetDecimal();
                }
            }
        }
        public async Task LoadPriceForCoinsAsync(List<CryptoListItem> coins)
        {
            if (coins.Count == 0) return;

            var ids = string.Join(",", coins.Select(c => c.Id));
            var url = $"https://api.coingecko.com/api/v3/simple/price?ids={ids}&vs_currencies=usd";

            var json = await _httpClient.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            foreach (var coin in coins)
            {
                if (root.TryGetProperty(coin.Id, out var coinData) && coinData.TryGetProperty("usd", out var usd))
                {
                    coin.PriceUsd = usd.GetDecimal();
                }
            }
        }

    }
}
