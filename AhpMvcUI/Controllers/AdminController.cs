using Microsoft.AspNetCore.Mvc;

namespace AhpMvcUI.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Surveys()
        {
            return View();
        }

        public IActionResult SurveyAnswers(int id)
        {
            ViewBag.SurveyId = id;
            return View();
        }

        public IActionResult Users()
        {
            return View();
        }

        public IActionResult Categories()
        {
            return View();
        }
    }
}