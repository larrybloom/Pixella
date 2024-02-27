using Filmzie.ApiService;
using Filmzie.ApiService.Interface;
using Filmzie.Context;
using Filmzie.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Filmzie.Controllers
{
    [Route("api/")]
    [ApiController]
    public class MediaController : ControllerBase
    {
        private readonly IOMDBService _omdbService;
        private readonly AppDbContext _dbContext;

        public MediaController(IOMDBService service, AppDbContext dbContext)
        {
            _omdbService = service;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Get details of a media by its ID.
        /// </summary>
        /// <param name="mediaId">The ID of the media.</param>
        /// <returns>Returns media details.</returns>
        [HttpGet("Id/{mediaId}")]
        public async Task<IActionResult> GetMediaDetails(string mediaId)
        {
            try
            {
                HttpResponseMessage response = await _omdbService.MediaByIdAsync(mediaId);

                if (response.IsSuccessStatusCode)
                {
                    var mediaDetails = await response.Content.ReadAsStringAsync();

                    return Ok(mediaDetails);
                }
                else
                {
                    return StatusCode((int)response.StatusCode, response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Search media by title and optional year.
        /// </summary>
        /// <param name="Title">The title of the media.</param>
        /// <param name="year">Optional parameter for the release year.</param>
        /// <returns>Returns search results for the specified title and year.</returns>
        [HttpGet("Title/{Title}")]
        public async Task<IActionResult> SearchMediaByYear(string Title, string? year)
        {
            try
            {
                SaveQueryToDatabase(Title);
                HttpResponseMessage response = await _omdbService.MediaByTitleAsync(Title, year);

                if (response.IsSuccessStatusCode)
                {
                    var mediaDetails = await response.Content.ReadAsStringAsync();

                    return Ok(mediaDetails);
                }
                else
                {
                    return StatusCode((int)response.StatusCode, response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, $"Error: {ex.Message}");
            }
        }



        /// <summary>
        /// Search media by query and page number.
        /// </summary>
        /// <param name="query">The search query.</param>
        /// <param name="page">The page number for paginated results.</param>
        /// <returns>Returns search results for the specified query and page.</returns>
        [HttpGet("search/{query}/{page}")]
        public async Task<IActionResult> SearchMedia(string query, int page)
        {
            try
            {
                SaveQueryToDatabase(query);
                HttpResponseMessage response = await _omdbService.MediaSearchAsync(query, page);

                if (response.IsSuccessStatusCode)
                {
                    var mediaSearchResults = await response.Content.ReadAsStringAsync();
                    return Ok(mediaSearchResults);
                }
                else
                {
                    return StatusCode((int)response.StatusCode, response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get the latest 5 search queries from the database.
        /// </summary>
        /// <returns>Returns the latest 5 search queries.</returns>
        [HttpGet("latest-queries")]
        public IActionResult GetLatestQueries()
        {
            try
            {
                var latestQueries = _dbContext.SearchQueries
                    .OrderByDescending(q => q.CreatedAt)
                    .Take(5)
                    .ToList();

                return Ok(latestQueries);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, $"Error: {ex.Message}");
            }
        }


        private void SaveQueryToDatabase(string query)
        {
            var searchQuery = new SearchQuery
            {
                Query = query,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.SearchQueries.Add(searchQuery);
            _dbContext.SaveChanges();
        }
    }
}
