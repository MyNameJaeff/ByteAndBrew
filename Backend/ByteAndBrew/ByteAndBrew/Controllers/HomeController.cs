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
            // Want to fetch this "https://localhost:7145/api/MenuItems/popular"
            var response = await _client.GetAsync("MenuItems/popular");

            var popularItems = await response.Content.ReadFromJsonAsync<List<MenuItem>>();

            return View(popularItems);
        }
    }
}
