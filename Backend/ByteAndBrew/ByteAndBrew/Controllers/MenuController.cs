using ByteAndBrew.Data;
using ByteAndBrew.Dtos.Menu;
using ByteAndBrew.Dtos.MenuItem;
using ByteAndBrew.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ByteAndBrew.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MenuItemsController : ControllerBase
    {
        private readonly ByteAndBrewDbContext _db;
        public MenuItemsController(ByteAndBrewDbContext db) => _db = db;

        [AllowAnonymous] // Everyone should be able to view the menu
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _db.MenuItems.ToListAsync();
            var dtoList = items.Select(m => new MenuItemReadDto
            {
                Id = m.Id,
                Name = m.Name,
                Description = m.Description,
                Price = m.Price,
                IsPopular = m.IsPopular,
                ImageUrl = m.ImageUrl
            }).ToList();

            return Ok(dtoList);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("popular")]
        public async Task<IActionResult> GetPopularItems()
        {
            var popularItems = await _db.MenuItems
                .Where(m => m.IsPopular)
                .Where(m => m.ImageUrl != null) // Ensure there's an image to show for popular items
                .ToListAsync();
            var dtoList = popularItems.Select(m => new MenuItemReadDto
            {
                Id = m.Id,
                Name = m.Name,
                Description = m.Description,
                Price = m.Price,
                IsPopular = m.IsPopular,
                ImageUrl = m.ImageUrl
            }).ToList();
            return Ok(dtoList);
        }

        [AllowAnonymous] // Everyone should be able to view the menu
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var m = await _db.MenuItems.FindAsync(id);
            if (m == null) return NotFound($"The menu item with id {id} does not exist");

            var dto = new MenuItemReadDto
            {
                Id = m.Id,
                Name = m.Name,
                Description = m.Description,
                Price = m.Price,
                IsPopular = m.IsPopular,
                ImageUrl = m.ImageUrl
            };

            return Ok(dto);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(MenuItemCreateDto dto)
        {
            var existingItem = await _db.MenuItems.FirstOrDefaultAsync(m => m.Name == dto.Name);
            if (existingItem != null) return BadRequest("Menu item with this name already exists.");

            var menuItem = new MenuItem
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                IsPopular = dto.IsPopular,
                ImageUrl = dto.ImageUrl
            };

            _db.MenuItems.Add(menuItem);
            await _db.SaveChangesAsync();

            var readDto = new MenuItemReadDto
            {
                Id = menuItem.Id,
                Name = menuItem.Name,
                Description = menuItem.Description,
                Price = menuItem.Price,
                IsPopular = menuItem.IsPopular,
                ImageUrl = menuItem.ImageUrl
            };

            return CreatedAtAction(nameof(Get), new { id = menuItem.Id }, readDto);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, MenuItemCreateDto dto)
        {
            var menuItem = await _db.MenuItems.FindAsync(id);
            if (menuItem == null) return NotFound($"The menu item with id {id} does not exist");

            menuItem.Name = dto.Name;
            menuItem.Description = dto.Description;
            menuItem.Price = dto.Price;
            menuItem.IsPopular = dto.IsPopular;
            menuItem.ImageUrl = dto.ImageUrl;

            await _db.SaveChangesAsync();

            var readDto = new MenuItemReadDto
            {
                Id = menuItem.Id,
                Name = menuItem.Name,
                Description = menuItem.Description,
                Price = menuItem.Price,
                IsPopular = menuItem.IsPopular,
                ImageUrl = menuItem.ImageUrl
            };

            return Ok(new { Message = $"The menu item with id {id} has been updated.", UpdatedItem = readDto });
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var m = await _db.MenuItems.FindAsync(id);
            if (m == null) return NotFound($"The menu item with id {id} does not exist");

            _db.MenuItems.Remove(m);
            await _db.SaveChangesAsync();

            return Ok(new { Message = $"The menu item with id {id} has been deleted." });
        }
    }

    // Everything below is for MVC view rendering, not API
    public class MenuController : Controller
    {
        private readonly HttpClient _client;
        public MenuController(IHttpClientFactory clientFactory)
        {
            _client = clientFactory.CreateClient("ByteAndBrewAPI");
        }

        public async Task<IActionResult> Index()
        {
            var response = await _client.GetAsync("MenuItems");

            var popularItems = await response.Content.ReadFromJsonAsync<List<MenuItem>>();

            return View(popularItems);
        }
    }
}
