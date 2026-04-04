using Microsoft.AspNetCore.Mvc;

namespace AhpMvcUI.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}