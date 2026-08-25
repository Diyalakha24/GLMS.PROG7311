namespace GLMS.API.Services
{
    public class CurrencyService
    {
        private readonly HttpClient _httpClient;

        public CurrencyService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<decimal> GetUsdToZarRateAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://open.er-api.com/v6/latest/USD");

                if (!response.IsSuccessStatusCode)
                    return 18.5m;

                var content = await response.Content.ReadAsStringAsync();
                var json = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(content);
                decimal rate = json["rates"]["ZAR"];
                return rate;
            }
            catch
            {
                return 18.5m;
            }
        }
    }
}