using ByteAndBrew.Models;
using Microsoft.AspNetCore.Mvc;

namespace ByteAndBrew.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _client;
        public HomeController(IHttpClientFactory clientFactory)
        {
            _client = clientFactory.CreateClient("ByteAndBrewAPI");
        }

        public async Task<IActionResult> Index()
        {
            var response = await _client.GetAsync("MenuItems/popular");

            if (!response.IsSuccessStatusCode)
                return View(new List<MenuItem>()); // fallback empty list

            var popularItems = await response.Content.ReadFromJsonAsync<List<MenuItem>>();

            if (popularItems == null || !popularItems.Any())
                return View(new List<MenuItem>()); // fallback empty list

            // Shuffle and take max 8 items
            var random = new Random();
            var randomPopularItems = popularItems
                .OrderBy(x => random.Next())
                .Take(8)
                .ToList();

            return View(randomPopularItems);
        }

    }
}
