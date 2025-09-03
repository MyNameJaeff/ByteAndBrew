using Microsoft.AspNetCore.Mvc;

namespace ByteAndBrew.Controllers
{
    public class AdminPanelController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
