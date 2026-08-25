namespace GLMS.Web.Services
{

    // Code Attribution
    // Title: Strategy Pattern
    // Author: Refactoring Guru
    // Date: 2026
    // Availability: https://refactoring.guru/design-patterns/strategy

    public interface ICurrencyStrategy
    {
        Task<decimal> GetRateAsync();
    }

    public class ApiCurrencyStrategy : ICurrencyStrategy
    {
        private readonly HttpClient _httpClient;
        public ApiCurrencyStrategy(HttpClient httpClient) { _httpClient = httpClient; }

        public async Task<decimal> GetRateAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://open.er-api.com/v6/latest/USD");
                if (!response.IsSuccessStatusCode) return 18.5m;
                var content = await response.Content.ReadAsStringAsync();
                var json = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(content);
                decimal rate = json["rates"]["ZAR"];
                return rate;
            }
            catch { return 18.5m; }
        }
    }

    public class FallbackCurrencyStrategy : ICurrencyStrategy
    {
        public Task<decimal> GetRateAsync() => Task.FromResult(18.5m);
    }
}