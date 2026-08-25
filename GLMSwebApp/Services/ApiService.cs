using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace GLMS.Web.Services
{
    // Code Attribution
    // Title: Make HTTP requests using IHttpClientFactory in ASP.NET Core
    // Author: Microsoft
    // Date: 2026
    // Availability: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests

    // Central service for all API calls — MVC controllers talk to THIS, not the DB
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        // Attach the JWT token from the session to every request
        private void AttachToken()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<T?> GetAsync<T>(string url)
        {
            AttachToken();
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return default;
            }
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }

        public async Task<HttpResponseMessage> PostAsync<T>(string url, T data)
        {
            AttachToken();
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await _httpClient.PostAsync(url, content);
        }

        // FIX: Added PutAsync — needed for client updates (full replace)
        public async Task<HttpResponseMessage> PutAsync<T>(string url, T data)
        {
            AttachToken();
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await _httpClient.PutAsync(url, content);
        }

        public async Task<HttpResponseMessage> PatchAsync<T>(string url, T data)
        {
            AttachToken();
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await _httpClient.PatchAsync(url, content);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string url)
        {
            AttachToken();
            return await _httpClient.DeleteAsync(url);
        }

        public async Task<string?> LoginAsync(string username, string password)
        {
            var loginData = new
            {
                username = username,
                password = password
            };
            var body = new StringContent(
                JsonConvert.SerializeObject(loginData),
                Encoding.UTF8,
                "application/json"
            );
            var response = await _httpClient.PostAsync("api/auth/login", body);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            var json = await response.Content.ReadAsStringAsync();
            dynamic? result = JsonConvert.DeserializeObject<dynamic>(json);
            return result?.token?.ToString();
        }
    }
}