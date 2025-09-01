using Microsoft.AspNetCore.Mvc;

namespace Byte___Brew.Controllers
{
    public class AdminPanelController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
