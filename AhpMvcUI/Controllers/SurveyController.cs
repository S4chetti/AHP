using Microsoft.AspNetCore.Mvc;

namespace AhpMvcUI.Controllers
{
    public class SurveyController : Controller
    {
        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Solve(int id)
        {
            ViewBag.SurveyId = id;
            return View();
        }
    }
}