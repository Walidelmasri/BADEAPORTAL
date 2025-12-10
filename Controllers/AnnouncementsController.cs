using Microsoft.AspNetCore.Mvc;

namespace BADEAPORTAL.Controllers
{
    public class AnnouncementsController : Controller
    {
        // GET: /Announcements
        public IActionResult Index()
        {
            return View();
        }
    }
}
