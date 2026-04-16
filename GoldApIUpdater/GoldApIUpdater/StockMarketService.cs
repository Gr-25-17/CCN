
using System.Net.Http.Json;


    public class StockMarketService
    {

        private readonly HttpClient _httpClient;

        public StockMarketService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Top10?> GetGoldAsync()
        {
            var url = "https://stockapinewsapp.azurewebsites.net/summary";

            var result = await _httpClient.GetFromJsonAsync<StockPrice>(url);

            // Match the actual API symbol
            var gold = result?.Top10Stock?
                .FirstOrDefault(x => x.Symbol == "GC=F" || x.Name == "Gold");

            return gold;
        }
    }

