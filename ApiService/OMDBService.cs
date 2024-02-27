using Filmzie.ApiService.Interface;
using Filmzie.Models;
using Microsoft.Extensions.Options;

namespace Filmzie.ApiService
{
    public class OMDBService : IOMDBService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _apikey;

        public OMDBService(HttpClient httpClient, IOptions<OMDBSettings> omdbConfig)
        {
            _httpClient = httpClient;
            _baseUrl = omdbConfig.Value.BaseUrl;
            _apikey = omdbConfig.Value.ApiKey; ;
        }

        public async Task<HttpResponseMessage> MediaByTitleAsync(string mediaTitle, string year = null)
        {
            string apiUrl = $"{_baseUrl}/?apikey={_apikey}&t={mediaTitle}&plot=full";

            if (!string.IsNullOrEmpty(year))
            {
                apiUrl += $"&y={year}";
            }

            return await _httpClient.GetAsync(apiUrl);
        }

        public async Task<HttpResponseMessage> MediaByIdAsync(string imdbId)
        {
            string apiUrl = $"{_baseUrl}/?apikey={_apikey}&i={imdbId}&plot=full";
            return await _httpClient.GetAsync(apiUrl);
        }


        public async Task<HttpResponseMessage> MedialistAsync(string mediaType, string mediaCategory, int page)
        {
            string apiUrl = $"{_baseUrl}/?apikey={_apikey}&type={mediaType}&s={mediaCategory}&page={page}";
            return await _httpClient.GetAsync(apiUrl);
        }


        public async Task<HttpResponseMessage> MediaSearchAsync(string query, int page)
        {
            string apiUrl = $"{_baseUrl}/?apikey={_apikey}&s={query}&page={page}";
            return await _httpClient.GetAsync(apiUrl);
        }


    }
}
