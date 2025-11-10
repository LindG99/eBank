using Azure;
using eBank.Models;
using eBank.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Model;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace eBank.Data
{
    public class CryptoController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly CoinService _coinService;
        public CryptoController(IHttpClientFactory httpClientFactory, CoinService coinService)
        {
            _httpClient = httpClientFactory.CreateClient();
            _coinService = coinService;
        }
        // Index action to list cryptocurrencies
        public async Task<IActionResult> Index(string search)
        {
            if (_coinService.Coins.Count == 0)
            {
                await _coinService.LoadCoinsAsync();
            }

            // Filter coins on search
            var filteredCoins = string.IsNullOrWhiteSpace(search)
                ? _coinService.Coins
                : _coinService.Coins
                    .Where(c => c.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                             || c.Symbol.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            
            await _coinService.LoadPriceForCoinsAsync(filteredCoins.Take(100).ToList());

            ViewBag.SearchTerm = search;
            return View(filteredCoins);
        }

        // Details action to show details of a specific cryptocurrency
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Coin ID saknas.");
            }
            //Api
            var url = $"https://api.coingecko.com/api/v3/coins/{id}"; // Example: bitcoin, ethereum. each coin has its own id
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return NotFound("Coin hittades inte eller API error.");
            }
            var json = await response.Content.ReadAsStringAsync();
            using var dock = JsonDocument.Parse(json);
            var root = dock.RootElement;

            var market = root.GetProperty("market_data");

            var coin = new CryptoDetails()
            {
                Id = root.GetProperty("id").GetString(),
                Name = root.GetProperty("name").GetString(),
                Symbol = root.GetProperty("symbol").GetString(),
                ImageUrl = root.GetProperty("image").GetProperty("large").GetString(),
                Description = root.GetProperty("description").GetProperty("en").GetString(),
                PriceUsd = market.GetProperty("current_price").GetProperty("usd").GetDecimal(),
                PriceSek = market.GetProperty("current_price").GetProperty("sek").GetDecimal(),
                PriceChange24h = market.GetProperty("price_change_percentage_24h").GetDecimal()
            };
            return View(coin);
        }
       
    }
}
