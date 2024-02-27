namespace Filmzie.ApiService.Interface
{
    public interface IOMDBService
    {
        Task<HttpResponseMessage> MedialistAsync(string mediaType, string mediaCategory, int page);
        Task<HttpResponseMessage> MediaByIdAsync(string mediaId);
        Task<HttpResponseMessage> MediaByTitleAsync(string mediaTitle, string year);
        Task<HttpResponseMessage> MediaSearchAsync(string query, int page);
    }
}
