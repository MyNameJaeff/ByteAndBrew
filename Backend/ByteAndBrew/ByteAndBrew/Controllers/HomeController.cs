using Microsoft.AspNetCore.Mvc;

namespace Byte___Brew.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
