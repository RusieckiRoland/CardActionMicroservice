using Microsoft.AspNetCore.Mvc;

namespace CardActionMicroservice.Controllers
{
    public class AllowedActionsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
